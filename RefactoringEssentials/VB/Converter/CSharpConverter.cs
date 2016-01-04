using System;
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
	static class ConversionHelpers
	{
		public static CS.SyntaxKind CSKind(this SyntaxToken token)
		{
			return CS.CSharpExtensions.Kind(token);
		}
	}

	public class CSharpConverter
	{
		/* TODO
		 * - Expressions
		 *   - inline assignment / increment / decrement
		 * - Handles clause
		 * - Implements clause on members
		 * 
		 * 
		 */
		enum TokenContext
		{
			Global,
			Member,
			Local
		}

        public static VisualBasicSyntaxNode Convert(CS.CSharpSyntaxNode input, SemanticModel semanticModel)
        {
            return input.Accept(new NodesVisitor(semanticModel));
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
                return new ConversionResult(Convert((CS.CSharpSyntaxNode)tree.GetRoot(), compilation.GetSemanticModel(tree, true)).NormalizeWhitespace().ToFullString());
            }
            catch (Exception ex)
            {
                return new ConversionResult(ex);
            }
        }

		static SyntaxTokenList ConvertModifiers(SyntaxTokenList modifiers, TokenContext context = TokenContext.Global)
		{
			return SyntaxFactory.TokenList(modifiers.Select(m => SyntaxFactory.Token(ConvertToken(CS.CSharpExtensions.Kind(m), context))));
		}

        static SyntaxToken ConvertModifier(SyntaxToken m, TokenContext context = TokenContext.Global)
		{
			return SyntaxFactory.Token(ConvertToken(CS.CSharpExtensions.Kind(m), context));
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
                } else
                {
                    declarators.Add(
                        SyntaxFactory.VariableDeclarator(
                            SyntaxFactory.SingletonSeparatedList(ExtractIdentifier(v)),
                            SyntaxFactory.SimpleAsClause(type),
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
            var id = SyntaxFactory.Identifier(v.Identifier.ValueText, SyntaxFacts.IsKeywordKind(v.Identifier.Kind()), v.Identifier.GetIdentifierText(), TypeCharacter.None);
            return SyntaxFactory.ModifiedIdentifier(id);
        }

        class NodesVisitor : CS.CSharpSyntaxVisitor<VisualBasicSyntaxNode>
		{
			SemanticModel semanticModel;
            readonly List<CSS.BaseTypeDeclarationSyntax> inlineAssignHelperMarkers = new List<CSS.BaseTypeDeclarationSyntax>();

            const string InlineAssignHelperCode = @"<Obsolete(""Please refactor code that uses this function, it is a simple work-around to simulate inline assignment in VB!"")>
Private Shared Function __InlineAssignHelper(Of T)(ByRef target As T, value As T) As T
target = value
Return value
End Function";

            void MarkPatchInlineAssignHelper(CS.CSharpSyntaxNode node)
            {
                var parentDefinition = node.AncestorsAndSelf().OfType<CSS.BaseTypeDeclarationSyntax>().FirstOrDefault();
                inlineAssignHelperMarkers.Add(parentDefinition);
            }

            IEnumerable<StatementSyntax> PatchInlineHelpers(CSS.BaseTypeDeclarationSyntax node)
            {
                if (inlineAssignHelperMarkers.Contains(node))
                {
                    inlineAssignHelperMarkers.Remove(node);
                    yield return SyntaxFactory.ParseSyntaxTree(InlineAssignHelperCode)
                        .GetRoot().ChildNodes().FirstOrDefault().NormalizeWhitespace() as StatementSyntax;
                }
            }

            public NodesVisitor(SemanticModel semanticModel)
			{
				this.semanticModel = semanticModel;
			}

			public override VisualBasicSyntaxNode DefaultVisit(SyntaxNode node)
			{
				throw new NotImplementedException(node.GetType() + " not implemented!");
			}

			public override VisualBasicSyntaxNode VisitCompilationUnit(CSS.CompilationUnitSyntax node)
			{
				var imports = node.Usings.Select(u => (ImportsStatementSyntax)u.Accept(this))
					.Concat(node.Externs.Select(e => (ImportsStatementSyntax)e.Accept(this)));
				var attributes = node.AttributeLists.Select(a => SyntaxFactory.AttributesStatement(SyntaxFactory.SingletonList((AttributeListSyntax)a.Accept(this))));
				var members = node.Members.Select(m => (StatementSyntax)m.Accept(this));

				return SyntaxFactory.CompilationUnit(
					SyntaxFactory.List<OptionStatementSyntax>(),
					SyntaxFactory.List(imports),
					SyntaxFactory.List(attributes),
					SyntaxFactory.List(members)
				);
			}

			#region Attributes
			public override VisualBasicSyntaxNode VisitAttributeList(CSS.AttributeListSyntax node)
			{
				return SyntaxFactory.AttributeList(SyntaxFactory.SeparatedList(node.Attributes.Select(a => (AttributeSyntax)a.Accept(this))));
			}

			public override VisualBasicSyntaxNode VisitAttribute(CSS.AttributeSyntax node)
			{
				var list = (CSS.AttributeListSyntax)node.Parent;
				return SyntaxFactory.Attribute((AttributeTargetSyntax)list.Target.Accept(this), (TypeSyntax)node.Name.Accept(this), (ArgumentListSyntax)node.ArgumentList?.Accept(this));
			}

			public override VisualBasicSyntaxNode VisitAttributeTargetSpecifier(CSS.AttributeTargetSpecifierSyntax node)
			{
				SyntaxToken id;
				switch (node.Identifier.CSKind())
				{
				case CS.SyntaxKind.AssemblyKeyword:
					id = SyntaxFactory.Token(SyntaxKind.AssemblyKeyword);
					break;
                case CS.SyntaxKind.ReturnKeyword:
                    // not necessary, return attributes are moved by ConvertAndSplitAttributes.
                    return null;
				default:
					throw new NotSupportedException();
				}
				return SyntaxFactory.AttributeTarget(id);
			}

			public override VisualBasicSyntaxNode VisitAttributeArgumentList(CSS.AttributeArgumentListSyntax node)
			{
				return SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList(node.Arguments.Select(a => (ArgumentSyntax)a.Accept(this))));
			}

			public override VisualBasicSyntaxNode VisitAttributeArgument(CSS.AttributeArgumentSyntax node)
			{
				NameColonEqualsSyntax name = null;
				if (node.NameColon != null)
				{
					name = SyntaxFactory.NameColonEquals((IdentifierNameSyntax)node.NameColon.Name.Accept(this));
				}
				else if (node.NameEquals != null)
				{
					name = SyntaxFactory.NameColonEquals((IdentifierNameSyntax)node.NameEquals.Name.Accept(this));
				}
				var value = (ExpressionSyntax)node.Expression.Accept(this);
				return SyntaxFactory.SimpleArgument(name, value);
			}
			#endregion

			public override VisualBasicSyntaxNode VisitNamespaceDeclaration(CSS.NamespaceDeclarationSyntax node)
			{
				var members = node.Members.Select(m => (StatementSyntax)m.Accept(this));

				IList<string> names;
				if (!node.Name.TryGetNameParts(out names))
					throw new NotSupportedException();

				return SyntaxFactory.NamespaceBlock(
					SyntaxFactory.NamespaceStatement((NameSyntax)node.Name.Accept(this)),
					SyntaxFactory.List(members)
				);
			}

			public override VisualBasicSyntaxNode VisitUsingDirective(CSS.UsingDirectiveSyntax node)
			{
				ImportAliasClauseSyntax alias = null;
				if (node.Alias != null)
				{
					var name = node.Alias.Name;
					var id = SyntaxFactory.Identifier(name.Identifier.ValueText, SyntaxFacts.IsKeywordKind(name.Identifier.Kind()), name.Identifier.GetIdentifierText(), TypeCharacter.None);
					alias = SyntaxFactory.ImportAliasClause(id);
                }
				ImportsClauseSyntax clause = SyntaxFactory.SimpleImportsClause(alias, (NameSyntax)node.Name.Accept(this));
				return SyntaxFactory.ImportsStatement(SyntaxFactory.SingletonSeparatedList(clause));
			}

			#region Namespace Members

			public override VisualBasicSyntaxNode VisitClassDeclaration(CSS.ClassDeclarationSyntax node)
			{
				var members = node.Members.Select(m => (StatementSyntax)m.Accept(this)).ToList();
				var id = SyntaxFactory.Identifier(node.Identifier.ValueText, SyntaxFacts.IsKeywordKind(node.Identifier.Kind()), node.Identifier.GetIdentifierText(), TypeCharacter.None);

				List<InheritsStatementSyntax> inherits = new List<InheritsStatementSyntax>();
				List<ImplementsStatementSyntax> implements = new List<ImplementsStatementSyntax>();
				ConvertBaseList(node, inherits, implements);
                members.AddRange(PatchInlineHelpers(node));

				return SyntaxFactory.ClassBlock(
					SyntaxFactory.ClassStatement(
						SyntaxFactory.List(node.AttributeLists.Select(a => (AttributeListSyntax)a.Accept(this))),
						ConvertModifiers(node.Modifiers),
						id, (TypeParameterListSyntax)node.TypeParameterList?.Accept(this)
					),
					SyntaxFactory.List(inherits),
					SyntaxFactory.List(implements),
					SyntaxFactory.List(members)
				);
			}

            public override VisualBasicSyntaxNode VisitStructDeclaration(CSS.StructDeclarationSyntax node)
			{
				var members = node.Members.Select(m => (StatementSyntax)m.Accept(this)).ToList();
				var id = SyntaxFactory.Identifier(node.Identifier.ValueText, SyntaxFacts.IsKeywordKind(node.Identifier.Kind()), node.Identifier.GetIdentifierText(), TypeCharacter.None);

				List<InheritsStatementSyntax> inherits = new List<InheritsStatementSyntax>();
				List<ImplementsStatementSyntax> implements = new List<ImplementsStatementSyntax>();
				ConvertBaseList(node, inherits, implements);
                members.AddRange(PatchInlineHelpers(node));

                return SyntaxFactory.StructureBlock(
					SyntaxFactory.StructureStatement(
						SyntaxFactory.List(node.AttributeLists.Select(a => (AttributeListSyntax)a.Accept(this))),
						ConvertModifiers(node.Modifiers),
						id, (TypeParameterListSyntax)node.TypeParameterList?.Accept(this)
					),
					SyntaxFactory.List(inherits),
					SyntaxFactory.List(implements),
					SyntaxFactory.List(members)
				);
			}

			public override VisualBasicSyntaxNode VisitInterfaceDeclaration(CSS.InterfaceDeclarationSyntax node)
			{
				var members = node.Members.Select(m => (StatementSyntax)m.Accept(this)).ToArray();
				var id = SyntaxFactory.Identifier(node.Identifier.ValueText, SyntaxFacts.IsKeywordKind(node.Identifier.Kind()), node.Identifier.GetIdentifierText(), TypeCharacter.None);

				List<InheritsStatementSyntax> inherits = new List<InheritsStatementSyntax>();
				List<ImplementsStatementSyntax> implements = new List<ImplementsStatementSyntax>();
				ConvertBaseList(node, inherits, implements);

				return SyntaxFactory.InterfaceBlock(
					SyntaxFactory.InterfaceStatement(
						SyntaxFactory.List(node.AttributeLists.Select(a => (AttributeListSyntax)a.Accept(this))),
						ConvertModifiers(node.Modifiers),
						id, (TypeParameterListSyntax)node.TypeParameterList?.Accept(this)
					),
					SyntaxFactory.List(inherits),
					SyntaxFactory.List(implements),
					SyntaxFactory.List(members)
				);
			}

			public override VisualBasicSyntaxNode VisitEnumDeclaration(CSS.EnumDeclarationSyntax node)
			{
				var members = node.Members.Select(m => (StatementSyntax)m.Accept(this));
				var baseType = (TypeSyntax)node.BaseList?.Types.Single().Accept(this);
                var id = SyntaxFactory.Identifier(node.Identifier.ValueText, SyntaxFacts.IsKeywordKind(node.Identifier.Kind()), node.Identifier.GetIdentifierText(), TypeCharacter.None);
				return SyntaxFactory.EnumBlock(
					SyntaxFactory.EnumStatement(
						SyntaxFactory.List(node.AttributeLists.Select(a => (AttributeListSyntax)a.Accept(this))),
						ConvertModifiers(node.Modifiers),
						id, baseType == null ? null : SyntaxFactory.SimpleAsClause(baseType)
					),
					SyntaxFactory.List(members)
				);
			}

			public override VisualBasicSyntaxNode VisitEnumMemberDeclaration(CSS.EnumMemberDeclarationSyntax node)
			{
				var initializer = (ExpressionSyntax)node.EqualsValue?.Value.Accept(this);
				var id = SyntaxFactory.Identifier(node.Identifier.ValueText, SyntaxFacts.IsKeywordKind(node.Identifier.Kind()), node.Identifier.GetIdentifierText(), TypeCharacter.None);
				return SyntaxFactory.EnumMemberDeclaration(
					SyntaxFactory.List(node.AttributeLists.Select(a => (AttributeListSyntax)a.Accept(this))),
					id,
					initializer == null ? null : SyntaxFactory.EqualsValue(initializer)
				);
			}

			public override VisualBasicSyntaxNode VisitDelegateDeclaration(CSS.DelegateDeclarationSyntax node)
			{
				var id = SyntaxFactory.Identifier(node.Identifier.ValueText, SyntaxFacts.IsKeywordKind(node.Identifier.Kind()), node.Identifier.GetIdentifierText(), TypeCharacter.None);
				var methodInfo = semanticModel.GetDeclaredSymbol(node) as INamedTypeSymbol;
				if (methodInfo.DelegateInvokeMethod.GetReturnType()?.SpecialType == SpecialType.System_Void)
				{
					return SyntaxFactory.DelegateSubStatement(
						SyntaxFactory.List(node.AttributeLists.Select(a => (AttributeListSyntax)a.Accept(this))),
						ConvertModifiers(node.Modifiers),
						id, (TypeParameterListSyntax)node.TypeParameterList?.Accept(this),
						(ParameterListSyntax)node.ParameterList?.Accept(this),
						null
					);
				} 
				else
				{
					return SyntaxFactory.DelegateFunctionStatement(
						SyntaxFactory.List(node.AttributeLists.Select(a => (AttributeListSyntax)a.Accept(this))),
						ConvertModifiers(node.Modifiers),
						id, (TypeParameterListSyntax)node.TypeParameterList?.Accept(this),
						(ParameterListSyntax)node.ParameterList?.Accept(this),
						SyntaxFactory.SimpleAsClause((TypeSyntax)node.ReturnType.Accept(this))
					);
				}
			}

            #endregion

            #region Type Members

            public override VisualBasicSyntaxNode VisitFieldDeclaration(CSS.FieldDeclarationSyntax node)
            {
                var modifiers = ConvertModifiers(node.Modifiers, TokenContext.Member);
                if (modifiers.Count == 0)
                    modifiers = modifiers.Add(SyntaxFactory.Token(SyntaxKind.PrivateKeyword));
                return SyntaxFactory.FieldDeclaration(
                    SyntaxFactory.List(node.AttributeLists.Select(a => (AttributeListSyntax)a.Accept(this))),
                    modifiers,
                    RemodelVariableDeclaration(node.Declaration, this)
                );
            }

            public override VisualBasicSyntaxNode VisitMethodDeclaration(CSS.MethodDeclarationSyntax node)
			{
				SyntaxList<StatementSyntax>? block = null;
				if (node.Body != null)
				{
					block = SyntaxFactory.List(node.Body.Statements.SelectMany(s => s.Accept(new MethodBodyVisitor(semanticModel, this))));
				}
				var id = SyntaxFactory.Identifier(node.Identifier.ValueText, SyntaxFacts.IsKeywordKind(node.Identifier.Kind()), node.Identifier.GetIdentifierText(), TypeCharacter.None);
				var methodInfo = semanticModel.GetDeclaredSymbol(node);
				if (methodInfo?.GetReturnType()?.SpecialType == SpecialType.System_Void)
				{
					var stmt = SyntaxFactory.SubStatement(
						SyntaxFactory.List(node.AttributeLists.Select(a => (AttributeListSyntax)a.Accept(this))),
						ConvertModifiers(node.Modifiers, TokenContext.Member),
						id, (TypeParameterListSyntax)node.TypeParameterList?.Accept(this),
						(ParameterListSyntax)node.ParameterList?.Accept(this),
						null, null, null
					);
					if (block == null)
						return stmt;
					return SyntaxFactory.SubBlock(stmt, block.Value);
				}
				else
				{
					var stmt = SyntaxFactory.FunctionStatement(
						SyntaxFactory.List(node.AttributeLists.Select(a => (AttributeListSyntax)a.Accept(this))),
						ConvertModifiers(node.Modifiers, TokenContext.Member),
						id, (TypeParameterListSyntax)node.TypeParameterList?.Accept(this),
						(ParameterListSyntax)node.ParameterList?.Accept(this),
						SyntaxFactory.SimpleAsClause((TypeSyntax)node.ReturnType.Accept(this)), null, null
					);
					if (block == null)
						return stmt;
                    return SyntaxFactory.FunctionBlock(stmt, block.Value);
				}
			}

            public override VisualBasicSyntaxNode VisitPropertyDeclaration(CSS.PropertyDeclarationSyntax node)
            {
                var id = SyntaxFactory.Identifier(node.Identifier.ValueText, SyntaxFacts.IsKeywordKind(node.Identifier.Kind()), node.Identifier.GetIdentifierText(), TypeCharacter.None);
                SyntaxList<AttributeListSyntax> attributes, returnAttributes;
                ConvertAndSplitAttributes(node.AttributeLists, out attributes, out returnAttributes);
                var stmt = SyntaxFactory.PropertyStatement(
                    attributes,
                    ConvertModifiers(node.Modifiers, TokenContext.Member),
                    id, null,
                    SyntaxFactory.SimpleAsClause(returnAttributes, (TypeSyntax)node.Type.Accept(this)), null, null
                );
                if (node.AccessorList.Accessors.All(a => a.Body == null))
                    return stmt;
                var accessors = node.AccessorList?.Accessors.Select(a => (AccessorBlockSyntax)a.Accept(this)).ToArray();
                return SyntaxFactory.PropertyBlock(stmt, SyntaxFactory.List(accessors));
            }

            public override VisualBasicSyntaxNode VisitEventDeclaration(CSS.EventDeclarationSyntax node)
            {
                return base.VisitEventDeclaration(node);
            }

            public override VisualBasicSyntaxNode VisitEventFieldDeclaration(CSS.EventFieldDeclarationSyntax node)
            {
                return base.VisitEventFieldDeclaration(node);
            }

            private void ConvertAndSplitAttributes(SyntaxList<CSS.AttributeListSyntax> attributeLists, out SyntaxList<AttributeListSyntax> attributes, out SyntaxList<AttributeListSyntax> returnAttributes)
            {
                var retAttr = new List<AttributeListSyntax>();
                var attr = new List<AttributeListSyntax>();

                foreach (var attrList in attributeLists)
                {
                    if (attrList.Target.Identifier.IsKind(CS.SyntaxKind.ReturnKeyword))
                        retAttr.Add((AttributeListSyntax)attrList.Accept(this));
                    else
                        attr.Add((AttributeListSyntax)attrList.Accept(this));
                }
                returnAttributes = SyntaxFactory.List(retAttr);
                attributes = SyntaxFactory.List(attr);
            }

            public override VisualBasicSyntaxNode VisitAccessorDeclaration(CSS.AccessorDeclarationSyntax node)
            {
                SyntaxKind blockKind;
                AccessorStatementSyntax stmt;
                EndBlockStatementSyntax endStmt;
                SyntaxList<StatementSyntax> body = SyntaxFactory.List<StatementSyntax>();
                if (node.Body != null)
                {
                    body = SyntaxFactory.List(node.Body.Statements.SelectMany(s => s.Accept(new MethodBodyVisitor(semanticModel, this))));
                }
                var attributes = SyntaxFactory.List(node.AttributeLists.Select(a => (AttributeListSyntax)a.Accept(this)));
                var modifiers = ConvertModifiers(node.Modifiers, TokenContext.Member);
                var parent = (CSS.BasePropertyDeclarationSyntax)node.Parent.Parent;
                ParameterSyntax valueParam;

                switch (CS.CSharpExtensions.Kind(node))
                {
                case CS.SyntaxKind.GetAccessorDeclaration:
                    blockKind = SyntaxKind.GetAccessorBlock;
                    stmt = SyntaxFactory.GetAccessorStatement(attributes, modifiers, null);
                    endStmt = SyntaxFactory.EndGetStatement();
                    break;
                case CS.SyntaxKind.SetAccessorDeclaration:
                    blockKind = SyntaxKind.SetAccessorBlock;
                    valueParam = SyntaxFactory.Parameter(SyntaxFactory.ModifiedIdentifier("value")).WithAsClause(SyntaxFactory.SimpleAsClause((TypeSyntax)parent.Type.Accept(this)));
                    stmt = SyntaxFactory.SetAccessorStatement(attributes, modifiers, SyntaxFactory.ParameterList(SyntaxFactory.SingletonSeparatedList(valueParam)));
                    endStmt = SyntaxFactory.EndSetStatement();
                    break;
                case CS.SyntaxKind.AddAccessorDeclaration:
                    blockKind = SyntaxKind.AddHandlerAccessorBlock;
                    valueParam = SyntaxFactory.Parameter(SyntaxFactory.ModifiedIdentifier("value")).WithAsClause(SyntaxFactory.SimpleAsClause((TypeSyntax)parent.Type.Accept(this)));
                    stmt = SyntaxFactory.AddHandlerAccessorStatement(attributes, modifiers, SyntaxFactory.ParameterList(SyntaxFactory.SingletonSeparatedList(valueParam)));
                    endStmt = SyntaxFactory.EndAddHandlerStatement();
                    break;
                case CS.SyntaxKind.RemoveAccessorDeclaration:
                    blockKind = SyntaxKind.RemoveHandlerAccessorBlock;
                    valueParam = SyntaxFactory.Parameter(SyntaxFactory.ModifiedIdentifier("value")).WithAsClause(SyntaxFactory.SimpleAsClause((TypeSyntax)parent.Type.Accept(this)));
                    stmt = SyntaxFactory.RemoveHandlerAccessorStatement(attributes, modifiers, SyntaxFactory.ParameterList(SyntaxFactory.SingletonSeparatedList(valueParam)));
                    endStmt = SyntaxFactory.EndRemoveHandlerStatement();
                    break;
                default:
                    throw new NotSupportedException();
                }
                return SyntaxFactory.AccessorBlock(blockKind, stmt, body, endStmt);
            }

            public override VisualBasicSyntaxNode VisitParameterList(CSS.ParameterListSyntax node)
			{
				return SyntaxFactory.ParameterList(SyntaxFactory.SeparatedList(node.Parameters.Select(p => (ParameterSyntax)p.Accept(this))));
			}

			public override VisualBasicSyntaxNode VisitParameter(CSS.ParameterSyntax node)
			{
				var id = SyntaxFactory.Identifier(node.Identifier.ValueText, SyntaxFacts.IsKeywordKind(node.Identifier.Kind()), node.Identifier.GetIdentifierText(), TypeCharacter.None);
				EqualsValueSyntax @default = null;
                if (node.Default != null)
                {
                    @default = SyntaxFactory.EqualsValue((ExpressionSyntax)node.Default?.Value.Accept(this));
                }
                AttributeListSyntax[] newAttributes;
                var modifiers = ConvertModifiers(node.Modifiers, TokenContext.Member);
                if (modifiers.Count == 0)
                {
                    modifiers = SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.ByValKeyword));
                    newAttributes = new AttributeListSyntax[0];
                } 
                else if (node.Modifiers.Any(m => m.IsKind(CS.SyntaxKind.OutKeyword)))
                {
                    newAttributes = new[] {
                        SyntaxFactory.AttributeList(
                            SyntaxFactory.SingletonSeparatedList(
                                SyntaxFactory.Attribute(SyntaxFactory.ParseTypeName("Out"))
                            )
                        )
                    };
                }
                else
                {
                    newAttributes = new AttributeListSyntax[0];
                }
                return SyntaxFactory.Parameter(
					SyntaxFactory.List(newAttributes.Concat(node.AttributeLists.Select(a => (AttributeListSyntax)a.Accept(this)))),
					modifiers,
					SyntaxFactory.ModifiedIdentifier(id),
					SyntaxFactory.SimpleAsClause((TypeSyntax)node.Type.Accept(this)),
					@default
				);
			}

			#endregion

			#region Expressions

			ExpressionSyntax Literal(object o)
			{
				return ComputeConstantValueCodeRefactoringProvider.GetLiteralExpression(o);
			}

			public override VisualBasicSyntaxNode VisitLiteralExpression(CSS.LiteralExpressionSyntax node)
			{
				return Literal(node.Token.Value);
			}

			public override VisualBasicSyntaxNode VisitParenthesizedExpression(CSS.ParenthesizedExpressionSyntax node)
			{
				return SyntaxFactory.ParenthesizedExpression((ExpressionSyntax)node.Expression.Accept(this));
			}

			public override VisualBasicSyntaxNode VisitPrefixUnaryExpression(CSS.PrefixUnaryExpressionSyntax node)
			{
                var kind = ConvertToken(CS.CSharpExtensions.Kind(node), TokenContext.Local);
                if (node.Parent is CSS.ExpressionStatementSyntax)
                {
                    return SyntaxFactory.AssignmentStatement(
                        kind,
                        (ExpressionSyntax)node.Operand.Accept(this),
                        SyntaxFactory.Token(VBUtil.GetBinaryExpressionOperatorTokenKind(kind)),
                        Literal(1)
                    );
                }
                else
                {
                    string operatorName;
                    if (kind == SyntaxKind.AddAssignmentStatement)
                        operatorName = "Increment";
                    else
                        operatorName = "Decrement";
                    return SyntaxFactory.InvocationExpression(
                        SyntaxFactory.ParseName("System.Threading.Interlocked." + operatorName),
                        SyntaxFactory.ArgumentList(
                            SyntaxFactory.SeparatedList(
                                new ArgumentSyntax[] {
                                    SyntaxFactory.SimpleArgument((ExpressionSyntax)node.Operand.Accept(this))
                                }
                            )
                        )
                    );
                }
            }

            public override VisualBasicSyntaxNode VisitPostfixUnaryExpression(CSS.PostfixUnaryExpressionSyntax node)
            {
                var kind = ConvertToken(CS.CSharpExtensions.Kind(node), TokenContext.Local);
                if (node.Parent is CSS.ExpressionStatementSyntax)
                {
                    return SyntaxFactory.AssignmentStatement(
                        ConvertToken(CS.CSharpExtensions.Kind(node), TokenContext.Local),
                        (ExpressionSyntax)node.Operand.Accept(this),
                        SyntaxFactory.Token(VBUtil.GetBinaryExpressionOperatorTokenKind(kind)),
                        Literal(1)
                    );
                } else
                {
                    string operatorName, minMax;
                    SyntaxKind op;
                    if (kind == SyntaxKind.AddAssignmentStatement)
                    {
                        operatorName = "Increment";
                        minMax = "Min";
                        op = SyntaxKind.SubtractExpression;
                    } else
                    {
                        operatorName = "Decrement";
                        minMax = "Max";
                        op = SyntaxKind.AddExpression;
                    }
                    return SyntaxFactory.InvocationExpression(
                        SyntaxFactory.ParseName("Math." + minMax),
                        SyntaxFactory.ArgumentList(
                            SyntaxFactory.SeparatedList(
                                new ArgumentSyntax[] {
                                    SyntaxFactory.SimpleArgument(
                                        SyntaxFactory.InvocationExpression(
                                            SyntaxFactory.ParseName("System.Threading.Interlocked." + operatorName),
                                            SyntaxFactory.ArgumentList(
                                                SyntaxFactory.SingletonSeparatedList<ArgumentSyntax>(
                                                    SyntaxFactory.SimpleArgument((ExpressionSyntax)node.Operand.Accept(this))
                                                )
                                            )
                                        )
                                    ),
                                    SyntaxFactory.SimpleArgument(SyntaxFactory.BinaryExpression(op, (ExpressionSyntax)node.Operand.Accept(this), SyntaxFactory.Token(VBUtil.GetBinaryExpressionOperatorTokenKind(op)), Literal(1)))
                                }
                            )
                        )
                    );
                }
            }

            public override VisualBasicSyntaxNode VisitAssignmentExpression(CSS.AssignmentExpressionSyntax node)
            {
                var kind = ConvertToken(CS.CSharpExtensions.Kind(node), TokenContext.Local);
                if (node.Parent is CSS.ExpressionStatementSyntax)
                {
                    return SyntaxFactory.AssignmentStatement(
                        kind,
                        (ExpressionSyntax)node.Left.Accept(this),
                        SyntaxFactory.Token(VBUtil.GetBinaryExpressionOperatorTokenKind(kind)),
                        (ExpressionSyntax)node.Right.Accept(this)
                    );
                }
                else
                {
                    MarkPatchInlineAssignHelper(node);
                    return SyntaxFactory.InvocationExpression(
                        SyntaxFactory.IdentifierName("__InlineAssignHelper"),
                        SyntaxFactory.ArgumentList(
                            SyntaxFactory.SeparatedList(
                                new ArgumentSyntax[] {
                                    SyntaxFactory.SimpleArgument((ExpressionSyntax)node.Left.Accept(this)),
                                    SyntaxFactory.SimpleArgument((ExpressionSyntax)node.Right.Accept(this))
                                }
                            )
                        )
                    );
                }
            }

            public override VisualBasicSyntaxNode VisitInvocationExpression(CSS.InvocationExpressionSyntax node)
			{
				return SyntaxFactory.InvocationExpression(
					(ExpressionSyntax)node.Expression.Accept(this),
					(ArgumentListSyntax)node.ArgumentList.Accept(this)
				);
			}

			public override VisualBasicSyntaxNode VisitMemberAccessExpression(CSS.MemberAccessExpressionSyntax node)
			{
				return SyntaxFactory.MemberAccessExpression(
					SyntaxKind.SimpleMemberAccessExpression,
					(ExpressionSyntax)node.Expression.Accept(this),
					SyntaxFactory.Token(SyntaxKind.DotToken),
					(SimpleNameSyntax)node.Name.Accept(this)
				);
			}

            public override VisualBasicSyntaxNode VisitDefaultExpression(CSS.DefaultExpressionSyntax node)
            {
                return SyntaxFactory.NothingLiteralExpression(SyntaxFactory.Token(SyntaxKind.NothingKeyword));
            }

            public override VisualBasicSyntaxNode VisitThisExpression(CSS.ThisExpressionSyntax node)
            {
                return SyntaxFactory.MeExpression();
            }

            public override VisualBasicSyntaxNode VisitBaseExpression(CSS.BaseExpressionSyntax node)
            {
                return SyntaxFactory.MyBaseExpression();
            }

            public override VisualBasicSyntaxNode VisitBinaryExpression(CSS.BinaryExpressionSyntax node)
			{
				var kind = ConvertToken(CS.CSharpExtensions.Kind(node), TokenContext.Local);
				return SyntaxFactory.BinaryExpression(
					kind,
					(ExpressionSyntax)node.Left.Accept(this),
					SyntaxFactory.Token(VBUtil.GetBinaryExpressionOperatorTokenKind(kind)),
					(ExpressionSyntax)node.Right.Accept(this)
				);
			}

            public override VisualBasicSyntaxNode VisitObjectCreationExpression(CSS.ObjectCreationExpressionSyntax node)
            {
                return SyntaxFactory.ObjectCreationExpression(
                    SyntaxFactory.List<AttributeListSyntax>(),
                    (TypeSyntax)node.Type.Accept(this),
                    (ArgumentListSyntax)node.ArgumentList.Accept(this),
                    (ObjectCreationInitializerSyntax)node.Initializer?.Accept(this)
                );
            }

            public override VisualBasicSyntaxNode VisitInitializerExpression(CSS.InitializerExpressionSyntax node)
            {
                if (node.Parent is CSS.ObjectCreationExpressionSyntax)
                    return SyntaxFactory.ObjectMemberInitializer();
                throw new NotImplementedException();
            }

            public override VisualBasicSyntaxNode VisitArgumentList(CSS.ArgumentListSyntax node)
			{
				return SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList(node.Arguments.Select(a => (ArgumentSyntax)a.Accept(this))));
			}

			public override VisualBasicSyntaxNode VisitArgument(CSS.ArgumentSyntax node)
			{
				NameColonEqualsSyntax name = null;
				if (node.NameColon != null)
				{
					name = SyntaxFactory.NameColonEquals((IdentifierNameSyntax)node.NameColon.Name.Accept(this));
				}
				var value = (ExpressionSyntax)node.Expression.Accept(this);
				return SyntaxFactory.SimpleArgument(name, value);
			}

			#endregion

			#region Types / Modifiers

			public override VisualBasicSyntaxNode VisitTypeParameterList(CSS.TypeParameterListSyntax node)
			{
				return SyntaxFactory.TypeParameterList(node.Parameters.Select(p => (TypeParameterSyntax)p.Accept(this)).ToArray());
			}

			public override VisualBasicSyntaxNode VisitTypeParameter(CSS.TypeParameterSyntax node)
			{
				SyntaxToken variance = default(SyntaxToken);
                if (!node.VarianceKeyword.IsKind(CS.SyntaxKind.None))
				{
					variance = SyntaxFactory.Token(node.VarianceKeyword.IsKind(CS.SyntaxKind.InKeyword) ? SyntaxKind.InKeyword : SyntaxKind.OutKeyword);
				}
                // copy generic constraints
                var clause = FindClauseForParameter(node);
				return SyntaxFactory.TypeParameter(variance, ConvertIdentifier(node.Identifier), (TypeParameterConstraintClauseSyntax)clause?.Accept(this));
			}

            public override VisualBasicSyntaxNode VisitTypeParameterConstraintClause(CSS.TypeParameterConstraintClauseSyntax node)
            {
                if (node.Constraints.Count == 1)
                    return SyntaxFactory.TypeParameterSingleConstraintClause((ConstraintSyntax)node.Constraints[0].Accept(this));
                return SyntaxFactory.TypeParameterMultipleConstraintClause(SyntaxFactory.SeparatedList(node.Constraints.Select(c => (ConstraintSyntax)c.Accept(this))));
            }

            public override VisualBasicSyntaxNode VisitClassOrStructConstraint(CSS.ClassOrStructConstraintSyntax node)
            {
                if (node.IsKind(CS.SyntaxKind.ClassConstraint))
                    return SyntaxFactory.ClassConstraint(SyntaxFactory.Token(SyntaxKind.ClassKeyword));
                if (node.IsKind(CS.SyntaxKind.StructConstraint))
                    return SyntaxFactory.StructureConstraint(SyntaxFactory.Token(SyntaxKind.StructureKeyword));
                throw new NotSupportedException();
            }

            public override VisualBasicSyntaxNode VisitTypeConstraint(CSS.TypeConstraintSyntax node)
            {
                return SyntaxFactory.TypeConstraint((TypeSyntax)node.Accept(this));
            }

            public override VisualBasicSyntaxNode VisitConstructorConstraint(CSS.ConstructorConstraintSyntax node)
            {
                return SyntaxFactory.NewConstraint(SyntaxFactory.Token(SyntaxKind.NewKeyword));
            }

            private CSS.TypeParameterConstraintClauseSyntax FindClauseForParameter(CSS.TypeParameterSyntax node)
            {
                SyntaxList<CSS.TypeParameterConstraintClauseSyntax> clauses;
                var parentBlock = node.Parent.Parent;
                if (parentBlock is CSS.MethodDeclarationSyntax)
                {
                    clauses = ((CSS.MethodDeclarationSyntax)parentBlock).ConstraintClauses;
                }
                else if (parentBlock is CSS.ClassDeclarationSyntax)
                {
                    clauses = ((CSS.ClassDeclarationSyntax)parentBlock).ConstraintClauses;
                }
                else
                {
                    throw new NotImplementedException($"{parentBlock.GetType().FullName} not implemented!");
                }
                return clauses.FirstOrDefault(c => c.Name.ToString() == node.ToString());
            }

			private void ConvertBaseList(CSS.BaseTypeDeclarationSyntax type, List<InheritsStatementSyntax> inherits, List<ImplementsStatementSyntax> implements)
			{
				TypeSyntax[] arr;
				switch (type.Kind())
				{
				case CS.SyntaxKind.ClassDeclaration:
					var classOrInterface = type.BaseList?.Types.FirstOrDefault()?.Type;
					if (classOrInterface == null) return;
					var classOrInterfaceSymbol = semanticModel.GetSymbolInfo(classOrInterface).Symbol;
					if (classOrInterfaceSymbol == null) return;
					if (classOrInterfaceSymbol.IsInterfaceType())
					{
						arr = type.BaseList?.Types.Select(t => (TypeSyntax)t.Type.Accept(this)).ToArray();
						if (arr.Length > 0)
							implements.Add(SyntaxFactory.ImplementsStatement(arr));
					}
					else
					{
						inherits.Add(SyntaxFactory.InheritsStatement((TypeSyntax)classOrInterface.Accept(this)));
						arr = type.BaseList?.Types.Skip(1).Select(t => (TypeSyntax)t.Type.Accept(this)).ToArray();
						if (arr.Length > 0)
							implements.Add(SyntaxFactory.ImplementsStatement(arr));
					}
					break;
				case CS.SyntaxKind.StructDeclaration:
					arr = type.BaseList?.Types.Select(t => (TypeSyntax)t.Type.Accept(this)).ToArray();
					if (arr.Length > 0)
						implements.Add(SyntaxFactory.ImplementsStatement(arr));
					break;
				case CS.SyntaxKind.InterfaceDeclaration:
					arr = type.BaseList?.Types.Select(t => (TypeSyntax)t.Type.Accept(this)).ToArray();
					if (arr.Length > 0)
						inherits.Add(SyntaxFactory.InheritsStatement(arr));
					break;
				}
			}

			public override VisualBasicSyntaxNode VisitPredefinedType(CSS.PredefinedTypeSyntax node)
			{
				return SyntaxFactory.PredefinedType(SyntaxFactory.Token(ConvertToken(CS.CSharpExtensions.Kind(node.Keyword))));
			}

			#endregion

			#region NameSyntax

			SyntaxToken ConvertIdentifier(SyntaxToken id)
			{
				if (SyntaxFacts.GetKeywordKind(id.ValueText) != SyntaxKind.None)
					return SyntaxFactory.Identifier("[" + id.ValueText + "]");
				return SyntaxFactory.Identifier(id.ValueText);
			}

			public override VisualBasicSyntaxNode VisitIdentifierName(CSS.IdentifierNameSyntax node)
			{
                return SyntaxFactory.IdentifierName(ConvertIdentifier(node.Identifier));
			}

			public override VisualBasicSyntaxNode VisitGenericName(CSS.GenericNameSyntax node)
			{
				return SyntaxFactory.GenericName(ConvertIdentifier(node.Identifier), (TypeArgumentListSyntax)node.TypeArgumentList.Accept(this));
			}

			public override VisualBasicSyntaxNode VisitQualifiedName(CSS.QualifiedNameSyntax node)
			{
				return SyntaxFactory.QualifiedName((NameSyntax)node.Left.Accept(this), (SimpleNameSyntax)node.Right.Accept(this));
			}

			public override VisualBasicSyntaxNode VisitTypeArgumentList(CSS.TypeArgumentListSyntax node)
			{
				return SyntaxFactory.TypeArgumentList(SyntaxFactory.SeparatedList(node.Arguments.Select(a => (TypeSyntax)a.Accept(this))));
			}

			#endregion
        }

		class MethodBodyVisitor : CS.CSharpSyntaxVisitor<SyntaxList<StatementSyntax>>
		{
			SemanticModel semanticModel;
			NodesVisitor nodesVisitor;

			public MethodBodyVisitor(SemanticModel semanticModel, NodesVisitor nodesVisitor)
			{
				this.semanticModel = semanticModel;
				this.nodesVisitor = nodesVisitor;
			}

			public override SyntaxList<StatementSyntax> DefaultVisit(SyntaxNode node)
			{
				throw new NotImplementedException(node.GetType() + " not implemented!");
			}

			public override SyntaxList<StatementSyntax> VisitLocalDeclarationStatement(CSS.LocalDeclarationStatementSyntax node)
			{
				var modifiers = ConvertModifiers(node.Modifiers, TokenContext.Local);
				if (modifiers.Count == 0)
					modifiers = modifiers.Add(SyntaxFactory.Token(SyntaxKind.DimKeyword));
                return SyntaxFactory.SingletonList<StatementSyntax>(
					SyntaxFactory.LocalDeclarationStatement(
						modifiers,
						RemodelVariableDeclaration(node.Declaration, nodesVisitor)
                    )
				);
			}

            public override SyntaxList<StatementSyntax> VisitExpressionStatement(CSS.ExpressionStatementSyntax node)
			{
				var exprNode = node.Expression.Accept(nodesVisitor);
				if (!(exprNode is StatementSyntax))
					exprNode = SyntaxFactory.ExpressionStatement((ExpressionSyntax)exprNode);

				return SyntaxFactory.SingletonList((StatementSyntax)exprNode);
			}

			public override SyntaxList<StatementSyntax> VisitIfStatement(CSS.IfStatementSyntax node)
			{
				StatementSyntax stmt;
                var elseIfBlocks = new List<ElseIfBlockSyntax>();
                ElseBlockSyntax elseBlock = null;
                CollectElseBlocks(node, elseIfBlocks, ref elseBlock);
                if (node.Statement is CSS.BlockSyntax)
				{
                    var b = (CSS.BlockSyntax)node.Statement;
                    stmt = SyntaxFactory.MultiLineIfBlock(
                        SyntaxFactory.IfStatement((ExpressionSyntax)node.Condition.Accept(nodesVisitor)).WithThenKeyword(SyntaxFactory.Token(SyntaxKind.ThenKeyword)),
                        SyntaxFactory.List(b.Statements.SelectMany(s => s.Accept(this))),
                        SyntaxFactory.List(elseIfBlocks),
                        elseBlock
                    );
                }
				else
				{
                    if (elseIfBlocks.Any())
                    {
                        stmt = SyntaxFactory.MultiLineIfBlock(
                             SyntaxFactory.IfStatement((ExpressionSyntax)node.Condition.Accept(nodesVisitor)).WithThenKeyword(SyntaxFactory.Token(SyntaxKind.ThenKeyword)),
                             node.Statement.Accept(this),
                             SyntaxFactory.List(elseIfBlocks),
                             elseBlock
                         );
                    }
                    else
                    {
                        stmt = SyntaxFactory.SingleLineIfStatement(
                            (ExpressionSyntax)node.Condition.Accept(nodesVisitor),
                            node.Statement.Accept(this),
                            SyntaxFactory.SingleLineElseClause(elseBlock.Statements)
                        ).WithThenKeyword(SyntaxFactory.Token(SyntaxKind.ThenKeyword));
                    }
                }
                return SyntaxFactory.SingletonList<StatementSyntax>(stmt);
			}

            void CollectElseBlocks(CSS.IfStatementSyntax node, List<ElseIfBlockSyntax> elseIfBlocks, ref ElseBlockSyntax elseBlock)
            {
                if (node.Else == null) return;
                if (node.Else.Statement is CSS.IfStatementSyntax)
                {
                    var elseIf = (CSS.IfStatementSyntax)node.Else.Statement;
                    if (elseIf.Statement is CSS.BlockSyntax)
                    {
                        var block = (CSS.BlockSyntax)elseIf.Statement;
                        elseIfBlocks.Add(
                            SyntaxFactory.ElseIfBlock(
                                SyntaxFactory.ElseIfStatement((ExpressionSyntax)elseIf.Condition.Accept(nodesVisitor)).WithThenKeyword(SyntaxFactory.Token(SyntaxKind.ThenKeyword)),
                                SyntaxFactory.List(block.Statements.SelectMany(s => s.Accept(this)))
                            )
                        );
                    }
                    else
                    {
                        elseIfBlocks.Add(
                            SyntaxFactory.ElseIfBlock(
                                SyntaxFactory.ElseIfStatement((ExpressionSyntax)elseIf.Condition.Accept(nodesVisitor)).WithThenKeyword(SyntaxFactory.Token(SyntaxKind.ThenKeyword)),
                                elseIf.Statement.Accept(this)
                            )
                        );
                    }
                    CollectElseBlocks(elseIf, elseIfBlocks, ref elseBlock);
                }
                else if (node.Else.Statement is CSS.BlockSyntax)
                {
                    var block = (CSS.BlockSyntax)node.Else.Statement;
                    elseBlock = SyntaxFactory.ElseBlock(SyntaxFactory.List(block.Statements.SelectMany(s => s.Accept(this))));
                }
                else
                {
                    elseBlock = SyntaxFactory.ElseBlock(SyntaxFactory.List(node.Else.Statement.Accept(this)));
                }
            }

            public override SyntaxList<StatementSyntax> VisitUsingStatement(CSS.UsingStatementSyntax node)
			{
				var stmt = SyntaxFactory.UsingStatement(
					(ExpressionSyntax)node.Expression?.Accept(nodesVisitor),
                    RemodelVariableDeclaration(node.Declaration, nodesVisitor)
                );
				SyntaxList<StatementSyntax> list;
				if (node.Statement is CSS.BlockSyntax)
				{
					var b = (CSS.BlockSyntax)node.Statement;
					list = SyntaxFactory.List(b.Statements.SelectMany(s => s.Accept(this)));
				}
				else
				{
					list = node.Statement.Accept(this);
				}
				return SyntaxFactory.SingletonList<StatementSyntax>(SyntaxFactory.UsingBlock(stmt, list));
			}

            public override SyntaxList<StatementSyntax> VisitReturnStatement(CSS.ReturnStatementSyntax node)
            {
                StatementSyntax stmt;
                if (node.Expression == null)
                    stmt = SyntaxFactory.ReturnStatement();
                else
                    stmt = SyntaxFactory.ReturnStatement((ExpressionSyntax)node.Expression.Accept(nodesVisitor));
                return SyntaxFactory.SingletonList(stmt);
            }

            public override SyntaxList<StatementSyntax> VisitCheckedStatement(CSS.CheckedStatementSyntax node)
			{
				return WrapInComment(Visit(node.Block), "Visual Basic does not support checked statement!");
			}

			private SyntaxList<StatementSyntax> WrapInComment(SyntaxList<StatementSyntax> nodes, string comment)
			{
				if (nodes.Count > 0)
				{
					nodes = nodes.Replace(nodes[0], nodes[0].WithPrependedLeadingTrivia(SyntaxFactory.CommentTrivia("BEGIN TODO : " + comment)));
					nodes = nodes.Replace(nodes.Last(), nodes.Last().WithAppendedTrailingTrivia(SyntaxFactory.CommentTrivia("END TODO : " + comment)));
				}

				return nodes;
			}
		}

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
				break;
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
				break;
			case CS.SyntaxKind.SealedKeyword:
				break;
			case CS.SyntaxKind.ConstKeyword:
				break;
			case CS.SyntaxKind.FixedKeyword:
				break;
			case CS.SyntaxKind.StackAllocKeyword:
				break;
			case CS.SyntaxKind.VolatileKeyword:
				break;
			case CS.SyntaxKind.NewKeyword:
				break;
			case CS.SyntaxKind.OverrideKeyword:
				break;
			case CS.SyntaxKind.AbstractKeyword:
				return context == TokenContext.Global ? SyntaxKind.MustInheritKeyword : SyntaxKind.MustOverrideKeyword;
			case CS.SyntaxKind.VirtualKeyword:
				break;
			case CS.SyntaxKind.UnsafeKeyword:
				break;
			case CS.SyntaxKind.ExternKeyword:
				break;
			case CS.SyntaxKind.RefKeyword:
                return SyntaxKind.ByRefKeyword;
			case CS.SyntaxKind.OutKeyword:
                return SyntaxKind.ByRefKeyword;
			case CS.SyntaxKind.InKeyword:
				break;
			case CS.SyntaxKind.IsKeyword:
				break;
			case CS.SyntaxKind.AsKeyword:
				break;
			case CS.SyntaxKind.ParamsKeyword:
				break;
			case CS.SyntaxKind.ArgListKeyword:
				break;
			case CS.SyntaxKind.MakeRefKeyword:
				break;
			case CS.SyntaxKind.RefTypeKeyword:
				break;
			case CS.SyntaxKind.RefValueKeyword:
				break;
			case CS.SyntaxKind.OperatorKeyword:
				break;
			case CS.SyntaxKind.ExplicitKeyword:
				break;
			case CS.SyntaxKind.ImplicitKeyword:
				break;
			case CS.SyntaxKind.YieldKeyword:
				break;
			case CS.SyntaxKind.PartialKeyword:
				break;
			case CS.SyntaxKind.AliasKeyword:
				break;
			case CS.SyntaxKind.GlobalKeyword:
				break;
			case CS.SyntaxKind.AssemblyKeyword:
				break;
			case CS.SyntaxKind.ModuleKeyword:
				break;
			case CS.SyntaxKind.TypeKeyword:
				break;
			case CS.SyntaxKind.FieldKeyword:
				break;
			case CS.SyntaxKind.MethodKeyword:
				break;
			case CS.SyntaxKind.ParamKeyword:
				break;
			case CS.SyntaxKind.PropertyKeyword:
				break;
			case CS.SyntaxKind.TypeVarKeyword:
				break;
			case CS.SyntaxKind.GetKeyword:
				break;
			case CS.SyntaxKind.SetKeyword:
				break;
			case CS.SyntaxKind.AddKeyword:
				break;
			case CS.SyntaxKind.RemoveKeyword:
				break;
			case CS.SyntaxKind.WhereKeyword:
				break;
			case CS.SyntaxKind.FromKeyword:
				break;
			case CS.SyntaxKind.GroupKeyword:
				break;
			case CS.SyntaxKind.JoinKeyword:
				break;
			case CS.SyntaxKind.IntoKeyword:
				break;
			case CS.SyntaxKind.LetKeyword:
				break;
			case CS.SyntaxKind.ByKeyword:
				break;
			case CS.SyntaxKind.SelectKeyword:
				break;
			case CS.SyntaxKind.OrderByKeyword:
				break;
			case CS.SyntaxKind.OnKeyword:
				break;
			case CS.SyntaxKind.EqualsKeyword:
				break;
			case CS.SyntaxKind.AscendingKeyword:
				break;
			case CS.SyntaxKind.DescendingKeyword:
				break;
			case CS.SyntaxKind.NameOfKeyword:
				break;
			case CS.SyntaxKind.AsyncKeyword:
				break;
			case CS.SyntaxKind.AwaitKeyword:
				break;
			case CS.SyntaxKind.WhenKeyword:
				break;
			case CS.SyntaxKind.ElifKeyword:
				break;
			case CS.SyntaxKind.EndIfKeyword:
				break;
			case CS.SyntaxKind.RegionKeyword:
				break;
			case CS.SyntaxKind.EndRegionKeyword:
				break;
			case CS.SyntaxKind.DefineKeyword:
				break;
			case CS.SyntaxKind.UndefKeyword:
				break;
			case CS.SyntaxKind.WarningKeyword:
				break;
			case CS.SyntaxKind.ErrorKeyword:
				break;
			case CS.SyntaxKind.LineKeyword:
				break;
			case CS.SyntaxKind.PragmaKeyword:
				break;
			case CS.SyntaxKind.HiddenKeyword:
				break;
			case CS.SyntaxKind.ChecksumKeyword:
				break;
			case CS.SyntaxKind.DisableKeyword:
				break;
			case CS.SyntaxKind.RestoreKeyword:
				break;
			case CS.SyntaxKind.ReferenceKeyword:
				break;
			case CS.SyntaxKind.InterpolatedStringStartToken:
				break;
			case CS.SyntaxKind.InterpolatedStringEndToken:
				break;
			case CS.SyntaxKind.InterpolatedVerbatimStringStartToken:
				break;
			case CS.SyntaxKind.OmittedTypeArgumentToken:
				break;
			case CS.SyntaxKind.OmittedArraySizeExpressionToken:
				break;
			case CS.SyntaxKind.EndOfDirectiveToken:
				break;
			case CS.SyntaxKind.EndOfDocumentationCommentToken:
				break;
			case CS.SyntaxKind.EndOfFileToken:
				break;
			case CS.SyntaxKind.BadToken:
				break;
			case CS.SyntaxKind.IdentifierToken:
				break;
			case CS.SyntaxKind.NumericLiteralToken:
				break;
			case CS.SyntaxKind.CharacterLiteralToken:
				break;
			case CS.SyntaxKind.StringLiteralToken:
				break;
			case CS.SyntaxKind.XmlEntityLiteralToken:
				break;
			case CS.SyntaxKind.XmlTextLiteralToken:
				break;
			case CS.SyntaxKind.XmlTextLiteralNewLineToken:
				break;
			case CS.SyntaxKind.InterpolatedStringToken:
				break;
			case CS.SyntaxKind.InterpolatedStringTextToken:
				break;
			case CS.SyntaxKind.TypeCref:
				break;
			case CS.SyntaxKind.QualifiedCref:
				break;
			case CS.SyntaxKind.NameMemberCref:
				break;
			case CS.SyntaxKind.IndexerMemberCref:
				break;
			case CS.SyntaxKind.OperatorMemberCref:
				break;
			case CS.SyntaxKind.ConversionOperatorMemberCref:
				break;
			case CS.SyntaxKind.CrefParameterList:
				break;
			case CS.SyntaxKind.CrefBracketedParameterList:
				break;
			case CS.SyntaxKind.CrefParameter:
				break;
			case CS.SyntaxKind.IdentifierName:
				break;
			case CS.SyntaxKind.QualifiedName:
				break;
			case CS.SyntaxKind.GenericName:
				break;
			case CS.SyntaxKind.TypeArgumentList:
				break;
			case CS.SyntaxKind.AliasQualifiedName:
				break;
			case CS.SyntaxKind.PredefinedType:
				break;
			case CS.SyntaxKind.ArrayType:
				break;
			case CS.SyntaxKind.ArrayRankSpecifier:
				break;
			case CS.SyntaxKind.PointerType:
				break;
			case CS.SyntaxKind.NullableType:
				break;
			case CS.SyntaxKind.OmittedTypeArgument:
				break;
			case CS.SyntaxKind.ParenthesizedExpression:
				break;
			case CS.SyntaxKind.ConditionalExpression:
				break;
			case CS.SyntaxKind.InvocationExpression:
				break;
			case CS.SyntaxKind.ElementAccessExpression:
				break;
			case CS.SyntaxKind.ArgumentList:
				break;
			case CS.SyntaxKind.BracketedArgumentList:
				break;
			case CS.SyntaxKind.Argument:
				break;
			case CS.SyntaxKind.NameColon:
				break;
			case CS.SyntaxKind.CastExpression:
				break;
			case CS.SyntaxKind.AnonymousMethodExpression:
				break;
			case CS.SyntaxKind.SimpleLambdaExpression:
				break;
			case CS.SyntaxKind.ParenthesizedLambdaExpression:
				break;
			case CS.SyntaxKind.ObjectInitializerExpression:
				break;
			case CS.SyntaxKind.CollectionInitializerExpression:
				break;
			case CS.SyntaxKind.ArrayInitializerExpression:
				break;
			case CS.SyntaxKind.AnonymousObjectMemberDeclarator:
				break;
			case CS.SyntaxKind.ComplexElementInitializerExpression:
				break;
			case CS.SyntaxKind.ObjectCreationExpression:
				break;
			case CS.SyntaxKind.AnonymousObjectCreationExpression:
				break;
			case CS.SyntaxKind.ArrayCreationExpression:
				break;
			case CS.SyntaxKind.ImplicitArrayCreationExpression:
				break;
			case CS.SyntaxKind.StackAllocArrayCreationExpression:
				break;
			case CS.SyntaxKind.OmittedArraySizeExpression:
				break;
			case CS.SyntaxKind.InterpolatedStringExpression:
				break;
			case CS.SyntaxKind.ImplicitElementAccess:
				break;
			case CS.SyntaxKind.AddExpression:
				break;
			case CS.SyntaxKind.SubtractExpression:
				break;
			case CS.SyntaxKind.MultiplyExpression:
				break;
			case CS.SyntaxKind.DivideExpression:
				break;
			case CS.SyntaxKind.ModuloExpression:
				break;
			case CS.SyntaxKind.LeftShiftExpression:
				break;
			case CS.SyntaxKind.RightShiftExpression:
				break;
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
			case CS.SyntaxKind.IsExpression:
				break;
			case CS.SyntaxKind.AsExpression:
				break;
			case CS.SyntaxKind.CoalesceExpression:
				break;
			case CS.SyntaxKind.SimpleMemberAccessExpression:
				break;
			case CS.SyntaxKind.PointerMemberAccessExpression:
				break;
			case CS.SyntaxKind.ConditionalAccessExpression:
				break;
			case CS.SyntaxKind.MemberBindingExpression:
				break;
			case CS.SyntaxKind.ElementBindingExpression:
				break;
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
				break;
			case CS.SyntaxKind.AndAssignmentExpression:
				break;
			case CS.SyntaxKind.ExclusiveOrAssignmentExpression:
				break;
			case CS.SyntaxKind.OrAssignmentExpression:
				break;
			case CS.SyntaxKind.LeftShiftAssignmentExpression:
				break;
			case CS.SyntaxKind.RightShiftAssignmentExpression:
				break;
			case CS.SyntaxKind.UnaryPlusExpression:
				break;
			case CS.SyntaxKind.UnaryMinusExpression:
				break;
			case CS.SyntaxKind.BitwiseNotExpression:
				break;
			case CS.SyntaxKind.LogicalNotExpression:
				break;
			case CS.SyntaxKind.PreIncrementExpression:
                return SyntaxKind.AddAssignmentStatement;
            case CS.SyntaxKind.PreDecrementExpression:
                return SyntaxKind.SubtractAssignmentStatement;
            case CS.SyntaxKind.PointerIndirectionExpression:
				break;
			case CS.SyntaxKind.AddressOfExpression:
				break;
			case CS.SyntaxKind.PostIncrementExpression:
                return SyntaxKind.AddAssignmentStatement;
			case CS.SyntaxKind.PostDecrementExpression:
                return SyntaxKind.SubtractAssignmentStatement;
            case CS.SyntaxKind.AwaitExpression:
				break;
			case CS.SyntaxKind.ThisExpression:
				break;
			case CS.SyntaxKind.BaseExpression:
				break;
			case CS.SyntaxKind.ArgListExpression:
				break;
			case CS.SyntaxKind.NumericLiteralExpression:
				break;
			case CS.SyntaxKind.StringLiteralExpression:
				break;
			case CS.SyntaxKind.CharacterLiteralExpression:
				break;
			case CS.SyntaxKind.TrueLiteralExpression:
				break;
			case CS.SyntaxKind.FalseLiteralExpression:
				break;
			case CS.SyntaxKind.NullLiteralExpression:
				break;
			case CS.SyntaxKind.TypeOfExpression:
				break;
			case CS.SyntaxKind.SizeOfExpression:
				break;
			case CS.SyntaxKind.CheckedExpression:
				break;
			case CS.SyntaxKind.UncheckedExpression:
				break;
			case CS.SyntaxKind.DefaultExpression:
				break;
			case CS.SyntaxKind.MakeRefExpression:
				break;
			case CS.SyntaxKind.RefValueExpression:
				break;
			case CS.SyntaxKind.RefTypeExpression:
				break;
			case CS.SyntaxKind.QueryExpression:
				break;
			case CS.SyntaxKind.QueryBody:
				break;
			case CS.SyntaxKind.FromClause:
				break;
			case CS.SyntaxKind.LetClause:
				break;
			case CS.SyntaxKind.JoinClause:
				break;
			case CS.SyntaxKind.JoinIntoClause:
				break;
			case CS.SyntaxKind.WhereClause:
				break;
			case CS.SyntaxKind.OrderByClause:
				break;
			case CS.SyntaxKind.AscendingOrdering:
				break;
			case CS.SyntaxKind.DescendingOrdering:
				break;
			case CS.SyntaxKind.SelectClause:
				break;
			case CS.SyntaxKind.GroupClause:
				break;
			case CS.SyntaxKind.QueryContinuation:
				break;
			case CS.SyntaxKind.Block:
				break;
			case CS.SyntaxKind.LocalDeclarationStatement:
				break;
			case CS.SyntaxKind.VariableDeclaration:
				break;
			case CS.SyntaxKind.VariableDeclarator:
				break;
			case CS.SyntaxKind.EqualsValueClause:
				break;
			case CS.SyntaxKind.ExpressionStatement:
				break;
			case CS.SyntaxKind.EmptyStatement:
				break;
			case CS.SyntaxKind.LabeledStatement:
				break;
			case CS.SyntaxKind.GotoStatement:
				break;
			case CS.SyntaxKind.GotoCaseStatement:
				break;
			case CS.SyntaxKind.GotoDefaultStatement:
				break;
			case CS.SyntaxKind.BreakStatement:
				break;
			case CS.SyntaxKind.ContinueStatement:
				break;
			case CS.SyntaxKind.ReturnStatement:
				break;
			case CS.SyntaxKind.YieldReturnStatement:
				break;
			case CS.SyntaxKind.YieldBreakStatement:
				break;
			case CS.SyntaxKind.ThrowStatement:
				break;
			case CS.SyntaxKind.WhileStatement:
				break;
			case CS.SyntaxKind.DoStatement:
				break;
			case CS.SyntaxKind.ForStatement:
				break;
			case CS.SyntaxKind.ForEachStatement:
				break;
			case CS.SyntaxKind.UsingStatement:
				break;
			case CS.SyntaxKind.FixedStatement:
				break;
			case CS.SyntaxKind.CheckedStatement:
				break;
			case CS.SyntaxKind.UncheckedStatement:
				break;
			case CS.SyntaxKind.UnsafeStatement:
				break;
			case CS.SyntaxKind.LockStatement:
				break;
			case CS.SyntaxKind.IfStatement:
				break;
			case CS.SyntaxKind.ElseClause:
				break;
			case CS.SyntaxKind.SwitchStatement:
				break;
			case CS.SyntaxKind.SwitchSection:
				break;
			case CS.SyntaxKind.CaseSwitchLabel:
				break;
			case CS.SyntaxKind.DefaultSwitchLabel:
				break;
			case CS.SyntaxKind.TryStatement:
				break;
			case CS.SyntaxKind.CatchClause:
				break;
			case CS.SyntaxKind.CatchDeclaration:
				break;
			case CS.SyntaxKind.CatchFilterClause:
				break;
			case CS.SyntaxKind.FinallyClause:
				break;
			case CS.SyntaxKind.CompilationUnit:
				break;
			case CS.SyntaxKind.GlobalStatement:
				break;
			case CS.SyntaxKind.NamespaceDeclaration:
				break;
			case CS.SyntaxKind.UsingDirective:
				break;
			case CS.SyntaxKind.ExternAliasDirective:
				break;
			case CS.SyntaxKind.AttributeList:
				break;
			case CS.SyntaxKind.AttributeTargetSpecifier:
				break;
			case CS.SyntaxKind.Attribute:
				break;
			case CS.SyntaxKind.AttributeArgumentList:
				break;
			case CS.SyntaxKind.AttributeArgument:
				break;
			case CS.SyntaxKind.NameEquals:
				break;
			case CS.SyntaxKind.ClassDeclaration:
				break;
			case CS.SyntaxKind.StructDeclaration:
				break;
			case CS.SyntaxKind.InterfaceDeclaration:
				break;
			case CS.SyntaxKind.EnumDeclaration:
				break;
			case CS.SyntaxKind.DelegateDeclaration:
				break;
			case CS.SyntaxKind.BaseList:
				break;
			case CS.SyntaxKind.SimpleBaseType:
				break;
			case CS.SyntaxKind.TypeParameterConstraintClause:
				break;
			case CS.SyntaxKind.ConstructorConstraint:
				break;
			case CS.SyntaxKind.ClassConstraint:
				break;
			case CS.SyntaxKind.StructConstraint:
				break;
			case CS.SyntaxKind.TypeConstraint:
				break;
			case CS.SyntaxKind.ExplicitInterfaceSpecifier:
				break;
			case CS.SyntaxKind.EnumMemberDeclaration:
				break;
			case CS.SyntaxKind.FieldDeclaration:
				break;
			case CS.SyntaxKind.EventFieldDeclaration:
				break;
			case CS.SyntaxKind.MethodDeclaration:
				break;
			case CS.SyntaxKind.OperatorDeclaration:
				break;
			case CS.SyntaxKind.ConversionOperatorDeclaration:
				break;
			case CS.SyntaxKind.ConstructorDeclaration:
				break;
			case CS.SyntaxKind.BaseConstructorInitializer:
				break;
			case CS.SyntaxKind.ThisConstructorInitializer:
				break;
			case CS.SyntaxKind.DestructorDeclaration:
				break;
			case CS.SyntaxKind.PropertyDeclaration:
				break;
			case CS.SyntaxKind.EventDeclaration:
				break;
			case CS.SyntaxKind.IndexerDeclaration:
				break;
			case CS.SyntaxKind.AccessorList:
				break;
			case CS.SyntaxKind.GetAccessorDeclaration:
				break;
			case CS.SyntaxKind.SetAccessorDeclaration:
				break;
			case CS.SyntaxKind.AddAccessorDeclaration:
				break;
			case CS.SyntaxKind.RemoveAccessorDeclaration:
				break;
			case CS.SyntaxKind.UnknownAccessorDeclaration:
				break;
			case CS.SyntaxKind.ParameterList:
				break;
			case CS.SyntaxKind.BracketedParameterList:
				break;
			case CS.SyntaxKind.Parameter:
				break;
			case CS.SyntaxKind.TypeParameterList:
				break;
			case CS.SyntaxKind.TypeParameter:
				break;
			case CS.SyntaxKind.IncompleteMember:
				break;
			case CS.SyntaxKind.ArrowExpressionClause:
				break;
			case CS.SyntaxKind.Interpolation:
				break;
			case CS.SyntaxKind.InterpolatedStringText:
				break;
			case CS.SyntaxKind.InterpolationAlignmentClause:
				break;
			case CS.SyntaxKind.InterpolationFormatClause:
				break;
            case CS.SyntaxKind.PlusPlusToken:
                return SyntaxKind.PlusToken;
            case CS.SyntaxKind.MinusMinusToken:
                return SyntaxKind.MinusToken;
            default:
				throw new ArgumentOutOfRangeException();
			}

			throw new NotSupportedException(t + " is not supported!");
		}
	}
}
