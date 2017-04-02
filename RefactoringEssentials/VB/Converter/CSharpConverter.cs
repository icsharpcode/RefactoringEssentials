﻿using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.CodeAnalysis;
using CS = Microsoft.CodeAnalysis.CSharp;
using CSS = Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.VisualBasic;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;
using RefactoringEssentials.VB.CodeRefactorings;
using RefactoringEssentials.Converter;
using Microsoft.CodeAnalysis.Text;

namespace RefactoringEssentials.VB.Converter
{
	public partial class CSharpConverter
    {
        enum TokenContext
        {
            Global,
            InterfaceOrModule,
            Member,
            VariableOrConst,
            Local
        }

        public static VisualBasicSyntaxNode Convert(CS.CSharpSyntaxNode input, SemanticModel semanticModel, Document targetDocument)
        {
            return input.Accept(new NodesVisitor(semanticModel, targetDocument));
        }

        public static ConversionResult ConvertText(string text, MetadataReference[] references)
        {
            if (text == null)
                throw new ArgumentNullException(nameof(text));
            if (references == null)
                throw new ArgumentNullException(nameof(references));
            var tree = CS.SyntaxFactory.ParseSyntaxTree(SourceText.From(text));
            var compilation = CS.CSharpCompilation.Create("Conversion", new[] { tree }, references);
            try
            {
                return new ConversionResult(Convert((CS.CSharpSyntaxNode)tree.GetRoot(), compilation.GetSemanticModel(tree, true), null).NormalizeWhitespace().ToFullString());
            }
            catch (Exception ex)
            {
                return new ConversionResult(ex);
            }
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
                if (!visibility && context == TokenContext.Member)
                    yield return CSharpDefaultVisibility(context); 
            }
            foreach (var token in modifiers.Where(m => !IgnoreInContext(m, context)))
            {
                var m = ConvertModifier(token, context);
                if (m.HasValue) yield return m.Value;
            }
        }

        static bool IgnoreInContext(SyntaxToken m, TokenContext context)
        {
            switch (context)
            {
                case TokenContext.InterfaceOrModule:
                    return m.IsKind(CS.SyntaxKind.PublicKeyword, CS.SyntaxKind.StaticKeyword);
            }
            return false;
        }

        static bool IsVisibility(SyntaxToken token, TokenContext context)
        {
            return token.IsKind(CS.SyntaxKind.PublicKeyword, CS.SyntaxKind.InternalKeyword, CS.SyntaxKind.ProtectedKeyword, CS.SyntaxKind.PrivateKeyword)
                || (context == TokenContext.VariableOrConst && token.IsKind(CS.SyntaxKind.ConstKeyword));
        }

        static SyntaxToken CSharpDefaultVisibility(TokenContext context)
        {
            switch (context)
            {
                case TokenContext.Global:
                    return SyntaxFactory.Token(SyntaxKind.FriendKeyword);
                case TokenContext.Local:
                case TokenContext.VariableOrConst:
                case TokenContext.Member:
                    return SyntaxFactory.Token(SyntaxKind.PrivateKeyword);
            }
            throw new ArgumentOutOfRangeException(nameof(context));
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
            var token = ConvertToken(CS.CSharpExtensions.Kind(m), context);
            return token == SyntaxKind.None ? null : new SyntaxToken?(SyntaxFactory.Token(token));
        }

        static SeparatedSyntaxList<VariableDeclaratorSyntax> RemodelVariableDeclaration(CSS.VariableDeclarationSyntax declaration, NodesVisitor nodesVisitor)
        {
            var type = (TypeSyntax)declaration.Type.Accept(nodesVisitor);
            var declaratorsWithoutInitializers = new List<CSS.VariableDeclaratorSyntax>();
            var declarators = new List<VariableDeclaratorSyntax>();

            foreach (var v in declaration.Variables)
            {
                if (v.Initializer == null)
                {
                    declaratorsWithoutInitializers.Add(v);
                    continue;
                }
                else
                {
                    declarators.Add(
                        SyntaxFactory.VariableDeclarator(
                            SyntaxFactory.SingletonSeparatedList(ExtractIdentifier(v)),
                            declaration.Type.IsVar ? null : SyntaxFactory.SimpleAsClause(type),
                            SyntaxFactory.EqualsValue((ExpressionSyntax)v.Initializer.Value.Accept(nodesVisitor))
                        )
                    );
                }
            }

            if (declaratorsWithoutInitializers.Count > 0)
            {
                declarators.Insert(0, SyntaxFactory.VariableDeclarator(SyntaxFactory.SeparatedList(declaratorsWithoutInitializers.Select(ExtractIdentifier)), SyntaxFactory.SimpleAsClause(type), null));
            }

            return SyntaxFactory.SeparatedList(declarators);
        }

        static ModifiedIdentifierSyntax ExtractIdentifier(CSS.VariableDeclaratorSyntax v)
        {
            return SyntaxFactory.ModifiedIdentifier(ConvertIdentifier(v.Identifier));
        }

