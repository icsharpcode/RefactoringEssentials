using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using VBSyntax = Microsoft.CodeAnalysis.VisualBasic.Syntax;
using VBasic = Microsoft.CodeAnalysis.VisualBasic;

namespace RefactoringEssentials.CSharp.Converter
{
	public partial class VisualBasicConverter
	{
		class NodesVisitor : VBasic.VisualBasicSyntaxVisitor<CSharpSyntaxNode>
		{
			SemanticModel semanticModel;
			Document targetDocument;
			CSharpCompilationOptions options;
			readonly Dictionary<MemberDeclarationSyntax, MemberDeclarationSyntax[]> additionalDeclarations = new Dictionary<MemberDeclarationSyntax, MemberDeclarationSyntax[]>();

			public NodesVisitor(SemanticModel semanticModel, Document targetDocument)
			{
				this.semanticModel = semanticModel;
				this.targetDocument = targetDocument;
				this.options = (CSharpCompilationOptions)targetDocument?.Project.CompilationOptions;
			}

			public override CSharpSyntaxNode DefaultVisit(SyntaxNode node)
			{
				throw new NotImplementedException(node.GetType() + " not implemented!");
			}

			#region Attributes

			IEnumerable<AttributeListSyntax> ConvertAttribute(VBSyntax.AttributeListSyntax attributeList)
			{
				return attributeList.Attributes.Select(a => (AttributeListSyntax)a.Accept(this));
			}

			public override CSharpSyntaxNode VisitAttribute(VBSyntax.AttributeSyntax node)
			{
				return SyntaxFactory.AttributeList(
					node.Target == null ? null : SyntaxFactory.AttributeTargetSpecifier(ConvertToken(node.Target.AttributeModifier)),
					SyntaxFactory.SingletonSeparatedList(SyntaxFactory.Attribute((NameSyntax)node.Name.Accept(this), (AttributeArgumentListSyntax)node.ArgumentList?.Accept(this)))
				);
			}

			#endregion

			public override CSharpSyntaxNode VisitCompilationUnit(VBSyntax.CompilationUnitSyntax node)
			{
				var attributes = SyntaxFactory.List(node.Attributes.SelectMany(a => a.AttributeLists).SelectMany(ConvertAttribute));
				var members = SyntaxFactory.List(node.Members.Select(m => (MemberDeclarationSyntax)m.Accept(this)));

				var options = (VBasic.VisualBasicCompilationOptions)semanticModel.Compilation.Options;

				return SyntaxFactory.CompilationUnit(
					SyntaxFactory.List<ExternAliasDirectiveSyntax>(),
					SyntaxFactory.List(options.GlobalImports.Select(gi => gi.Clause).Concat(node.Imports.SelectMany(imp => imp.ImportsClauses)).Select(c => (UsingDirectiveSyntax)c.Accept(this))),
					attributes,
					members
				);
			}

			public override CSharpSyntaxNode VisitSimpleImportsClause(VBSyntax.SimpleImportsClauseSyntax node)
			{
				if (node.Alias != null)
				{
					return SyntaxFactory.UsingDirective(SyntaxFactory.NameEquals(SyntaxFactory.IdentifierName(ConvertIdentifier(node.Alias.Identifier, semanticModel))), (NameSyntax)node.Name.Accept(this));
				}
				return SyntaxFactory.UsingDirective((NameSyntax)node.Name.Accept(this));
			}

			public override CSharpSyntaxNode VisitNamespaceBlock(VBSyntax.NamespaceBlockSyntax node)
			{
				var members = node.Members.Select(m => (MemberDeclarationSyntax)m.Accept(this));

				return SyntaxFactory.NamespaceDeclaration(
					(NameSyntax)node.NamespaceStatement.Name.Accept(this),
					SyntaxFactory.List<ExternAliasDirectiveSyntax>(),
					SyntaxFactory.List<UsingDirectiveSyntax>(),
					SyntaxFactory.List(members)
				);
			}

			#region Namespace Members

			IEnumerable<MemberDeclarationSyntax> ConvertMembers(SyntaxList<VBSyntax.StatementSyntax> members)
			{
				foreach (var member in members.Select(m => (MemberDeclarationSyntax)m.Accept(this)))
				{
					MemberDeclarationSyntax[] declarations;
					if (member is BaseFieldDeclarationSyntax && additionalDeclarations.TryGetValue(member, out declarations))
					{
						foreach (var d in declarations)
							yield return d;
						additionalDeclarations.Remove(member);
					}
					else
					{
						yield return member;
					}
				}
			}

			public override CSharpSyntaxNode VisitClassBlock(VBSyntax.ClassBlockSyntax node)
			{
				var stmt = node.ClassStatement;
				var attributes = SyntaxFactory.List(stmt.AttributeLists.SelectMany(ConvertAttribute));
				var members = SyntaxFactory.List(ConvertMembers(node.Members));

				TypeParameterListSyntax parameters;
				SyntaxList<TypeParameterConstraintClauseSyntax> constraints;
				SplitTypeParameters(stmt.TypeParameterList, out parameters, out constraints);

				return SyntaxFactory.ClassDeclaration(
					attributes,
					ConvertModifiers(stmt.Modifiers),
					ConvertIdentifier(stmt.Identifier, semanticModel),
					parameters,
					ConvertInheritsAndImplements(node.Inherits, node.Implements),
					constraints,
					members
				);
			}

			private BaseListSyntax ConvertInheritsAndImplements(SyntaxList<VBSyntax.InheritsStatementSyntax> inherits, SyntaxList<VBSyntax.ImplementsStatementSyntax> implements)
			{
				if (inherits.Count + implements.Count == 0)
					return null;
				var baseTypes = new List<BaseTypeSyntax>();
				foreach (var t in inherits.SelectMany(c => c.Types).Concat(implements.SelectMany(c => c.Types)))
					baseTypes.Add(SyntaxFactory.SimpleBaseType((TypeSyntax)t.Accept(this)));
				return SyntaxFactory.BaseList(SyntaxFactory.SeparatedList(baseTypes));
			}

			public override CSharpSyntaxNode VisitModuleBlock(VBSyntax.ModuleBlockSyntax node)
			{
				var stmt = node.ModuleStatement;
				var attributes = SyntaxFactory.List(stmt.AttributeLists.SelectMany(ConvertAttribute));
				var members = SyntaxFactory.List(ConvertMembers(node.Members));

				TypeParameterListSyntax parameters;
				SyntaxList<TypeParameterConstraintClauseSyntax> constraints;
				SplitTypeParameters(stmt.TypeParameterList, out parameters, out constraints);

				return SyntaxFactory.ClassDeclaration(
					attributes,
					ConvertModifiers(stmt.Modifiers, TokenContext.InterfaceOrModule).Add(SyntaxFactory.Token(SyntaxKind.StaticKeyword)),
					ConvertIdentifier(stmt.Identifier, semanticModel),
					parameters,
					ConvertInheritsAndImplements(node.Inherits, node.Implements),
					constraints,
					members
				);
			}

