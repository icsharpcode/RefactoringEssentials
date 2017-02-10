﻿using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using VBasic = Microsoft.CodeAnalysis.VisualBasic;
using VBSyntax = Microsoft.CodeAnalysis.VisualBasic.Syntax;
using RefactoringEssentials.Converter;
using Microsoft.CodeAnalysis.Text;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace RefactoringEssentials.CSharp.Converter
{
	public partial class VisualBasicConverter
	{
		enum TokenContext
		{
			Global,
			InterfaceOrModule,
			Member,
			VariableOrConst,
			Local,
			MemberInModule
		}

		public static CSharpSyntaxNode Convert(VBasic.VisualBasicSyntaxNode input, SemanticModel semanticModel, Document targetDocument)
		{
			return input.Accept(new NodesVisitor(semanticModel, targetDocument));
		}

		public static ConversionResult ConvertText(string text, MetadataReference[] references)
		{
			if (text == null)
				throw new ArgumentNullException(nameof(text));
			if (references == null)
				throw new ArgumentNullException(nameof(references));
			var tree = VBasic.SyntaxFactory.ParseSyntaxTree(SourceText.From(text));
			var compilation = VBasic.VisualBasicCompilation.Create("Conversion", new[] { tree }, references);
			try
			{
				return new ConversionResult(Convert((VBasic.VisualBasicSyntaxNode)tree.GetRoot(), compilation.GetSemanticModel(tree, true), null).NormalizeWhitespace().ToFullString());
			}
			catch (Exception ex)
			{
				return new ConversionResult(ex);
			}
		}

		static Dictionary<string, VariableDeclarationSyntax> SplitVariableDeclarations(VBSyntax.VariableDeclaratorSyntax declarator, NodesVisitor nodesVisitor, SemanticModel semanticModel)
		{
			var rawType = (TypeSyntax)declarator.AsClause?.TypeSwitch(
				(VBSyntax.SimpleAsClauseSyntax c) => c.Type,
				(VBSyntax.AsNewClauseSyntax c) => VBasic.SyntaxExtensions.Type(c.NewExpression),
				_ => { throw new NotImplementedException($"{_.GetType().FullName} not implemented!"); }
			)?.Accept(nodesVisitor) ?? SyntaxFactory.ParseTypeName("var");

			var initializer = (ExpressionSyntax)declarator.AsClause?.TypeSwitch(
				(VBSyntax.SimpleAsClauseSyntax _) => declarator.Initializer?.Value,
				(VBSyntax.AsNewClauseSyntax c) => c.NewExpression
			)?.Accept(nodesVisitor) ?? (ExpressionSyntax)declarator.Initializer?.Value.Accept(nodesVisitor);

			var newDecls = new Dictionary<string, VariableDeclarationSyntax>();

			foreach (var name in declarator.Names)
			{
				var type = rawType;
				if (!name.Nullable.IsKind(VBasic.SyntaxKind.None))
				{
					if (type is ArrayTypeSyntax)
						type = ((ArrayTypeSyntax)type).WithElementType(SyntaxFactory.NullableType(((ArrayTypeSyntax)type).ElementType));
					else
						type = SyntaxFactory.NullableType(type);
				}
				if (name.ArrayRankSpecifiers.Count > 0)
					type = SyntaxFactory.ArrayType(type, SyntaxFactory.List(name.ArrayRankSpecifiers.Select(a => (ArrayRankSpecifierSyntax)a.Accept(nodesVisitor))));
				VariableDeclarationSyntax decl;
				var v = SyntaxFactory.VariableDeclarator(ConvertIdentifier(name.Identifier, semanticModel), null, initializer == null ? null : SyntaxFactory.EqualsValueClause(initializer));
				string k = type.ToString();
				if (newDecls.TryGetValue(k, out decl))
					newDecls[k] = decl.AddVariables(v);
				else
					newDecls[k] = SyntaxFactory.VariableDeclaration(type, SyntaxFactory.SingletonSeparatedList(v));
			}

			return newDecls;
		}

		static ExpressionSyntax Literal(object o) => CodeRefactorings.ComputeConstantValueCodeRefactoringProvider.GetLiteralExpression(o);

		static SyntaxToken ConvertIdentifier(SyntaxToken id, SemanticModel semanticModel, bool isAttribute = false)
		{
			var keywordKind = SyntaxFacts.GetKeywordKind(id.ValueText);
			if (keywordKind != SyntaxKind.None && !SyntaxFacts.IsPredefinedType(keywordKind))
				return SyntaxFactory.Identifier("@" + id.ValueText);
			string text = id.ValueText;
			if (id.SyntaxTree == semanticModel.SyntaxTree)
			{
				var symbol = semanticModel.GetSymbolInfo(id.Parent).Symbol;
                if (symbol != null && !string.IsNullOrWhiteSpace(symbol.Name))
                {
                    if (symbol.IsConstructor() && isAttribute)
                    {
                        text = symbol.ContainingType.Name;
                        if (text.EndsWith("Attribute", StringComparison.Ordinal))
                            text = text.Remove(text.Length - "Attribute".Length);
                    }
                    else
                        text = symbol.Name;
                }
			}
			return SyntaxFactory.Identifier(text);
		}

		static SyntaxTokenList ConvertModifiers(IEnumerable<SyntaxToken> modifiers, TokenContext context = TokenContext.Global)
		{
			return SyntaxFactory.TokenList(ConvertModifiersCore(modifiers, context));
		}

		static SyntaxTokenList ConvertModifiers(SyntaxTokenList modifiers, TokenContext context = TokenContext.Global)
		{
			return SyntaxFactory.TokenList(ConvertModifiersCore(modifiers, context));
		}

		static SyntaxToken? ConvertModifier(SyntaxToken m, TokenContext context = TokenContext.Global)
		{
			var token = ConvertToken(VBasic.VisualBasicExtensions.Kind(m), context);
			return token == SyntaxKind.None ? null : new SyntaxToken?(SyntaxFactory.Token(token));
		}

		static IEnumerable<SyntaxToken> ConvertModifiersCore(IEnumerable<SyntaxToken> modifiers, TokenContext context)
		{
			if (context != TokenContext.Local && context != TokenContext.InterfaceOrModule)
			{
				bool visibility = false;
				foreach (var token in modifiers)
				{
					if (IsVisibility(token, context))
					{
						visibility = true;
						break;
					}
				}
				if (!visibility && (context == TokenContext.MemberInModule || context == TokenContext.Member))
					yield return VisualBasicDefaultVisibility(context);
			}
			foreach (var token in modifiers.Where(m => !IgnoreInContext(m, context)))
			{
				var m = ConvertModifier(token, context);
				if (m.HasValue) yield return m.Value;
			}
			if (context == TokenContext.MemberInModule)
				yield return SyntaxFactory.Token(SyntaxKind.StaticKeyword);
		}

		static bool IgnoreInContext(SyntaxToken m, TokenContext context)
		{
			switch (VBasic.VisualBasicExtensions.Kind(m))
			{
				case VBasic.SyntaxKind.OptionalKeyword:
				case VBasic.SyntaxKind.ByValKeyword:
				case VBasic.SyntaxKind.IteratorKeyword:
				case VBasic.SyntaxKind.DimKeyword:
					return true;
				case VBasic.SyntaxKind.ReadOnlyKeyword:
				case VBasic.SyntaxKind.WriteOnlyKeyword:
					return context == TokenContext.Member;
				default:
					return false;
			}
		}

		static bool IsVisibility(SyntaxToken token, TokenContext context)
		{
			return token.IsKind(VBasic.SyntaxKind.PublicKeyword, VBasic.SyntaxKind.FriendKeyword, VBasic.SyntaxKind.ProtectedKeyword, VBasic.SyntaxKind.PrivateKeyword)
				|| (context == TokenContext.VariableOrConst && token.IsKind(VBasic.SyntaxKind.ConstKeyword));
		}

		static SyntaxToken VisualBasicDefaultVisibility(TokenContext context)
		{
			switch (context)
			{
				case TokenContext.Global:
					return SyntaxFactory.Token(SyntaxKind.InternalKeyword);
				case TokenContext.Local:
				case TokenContext.VariableOrConst:
				case TokenContext.Member:
					return SyntaxFactory.Token(SyntaxKind.PrivateKeyword);
				case TokenContext.MemberInModule:
					return SyntaxFactory.Token(SyntaxKind.PublicKeyword);
			}
			throw new ArgumentOutOfRangeException(nameof(context));
		}

		static SyntaxToken ConvertToken(SyntaxToken t, TokenContext context = TokenContext.Global)
		{
			return SyntaxFactory.Token(ConvertToken(VBasic.VisualBasicExtensions.Kind(t), context));
		}

		static SyntaxKind ConvertToken(VBasic.SyntaxKind t, TokenContext context = TokenContext.Global)
		{
			switch (t)
			{
				case VBasic.SyntaxKind.None:
					return SyntaxKind.None;
				// built-in types
				case VBasic.SyntaxKind.BooleanKeyword:
					return SyntaxKind.BoolKeyword;
				case VBasic.SyntaxKind.ByteKeyword:
					return SyntaxKind.ByteKeyword;
				case VBasic.SyntaxKind.SByteKeyword:
					return SyntaxKind.SByteKeyword;
				case VBasic.SyntaxKind.ShortKeyword:
					return SyntaxKind.ShortKeyword;
				case VBasic.SyntaxKind.UShortKeyword:
					return SyntaxKind.UShortKeyword;
				case VBasic.SyntaxKind.IntegerKeyword:
					return SyntaxKind.IntKeyword;
				case VBasic.SyntaxKind.UIntegerKeyword:
					return SyntaxKind.UIntKeyword;
				case VBasic.SyntaxKind.LongKeyword:
					return SyntaxKind.LongKeyword;
				case VBasic.SyntaxKind.ULongKeyword:
					return SyntaxKind.ULongKeyword;
				case VBasic.SyntaxKind.DoubleKeyword:
					return SyntaxKind.DoubleKeyword;
				case VBasic.SyntaxKind.SingleKeyword:
					return SyntaxKind.FloatKeyword;
				case VBasic.SyntaxKind.DecimalKeyword:
					return SyntaxKind.DecimalKeyword;
				case VBasic.SyntaxKind.StringKeyword:
					return SyntaxKind.StringKeyword;
				case VBasic.SyntaxKind.CharKeyword:
					return SyntaxKind.CharKeyword;
				case VBasic.SyntaxKind.ObjectKeyword:
					return SyntaxKind.ObjectKeyword;
				// literals
				case VBasic.SyntaxKind.NothingKeyword:
					return SyntaxKind.NullKeyword;
				case VBasic.SyntaxKind.TrueKeyword:
					return SyntaxKind.TrueKeyword;
				case VBasic.SyntaxKind.FalseKeyword:
					return SyntaxKind.FalseKeyword;
				case VBasic.SyntaxKind.MeKeyword:
					return SyntaxKind.ThisKeyword;
				case VBasic.SyntaxKind.MyBaseKeyword:
					return SyntaxKind.BaseKeyword;
				// modifiers
				case VBasic.SyntaxKind.PublicKeyword:
					return SyntaxKind.PublicKeyword;
				case VBasic.SyntaxKind.FriendKeyword:
					return SyntaxKind.InternalKeyword;
				case VBasic.SyntaxKind.ProtectedKeyword:
					return SyntaxKind.ProtectedKeyword;
				case VBasic.SyntaxKind.PrivateKeyword:
					return SyntaxKind.PrivateKeyword;
				case VBasic.SyntaxKind.ByRefKeyword:
					return SyntaxKind.RefKeyword;
				case VBasic.SyntaxKind.ParamArrayKeyword:
					return SyntaxKind.ParamsKeyword;
				case VBasic.SyntaxKind.ReadOnlyKeyword:
					return SyntaxKind.ReadOnlyKeyword;
				case VBasic.SyntaxKind.OverridesKeyword:
					return SyntaxKind.OverrideKeyword;
				case VBasic.SyntaxKind.SharedKeyword:
					return SyntaxKind.StaticKeyword;
				case VBasic.SyntaxKind.ConstKeyword:
					return SyntaxKind.ConstKeyword;
				case VBasic.SyntaxKind.PartialKeyword:
					return SyntaxKind.PartialKeyword;
				case VBasic.SyntaxKind.MustInheritKeyword:
					return SyntaxKind.AbstractKeyword;
				case VBasic.SyntaxKind.MustOverrideKeyword:
					return SyntaxKind.AbstractKeyword;
                case VBasic.SyntaxKind.NotOverridableKeyword:
                case VBasic.SyntaxKind.NotInheritableKeyword:
					return SyntaxKind.SealedKeyword;
				// unary operators
				case VBasic.SyntaxKind.UnaryMinusExpression:
					return SyntaxKind.UnaryMinusExpression;
				case VBasic.SyntaxKind.UnaryPlusExpression:
					return SyntaxKind.UnaryPlusExpression;
				case VBasic.SyntaxKind.NotExpression:
					return SyntaxKind.LogicalNotExpression;
				// binary operators
				case VBasic.SyntaxKind.ConcatenateExpression:
				case VBasic.SyntaxKind.AddExpression:
					return SyntaxKind.AddExpression;
				case VBasic.SyntaxKind.SubtractExpression:
					return SyntaxKind.SubtractExpression;
				case VBasic.SyntaxKind.MultiplyExpression:
					return SyntaxKind.MultiplyExpression;
				case VBasic.SyntaxKind.DivideExpression:
					return SyntaxKind.DivideExpression;
				case VBasic.SyntaxKind.ModuloExpression:
					return SyntaxKind.ModuloExpression;
				case VBasic.SyntaxKind.AndAlsoExpression:
					return SyntaxKind.LogicalAndExpression;
				case VBasic.SyntaxKind.OrElseExpression:
					return SyntaxKind.LogicalOrExpression;
				case VBasic.SyntaxKind.OrExpression:
					return SyntaxKind.BitwiseOrExpression;
				case VBasic.SyntaxKind.AndExpression:
					return SyntaxKind.BitwiseAndExpression;
				case VBasic.SyntaxKind.ExclusiveOrExpression:
					return SyntaxKind.ExclusiveOrExpression;
				case VBasic.SyntaxKind.EqualsExpression:
					return SyntaxKind.EqualsExpression;
				case VBasic.SyntaxKind.NotEqualsExpression:
					return SyntaxKind.NotEqualsExpression;
				case VBasic.SyntaxKind.GreaterThanExpression:
					return SyntaxKind.GreaterThanExpression;
				case VBasic.SyntaxKind.GreaterThanOrEqualExpression:
					return SyntaxKind.GreaterThanOrEqualExpression;
				case VBasic.SyntaxKind.LessThanExpression:
					return SyntaxKind.LessThanExpression;
				case VBasic.SyntaxKind.LessThanOrEqualExpression:
					return SyntaxKind.LessThanOrEqualExpression;
				// assignment
				case VBasic.SyntaxKind.SimpleAssignmentStatement:
					return SyntaxKind.SimpleAssignmentExpression;
				case VBasic.SyntaxKind.AddAssignmentStatement:
					return SyntaxKind.AddAssignmentExpression;
				case VBasic.SyntaxKind.SubtractAssignmentStatement:
					return SyntaxKind.SubtractAssignmentExpression;
				case VBasic.SyntaxKind.MultiplyAssignmentStatement:
					return SyntaxKind.MultiplyAssignmentExpression;
				case VBasic.SyntaxKind.DivideAssignmentStatement:
					return SyntaxKind.DivideAssignmentExpression;
				// Casts
				case VBasic.SyntaxKind.CObjKeyword:
					return SyntaxKind.ObjectKeyword;
				case VBasic.SyntaxKind.CBoolKeyword:
					return SyntaxKind.BoolKeyword;
				case VBasic.SyntaxKind.CCharKeyword:
					return SyntaxKind.CharKeyword;
				case VBasic.SyntaxKind.CSByteKeyword:
					return SyntaxKind.SByteKeyword;
				case VBasic.SyntaxKind.CByteKeyword:
					return SyntaxKind.ByteKeyword;
				case VBasic.SyntaxKind.CShortKeyword:
					return SyntaxKind.ShortKeyword;
				case VBasic.SyntaxKind.CUShortKeyword:
					return SyntaxKind.UShortKeyword;
				case VBasic.SyntaxKind.CIntKeyword:
					return SyntaxKind.IntKeyword;
				case VBasic.SyntaxKind.CUIntKeyword:
					return SyntaxKind.UIntKeyword;
				case VBasic.SyntaxKind.CLngKeyword:
					return SyntaxKind.LongKeyword;
				case VBasic.SyntaxKind.CULngKeyword:
					return SyntaxKind.ULongKeyword;
				case VBasic.SyntaxKind.CDecKeyword:
					return SyntaxKind.DecimalKeyword;
				case VBasic.SyntaxKind.CSngKeyword:
					return SyntaxKind.FloatKeyword;
				case VBasic.SyntaxKind.CDblKeyword:
					return SyntaxKind.DoubleKeyword;
				case VBasic.SyntaxKind.CStrKeyword:
					return SyntaxKind.StringKeyword;
				//
				case VBasic.SyntaxKind.AssemblyKeyword:
					return SyntaxKind.AssemblyKeyword;
				case VBasic.SyntaxKind.AsyncKeyword:
					return SyntaxKind.AsyncKeyword;
			}
			throw new NotSupportedException(t + " not supported!");
		}
	}
}