		static SyntaxToken ConvertIdentifier(SyntaxToken id)
		{
			var keywordKind = SyntaxFacts.GetKeywordKind(id.ValueText);
			if (keywordKind != SyntaxKind.None && !SyntaxFacts.IsPredefinedType(keywordKind))
				return SyntaxFactory.Identifier("[" + id.ValueText + "]");
			return SyntaxFactory.Identifier(id.ValueText);
		}

		static ExpressionSyntax Literal(object o) => ComputeConstantValueCodeRefactoringProvider.GetLiteralExpression(o);

		static SyntaxKind ConvertToken(CS.SyntaxKind t, TokenContext context = TokenContext.Global)
        {
            switch (t)
            {
                case CS.SyntaxKind.None:
                    return SyntaxKind.None;
                // built-in types
                case CS.SyntaxKind.BoolKeyword:
                    return SyntaxKind.BooleanKeyword;
                case CS.SyntaxKind.ByteKeyword:
                    return SyntaxKind.ByteKeyword;
                case CS.SyntaxKind.SByteKeyword:
                    return SyntaxKind.SByteKeyword;
                case CS.SyntaxKind.ShortKeyword:
                    return SyntaxKind.ShortKeyword;
                case CS.SyntaxKind.UShortKeyword:
                    return SyntaxKind.UShortKeyword;
                case CS.SyntaxKind.IntKeyword:
                    return SyntaxKind.IntegerKeyword;
                case CS.SyntaxKind.UIntKeyword:
                    return SyntaxKind.UIntegerKeyword;
                case CS.SyntaxKind.LongKeyword:
                    return SyntaxKind.LongKeyword;
                case CS.SyntaxKind.ULongKeyword:
                    return SyntaxKind.ULongKeyword;
                case CS.SyntaxKind.DoubleKeyword:
                    return SyntaxKind.DoubleKeyword;
                case CS.SyntaxKind.FloatKeyword:
                    return SyntaxKind.SingleKeyword;
                case CS.SyntaxKind.DecimalKeyword:
                    return SyntaxKind.DecimalKeyword;
                case CS.SyntaxKind.StringKeyword:
                    return SyntaxKind.StringKeyword;
                case CS.SyntaxKind.CharKeyword:
                    return SyntaxKind.CharKeyword;
                case CS.SyntaxKind.VoidKeyword:
                    // not supported
                    return SyntaxKind.None;
                case CS.SyntaxKind.ObjectKeyword:
                    return SyntaxKind.ObjectKeyword;
                // literals
                case CS.SyntaxKind.NullKeyword:
                    return SyntaxKind.NothingKeyword;
                case CS.SyntaxKind.TrueKeyword:
                    return SyntaxKind.TrueKeyword;
                case CS.SyntaxKind.FalseKeyword:
                    return SyntaxKind.FalseKeyword;
                case CS.SyntaxKind.ThisKeyword:
                    return SyntaxKind.MeKeyword;
                case CS.SyntaxKind.BaseKeyword:
                    return SyntaxKind.MyBaseKeyword;
                // modifiers
                case CS.SyntaxKind.PublicKeyword:
                    return SyntaxKind.PublicKeyword;
                case CS.SyntaxKind.PrivateKeyword:
                    return SyntaxKind.PrivateKeyword;
                case CS.SyntaxKind.InternalKeyword:
                    return SyntaxKind.FriendKeyword;
                case CS.SyntaxKind.ProtectedKeyword:
                    return SyntaxKind.ProtectedKeyword;
                case CS.SyntaxKind.StaticKeyword:
                    return SyntaxKind.SharedKeyword;
                case CS.SyntaxKind.ReadOnlyKeyword:
                    return SyntaxKind.ReadOnlyKeyword;
                case CS.SyntaxKind.SealedKeyword:
                    return context == TokenContext.Global ? SyntaxKind.NotInheritableKeyword : SyntaxKind.NotOverridableKeyword;
                case CS.SyntaxKind.ConstKeyword:
                    return SyntaxKind.ConstKeyword;
                case CS.SyntaxKind.OverrideKeyword:
                    return SyntaxKind.OverridesKeyword;
                case CS.SyntaxKind.AbstractKeyword:
                    return context == TokenContext.Global ? SyntaxKind.MustInheritKeyword : SyntaxKind.MustOverrideKeyword;
                case CS.SyntaxKind.VirtualKeyword:
                    return SyntaxKind.OverridableKeyword;
                case CS.SyntaxKind.RefKeyword:
                    return SyntaxKind.ByRefKeyword;
                case CS.SyntaxKind.OutKeyword:
                    return SyntaxKind.ByRefKeyword;
                case CS.SyntaxKind.PartialKeyword:
                    return SyntaxKind.PartialKeyword;
                case CS.SyntaxKind.AsyncKeyword:
                    return SyntaxKind.AsyncKeyword;
                case CS.SyntaxKind.ExternKeyword:
                    // not supported
                    return SyntaxKind.None;
                case CS.SyntaxKind.NewKeyword:
                    return SyntaxKind.ShadowsKeyword;
                case CS.SyntaxKind.ParamsKeyword:
                    return SyntaxKind.ParamArrayKeyword;
                // others
                case CS.SyntaxKind.AscendingKeyword:
                    return SyntaxKind.AscendingKeyword;
                case CS.SyntaxKind.DescendingKeyword:
                    return SyntaxKind.DescendingKeyword;
                case CS.SyntaxKind.AwaitKeyword:
                    return SyntaxKind.AwaitKeyword;
                // expressions
                case CS.SyntaxKind.AddExpression:
                    return SyntaxKind.AddExpression;
                case CS.SyntaxKind.SubtractExpression:
                    return SyntaxKind.SubtractExpression;
                case CS.SyntaxKind.MultiplyExpression:
                    return SyntaxKind.MultiplyExpression;
                case CS.SyntaxKind.DivideExpression:
                    return SyntaxKind.DivideExpression;
                case CS.SyntaxKind.ModuloExpression:
                    return SyntaxKind.ModuloExpression;
                case CS.SyntaxKind.LeftShiftExpression:
                    return SyntaxKind.LeftShiftExpression;
                case CS.SyntaxKind.RightShiftExpression:
                    return SyntaxKind.RightShiftExpression;
                case CS.SyntaxKind.LogicalOrExpression:
                    return SyntaxKind.OrElseExpression;
                case CS.SyntaxKind.LogicalAndExpression:
                    return SyntaxKind.AndAlsoExpression;
                case CS.SyntaxKind.BitwiseOrExpression:
                    return SyntaxKind.OrExpression;
                case CS.SyntaxKind.BitwiseAndExpression:
                    return SyntaxKind.AndExpression;
                case CS.SyntaxKind.ExclusiveOrExpression:
                    return SyntaxKind.ExclusiveOrExpression;
                case CS.SyntaxKind.EqualsExpression:
                    return SyntaxKind.EqualsExpression;
                case CS.SyntaxKind.NotEqualsExpression:
                    return SyntaxKind.NotEqualsExpression;
                case CS.SyntaxKind.LessThanExpression:
                    return SyntaxKind.LessThanExpression;
                case CS.SyntaxKind.LessThanOrEqualExpression:
                    return SyntaxKind.LessThanOrEqualExpression;
                case CS.SyntaxKind.GreaterThanExpression:
                    return SyntaxKind.GreaterThanExpression;
                case CS.SyntaxKind.GreaterThanOrEqualExpression:
                    return SyntaxKind.GreaterThanOrEqualExpression;
                case CS.SyntaxKind.SimpleAssignmentExpression:
                    return SyntaxKind.SimpleAssignmentStatement;
                case CS.SyntaxKind.AddAssignmentExpression:
                    return SyntaxKind.AddAssignmentStatement;
                case CS.SyntaxKind.SubtractAssignmentExpression:
                    return SyntaxKind.SubtractAssignmentStatement;
                case CS.SyntaxKind.MultiplyAssignmentExpression:
                    return SyntaxKind.MultiplyAssignmentStatement;
                case CS.SyntaxKind.DivideAssignmentExpression:
                    return SyntaxKind.DivideAssignmentStatement;
                case CS.SyntaxKind.ModuloAssignmentExpression:
                    return SyntaxKind.ModuloExpression;
                case CS.SyntaxKind.AndAssignmentExpression:
                    return SyntaxKind.AndExpression;
                case CS.SyntaxKind.ExclusiveOrAssignmentExpression:
                    return SyntaxKind.ExclusiveOrExpression;
                case CS.SyntaxKind.OrAssignmentExpression:
                    return SyntaxKind.OrExpression;
                case CS.SyntaxKind.LeftShiftAssignmentExpression:
                    break;
                case CS.SyntaxKind.RightShiftAssignmentExpression:
                    break;
                case CS.SyntaxKind.UnaryPlusExpression:
                    return SyntaxKind.UnaryPlusExpression;
                case CS.SyntaxKind.UnaryMinusExpression:
                    return SyntaxKind.UnaryMinusExpression;
                case CS.SyntaxKind.BitwiseNotExpression:
                    return SyntaxKind.NotExpression;
                case CS.SyntaxKind.LogicalNotExpression:
                    return SyntaxKind.NotExpression;
                case CS.SyntaxKind.PreIncrementExpression:
                    return SyntaxKind.AddAssignmentStatement;
                case CS.SyntaxKind.PreDecrementExpression:
                    return SyntaxKind.SubtractAssignmentStatement;
                case CS.SyntaxKind.PostIncrementExpression:
                    return SyntaxKind.AddAssignmentStatement;
                case CS.SyntaxKind.PostDecrementExpression:
                    return SyntaxKind.SubtractAssignmentStatement;
                case CS.SyntaxKind.PlusPlusToken:
                    return SyntaxKind.PlusToken;
                case CS.SyntaxKind.MinusMinusToken:
                    return SyntaxKind.MinusToken;
            }

            throw new NotSupportedException(t + " is not supported!");
        }
    }
}