			public override CSharpSyntaxNode VisitStructureBlock(VBSyntax.StructureBlockSyntax node)
			{
				var stmt = node.StructureStatement;
				var attributes = SyntaxFactory.List(stmt.AttributeLists.SelectMany(ConvertAttribute));
				var members = SyntaxFactory.List(ConvertMembers(node.Members));

				TypeParameterListSyntax parameters;
				SyntaxList<TypeParameterConstraintClauseSyntax> constraints;
				SplitTypeParameters(stmt.TypeParameterList, out parameters, out constraints);

				return SyntaxFactory.StructDeclaration(
					attributes,
					ConvertModifiers(stmt.Modifiers, TokenContext.Global),
					ConvertIdentifier(stmt.Identifier, semanticModel),
					parameters,
					ConvertInheritsAndImplements(node.Inherits, node.Implements),
					constraints,
					members
				);
			}

			public override CSharpSyntaxNode VisitInterfaceBlock(VBSyntax.InterfaceBlockSyntax node)
			{
				var stmt = node.InterfaceStatement;
				var attributes = SyntaxFactory.List(stmt.AttributeLists.SelectMany(ConvertAttribute));
				var members = SyntaxFactory.List(ConvertMembers(node.Members));

				TypeParameterListSyntax parameters;
				SyntaxList<TypeParameterConstraintClauseSyntax> constraints;
				SplitTypeParameters(stmt.TypeParameterList, out parameters, out constraints);

				return SyntaxFactory.InterfaceDeclaration(
					attributes,
					ConvertModifiers(stmt.Modifiers, TokenContext.InterfaceOrModule),
					ConvertIdentifier(stmt.Identifier, semanticModel),
					parameters,
					ConvertInheritsAndImplements(node.Inherits, node.Implements),
					constraints,
					members
				);
			}

			public override CSharpSyntaxNode VisitEnumBlock(VBSyntax.EnumBlockSyntax node)
			{
				var stmt = node.EnumStatement;
				// we can cast to SimpleAsClause because other types make no sense as enum-type.
				var asClause = (VBSyntax.SimpleAsClauseSyntax)stmt.UnderlyingType;
				var attributes = stmt.AttributeLists.SelectMany(ConvertAttribute);
				BaseListSyntax baseList = null;
				if (asClause != null)
				{
					baseList = SyntaxFactory.BaseList(SyntaxFactory.SingletonSeparatedList<BaseTypeSyntax>(SyntaxFactory.SimpleBaseType((TypeSyntax)asClause.Type.Accept(this))));
					if (asClause.AttributeLists.Count > 0)
					{
						attributes = attributes.Concat(
							SyntaxFactory.AttributeList(
								SyntaxFactory.AttributeTargetSpecifier(SyntaxFactory.Token(SyntaxKind.ReturnKeyword)),
								SyntaxFactory.SeparatedList(asClause.AttributeLists.SelectMany(l => ConvertAttribute(l).SelectMany(a => a.Attributes)))
							)
						);
					}
				}
				var members = SyntaxFactory.SeparatedList(node.Members.Select(m => (EnumMemberDeclarationSyntax)m.Accept(this)));
				return SyntaxFactory.EnumDeclaration(
					SyntaxFactory.List(attributes),
					ConvertModifiers(stmt.Modifiers, TokenContext.Global),
					ConvertIdentifier(stmt.Identifier, semanticModel),
					baseList,
					members
				);
			}

			public override CSharpSyntaxNode VisitEnumMemberDeclaration(VBSyntax.EnumMemberDeclarationSyntax node)
			{
				var attributes = SyntaxFactory.List(node.AttributeLists.SelectMany(ConvertAttribute));
				return SyntaxFactory.EnumMemberDeclaration(
					attributes,
					ConvertIdentifier(node.Identifier, semanticModel),
					(EqualsValueClauseSyntax)node.Initializer?.Accept(this)
				);
			}

			public override CSharpSyntaxNode VisitDelegateStatement(VBSyntax.DelegateStatementSyntax node)
			{
				var attributes = node.AttributeLists.SelectMany(ConvertAttribute);

				TypeParameterListSyntax typeParameters;
				SyntaxList<TypeParameterConstraintClauseSyntax> constraints;
				SplitTypeParameters(node.TypeParameterList, out typeParameters, out constraints);

				TypeSyntax returnType;
				var asClause = node.AsClause;
				if (asClause == null)
				{
					returnType = SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.VoidKeyword));
				}
				else
				{
					returnType = (TypeSyntax)asClause.Type.Accept(this);
					if (asClause.AttributeLists.Count > 0)
					{
						attributes = attributes.Concat(
							SyntaxFactory.AttributeList(
								SyntaxFactory.AttributeTargetSpecifier(SyntaxFactory.Token(SyntaxKind.ReturnKeyword)),
								SyntaxFactory.SeparatedList(asClause.AttributeLists.SelectMany(l => ConvertAttribute(l).SelectMany(a => a.Attributes)))
							)
						);
					}
				}

				return SyntaxFactory.DelegateDeclaration(
					SyntaxFactory.List(attributes),
					ConvertModifiers(node.Modifiers, TokenContext.Global),
					returnType,
					ConvertIdentifier(node.Identifier, semanticModel),
					typeParameters,
					(ParameterListSyntax)node.ParameterList?.Accept(this),
					constraints
				);
			}

			#endregion

			#region Type Members

			public override CSharpSyntaxNode VisitFieldDeclaration(VBSyntax.FieldDeclarationSyntax node)
			{
				var attributes = node.AttributeLists.SelectMany(ConvertAttribute);
				var modifiers = ConvertModifiers(node.Modifiers, TokenContext.VariableOrConst);
				var key = SyntaxFactory.FieldDeclaration(SyntaxFactory.VariableDeclaration(SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.VoidKeyword))));
				var declarations = new List<BaseFieldDeclarationSyntax>(node.Declarators.Count);

				foreach (var declarator in node.Declarators)
				{
					foreach (var decl in SplitVariableDeclarations(declarator, this, semanticModel).Values)
					{
						declarations.Add(SyntaxFactory.FieldDeclaration(
							SyntaxFactory.List(attributes),
							modifiers,
							decl
						));
					}
				}

				additionalDeclarations.Add(key, declarations.ToArray());
				return key;
			}

			public override CSharpSyntaxNode VisitPropertyStatement(VBSyntax.PropertyStatementSyntax node)
			{
				bool hasBody = node.Parent is VBSyntax.PropertyBlockSyntax;
				var attributes = node.AttributeLists.SelectMany(ConvertAttribute);
				var modifiers = ConvertModifiers(node.Modifiers, TokenContext.Member);

				var isIndexer = node.Modifiers.Any(m => m.IsKind(VBasic.SyntaxKind.DefaultKeyword)) && node.Identifier.ValueText.Equals("Items", StringComparison.OrdinalIgnoreCase);

				var rawType = (TypeSyntax)node.AsClause?.TypeSwitch(
					(VBSyntax.SimpleAsClauseSyntax c) => c.Type,
					(VBSyntax.AsNewClauseSyntax c) => VBasic.SyntaxExtensions.Type(c.NewExpression),
					_ => { throw new NotImplementedException($"{_.GetType().FullName} not implemented!"); }
				)?.Accept(this) ?? SyntaxFactory.ParseTypeName("var");

				AccessorListSyntax accessors = null;

				if (!hasBody)
				{
					accessors = SyntaxFactory.AccessorList(
						SyntaxFactory.List(new[] {
							SyntaxFactory.AccessorDeclaration(SyntaxKind.GetAccessorDeclaration).WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken)),
							SyntaxFactory.AccessorDeclaration(SyntaxKind.SetAccessorDeclaration).WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken))
                        })
					);
				}
				else
				{
					accessors = SyntaxFactory.AccessorList(
						SyntaxFactory.List(
							((VBSyntax.PropertyBlockSyntax)node.Parent).Accessors.Select(a => (AccessorDeclarationSyntax)a.Accept(this))
						)
					);
				}

				if (isIndexer)
					return SyntaxFactory.IndexerDeclaration(
						SyntaxFactory.List(attributes),
						modifiers,
						rawType,
						null,
						SyntaxFactory.BracketedParameterList(SyntaxFactory.SeparatedList(node.ParameterList.Parameters.Select(p => (ParameterSyntax)p.Accept(this)))),
						accessors
					);
				else
					return SyntaxFactory.PropertyDeclaration(
						SyntaxFactory.List(attributes),
						modifiers,
						rawType,
						null,
						ConvertIdentifier(node.Identifier, semanticModel), accessors,
						null,
						null);
			}

			public override CSharpSyntaxNode VisitPropertyBlock(VBSyntax.PropertyBlockSyntax node)
			{
				return node.PropertyStatement.Accept(this);
			}

			public override CSharpSyntaxNode VisitAccessorBlock(VBSyntax.AccessorBlockSyntax node)
			{
				SyntaxKind blockKind;
				bool isIterator = node.GetModifiers().Any(m => m.IsKind(VBasic.SyntaxKind.IteratorKeyword));
				var body = SyntaxFactory.Block(node.Statements.SelectMany(s => s.Accept(new MethodBodyVisitor(semanticModel, this) { IsIterator = isIterator })));
				var attributes = SyntaxFactory.List(node.AccessorStatement.AttributeLists.Select(a => (AttributeListSyntax)a.Accept(this)));
				var modifiers = ConvertModifiers(node.AccessorStatement.Modifiers, TokenContext.Local);

				switch (node.Kind())
				{
					case VBasic.SyntaxKind.GetAccessorBlock:
						blockKind = SyntaxKind.GetAccessorDeclaration;
						break;
					case VBasic.SyntaxKind.SetAccessorBlock:
						blockKind = SyntaxKind.SetAccessorDeclaration;
						break;
					case VBasic.SyntaxKind.AddHandlerAccessorBlock:
						blockKind = SyntaxKind.AddAccessorDeclaration;
						break;
					case VBasic.SyntaxKind.RemoveHandlerAccessorBlock:
						blockKind = SyntaxKind.RemoveAccessorDeclaration;
						break;
					default:
						throw new NotSupportedException();
				}
				return SyntaxFactory.AccessorDeclaration(blockKind, attributes, modifiers, body);
			}

			public override CSharpSyntaxNode VisitMethodBlock(VBSyntax.MethodBlockSyntax node)
			{
				BaseMethodDeclarationSyntax block = (BaseMethodDeclarationSyntax)node.SubOrFunctionStatement.Accept(this);
				bool isIterator = node.SubOrFunctionStatement.Modifiers.Any(m => m.IsKind(VBasic.SyntaxKind.IteratorKeyword));

				return block.WithBody(SyntaxFactory.Block(node.Statements.SelectMany(s => s.Accept(new MethodBodyVisitor(semanticModel, this) { IsIterator = isIterator }))));
			}

			public override CSharpSyntaxNode VisitMethodStatement(VBSyntax.MethodStatementSyntax node)
			{
				var attributes = node.AttributeLists.SelectMany(ConvertAttribute);
				bool hasBody = node.Parent is VBSyntax.MethodBlockBaseSyntax;

				if ("Finalize".Equals(node.Identifier.ValueText, StringComparison.OrdinalIgnoreCase)
					&& node.Modifiers.Any(m => VBasic.VisualBasicExtensions.Kind(m) == VBasic.SyntaxKind.OverridesKeyword))
				{
					var decl = SyntaxFactory.DestructorDeclaration(
						ConvertIdentifier(node.GetAncestor<VBSyntax.TypeBlockSyntax>().BlockStatement.Identifier, semanticModel)
					).WithAttributeLists(SyntaxFactory.List(attributes));
					if (hasBody) return decl;
					return decl.WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken));
				}
				else
				{
					var parentType = semanticModel.GetDeclaredSymbol(node).ContainingType;
					var modifiers = ConvertModifiers(node.Modifiers, parentType.TypeKind == TypeKind.Module ? TokenContext.MemberInModule : TokenContext.Member);

					TypeParameterListSyntax typeParameters;
					SyntaxList<TypeParameterConstraintClauseSyntax> constraints;
					SplitTypeParameters(node.TypeParameterList, out typeParameters, out constraints);

					var decl = SyntaxFactory.MethodDeclaration(
						SyntaxFactory.List(attributes),
						modifiers,
						(TypeSyntax)node.AsClause?.Type.Accept(this) ?? SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.VoidKeyword)),
						null,
						ConvertIdentifier(node.Identifier, semanticModel),
						typeParameters,
						(ParameterListSyntax)node.ParameterList.Accept(this),
						constraints,
						null,
						null
					);
					if (hasBody) return decl;
					return decl.WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken));
				}
			}

			public override CSharpSyntaxNode VisitEventBlock(VBSyntax.EventBlockSyntax node)
			{
				var block = node.EventStatement;
				var attributes = block.AttributeLists.SelectMany(ConvertAttribute);
				var modifiers = ConvertModifiers(block.Modifiers, TokenContext.Member);

				var rawType = (TypeSyntax)block.AsClause?.Type.Accept(this) ?? SyntaxFactory.ParseTypeName("var");

				return SyntaxFactory.EventDeclaration(
					SyntaxFactory.List(attributes),
					modifiers,
					rawType,
					null,
					ConvertIdentifier(block.Identifier, semanticModel),
					SyntaxFactory.AccessorList(SyntaxFactory.List(node.Accessors.Select(a => (AccessorDeclarationSyntax)a.Accept(this))))
				);
			}

			public override CSharpSyntaxNode VisitEventStatement(VBSyntax.EventStatementSyntax node)
			{
				var attributes = node.AttributeLists.SelectMany(ConvertAttribute);
				var modifiers = ConvertModifiers(node.Modifiers, TokenContext.Member);
				var id = ConvertIdentifier(node.Identifier, semanticModel);

				if (node.AsClause == null)
				{
					var key = SyntaxFactory.EventFieldDeclaration(SyntaxFactory.VariableDeclaration(SyntaxFactory.ParseTypeName("__event" + id.ValueText)));
					var delegateName = SyntaxFactory.Identifier(id.ValueText + "EventHandler");

					var delegateDecl = SyntaxFactory.DelegateDeclaration(
						SyntaxFactory.List<AttributeListSyntax>(),
						modifiers,
						SyntaxFactory.ParseTypeName("void"),
						delegateName,
						null,
						(ParameterListSyntax)node.ParameterList.Accept(this),
						SyntaxFactory.List<TypeParameterConstraintClauseSyntax>()
					);

					var eventDecl = SyntaxFactory.EventFieldDeclaration(
						SyntaxFactory.List(attributes),
						modifiers,
						SyntaxFactory.VariableDeclaration(SyntaxFactory.IdentifierName(delegateName),
						SyntaxFactory.SingletonSeparatedList(SyntaxFactory.VariableDeclarator(id)))
					);

					additionalDeclarations.Add(key, new MemberDeclarationSyntax[] { eventDecl, delegateDecl });
					return key;
				}
				else
				{
					return SyntaxFactory.EventFieldDeclaration(
						SyntaxFactory.List(attributes),
						modifiers,
						SyntaxFactory.VariableDeclaration((TypeSyntax)node.AsClause.Type.Accept(this),
						SyntaxFactory.SingletonSeparatedList(SyntaxFactory.VariableDeclarator(id)))
					);
				}
				throw new NotSupportedException();
			}

			public override CSharpSyntaxNode VisitOperatorBlock(VBSyntax.OperatorBlockSyntax node)
			{
				var block = node.OperatorStatement;
				var attributes = block.AttributeLists.SelectMany(ConvertAttribute);
				var modifiers = ConvertModifiers(block.Modifiers, TokenContext.Member);
				return SyntaxFactory.OperatorDeclaration(
					SyntaxFactory.List(attributes),
					modifiers,
					(TypeSyntax)block.AsClause?.Type.Accept(this) ?? SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.VoidKeyword)),
					ConvertToken(block.OperatorToken),
					(ParameterListSyntax)block.ParameterList.Accept(this),
					SyntaxFactory.Block(node.Statements.SelectMany(s => s.Accept(new MethodBodyVisitor(semanticModel, this)))),
					null
				);
			}

			public override CSharpSyntaxNode VisitConstructorBlock(VBSyntax.ConstructorBlockSyntax node)
			{
				var block = node.BlockStatement;
				var attributes = block.AttributeLists.SelectMany(ConvertAttribute);
				var modifiers = ConvertModifiers(block.Modifiers, TokenContext.Member);


				var ctor = (node.Statements.FirstOrDefault() as VBSyntax.ExpressionStatementSyntax)?.Expression as VBSyntax.InvocationExpressionSyntax;
				var ctorExpression = ctor?.Expression as VBSyntax.MemberAccessExpressionSyntax;
				var ctorArgs = (ArgumentListSyntax)ctor?.ArgumentList.Accept(this);

				IEnumerable<VBSyntax.StatementSyntax> statements;
				ConstructorInitializerSyntax ctorCall;
				if (ctorExpression == null || !ctorExpression.Name.Identifier.IsKindOrHasMatchingText(VBasic.SyntaxKind.NewKeyword))
				{
					statements = node.Statements;
					ctorCall = null;
				}
				else if (ctorExpression.Expression is VBSyntax.MyBaseExpressionSyntax)
				{
					statements = node.Statements.Skip(1);
					ctorCall = SyntaxFactory.ConstructorInitializer(SyntaxKind.BaseConstructorInitializer, ctorArgs ?? SyntaxFactory.ArgumentList());
				}
				else if (ctorExpression.Expression is VBSyntax.MeExpressionSyntax || ctorExpression.Expression is VBSyntax.MyClassExpressionSyntax)
				{
					statements = node.Statements.Skip(1);
					ctorCall = SyntaxFactory.ConstructorInitializer(SyntaxKind.ThisConstructorInitializer, ctorArgs ?? SyntaxFactory.ArgumentList());
				}
				else
				{
					statements = node.Statements;
					ctorCall = null;
				}

				return SyntaxFactory.ConstructorDeclaration(
					SyntaxFactory.List(attributes),
					modifiers,
					ConvertIdentifier(node.GetAncestor<VBSyntax.TypeBlockSyntax>().BlockStatement.Identifier, semanticModel),
					(ParameterListSyntax)block.ParameterList.Accept(this),
					ctorCall,
					SyntaxFactory.Block(statements.SelectMany(s => s.Accept(new MethodBodyVisitor(semanticModel, this))))
				);
			}

			public override CSharpSyntaxNode VisitTypeParameterList(VBSyntax.TypeParameterListSyntax node)
			{
				return SyntaxFactory.TypeParameterList(
					SyntaxFactory.SeparatedList(node.Parameters.Select(p => (TypeParameterSyntax)p.Accept(this)))
				);
			}

			public override CSharpSyntaxNode VisitParameterList(VBSyntax.ParameterListSyntax node)
			{
				if (node.Parent is VBSyntax.PropertyStatementSyntax)
				{
					return SyntaxFactory.BracketedParameterList(SyntaxFactory.SeparatedList(node.Parameters.Select(p => (ParameterSyntax)p.Accept(this))));
				}
				return SyntaxFactory.ParameterList(SyntaxFactory.SeparatedList(node.Parameters.Select(p => (ParameterSyntax)p.Accept(this))));
			}

			public override CSharpSyntaxNode VisitParameter(VBSyntax.ParameterSyntax node)
			{
				var id = ConvertIdentifier(node.Identifier.Identifier, semanticModel);
				var returnType = (TypeSyntax)node.AsClause?.Type.Accept(this);
				if (returnType != null && !node.Identifier.Nullable.IsKind(SyntaxKind.None))
				{
					var arrayType = returnType as ArrayTypeSyntax;
					if (arrayType == null)
					{
						returnType = SyntaxFactory.NullableType(returnType);
					}
					else
					{
						returnType = arrayType.WithElementType(SyntaxFactory.NullableType(arrayType.ElementType));
					}
				}
				EqualsValueClauseSyntax @default = null;
				if (node.Default != null)
				{
					@default = SyntaxFactory.EqualsValueClause((ExpressionSyntax)node.Default?.Value.Accept(this));
				}
				var attributes = node.AttributeLists.SelectMany(ConvertAttribute).ToList();
				int outAttributeIndex = attributes.FindIndex(a => a.Attributes.Single().Name.ToString() == "Out");
				var modifiers = ConvertModifiers(node.Modifiers, TokenContext.Local);
				if (outAttributeIndex > -1)
				{
					attributes.RemoveAt(outAttributeIndex);
					modifiers = modifiers.Replace(SyntaxFactory.Token(SyntaxKind.RefKeyword), SyntaxFactory.Token(SyntaxKind.OutKeyword));
				}
				return SyntaxFactory.Parameter(
					SyntaxFactory.List(attributes),
					modifiers,
					returnType,
					id,
					@default
				);
			}

			#endregion

			#region Expressions

			public override CSharpSyntaxNode VisitAwaitExpression(VBSyntax.AwaitExpressionSyntax node)
			{
				return SyntaxFactory.AwaitExpression((ExpressionSyntax)node.Expression.Accept(this));
			}

			public override CSharpSyntaxNode VisitCatchBlock(VBSyntax.CatchBlockSyntax node)
			{
				var stmt = node.CatchStatement;
				CatchDeclarationSyntax catcher;
				if (stmt.IdentifierName == null)
					catcher = null;
				else
				{
					var typeInfo = semanticModel.GetTypeInfo(stmt.IdentifierName).Type;
					catcher = SyntaxFactory.CatchDeclaration(
						SyntaxFactory.ParseTypeName(typeInfo.ToMinimalDisplayString(semanticModel, node.SpanStart)),
						ConvertIdentifier(stmt.IdentifierName.Identifier, semanticModel)
					);
				}

				var filter = (CatchFilterClauseSyntax)stmt.WhenClause?.Accept(this);

				return SyntaxFactory.CatchClause(
					catcher,
					filter,
					SyntaxFactory.Block(node.Statements.SelectMany(s => s.Accept(new MethodBodyVisitor(semanticModel, this))))
				);
			}

			public override CSharpSyntaxNode VisitCatchFilterClause(VBSyntax.CatchFilterClauseSyntax node)
			{
				return SyntaxFactory.CatchFilterClause((ExpressionSyntax)node.Filter.Accept(this));
			}

			public override CSharpSyntaxNode VisitFinallyBlock(VBSyntax.FinallyBlockSyntax node)
			{
				return SyntaxFactory.FinallyClause(SyntaxFactory.Block(node.Statements.SelectMany(s => s.Accept(new MethodBodyVisitor(semanticModel, this)))));
			}

			public override CSharpSyntaxNode VisitCTypeExpression(VBSyntax.CTypeExpressionSyntax node)
			{
				return SyntaxFactory.CastExpression(
					(TypeSyntax)node.Type.Accept(this),
					(ExpressionSyntax)node.Expression.Accept(this)
				);
			}

			public override CSharpSyntaxNode VisitPredefinedCastExpression(VBSyntax.PredefinedCastExpressionSyntax node)
			{
				if (node.Keyword.IsKind(VBasic.SyntaxKind.CDateKeyword))
				{
					return SyntaxFactory.CastExpression(
						SyntaxFactory.ParseTypeName("DateTime"),
						(ExpressionSyntax)node.Expression.Accept(this)
					);
				}
				return SyntaxFactory.CastExpression(
					SyntaxFactory.PredefinedType(ConvertToken(node.Keyword)),
					(ExpressionSyntax)node.Expression.Accept(this)
				);
			}

			public override CSharpSyntaxNode VisitTryCastExpression(VBSyntax.TryCastExpressionSyntax node)
			{
				return SyntaxFactory.BinaryExpression(
					SyntaxKind.AsExpression,
					(ExpressionSyntax)node.Expression.Accept(this),
					(TypeSyntax)node.Type.Accept(this)
				);
			}

			public override CSharpSyntaxNode VisitLiteralExpression(VBSyntax.LiteralExpressionSyntax node)
			{
				if (node.Token.Value == null)
				{
					var type = semanticModel.GetTypeInfo(node).ConvertedType;
					return !type.IsReferenceType ? SyntaxFactory.DefaultExpression(SyntaxFactory.ParseTypeName(type.ToMinimalDisplayString(semanticModel, node.SpanStart))) : Literal(null);
				}
				return Literal(node.Token.Value);
			}

			public override CSharpSyntaxNode VisitInterpolatedStringExpression(VBSyntax.InterpolatedStringExpressionSyntax node)
			{
				return SyntaxFactory.InterpolatedStringExpression(SyntaxFactory.Token(SyntaxKind.InterpolatedStringStartToken), SyntaxFactory.List(node.Contents.Select(c => (InterpolatedStringContentSyntax)c.Accept(this))));
			}

			public override CSharpSyntaxNode VisitInterpolatedStringText(VBSyntax.InterpolatedStringTextSyntax node)
			{
				return SyntaxFactory.InterpolatedStringText(SyntaxFactory.Token(default(SyntaxTriviaList), SyntaxKind.InterpolatedStringTextToken, node.TextToken.Text, node.TextToken.ValueText, default(SyntaxTriviaList)));
			}

			public override CSharpSyntaxNode VisitInterpolation(VBSyntax.InterpolationSyntax node)
			{
				return SyntaxFactory.Interpolation((ExpressionSyntax)node.Expression.Accept(this));
			}

			public override CSharpSyntaxNode VisitInterpolationFormatClause(VBSyntax.InterpolationFormatClauseSyntax node)
			{
				return base.VisitInterpolationFormatClause(node);
			}

			public override CSharpSyntaxNode VisitMeExpression(VBSyntax.MeExpressionSyntax node)
			{
				return SyntaxFactory.ThisExpression();
			}

			public override CSharpSyntaxNode VisitMyBaseExpression(VBSyntax.MyBaseExpressionSyntax node)
			{
				return SyntaxFactory.BaseExpression();
			}

			public override CSharpSyntaxNode VisitParenthesizedExpression(VBSyntax.ParenthesizedExpressionSyntax node)
			{
				return SyntaxFactory.ParenthesizedExpression((ExpressionSyntax)node.Expression.Accept(this));
			}

			public override CSharpSyntaxNode VisitMemberAccessExpression(VBSyntax.MemberAccessExpressionSyntax node)
			{
				var left = (ExpressionSyntax)node.Expression?.Accept(this);
				if (left == null)
					return SyntaxFactory.MemberBindingExpression((SimpleNameSyntax)node.Name.Accept(this));
				else
					return SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, left, (SimpleNameSyntax)node.Name.Accept(this));
			}

			public override CSharpSyntaxNode VisitConditionalAccessExpression(VBSyntax.ConditionalAccessExpressionSyntax node)
			{
				return SyntaxFactory.ConditionalAccessExpression((ExpressionSyntax)node.Expression.Accept(this), (ExpressionSyntax)node.WhenNotNull.Accept(this));
			}

			public override CSharpSyntaxNode VisitArgumentList(VBSyntax.ArgumentListSyntax node)
			{
				if (node.Parent.IsKind(VBasic.SyntaxKind.Attribute))
				{
					return SyntaxFactory.AttributeArgumentList(SyntaxFactory.SeparatedList(node.Arguments.Select(ToAttributeArgument)));
				}
				return SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList(node.Arguments.Select(a => (ArgumentSyntax)a.Accept(this))));
			}

			public override CSharpSyntaxNode VisitSimpleArgument(VBSyntax.SimpleArgumentSyntax node)
			{
				int argID = ((VBSyntax.ArgumentListSyntax)node.Parent).Arguments.IndexOf(node);
				var invocation = node.Parent.Parent;
				if (invocation is VBSyntax.ArrayCreationExpressionSyntax)
					return node.Expression.Accept(this);
				var symbol = invocation.TypeSwitch(
					(VBSyntax.InvocationExpressionSyntax e) => ExtractMatch(semanticModel.GetSymbolInfo(e)),
					(VBSyntax.ObjectCreationExpressionSyntax e) => ExtractMatch(semanticModel.GetSymbolInfo(e)),
                    (VBSyntax.RaiseEventStatementSyntax e) => ExtractMatch(semanticModel.GetSymbolInfo(e.Name)),
					_ => { throw new NotSupportedException(); }
				);
				SyntaxToken token = default(SyntaxToken);
				if (symbol != null)
				{
                    var p = symbol.GetParameters()[argID];
					switch (p.RefKind)
					{
						case RefKind.None:
							token = default(SyntaxToken);
							break;
						case RefKind.Ref:
							token = SyntaxFactory.Token(SyntaxKind.RefKeyword);
							break;
						case RefKind.Out:
							token = SyntaxFactory.Token(SyntaxKind.OutKeyword);
							break;
						default:
							throw new ArgumentOutOfRangeException();
					}
				}
				return SyntaxFactory.Argument(
					node.IsNamed ? SyntaxFactory.NameColon((IdentifierNameSyntax)node.NameColonEquals.Name.Accept(this)) : null,
					token,
					(ExpressionSyntax)node.Expression.Accept(this)
				);
			}

			private ISymbol ExtractMatch(SymbolInfo info)
			{
				if (info.Symbol == null && info.CandidateSymbols.Length == 0)
					return null;
				if (info.Symbol != null)
					return info.Symbol;
				if (info.CandidateSymbols.Length == 1)
					return info.CandidateSymbols[0];
				return null;
			}

			private AttributeArgumentSyntax ToAttributeArgument(VBSyntax.ArgumentSyntax arg)
			{
				if (!(arg is VBSyntax.SimpleArgumentSyntax))
					throw new NotSupportedException();
				var a = (VBSyntax.SimpleArgumentSyntax)arg;
				var attr = SyntaxFactory.AttributeArgument((ExpressionSyntax)a.Expression.Accept(this));
				if (a.IsNamed)
				{
					attr = attr.WithNameEquals(SyntaxFactory.NameEquals((IdentifierNameSyntax)a.NameColonEquals.Name.Accept(this)));
				}
				return attr;
			}

			public override CSharpSyntaxNode VisitNameOfExpression(VBSyntax.NameOfExpressionSyntax node)
			{
				return SyntaxFactory.InvocationExpression(SyntaxFactory.IdentifierName("nameof"), SyntaxFactory.ArgumentList(SyntaxFactory.SingletonSeparatedList(SyntaxFactory.Argument((ExpressionSyntax)node.Argument.Accept(this)))));
			}

			public override CSharpSyntaxNode VisitEqualsValue(VBSyntax.EqualsValueSyntax node)
			{
				return SyntaxFactory.EqualsValueClause((ExpressionSyntax)node.Value.Accept(this));
			}

			public override CSharpSyntaxNode VisitAnonymousObjectCreationExpression(VBSyntax.AnonymousObjectCreationExpressionSyntax node)
			{
				return base.VisitAnonymousObjectCreationExpression(node);
			}

			public override CSharpSyntaxNode VisitObjectCreationExpression(VBSyntax.ObjectCreationExpressionSyntax node)
			{
				return SyntaxFactory.ObjectCreationExpression(
					(TypeSyntax)node.Type.Accept(this),
					(ArgumentListSyntax)node.ArgumentList?.Accept(this),
					(InitializerExpressionSyntax)node.Initializer?.Accept(this)
				);
			}

			public override CSharpSyntaxNode VisitArrayCreationExpression(VBSyntax.ArrayCreationExpressionSyntax node)
			{
				IEnumerable<ExpressionSyntax> arguments;
				if (node.ArrayBounds != null)
					arguments = node.ArrayBounds.Arguments.Select(a => IncreaseArrayUpperBoundExpression(((VBSyntax.SimpleArgumentSyntax)a).Expression));
				else
					arguments = Enumerable.Empty<ExpressionSyntax>();
				var bounds = SyntaxFactory.List(node.RankSpecifiers.Select(r => (ArrayRankSpecifierSyntax)r.Accept(this)));
				return SyntaxFactory.ArrayCreationExpression(
					SyntaxFactory.ArrayType((TypeSyntax)node.Type.Accept(this), bounds),
					(InitializerExpressionSyntax)node.Initializer?.Accept(this)
				);
			}

			public override CSharpSyntaxNode VisitCollectionInitializer(VBSyntax.CollectionInitializerSyntax node)
			{
				if (node.Initializers.Count == 0 && node.Parent is VBSyntax.ArrayCreationExpressionSyntax)
					return null;
				return SyntaxFactory.InitializerExpression(SyntaxKind.CollectionInitializerExpression, SyntaxFactory.SeparatedList(node.Initializers.Select(i => (ExpressionSyntax)i.Accept(this))));
			}

			ExpressionSyntax IncreaseArrayUpperBoundExpression(VBSyntax.ExpressionSyntax expr)
			{
				var constant = semanticModel.GetConstantValue(expr);
				if (constant.HasValue && constant.Value is int)
					return SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal((int)constant.Value + 1));

				return SyntaxFactory.BinaryExpression(
					SyntaxKind.SubtractExpression,
					(ExpressionSyntax)expr.Accept(this), SyntaxFactory.Token(SyntaxKind.PlusToken), SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal(1)));
			}

			public override CSharpSyntaxNode VisitBinaryConditionalExpression(VBSyntax.BinaryConditionalExpressionSyntax node)
			{
				return SyntaxFactory.BinaryExpression(
					SyntaxKind.CoalesceExpression,
					(ExpressionSyntax)node.FirstExpression.Accept(this),
					(ExpressionSyntax)node.SecondExpression.Accept(this)
				);
			}

			public override CSharpSyntaxNode VisitTernaryConditionalExpression(VBSyntax.TernaryConditionalExpressionSyntax node)
			{
				return SyntaxFactory.ConditionalExpression(
					(ExpressionSyntax)node.Condition.Accept(this),
					(ExpressionSyntax)node.WhenTrue.Accept(this),
					(ExpressionSyntax)node.WhenFalse.Accept(this)
				);
			}

			public override CSharpSyntaxNode VisitTypeOfExpression(VBSyntax.TypeOfExpressionSyntax node)
			{
				var expr = SyntaxFactory.BinaryExpression(
					SyntaxKind.IsExpression,
					(ExpressionSyntax)node.Expression.Accept(this),
					(TypeSyntax)node.Type.Accept(this)
				);
				if (node.IsKind(VBasic.SyntaxKind.TypeOfIsNotExpression))
					return SyntaxFactory.PrefixUnaryExpression(SyntaxKind.LogicalNotExpression, expr);
				else
					return expr;
			}

			public override CSharpSyntaxNode VisitUnaryExpression(VBSyntax.UnaryExpressionSyntax node)
			{
				var expr = (ExpressionSyntax)node.Operand.Accept(this);
				if (node.IsKind(VBasic.SyntaxKind.AddressOfExpression))
					return expr;
				var kind = ConvertToken(VBasic.VisualBasicExtensions.Kind(node), TokenContext.Local);
				return SyntaxFactory.PrefixUnaryExpression(
					kind,
					SyntaxFactory.Token(CSharpUtil.GetExpressionOperatorTokenKind(kind)),
					expr
				);
			}

			public override CSharpSyntaxNode VisitBinaryExpression(VBSyntax.BinaryExpressionSyntax node)
			{
				if (node.IsKind(VBasic.SyntaxKind.IsExpression))
				{
					ExpressionSyntax otherArgument = null;
					if (node.Left.IsKind(VBasic.SyntaxKind.NothingLiteralExpression))
					{
						otherArgument = (ExpressionSyntax)node.Right.Accept(this);
					}
					if (node.Right.IsKind(VBasic.SyntaxKind.NothingLiteralExpression))
					{
						otherArgument = (ExpressionSyntax)node.Left.Accept(this);
					}
					if (otherArgument != null)
					{
						return SyntaxFactory.BinaryExpression(SyntaxKind.EqualsExpression, otherArgument, SyntaxFactory.LiteralExpression(SyntaxKind.NullLiteralExpression));
					}
				}
				if (node.IsKind(VBasic.SyntaxKind.IsNotExpression))
				{
					ExpressionSyntax otherArgument = null;
					if (node.Left.IsKind(VBasic.SyntaxKind.NothingLiteralExpression))
					{
						otherArgument = (ExpressionSyntax)node.Right.Accept(this);
					}
					if (node.Right.IsKind(VBasic.SyntaxKind.NothingLiteralExpression))
					{
						otherArgument = (ExpressionSyntax)node.Left.Accept(this);
					}
					if (otherArgument != null)
					{
						return SyntaxFactory.BinaryExpression(SyntaxKind.NotEqualsExpression, otherArgument, SyntaxFactory.LiteralExpression(SyntaxKind.NullLiteralExpression));
					}
				}
				var kind = ConvertToken(VBasic.VisualBasicExtensions.Kind(node), TokenContext.Local);
				return SyntaxFactory.BinaryExpression(
					kind,
					(ExpressionSyntax)node.Left.Accept(this),
					SyntaxFactory.Token(CSharpUtil.GetExpressionOperatorTokenKind(kind)),
					(ExpressionSyntax)node.Right.Accept(this)
				);
			}

			public override CSharpSyntaxNode VisitInvocationExpression(VBSyntax.InvocationExpressionSyntax node)
			{
				var invocationSymbol = semanticModel.GetSymbolInfo(node).Symbol;
				var symbol = semanticModel.GetSymbolInfo(node.Expression).Symbol;
				if (invocationSymbol?.IsIndexer() == true || symbol?.GetReturnType()?.IsArrayType() == true)
				{
					return SyntaxFactory.ElementAccessExpression(
						(ExpressionSyntax)node.Expression.Accept(this),
						SyntaxFactory.BracketedArgumentList(SyntaxFactory.SeparatedList(node.ArgumentList.Arguments.Select(a => (ArgumentSyntax)a.Accept(this)))));
				}
				return SyntaxFactory.InvocationExpression(
					(ExpressionSyntax)node.Expression.Accept(this),
					(ArgumentListSyntax)node.ArgumentList.Accept(this)
				);
			}

			public override CSharpSyntaxNode VisitSingleLineLambdaExpression(VBSyntax.SingleLineLambdaExpressionSyntax node)
			{
				CSharpSyntaxNode body;
				if (node.Body is VBSyntax.ExpressionSyntax)
					body = node.Body.Accept(this);
				else
				{
					var stmt = node.Body.Accept(new MethodBodyVisitor(semanticModel, this));
					if (stmt.Count == 1)
						body = stmt[0];
					else
					{
						body = SyntaxFactory.Block(stmt);
					}
				}
				var param = (ParameterListSyntax)node.SubOrFunctionHeader.ParameterList.Accept(this);
				if (param.Parameters.Count == 1)
					return SyntaxFactory.SimpleLambdaExpression(param.Parameters[0], body);
				return SyntaxFactory.ParenthesizedLambdaExpression(param, body);
			}

			public override CSharpSyntaxNode VisitMultiLineLambdaExpression(VBSyntax.MultiLineLambdaExpressionSyntax node)
			{
				var body = SyntaxFactory.Block(node.Statements.SelectMany(s => s.Accept(new MethodBodyVisitor(semanticModel, this))));
				var param = (ParameterListSyntax)node.SubOrFunctionHeader.ParameterList.Accept(this);
				if (param.Parameters.Count == 1)
					return SyntaxFactory.SimpleLambdaExpression(param.Parameters[0], body);
				return SyntaxFactory.ParenthesizedLambdaExpression(param, body);
			}

			#endregion

			#region Type Name / Modifier

			public override CSharpSyntaxNode VisitPredefinedType(VBSyntax.PredefinedTypeSyntax node)
			{
				return SyntaxFactory.PredefinedType(ConvertToken(node.Keyword));
			}

			public override CSharpSyntaxNode VisitNullableType(VBSyntax.NullableTypeSyntax node)
			{
				return SyntaxFactory.NullableType((TypeSyntax)node.ElementType.Accept(this));
			}

			public override CSharpSyntaxNode VisitArrayType(VBSyntax.ArrayTypeSyntax node)
			{
				return SyntaxFactory.ArrayType((TypeSyntax)node.ElementType.Accept(this), SyntaxFactory.List(node.RankSpecifiers.Select(r => (ArrayRankSpecifierSyntax)r.Accept(this))));
			}

			public override CSharpSyntaxNode VisitArrayRankSpecifier(VBSyntax.ArrayRankSpecifierSyntax node)
			{
				return SyntaxFactory.ArrayRankSpecifier(SyntaxFactory.SeparatedList(Enumerable.Repeat<ExpressionSyntax>(SyntaxFactory.OmittedArraySizeExpression(), node.Rank)));
			}

			private void SplitTypeParameters(VBSyntax.TypeParameterListSyntax typeParameterList, out TypeParameterListSyntax parameters, out SyntaxList<TypeParameterConstraintClauseSyntax> constraints)
			{
				parameters = null;
				constraints = SyntaxFactory.List<TypeParameterConstraintClauseSyntax>();
				if (typeParameterList == null)
					return;
				var paramList = new List<TypeParameterSyntax>();
				var constraintList = new List<TypeParameterConstraintClauseSyntax>();
				foreach (var p in typeParameterList.Parameters)
				{
					var tp = (TypeParameterSyntax)p.Accept(this);
					paramList.Add(tp);
					var constraint = (TypeParameterConstraintClauseSyntax)p.TypeParameterConstraintClause?.Accept(this);
					if (constraint != null)
						constraintList.Add(constraint);
				}
				parameters = SyntaxFactory.TypeParameterList(SyntaxFactory.SeparatedList(paramList));
				constraints = SyntaxFactory.List(constraintList);
			}

			public override CSharpSyntaxNode VisitTypeParameter(VBSyntax.TypeParameterSyntax node)
			{
				SyntaxToken variance = default(SyntaxToken);
				if (!node.VarianceKeyword.IsKind(VBasic.SyntaxKind.None))
				{
					variance = SyntaxFactory.Token(node.VarianceKeyword.IsKind(VBasic.SyntaxKind.InKeyword) ? SyntaxKind.InKeyword : SyntaxKind.OutKeyword);
				}
				return SyntaxFactory.TypeParameter(SyntaxFactory.List<AttributeListSyntax>(), variance, ConvertIdentifier(node.Identifier, semanticModel));
			}

			public override CSharpSyntaxNode VisitTypeParameterSingleConstraintClause(VBSyntax.TypeParameterSingleConstraintClauseSyntax node)
			{
				var id = SyntaxFactory.IdentifierName(ConvertIdentifier(((VBSyntax.TypeParameterSyntax)node.Parent).Identifier, semanticModel));
				return SyntaxFactory.TypeParameterConstraintClause(id, SyntaxFactory.SingletonSeparatedList((TypeParameterConstraintSyntax)node.Constraint.Accept(this)));
			}

			public override CSharpSyntaxNode VisitTypeParameterMultipleConstraintClause(VBSyntax.TypeParameterMultipleConstraintClauseSyntax node)
			{
				var id = SyntaxFactory.IdentifierName(ConvertIdentifier(((VBSyntax.TypeParameterSyntax)node.Parent).Identifier, semanticModel));
				return SyntaxFactory.TypeParameterConstraintClause(id, SyntaxFactory.SeparatedList(node.Constraints.Select(c => (TypeParameterConstraintSyntax)c.Accept(this))));
			}

			public override CSharpSyntaxNode VisitSpecialConstraint(VBSyntax.SpecialConstraintSyntax node)
			{
				if (node.ConstraintKeyword.IsKind(VBasic.SyntaxKind.NewKeyword))
					return SyntaxFactory.ConstructorConstraint();
				return SyntaxFactory.ClassOrStructConstraint(node.IsKind(VBasic.SyntaxKind.ClassConstraint) ? SyntaxKind.ClassConstraint : SyntaxKind.StructConstraint);
			}

			public override CSharpSyntaxNode VisitTypeConstraint(VBSyntax.TypeConstraintSyntax node)
			{
				return SyntaxFactory.TypeConstraint((TypeSyntax)node.Type.Accept(this));
			}

			#endregion

			#region NameSyntax

			public override CSharpSyntaxNode VisitIdentifierName(VBSyntax.IdentifierNameSyntax node)
			{
				return SyntaxFactory.IdentifierName(ConvertIdentifier(node.Identifier, semanticModel, node.Parent is VBSyntax.AttributeSyntax));
			}

			public override CSharpSyntaxNode VisitQualifiedName(VBSyntax.QualifiedNameSyntax node)
			{
				return SyntaxFactory.QualifiedName((NameSyntax)node.Left.Accept(this), (SimpleNameSyntax)node.Right.Accept(this));
			}

			public override CSharpSyntaxNode VisitGenericName(VBSyntax.GenericNameSyntax node)
			{
				return SyntaxFactory.GenericName(ConvertIdentifier(node.Identifier, semanticModel), (TypeArgumentListSyntax)node.TypeArgumentList?.Accept(this));
			}

			public override CSharpSyntaxNode VisitTypeArgumentList(VBSyntax.TypeArgumentListSyntax node)
			{
				return SyntaxFactory.TypeArgumentList(SyntaxFactory.SeparatedList(node.Arguments.Select(a => (TypeSyntax)a.Accept(this))));
			}

			#endregion
		}
	}
}
