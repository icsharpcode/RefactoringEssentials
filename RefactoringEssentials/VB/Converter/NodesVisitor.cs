﻿using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.VisualBasic;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using CS = Microsoft.CodeAnalysis.CSharp;
using CSS = Microsoft.CodeAnalysis.CSharp.Syntax;

namespace RefactoringEssentials.VB.Converter
{
	public partial class CSharpConverter
    {
        class NodesVisitor : CS.CSharpSyntaxVisitor<VisualBasicSyntaxNode>
        {
            SemanticModel semanticModel;
            Document targetDocument;
            VisualBasicCompilationOptions options;

            readonly List<CSS.BaseTypeDeclarationSyntax> inlineAssignHelperMarkers = new List<CSS.BaseTypeDeclarationSyntax>();
            readonly List<ImportsStatementSyntax> allImports = new List<ImportsStatementSyntax>();

            const string InlineAssignHelperCode = @"<Obsolete(""Please refactor code that uses this function, it is a simple work-around to simulate inline assignment in VB!"")>
Private Shared Function __InlineAssignHelper(Of T)(ByRef target As T, value As T) As T
target = value
Return value
End Function";

            int placeholder = 1;

            string GeneratePlaceholder(string v)
            {
                return $"__{v}{placeholder++}__";
            }

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

            IEnumerable<ImportsStatementSyntax> TidyImportsList(IEnumerable<ImportsStatementSyntax> allImports)
            {
                foreach (var import in allImports)
                    foreach (var clause in import.ImportsClauses)
                    {
                        if (ImportIsNecessary(clause))
                            yield return SyntaxFactory.ImportsStatement(SyntaxFactory.SingletonSeparatedList(clause));
                    }
            }

            bool ImportIsNecessary(ImportsClauseSyntax import)
            {
                if (import is SimpleImportsClauseSyntax)
                {
                    var i = (SimpleImportsClauseSyntax)import;
                    if (i.Alias != null)
                        return true;
                    return options?.GlobalImports.Any(g => i.ToString().Equals(g.Clause.ToString(), StringComparison.OrdinalIgnoreCase)) != true;
                }
                return true;
            }

            public NodesVisitor(SemanticModel semanticModel, Document targetDocument)
            {
                this.semanticModel = semanticModel;
                this.targetDocument = targetDocument;
                this.options = (VisualBasicCompilationOptions)targetDocument?.Project.CompilationOptions;
            }

            public override VisualBasicSyntaxNode DefaultVisit(SyntaxNode node)
            {
                throw new NotImplementedException(node.GetType() + " not implemented!");
            }

            public override VisualBasicSyntaxNode VisitCompilationUnit(CSS.CompilationUnitSyntax node)
            {
                foreach (var @using in node.Usings)
                    @using.Accept(this);
                foreach (var @extern in node.Externs)
                    @extern.Accept(this);
                var attributes = SyntaxFactory.List(node.AttributeLists.Select(a => SyntaxFactory.AttributesStatement(SyntaxFactory.SingletonList((AttributeListSyntax)a.Accept(this)))));
                var members = SyntaxFactory.List(node.Members.Select(m => (StatementSyntax)m.Accept(this)));

                return SyntaxFactory.CompilationUnit(
                    SyntaxFactory.List<OptionStatementSyntax>(),
                    SyntaxFactory.List(TidyImportsList(allImports)),
                    attributes,
                    members
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
                return SyntaxFactory.Attribute((AttributeTargetSyntax)list.Target?.Accept(this), (TypeSyntax)node.Name.Accept(this), (ArgumentListSyntax)node.ArgumentList?.Accept(this));
            }

            public override VisualBasicSyntaxNode VisitAttributeTargetSpecifier(CSS.AttributeTargetSpecifierSyntax node)
            {
                SyntaxToken id;
                switch (CS.CSharpExtensions.Kind(node.Identifier))
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
                foreach (var @using in node.Usings)
                    @using.Accept(this);
                foreach (var @extern in node.Externs)
                    @extern.Accept(this);
                var members = node.Members.Select(m => (StatementSyntax)m.Accept(this));

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
                    var id = ConvertIdentifier(name.Identifier);
                    alias = SyntaxFactory.ImportAliasClause(id);
                }
                ImportsClauseSyntax clause = SyntaxFactory.SimpleImportsClause(alias, (NameSyntax)node.Name.Accept(this));
                var import = SyntaxFactory.ImportsStatement(SyntaxFactory.SingletonSeparatedList(clause));
                allImports.Add(import);
                return null;
            }

            #region Namespace Members

            public override VisualBasicSyntaxNode VisitClassDeclaration(CSS.ClassDeclarationSyntax node)
            {
                var members = node.Members.Select(m => (StatementSyntax)m.Accept(this)).ToList();
                var id = ConvertIdentifier(node.Identifier);

                List<InheritsStatementSyntax> inherits = new List<InheritsStatementSyntax>();
                List<ImplementsStatementSyntax> implements = new List<ImplementsStatementSyntax>();
                ConvertBaseList(node, inherits, implements);
                members.AddRange(PatchInlineHelpers(node));
                if (node.Modifiers.Any(CS.SyntaxKind.StaticKeyword))
                {
                    return SyntaxFactory.ModuleBlock(
                        SyntaxFactory.ModuleStatement(
                            SyntaxFactory.List(node.AttributeLists.Select(a => (AttributeListSyntax)a.Accept(this))),
                            ConvertModifiers(node.Modifiers, TokenContext.InterfaceOrModule),
                            id, (TypeParameterListSyntax)node.TypeParameterList?.Accept(this)
                        ),
                        SyntaxFactory.List(inherits),
                        SyntaxFactory.List(implements),
                        SyntaxFactory.List(members)
                    );
                }
                else
                {
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
            }

            public override VisualBasicSyntaxNode VisitStructDeclaration(CSS.StructDeclarationSyntax node)
            {
                var members = node.Members.Select(m => (StatementSyntax)m.Accept(this)).ToList();

                List<InheritsStatementSyntax> inherits = new List<InheritsStatementSyntax>();
                List<ImplementsStatementSyntax> implements = new List<ImplementsStatementSyntax>();
                ConvertBaseList(node, inherits, implements);
                members.AddRange(PatchInlineHelpers(node));

                return SyntaxFactory.StructureBlock(
                    SyntaxFactory.StructureStatement(
                        SyntaxFactory.List(node.AttributeLists.Select(a => (AttributeListSyntax)a.Accept(this))),
                        ConvertModifiers(node.Modifiers),
                        ConvertIdentifier(node.Identifier),
                        (TypeParameterListSyntax)node.TypeParameterList?.Accept(this)
                    ),
                    SyntaxFactory.List(inherits),
                    SyntaxFactory.List(implements),
                    SyntaxFactory.List(members)
                );
            }

            public override VisualBasicSyntaxNode VisitInterfaceDeclaration(CSS.InterfaceDeclarationSyntax node)
            {
                var members = node.Members.Select(m => (StatementSyntax)m.Accept(this)).ToArray();

                List<InheritsStatementSyntax> inherits = new List<InheritsStatementSyntax>();
                List<ImplementsStatementSyntax> implements = new List<ImplementsStatementSyntax>();
                ConvertBaseList(node, inherits, implements);

                return SyntaxFactory.InterfaceBlock(
                    SyntaxFactory.InterfaceStatement(
                        SyntaxFactory.List(node.AttributeLists.Select(a => (AttributeListSyntax)a.Accept(this))),
                        ConvertModifiers(node.Modifiers, TokenContext.InterfaceOrModule),
                        ConvertIdentifier(node.Identifier),
                        (TypeParameterListSyntax)node.TypeParameterList?.Accept(this)
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
                return SyntaxFactory.EnumBlock(
                    SyntaxFactory.EnumStatement(
                        SyntaxFactory.List(node.AttributeLists.Select(a => (AttributeListSyntax)a.Accept(this))),
                        ConvertModifiers(node.Modifiers),
                        ConvertIdentifier(node.Identifier),
                        baseType == null ? null : SyntaxFactory.SimpleAsClause(baseType)
                    ),
                    SyntaxFactory.List(members)
                );
            }

            public override VisualBasicSyntaxNode VisitEnumMemberDeclaration(CSS.EnumMemberDeclarationSyntax node)
            {
                var initializer = (ExpressionSyntax)node.EqualsValue?.Value.Accept(this);
                return SyntaxFactory.EnumMemberDeclaration(
                    SyntaxFactory.List(node.AttributeLists.Select(a => (AttributeListSyntax)a.Accept(this))),
                    ConvertIdentifier(node.Identifier),
                    initializer == null ? null : SyntaxFactory.EqualsValue(initializer)
                );
            }

            public override VisualBasicSyntaxNode VisitDelegateDeclaration(CSS.DelegateDeclarationSyntax node)
            {
                var id = ConvertIdentifier(node.Identifier);
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
                var modifiers = ConvertModifiers(node.Modifiers, TokenContext.VariableOrConst);
                if (modifiers.Count == 0)
                    modifiers = modifiers.Add(SyntaxFactory.Token(SyntaxKind.PrivateKeyword));
                return SyntaxFactory.FieldDeclaration(
                    SyntaxFactory.List(node.AttributeLists.Select(a => (AttributeListSyntax)a.Accept(this))),
                    modifiers,
                    RemodelVariableDeclaration(node.Declaration, this)
                );
            }

            public override VisualBasicSyntaxNode VisitConstructorDeclaration(CSS.ConstructorDeclarationSyntax node)
            {
                return SyntaxFactory.ConstructorBlock(
                    SyntaxFactory.SubNewStatement(
                        SyntaxFactory.List(node.AttributeLists.Select(a => (AttributeListSyntax)a.Accept(this))),
                        ConvertModifiers(node.Modifiers, TokenContext.Member),
                        (ParameterListSyntax)node.ParameterList?.Accept(this)
                    ),
                    SyntaxFactory.List(node.Body.Statements.SelectMany(s => s.Accept(new MethodBodyVisitor(semanticModel, this))))
                );
            }

            public override VisualBasicSyntaxNode VisitDestructorDeclaration(CSS.DestructorDeclarationSyntax node)
            {
                return SyntaxFactory.SubBlock(
                    SyntaxFactory.SubStatement(
                        SyntaxFactory.List(node.AttributeLists.Select(a => (AttributeListSyntax)a.Accept(this))),
                        SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.ProtectedKeyword), SyntaxFactory.Token(SyntaxKind.OverridesKeyword)),
                        SyntaxFactory.Identifier("Finalize"), null,
                        (ParameterListSyntax)node.ParameterList?.Accept(this),
                        null, null, null
                    ),
                    SyntaxFactory.List(node.Body.Statements.SelectMany(s => s.Accept(new MethodBodyVisitor(semanticModel, this))))
                );
            }

            public override VisualBasicSyntaxNode VisitMethodDeclaration(CSS.MethodDeclarationSyntax node)
            {
                SyntaxList<StatementSyntax>? block = null;
                var visitor = new MethodBodyVisitor(semanticModel, this);
                if (node.Body != null)
                {
                    block = SyntaxFactory.List(node.Body.Statements.SelectMany(s => s.Accept(visitor)));
                }
                if (node.ExpressionBody != null)
                {
                    block = SyntaxFactory.SingletonList<StatementSyntax>(
                        SyntaxFactory.ReturnStatement((ExpressionSyntax)node.ExpressionBody.Expression.Accept(this))
                    );
                }
                if (node.Modifiers.Any(m => m.IsKind(CS.SyntaxKind.ExternKeyword)))
                {
                    block = SyntaxFactory.List<StatementSyntax>();
                }
                var id = ConvertIdentifier(node.Identifier);
                var methodInfo = semanticModel.GetDeclaredSymbol(node);
                var containingType = methodInfo?.ContainingType;
                var attributes = SyntaxFactory.List(node.AttributeLists.Select(a => (AttributeListSyntax)a.Accept(this)));
                var parameterList = (ParameterListSyntax)node.ParameterList?.Accept(this);
                var modifiers = ConvertModifiers(node.Modifiers, containingType?.IsInterfaceType() == true ? TokenContext.Local : TokenContext.Member);
                if (visitor.IsInterator)
                    modifiers = modifiers.Add(SyntaxFactory.Token(SyntaxKind.IteratorKeyword));
                if (node.ParameterList.Parameters.Count > 0 && node.ParameterList.Parameters[0].Modifiers.Any(CS.SyntaxKind.ThisKeyword))
                {
                    attributes = attributes.Insert(0, SyntaxFactory.AttributeList(SyntaxFactory.SingletonSeparatedList(SyntaxFactory.Attribute(null, SyntaxFactory.ParseTypeName("Extension"), SyntaxFactory.ArgumentList()))));
                    if (!((CS.CSharpSyntaxTree)node.SyntaxTree).HasUsingDirective("System.Runtime.CompilerServices"))
                        allImports.Add(SyntaxFactory.ImportsStatement(SyntaxFactory.SingletonSeparatedList<ImportsClauseSyntax>(SyntaxFactory.SimpleImportsClause(SyntaxFactory.ParseName("System.Runtime.CompilerServices")))));
                }
                if (containingType?.IsStatic == true)
                {
                    modifiers = SyntaxFactory.TokenList(modifiers.Where(t => !(t.IsKind(SyntaxKind.SharedKeyword, SyntaxKind.PublicKeyword))));
                }
                if (methodInfo?.GetReturnType()?.SpecialType == SpecialType.System_Void)
                {
                    var stmt = SyntaxFactory.SubStatement(
                        attributes,
                        modifiers,
                        id, (TypeParameterListSyntax)node.TypeParameterList?.Accept(this),
                        parameterList,
                        null, null, null
                    );
                    if (block == null)
                        return stmt;
                    return SyntaxFactory.SubBlock(stmt, block.Value);
                }
                else
                {
                    var stmt = SyntaxFactory.FunctionStatement(
                        attributes,
                        modifiers,
                        id, (TypeParameterListSyntax)node.TypeParameterList?.Accept(this),
                        parameterList,
                        SyntaxFactory.SimpleAsClause((TypeSyntax)node.ReturnType.Accept(this)), null, null
                    );
                    if (block == null)
                        return stmt;
                    return SyntaxFactory.FunctionBlock(stmt, block.Value);
                }
            }

            public override VisualBasicSyntaxNode VisitPropertyDeclaration(CSS.PropertyDeclarationSyntax node)
            {
                var id = ConvertIdentifier(node.Identifier);
                SyntaxList<AttributeListSyntax> attributes, returnAttributes;
                ConvertAndSplitAttributes(node.AttributeLists, out attributes, out returnAttributes);
                bool isIterator = false;
                List<AccessorBlockSyntax> accessors = new List<AccessorBlockSyntax>();
                if (node.ExpressionBody != null)
                {
                    accessors.Add(
                        SyntaxFactory.AccessorBlock(
                            SyntaxKind.GetAccessorBlock,
                            SyntaxFactory.GetAccessorStatement(),
                            SyntaxFactory.SingletonList<StatementSyntax>(SyntaxFactory.ReturnStatement((ExpressionSyntax)node.ExpressionBody.Expression.Accept(this))),
                            SyntaxFactory.EndGetStatement()
                        )
                    );
                }
                else
                {
                    foreach (var a in node.AccessorList?.Accessors)
                    {
                        bool _isIterator;
                        accessors.Add(ConvertAccessor(a, out _isIterator));
                        isIterator |= _isIterator;
                    }
                }
                var modifiers = ConvertModifiers(node.Modifiers, TokenContext.Member);
                if (isIterator) modifiers = modifiers.Add(SyntaxFactory.Token(SyntaxKind.IteratorKeyword));
                var stmt = SyntaxFactory.PropertyStatement(
                    attributes,
                    modifiers,
                    id, null,
                    SyntaxFactory.SimpleAsClause(returnAttributes, (TypeSyntax)node.Type.Accept(this)),
                    node.Initializer == null ? null : SyntaxFactory.EqualsValue((ExpressionSyntax)node.Initializer.Value.Accept(this)), null
                );

                if (node.AccessorList?.Accessors.All(a => a.Body == null) == true)
                    return stmt;
                return SyntaxFactory.PropertyBlock(stmt, SyntaxFactory.List(accessors));
            }

            public override VisualBasicSyntaxNode VisitIndexerDeclaration(CSS.IndexerDeclarationSyntax node)
            {
                var id = SyntaxFactory.Identifier("Item");
                SyntaxList<AttributeListSyntax> attributes, returnAttributes;
                ConvertAndSplitAttributes(node.AttributeLists, out attributes, out returnAttributes);
                bool isIterator = false;
                List<AccessorBlockSyntax> accessors = new List<AccessorBlockSyntax>();
                foreach (var a in node.AccessorList?.Accessors)
                {
                    bool _isIterator;
                    accessors.Add(ConvertAccessor(a, out _isIterator));
                    isIterator |= _isIterator;
                }
                var modifiers = ConvertModifiers(node.Modifiers, TokenContext.Member).Insert(0, SyntaxFactory.Token(SyntaxKind.DefaultKeyword));
                if (isIterator) modifiers = modifiers.Add(SyntaxFactory.Token(SyntaxKind.IteratorKeyword));
                var parameterList = (ParameterListSyntax)node.ParameterList?.Accept(this);
                var stmt = SyntaxFactory.PropertyStatement(
                    attributes,
                    modifiers,
                    id, parameterList,
                    SyntaxFactory.SimpleAsClause(returnAttributes, (TypeSyntax)node.Type.Accept(this)), null, null
                );
                if (node.AccessorList.Accessors.All(a => a.Body == null))
                    return stmt;
                return SyntaxFactory.PropertyBlock(stmt, SyntaxFactory.List(accessors));
            }

            public override VisualBasicSyntaxNode VisitEventDeclaration(CSS.EventDeclarationSyntax node)
            {
                SyntaxList<AttributeListSyntax> attributes, returnAttributes;
                ConvertAndSplitAttributes(node.AttributeLists, out attributes, out returnAttributes);
                var stmt = SyntaxFactory.EventStatement(
                    attributes,
                    ConvertModifiers(node.Modifiers, TokenContext.Member),
                    ConvertIdentifier(node.Identifier), null,
                    SyntaxFactory.SimpleAsClause(returnAttributes, (TypeSyntax)node.Type.Accept(this)), null
                );
                if (node.AccessorList.Accessors.All(a => a.Body == null))
                    return stmt;
                bool unused;
                var accessors = node.AccessorList?.Accessors.Select(a => ConvertAccessor(a, out unused)).ToArray();
                return SyntaxFactory.EventBlock(stmt, SyntaxFactory.List(accessors));
            }

            public override VisualBasicSyntaxNode VisitEventFieldDeclaration(CSS.EventFieldDeclarationSyntax node)
            {
                var decl = node.Declaration.Variables.Single();
                var id = SyntaxFactory.Identifier(decl.Identifier.ValueText, SyntaxFacts.IsKeywordKind(decl.Identifier.Kind()), decl.Identifier.GetIdentifierText(), TypeCharacter.None);
                SyntaxList<AttributeListSyntax> attributes, returnAttributes;
                ConvertAndSplitAttributes(node.AttributeLists, out attributes, out returnAttributes);
                return SyntaxFactory.EventStatement(attributes, ConvertModifiers(node.Modifiers, TokenContext.Member), id, null, SyntaxFactory.SimpleAsClause(returnAttributes, (TypeSyntax)node.Declaration.Type.Accept(this)), null);
            }

            private void ConvertAndSplitAttributes(SyntaxList<CSS.AttributeListSyntax> attributeLists, out SyntaxList<AttributeListSyntax> attributes, out SyntaxList<AttributeListSyntax> returnAttributes)
            {
                var retAttr = new List<AttributeListSyntax>();
                var attr = new List<AttributeListSyntax>();

                foreach (var attrList in attributeLists)
                {
                    if (attrList.Target?.Identifier.IsKind(CS.SyntaxKind.ReturnKeyword) == true)
                        retAttr.Add((AttributeListSyntax)attrList.Accept(this));
                    else
                        attr.Add((AttributeListSyntax)attrList.Accept(this));
                }
                returnAttributes = SyntaxFactory.List(retAttr);
                attributes = SyntaxFactory.List(attr);
            }

            AccessorBlockSyntax ConvertAccessor(CSS.AccessorDeclarationSyntax node, out bool isIterator)
            {
                SyntaxKind blockKind;
                AccessorStatementSyntax stmt;
                EndBlockStatementSyntax endStmt;
                SyntaxList<StatementSyntax> body = SyntaxFactory.List<StatementSyntax>();
                isIterator = false;
                if (node.Body != null)
                {
                    var visitor = new MethodBodyVisitor(semanticModel, this);
                    body = SyntaxFactory.List(node.Body.Statements.SelectMany(s => s.Accept(visitor)));
                    isIterator = visitor.IsInterator;
                }
                var attributes = SyntaxFactory.List(node.AttributeLists.Select(a => (AttributeListSyntax)a.Accept(this)));
                var modifiers = ConvertModifiers(node.Modifiers, TokenContext.Local);
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
                        valueParam = SyntaxFactory.Parameter(SyntaxFactory.ModifiedIdentifier("value"))
                            .WithAsClause(SyntaxFactory.SimpleAsClause((TypeSyntax)parent.Type.Accept(this)))
                            .WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.ByValKeyword)));
                        stmt = SyntaxFactory.SetAccessorStatement(attributes, modifiers, SyntaxFactory.ParameterList(SyntaxFactory.SingletonSeparatedList(valueParam)));
                        endStmt = SyntaxFactory.EndSetStatement();
                        break;
                    case CS.SyntaxKind.AddAccessorDeclaration:
                        blockKind = SyntaxKind.AddHandlerAccessorBlock;
                        valueParam = SyntaxFactory.Parameter(SyntaxFactory.ModifiedIdentifier("value"))
                            .WithAsClause(SyntaxFactory.SimpleAsClause((TypeSyntax)parent.Type.Accept(this)))
                            .WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.ByValKeyword)));
                        stmt = SyntaxFactory.AddHandlerAccessorStatement(attributes, modifiers, SyntaxFactory.ParameterList(SyntaxFactory.SingletonSeparatedList(valueParam)));
                        endStmt = SyntaxFactory.EndAddHandlerStatement();
                        break;
                    case CS.SyntaxKind.RemoveAccessorDeclaration:
                        blockKind = SyntaxKind.RemoveHandlerAccessorBlock;
                        valueParam = SyntaxFactory.Parameter(SyntaxFactory.ModifiedIdentifier("value"))
                            .WithAsClause(SyntaxFactory.SimpleAsClause((TypeSyntax)parent.Type.Accept(this)))
                            .WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.ByValKeyword)));
                        stmt = SyntaxFactory.RemoveHandlerAccessorStatement(attributes, modifiers, SyntaxFactory.ParameterList(SyntaxFactory.SingletonSeparatedList(valueParam)));
                        endStmt = SyntaxFactory.EndRemoveHandlerStatement();
                        break;
                    default:
                        throw new NotSupportedException();
                }
                return SyntaxFactory.AccessorBlock(blockKind, stmt, body, endStmt);
            }

            public override VisualBasicSyntaxNode VisitOperatorDeclaration(CSS.OperatorDeclarationSyntax node)
            {
                SyntaxList<AttributeListSyntax> attributes, returnAttributes;
                ConvertAndSplitAttributes(node.AttributeLists, out attributes, out returnAttributes);
                var visitor = new MethodBodyVisitor(semanticModel, this);
                var body = SyntaxFactory.List(node.Body.Statements.SelectMany(s => s.Accept(visitor)));
                var parameterList = (ParameterListSyntax)node.ParameterList?.Accept(this);
                var stmt = SyntaxFactory.OperatorStatement(
                    attributes,
                    ConvertModifiers(node.Modifiers, TokenContext.Member),
                    SyntaxFactory.Token(ConvertOperatorDeclarationToken(CS.CSharpExtensions.Kind(node.OperatorToken))),
                    parameterList,
                    SyntaxFactory.SimpleAsClause(returnAttributes, (TypeSyntax)node.ReturnType.Accept(this))
                );
                return SyntaxFactory.OperatorBlock(stmt, body);
            }

            SyntaxKind ConvertOperatorDeclarationToken(CS.SyntaxKind syntaxKind)
            {
                switch (syntaxKind)
                {
                    case CS.SyntaxKind.EqualsEqualsToken:
                        return SyntaxKind.EqualsToken;
                    case CS.SyntaxKind.ExclamationEqualsToken:
                        return SyntaxKind.LessThanGreaterThanToken;
                }
                throw new NotSupportedException();
            }

            public override VisualBasicSyntaxNode VisitConversionOperatorDeclaration(CSS.ConversionOperatorDeclarationSyntax node)
            {
                return base.VisitConversionOperatorDeclaration(node);
            }

            public override VisualBasicSyntaxNode VisitParameterList(CSS.ParameterListSyntax node)
            {
                return SyntaxFactory.ParameterList(SyntaxFactory.SeparatedList(node.Parameters.Select(p => (ParameterSyntax)p.Accept(this))));
            }

            public override VisualBasicSyntaxNode VisitBracketedParameterList(CSS.BracketedParameterListSyntax node)
            {
                return SyntaxFactory.ParameterList(SyntaxFactory.SeparatedList(node.Parameters.Select(p => (ParameterSyntax)p.Accept(this))));
            }

            public override VisualBasicSyntaxNode VisitParameter(CSS.ParameterSyntax node)
            {
                var id = ConvertIdentifier(node.Identifier);
                var returnType = (TypeSyntax)node.Type?.Accept(this);
                EqualsValueSyntax @default = null;
                if (node.Default != null)
                {
                    @default = SyntaxFactory.EqualsValue((ExpressionSyntax)node.Default?.Value.Accept(this));
                }
                AttributeListSyntax[] newAttributes;
                var modifiers = ConvertModifiers(node.Modifiers, TokenContext.Local);
                if ((modifiers.Count == 0 && returnType != null) || node.Modifiers.Any(CS.SyntaxKind.ThisKeyword))
                {
                    modifiers = SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.ByValKeyword));
                    newAttributes = new AttributeListSyntax[0];
                }
                else if (node.Modifiers.Any(CS.SyntaxKind.OutKeyword))
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
                if (@default != null)
                {
                    modifiers = modifiers.Add(SyntaxFactory.Token(SyntaxKind.OptionalKeyword));
                }
                return SyntaxFactory.Parameter(
                    SyntaxFactory.List(newAttributes.Concat(node.AttributeLists.Select(a => (AttributeListSyntax)a.Accept(this)))),
                    modifiers,
                    SyntaxFactory.ModifiedIdentifier(id),
                    returnType == null ? null : SyntaxFactory.SimpleAsClause(returnType),
                    @default
                );
            }

            #endregion

            #region Expressions

            public override VisualBasicSyntaxNode VisitLiteralExpression(CSS.LiteralExpressionSyntax node)
            {
                // now this looks somehow hacky... is there a better way?
                if (node.IsKind(CS.SyntaxKind.StringLiteralExpression) && node.Token.Text.StartsWith("@", StringComparison.Ordinal))
                {
                    return SyntaxFactory.StringLiteralExpression(
                        SyntaxFactory.StringLiteralToken(
                            node.Token.Text.Substring(1),
                            (string)node.Token.Value
                        )
                    );
                }
                else
                {
                    return Literal(node.Token.Value);
                }
            }

            public override VisualBasicSyntaxNode VisitInterpolatedStringExpression(CSS.InterpolatedStringExpressionSyntax node)
            {
                return SyntaxFactory.InterpolatedStringExpression(node.Contents.Select(c => (InterpolatedStringContentSyntax)c.Accept(this)).ToArray());
            }

            public override VisualBasicSyntaxNode VisitInterpolatedStringText(CSS.InterpolatedStringTextSyntax node)
            {
                return SyntaxFactory.InterpolatedStringText(SyntaxFactory.InterpolatedStringTextToken(node.TextToken.Text, node.TextToken.ValueText));
            }

            public override VisualBasicSyntaxNode VisitInterpolation(CSS.InterpolationSyntax node)
            {
                return SyntaxFactory.Interpolation((ExpressionSyntax)node.Expression.Accept(this));
            }

            public override VisualBasicSyntaxNode VisitInterpolationFormatClause(CSS.InterpolationFormatClauseSyntax node)
            {
                return base.VisitInterpolationFormatClause(node);
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
                        SyntaxFactory.Token(VBUtil.GetExpressionOperatorTokenKind(kind)),
                        Literal(1)
                    );
                }
                if (kind == SyntaxKind.AddAssignmentStatement || kind == SyntaxKind.SubtractAssignmentStatement)
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
                return SyntaxFactory.UnaryExpression(kind, SyntaxFactory.Token(VBUtil.GetExpressionOperatorTokenKind(kind)), (ExpressionSyntax)node.Operand.Accept(this));
            }

            public override VisualBasicSyntaxNode VisitPostfixUnaryExpression(CSS.PostfixUnaryExpressionSyntax node)
            {
                var kind = ConvertToken(CS.CSharpExtensions.Kind(node), TokenContext.Local);
                if (node.Parent is CSS.ExpressionStatementSyntax || node.Parent is CSS.ForStatementSyntax)
                {
                    return SyntaxFactory.AssignmentStatement(
                        ConvertToken(CS.CSharpExtensions.Kind(node), TokenContext.Local),
                        (ExpressionSyntax)node.Operand.Accept(this),
                        SyntaxFactory.Token(VBUtil.GetExpressionOperatorTokenKind(kind)),
                        Literal(1)
                    );
                }
                else
                {
                    string operatorName, minMax;
                    SyntaxKind op;
                    if (kind == SyntaxKind.AddAssignmentStatement)
                    {
                        operatorName = "Increment";
                        minMax = "Min";
                        op = SyntaxKind.SubtractExpression;
                    }
                    else
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
                                    SyntaxFactory.SimpleArgument(SyntaxFactory.BinaryExpression(op, (ExpressionSyntax)node.Operand.Accept(this), SyntaxFactory.Token(VBUtil.GetExpressionOperatorTokenKind(op)), Literal(1)))
                                }
                            )
                        )
                    );
                }
            }

            public override VisualBasicSyntaxNode VisitAssignmentExpression(CSS.AssignmentExpressionSyntax node)
            {
                if (node.Parent is CSS.ExpressionStatementSyntax)
                {
                    if (semanticModel.GetTypeInfo(node.Right).ConvertedType.IsDelegateType())
                    {
                        if (node.OperatorToken.IsKind(CS.SyntaxKind.PlusEqualsToken))
                        {
                            return SyntaxFactory.AddHandlerStatement((ExpressionSyntax)node.Left.Accept(this), (ExpressionSyntax)node.Right.Accept(this));
                        }
                        if (node.OperatorToken.IsKind(CS.SyntaxKind.MinusEqualsToken))
                        {
                            return SyntaxFactory.RemoveHandlerStatement((ExpressionSyntax)node.Left.Accept(this), (ExpressionSyntax)node.Right.Accept(this));
                        }
                    }
                    return MakeAssignmentStatement(node);
                }
                if (node.Parent is CSS.ForStatementSyntax)
                {
                    return MakeAssignmentStatement(node);
                }
                if (node.Parent is CSS.InitializerExpressionSyntax)
                {
                    if (node.Left is CSS.ImplicitElementAccessSyntax)
                    {
                        return SyntaxFactory.CollectionInitializer(
                            SyntaxFactory.SeparatedList(new[] {
                                (ExpressionSyntax)node.Left.Accept(this),
                                (ExpressionSyntax)node.Right.Accept(this)
                            })
                        );
                    }
                    else
                    {
                        return SyntaxFactory.NamedFieldInitializer(
                            (IdentifierNameSyntax)node.Left.Accept(this),
                            (ExpressionSyntax)node.Right.Accept(this)
                        );
                    }
                }
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

            AssignmentStatementSyntax MakeAssignmentStatement(CSS.AssignmentExpressionSyntax node)
            {
                var kind = ConvertToken(CS.CSharpExtensions.Kind(node), TokenContext.Local);
                if (node.IsKind(CS.SyntaxKind.AndAssignmentExpression, CS.SyntaxKind.OrAssignmentExpression, CS.SyntaxKind.ExclusiveOrAssignmentExpression, CS.SyntaxKind.ModuloAssignmentExpression))
                {
                    return SyntaxFactory.SimpleAssignmentStatement(
                        (ExpressionSyntax)node.Left.Accept(this),
                        SyntaxFactory.BinaryExpression(
                            kind,
                            (ExpressionSyntax)node.Left.Accept(this),
                            SyntaxFactory.Token(VBUtil.GetExpressionOperatorTokenKind(kind)),
                            (ExpressionSyntax)node.Right.Accept(this)
                        )
                    );
                }
                return SyntaxFactory.AssignmentStatement(
                    kind,
                    (ExpressionSyntax)node.Left.Accept(this),
                    SyntaxFactory.Token(VBUtil.GetExpressionOperatorTokenKind(kind)),
                    (ExpressionSyntax)node.Right.Accept(this)
                );
            }

            public override VisualBasicSyntaxNode VisitInvocationExpression(CSS.InvocationExpressionSyntax node)
            {
                if (node.Expression.ToString() == "nameof")
                {
                    var argument = node.ArgumentList.Arguments.Single().Expression;
                    return SyntaxFactory.NameOfExpression((ExpressionSyntax)argument.Accept(this));
                }
                return SyntaxFactory.InvocationExpression(
                    (ExpressionSyntax)node.Expression.Accept(this),
                    (ArgumentListSyntax)node.ArgumentList.Accept(this)
                );
            }

            public override VisualBasicSyntaxNode VisitConditionalExpression(CSS.ConditionalExpressionSyntax node)
            {
                return SyntaxFactory.TernaryConditionalExpression(
                    (ExpressionSyntax)node.Condition.Accept(this),
                    (ExpressionSyntax)node.WhenTrue.Accept(this),
                    (ExpressionSyntax)node.WhenFalse.Accept(this)
                );
            }

            public override VisualBasicSyntaxNode VisitConditionalAccessExpression(CSS.ConditionalAccessExpressionSyntax node)
            {
                return SyntaxFactory.ConditionalAccessExpression(
                    (ExpressionSyntax)node.Expression.Accept(this),
                    SyntaxFactory.Token(SyntaxKind.QuestionToken),
                    (ExpressionSyntax)node.WhenNotNull.Accept(this)
                );
            }

            public override VisualBasicSyntaxNode VisitMemberAccessExpression(CSS.MemberAccessExpressionSyntax node)
            {
                return WrapTypedNameIfNecessary(SyntaxFactory.MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    (ExpressionSyntax)node.Expression.Accept(this),
                    SyntaxFactory.Token(SyntaxKind.DotToken),
                    (SimpleNameSyntax)node.Name.Accept(this)
                ), node);
            }

            public override VisualBasicSyntaxNode VisitImplicitElementAccess(CSS.ImplicitElementAccessSyntax node)
            {
                if (node.ArgumentList.Arguments.Count > 1)
                    throw new NotSupportedException("ImplicitElementAccess can only have one argument!");
                return node.ArgumentList.Arguments[0].Expression.Accept(this);
            }

            public override VisualBasicSyntaxNode VisitElementAccessExpression(CSS.ElementAccessExpressionSyntax node)
            {
                return SyntaxFactory.InvocationExpression(
                    (ExpressionSyntax)node.Expression.Accept(this),
                    (ArgumentListSyntax)node.ArgumentList.Accept(this)
                );
            }

            public override VisualBasicSyntaxNode VisitMemberBindingExpression(CSS.MemberBindingExpressionSyntax node)
            {
                return SyntaxFactory.SimpleMemberAccessExpression((SimpleNameSyntax)node.Name.Accept(this));
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
                if (node.IsKind(CS.SyntaxKind.CoalesceExpression))
                {
                    return SyntaxFactory.BinaryConditionalExpression(
                         (ExpressionSyntax)node.Left.Accept(this),
                         (ExpressionSyntax)node.Right.Accept(this)
                    );
                }
                if (node.IsKind(CS.SyntaxKind.AsExpression))
                {
                    return SyntaxFactory.TryCastExpression((ExpressionSyntax)node.Left.Accept(this), (TypeSyntax)node.Right.Accept(this));
                }
                if (node.IsKind(CS.SyntaxKind.IsExpression))
                {
                    return SyntaxFactory.TypeOfIsExpression((ExpressionSyntax)node.Left.Accept(this), (TypeSyntax)node.Right.Accept(this));
                }
                if (node.OperatorToken.IsKind(CS.SyntaxKind.EqualsEqualsToken))
                {
                    ExpressionSyntax otherArgument = null;
                    if (node.Left.IsKind(CS.SyntaxKind.NullLiteralExpression))
                    {
                        otherArgument = (ExpressionSyntax)node.Right.Accept(this);
                    }
                    if (node.Right.IsKind(CS.SyntaxKind.NullLiteralExpression))
                    {
                        otherArgument = (ExpressionSyntax)node.Left.Accept(this);
                    }
                    if (otherArgument != null)
                    {
                        return SyntaxFactory.IsExpression(otherArgument, Literal(null));
                    }
                }
                if (node.OperatorToken.IsKind(CS.SyntaxKind.ExclamationEqualsToken))
                {
                    ExpressionSyntax otherArgument = null;
                    if (node.Left.IsKind(CS.SyntaxKind.NullLiteralExpression))
                    {
                        otherArgument = (ExpressionSyntax)node.Right.Accept(this);
                    }
                    if (node.Right.IsKind(CS.SyntaxKind.NullLiteralExpression))
                    {
                        otherArgument = (ExpressionSyntax)node.Left.Accept(this);
                    }
                    if (otherArgument != null)
                    {
                        return SyntaxFactory.IsNotExpression(otherArgument, Literal(null));
                    }
                }
                var kind = ConvertToken(CS.CSharpExtensions.Kind(node), TokenContext.Local);
                if (node.IsKind(CS.SyntaxKind.AddExpression) && (semanticModel.GetTypeInfo(node.Left).ConvertedType?.SpecialType == SpecialType.System_String 
                    || semanticModel.GetTypeInfo(node.Right).ConvertedType?.SpecialType == SpecialType.System_String))
                {
                    kind = SyntaxKind.ConcatenateExpression;
                }
                return SyntaxFactory.BinaryExpression(
                    kind,
                    (ExpressionSyntax)node.Left.Accept(this),
                    SyntaxFactory.Token(VBUtil.GetExpressionOperatorTokenKind(kind)),
                    (ExpressionSyntax)node.Right.Accept(this)
                );
            }

            public override VisualBasicSyntaxNode VisitTypeOfExpression(CSS.TypeOfExpressionSyntax node)
            {
                return SyntaxFactory.GetTypeExpression((TypeSyntax)node.Type.Accept(this));
            }

            public override VisualBasicSyntaxNode VisitCastExpression(CSS.CastExpressionSyntax node)
            {
                var type = semanticModel.GetTypeInfo(node.Type).Type;
                var expr = (ExpressionSyntax)node.Expression.Accept(this);
                switch (type.SpecialType)
                {
                    case SpecialType.System_Object:
                        return SyntaxFactory.PredefinedCastExpression(SyntaxFactory.Token(SyntaxKind.CObjKeyword), expr);
                    case SpecialType.System_Boolean:
                        return SyntaxFactory.PredefinedCastExpression(SyntaxFactory.Token(SyntaxKind.CBoolKeyword), expr);
                    case SpecialType.System_Char:
                        return SyntaxFactory.PredefinedCastExpression(SyntaxFactory.Token(SyntaxKind.CCharKeyword), expr);
                    case SpecialType.System_SByte:
                        return SyntaxFactory.PredefinedCastExpression(SyntaxFactory.Token(SyntaxKind.CSByteKeyword), expr);
                    case SpecialType.System_Byte:
                        return SyntaxFactory.PredefinedCastExpression(SyntaxFactory.Token(SyntaxKind.CByteKeyword), expr);
                    case SpecialType.System_Int16:
                        return SyntaxFactory.PredefinedCastExpression(SyntaxFactory.Token(SyntaxKind.CShortKeyword), expr);
                    case SpecialType.System_UInt16:
                        return SyntaxFactory.PredefinedCastExpression(SyntaxFactory.Token(SyntaxKind.CUShortKeyword), expr);
                    case SpecialType.System_Int32:
                        return SyntaxFactory.PredefinedCastExpression(SyntaxFactory.Token(SyntaxKind.CIntKeyword), expr);
                    case SpecialType.System_UInt32:
                        return SyntaxFactory.PredefinedCastExpression(SyntaxFactory.Token(SyntaxKind.CUIntKeyword), expr);
                    case SpecialType.System_Int64:
                        return SyntaxFactory.PredefinedCastExpression(SyntaxFactory.Token(SyntaxKind.CLngKeyword), expr);
                    case SpecialType.System_UInt64:
                        return SyntaxFactory.PredefinedCastExpression(SyntaxFactory.Token(SyntaxKind.CULngKeyword), expr);
                    case SpecialType.System_Decimal:
                        return SyntaxFactory.PredefinedCastExpression(SyntaxFactory.Token(SyntaxKind.CDecKeyword), expr);
                    case SpecialType.System_Single:
                        return SyntaxFactory.PredefinedCastExpression(SyntaxFactory.Token(SyntaxKind.CSngKeyword), expr);
                    case SpecialType.System_Double:
                        return SyntaxFactory.PredefinedCastExpression(SyntaxFactory.Token(SyntaxKind.CDblKeyword), expr);
                    case SpecialType.System_String:
                        return SyntaxFactory.PredefinedCastExpression(SyntaxFactory.Token(SyntaxKind.CStrKeyword), expr);
                    case SpecialType.System_DateTime:
                        return SyntaxFactory.PredefinedCastExpression(SyntaxFactory.Token(SyntaxKind.CDateKeyword), expr);
                    default:
                        return SyntaxFactory.CTypeExpression(expr, (TypeSyntax)node.Type.Accept(this));
                }
            }

            public override VisualBasicSyntaxNode VisitObjectCreationExpression(CSS.ObjectCreationExpressionSyntax node)
            {
                return SyntaxFactory.ObjectCreationExpression(
                    SyntaxFactory.List<AttributeListSyntax>(),
                    (TypeSyntax)node.Type.Accept(this),
                    (ArgumentListSyntax)node.ArgumentList?.Accept(this),
                    (ObjectCreationInitializerSyntax)node.Initializer?.Accept(this)
                );
            }

            public override VisualBasicSyntaxNode VisitAnonymousObjectCreationExpression(CSS.AnonymousObjectCreationExpressionSyntax node)
            {
                return SyntaxFactory.AnonymousObjectCreationExpression(
                    SyntaxFactory.ObjectMemberInitializer(SyntaxFactory.SeparatedList(
                        node.Initializers.Select(i => (FieldInitializerSyntax)i.Accept(this))
                    ))
                );
            }

            public override VisualBasicSyntaxNode VisitAnonymousObjectMemberDeclarator(CSS.AnonymousObjectMemberDeclaratorSyntax node)
            {
                if (node.NameEquals == null)
                {
                    return SyntaxFactory.InferredFieldInitializer((ExpressionSyntax)node.Expression.Accept(this));
                }
                else
                {
                    return SyntaxFactory.NamedFieldInitializer(
                        SyntaxFactory.Token(SyntaxKind.KeyKeyword),
                        SyntaxFactory.Token(SyntaxKind.DotToken),
                        (IdentifierNameSyntax)node.NameEquals.Name.Accept(this),
                        SyntaxFactory.Token(SyntaxKind.EqualsToken),
                        (ExpressionSyntax)node.Expression.Accept(this)
                    );
                }
            }

            public override VisualBasicSyntaxNode VisitArrayCreationExpression(CSS.ArrayCreationExpressionSyntax node)
            {
                var upperBoundArguments = node.Type.RankSpecifiers.First()?.Sizes.Where(s => !(s is CSS.OmittedArraySizeExpressionSyntax)).Select(
                    s => (ArgumentSyntax) SyntaxFactory.SimpleArgument(ReduceArrayUpperBoundExpression(s)));
                var rankSpecifiers = node.Type.RankSpecifiers.Select(rs => (ArrayRankSpecifierSyntax)rs.Accept(this));

                return SyntaxFactory.ArrayCreationExpression(
                    SyntaxFactory.Token(SyntaxKind.NewKeyword),
                    SyntaxFactory.List<AttributeListSyntax>(),
                    (TypeSyntax)node.Type.ElementType.Accept(this),
                    upperBoundArguments.Any() ? SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList(upperBoundArguments)) : null,
                    upperBoundArguments.Any() ? SyntaxFactory.List(rankSpecifiers.Skip(1)) : SyntaxFactory.List(rankSpecifiers),
                    (CollectionInitializerSyntax)node.Initializer?.Accept(this) ?? SyntaxFactory.CollectionInitializer()
                );
            }

            public override VisualBasicSyntaxNode VisitImplicitArrayCreationExpression(CSS.ImplicitArrayCreationExpressionSyntax node)
            {
                return SyntaxFactory.CollectionInitializer(
                    SyntaxFactory.SeparatedList(node.Initializer.Expressions.Select(e => (ExpressionSyntax)e.Accept(this)))
                );
            }

            ExpressionSyntax ReduceArrayUpperBoundExpression(CSS.ExpressionSyntax expr)
            {
				var constant = semanticModel.GetConstantValue(expr);
				if (constant.HasValue && constant.Value is int)
					return SyntaxFactory.NumericLiteralExpression(SyntaxFactory.Literal((int)constant.Value - 1));

                return SyntaxFactory.BinaryExpression(
                    SyntaxKind.SubtractExpression,
                    (ExpressionSyntax)expr.Accept(this), SyntaxFactory.Token(SyntaxKind.MinusToken), SyntaxFactory.NumericLiteralExpression(SyntaxFactory.Literal(1)));
            }

            public override VisualBasicSyntaxNode VisitInitializerExpression(CSS.InitializerExpressionSyntax node)
            {
                if (node.IsKind(CS.SyntaxKind.ObjectInitializerExpression))
                {
                    var expressions = node.Expressions.Select(e => e.Accept(this));
                    if (expressions.OfType<FieldInitializerSyntax>().Any())
                    {
                        return SyntaxFactory.ObjectMemberInitializer(
                           SyntaxFactory.SeparatedList(expressions.OfType<FieldInitializerSyntax>())
                       );
                    }

                    return SyntaxFactory.ObjectCollectionInitializer(
                        SyntaxFactory.CollectionInitializer(
                            SyntaxFactory.SeparatedList(expressions.OfType<ExpressionSyntax>())
                        )
                    );
                }
                if (node.IsKind(CS.SyntaxKind.ArrayInitializerExpression))
                    return SyntaxFactory.CollectionInitializer(
                        SyntaxFactory.SeparatedList(node.Expressions.Select(e => (ExpressionSyntax)e.Accept(this)))
                    );
                if (node.IsKind(CS.SyntaxKind.CollectionInitializerExpression))
                    return SyntaxFactory.ObjectCollectionInitializer(
                        SyntaxFactory.CollectionInitializer(
                            SyntaxFactory.SeparatedList(node.Expressions.Select(e => (ExpressionSyntax)e.Accept(this)))
                        )
                    );
                return SyntaxFactory.CollectionInitializer(SyntaxFactory.SeparatedList(node.Expressions.Select(e => (ExpressionSyntax)e.Accept(this))));
            }

            public override VisualBasicSyntaxNode VisitAnonymousMethodExpression(CSS.AnonymousMethodExpressionSyntax node)
            {
                return ConvertLambdaExpression(node, node.Block.Statements, node.ParameterList.Parameters, SyntaxFactory.TokenList(node.AsyncKeyword));
            }

            public override VisualBasicSyntaxNode VisitSimpleLambdaExpression(CSS.SimpleLambdaExpressionSyntax node)
            {
                return ConvertLambdaExpression(node, node.Body, SyntaxFactory.SingletonSeparatedList(node.Parameter), SyntaxFactory.TokenList(node.AsyncKeyword));
            }

            public override VisualBasicSyntaxNode VisitParenthesizedLambdaExpression(CSS.ParenthesizedLambdaExpressionSyntax node)
            {
                return ConvertLambdaExpression(node, node.Body, node.ParameterList.Parameters, SyntaxFactory.TokenList(node.AsyncKeyword));
            }

            LambdaExpressionSyntax ConvertLambdaExpression(CSS.AnonymousFunctionExpressionSyntax node, object block, SeparatedSyntaxList<CSS.ParameterSyntax> parameters, SyntaxTokenList modifiers)
            {
                var symbol = semanticModel.GetSymbolInfo(node).Symbol as IMethodSymbol;
                var parameterList = SyntaxFactory.ParameterList(SyntaxFactory.SeparatedList(parameters.Select(p => (ParameterSyntax)p.Accept(this))));
                LambdaHeaderSyntax header;
                if (symbol.ReturnsVoid)
                    header = SyntaxFactory.SubLambdaHeader(SyntaxFactory.List<AttributeListSyntax>(), ConvertModifiers(modifiers, TokenContext.Local), parameterList, null);
                else
                    header = SyntaxFactory.FunctionLambdaHeader(SyntaxFactory.List<AttributeListSyntax>(), ConvertModifiers(modifiers, TokenContext.Local), parameterList, null);
                if (block is CSS.BlockSyntax)
                    block = ((CSS.BlockSyntax)block).Statements;
                if (block is CS.CSharpSyntaxNode)
                {
                    var syntaxKind = symbol.ReturnsVoid ? SyntaxKind.SingleLineSubLambdaExpression : SyntaxKind.SingleLineFunctionLambdaExpression;
                    return SyntaxFactory.SingleLineLambdaExpression(syntaxKind, header, ((CS.CSharpSyntaxNode)block).Accept(this));
                }
                if (!(block is SyntaxList<CSS.StatementSyntax>))
                    throw new NotSupportedException();
                var statements = SyntaxFactory.List(((SyntaxList<CSS.StatementSyntax>)block).SelectMany(s => s.Accept(new MethodBodyVisitor(semanticModel, this))));

                ExpressionSyntax expression;
                if (statements.Count == 1 && UnpackExpressionFromStatement(statements[0], out expression))
                {
                    var syntaxKind = symbol.ReturnsVoid ? SyntaxKind.SingleLineSubLambdaExpression : SyntaxKind.SingleLineFunctionLambdaExpression;
                    return SyntaxFactory.SingleLineLambdaExpression(syntaxKind, header, expression);
                }

                EndBlockStatementSyntax endBlock;
                SyntaxKind expressionKind;
                if (symbol.ReturnsVoid)
                {
                    endBlock = SyntaxFactory.EndBlockStatement(SyntaxKind.EndSubStatement, SyntaxFactory.Token(SyntaxKind.SubKeyword));
                    expressionKind = SyntaxKind.MultiLineSubLambdaExpression;
                }
                else
                {
                    endBlock = SyntaxFactory.EndBlockStatement(SyntaxKind.EndFunctionStatement, SyntaxFactory.Token(SyntaxKind.FunctionKeyword));
                    expressionKind = SyntaxKind.MultiLineFunctionLambdaExpression;
                }
                return SyntaxFactory.MultiLineLambdaExpression(expressionKind, header, statements, endBlock);
            }

            public override VisualBasicSyntaxNode VisitAwaitExpression(CSS.AwaitExpressionSyntax node)
            {
                return SyntaxFactory.AwaitExpression((ExpressionSyntax)node.Expression.Accept(this));
            }

            bool UnpackExpressionFromStatement(StatementSyntax statementSyntax, out ExpressionSyntax expression)
            {
                if (statementSyntax is ReturnStatementSyntax)
                    expression = ((ReturnStatementSyntax)statementSyntax).Expression;
                else if (statementSyntax is YieldStatementSyntax)
                    expression = ((YieldStatementSyntax)statementSyntax).Expression;
                else
                    expression = null;
                return expression != null;
            }

            public override VisualBasicSyntaxNode VisitQueryExpression(CSS.QueryExpressionSyntax node)
            {
                return SyntaxFactory.QueryExpression(
                    SyntaxFactory.SingletonList((QueryClauseSyntax)node.FromClause.Accept(this))
                    .AddRange(node.Body.Clauses.Select(c => (QueryClauseSyntax)c.Accept(this)))
                    .AddRange(ConvertQueryBody(node.Body))
                );
            }

            public override VisualBasicSyntaxNode VisitFromClause(CSS.FromClauseSyntax node)
            {
                return SyntaxFactory.FromClause(
                    SyntaxFactory.CollectionRangeVariable(SyntaxFactory.ModifiedIdentifier(ConvertIdentifier(node.Identifier)),
                    (ExpressionSyntax)node.Expression.Accept(this))
                );
            }

            public override VisualBasicSyntaxNode VisitWhereClause(CSS.WhereClauseSyntax node)
            {
                return SyntaxFactory.WhereClause((ExpressionSyntax)node.Condition.Accept(this));
            }

            public override VisualBasicSyntaxNode VisitSelectClause(CSS.SelectClauseSyntax node)
            {
                return SyntaxFactory.SelectClause(
                    SyntaxFactory.ExpressionRangeVariable((ExpressionSyntax)node.Expression.Accept(this))
                );
            }

            IEnumerable<QueryClauseSyntax> ConvertQueryBody(CSS.QueryBodySyntax body)
            {
                if (body.SelectOrGroup is CSS.GroupClauseSyntax && body.Continuation == null)
                    throw new NotSupportedException("group by clause without into not supported in VB");
                if (body.SelectOrGroup is CSS.SelectClauseSyntax)
                {
                    yield return (QueryClauseSyntax)body.SelectOrGroup.Accept(this);
                }
                else
                {
                    var group = (CSS.GroupClauseSyntax)body.SelectOrGroup;
                    yield return SyntaxFactory.GroupByClause(
                        SyntaxFactory.SingletonSeparatedList(SyntaxFactory.ExpressionRangeVariable((ExpressionSyntax)group.GroupExpression.Accept(this))),
                        SyntaxFactory.SingletonSeparatedList(SyntaxFactory.ExpressionRangeVariable(SyntaxFactory.VariableNameEquals(SyntaxFactory.ModifiedIdentifier(GeneratePlaceholder("groupByKey"))), (ExpressionSyntax)group.ByExpression.Accept(this))),
                        SyntaxFactory.SingletonSeparatedList(SyntaxFactory.AggregationRangeVariable(SyntaxFactory.FunctionAggregation(ConvertIdentifier(body.Continuation.Identifier))))
                    );
                    if (body.Continuation.Body != null) {
                        foreach (var clause in ConvertQueryBody(body.Continuation.Body))
                            yield return clause;
                    }
                }
            }

            public override VisualBasicSyntaxNode VisitLetClause(CSS.LetClauseSyntax node)
            {
                return SyntaxFactory.LetClause(
                    SyntaxFactory.SingletonSeparatedList(
                        SyntaxFactory.ExpressionRangeVariable(
                            SyntaxFactory.VariableNameEquals(SyntaxFactory.ModifiedIdentifier(ConvertIdentifier(node.Identifier))),
                            (ExpressionSyntax)node.Expression.Accept(this)
                        )
                    )
                );
            }

            public override VisualBasicSyntaxNode VisitJoinClause(CSS.JoinClauseSyntax node)
            {
                if (node.Into != null)
                {
                    return SyntaxFactory.GroupJoinClause(
                        SyntaxFactory.SingletonSeparatedList(SyntaxFactory.CollectionRangeVariable(SyntaxFactory.ModifiedIdentifier(ConvertIdentifier(node.Identifier)), node.Type == null ? null : SyntaxFactory.SimpleAsClause((TypeSyntax)node.Type.Accept(this)), (ExpressionSyntax)node.InExpression.Accept(this))),
                        SyntaxFactory.SingletonSeparatedList(SyntaxFactory.JoinCondition((ExpressionSyntax)node.LeftExpression.Accept(this), (ExpressionSyntax)node.RightExpression.Accept(this))),
                        SyntaxFactory.SingletonSeparatedList(SyntaxFactory.AggregationRangeVariable(SyntaxFactory.VariableNameEquals(SyntaxFactory.ModifiedIdentifier(ConvertIdentifier(node.Into.Identifier))), SyntaxFactory.GroupAggregation()))
                    );
                }
                else
                {
                    return SyntaxFactory.SimpleJoinClause(
                        SyntaxFactory.SingletonSeparatedList(SyntaxFactory.CollectionRangeVariable(SyntaxFactory.ModifiedIdentifier(ConvertIdentifier(node.Identifier)), node.Type == null ? null : SyntaxFactory.SimpleAsClause((TypeSyntax)node.Type.Accept(this)), (ExpressionSyntax)node.InExpression.Accept(this))),
                        SyntaxFactory.SingletonSeparatedList(SyntaxFactory.JoinCondition((ExpressionSyntax)node.LeftExpression.Accept(this), (ExpressionSyntax)node.RightExpression.Accept(this)))
                    );
                }
            }

            public override VisualBasicSyntaxNode VisitOrderByClause(CSS.OrderByClauseSyntax node)
            {
                return SyntaxFactory.OrderByClause(
                    SyntaxFactory.SeparatedList(node.Orderings.Select(o => (OrderingSyntax)o.Accept(this)))
                );
            }

            public override VisualBasicSyntaxNode VisitOrdering(CSS.OrderingSyntax node)
            {
                if (node.IsKind(CS.SyntaxKind.DescendingOrdering))
                {
                    return SyntaxFactory.Ordering(SyntaxKind.DescendingOrdering, (ExpressionSyntax)node.Expression.Accept(this));
                }
                else
                {
                    return SyntaxFactory.Ordering(SyntaxKind.AscendingOrdering, (ExpressionSyntax)node.Expression.Accept(this));
                }
            }

            public override VisualBasicSyntaxNode VisitArgumentList(CSS.ArgumentListSyntax node)
            {
                return SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList(node.Arguments.Select(a => (ArgumentSyntax)a.Accept(this))));
            }

            public override VisualBasicSyntaxNode VisitBracketedArgumentList(CSS.BracketedArgumentListSyntax node)
            {
                return  SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList(node.Arguments.Select(a => (ArgumentSyntax)a.Accept(this))));
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

            public override VisualBasicSyntaxNode VisitArrayType(CSS.ArrayTypeSyntax node)
            {
                return SyntaxFactory.ArrayType((TypeSyntax)node.ElementType.Accept(this),
                    SyntaxFactory.List(node.RankSpecifiers.Select(rs => (ArrayRankSpecifierSyntax)rs.Accept(this))));
            }

            public override VisualBasicSyntaxNode VisitArrayRankSpecifier(CSS.ArrayRankSpecifierSyntax node)
            {
                return SyntaxFactory.ArrayRankSpecifier(
                    SyntaxFactory.Token(SyntaxKind.OpenParenToken),
                    SyntaxFactory.TokenList(Enumerable.Repeat(SyntaxFactory.Token(SyntaxKind.CommaToken), node.Rank - 1)),
                    SyntaxFactory.Token(SyntaxKind.CloseParenToken));
            }

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
                return SyntaxFactory.TypeConstraint((TypeSyntax)node.Type.Accept(this));
            }

            public override VisualBasicSyntaxNode VisitConstructorConstraint(CSS.ConstructorConstraintSyntax node)
            {
                return SyntaxFactory.NewConstraint(SyntaxFactory.Token(SyntaxKind.NewKeyword));
            }

            private CSS.TypeParameterConstraintClauseSyntax FindClauseForParameter(CSS.TypeParameterSyntax node)
            {
                SyntaxList<CSS.TypeParameterConstraintClauseSyntax> clauses;
                var parentBlock = node.Parent.Parent;
                clauses = parentBlock.TypeSwitch(
                    (CSS.MethodDeclarationSyntax m) => m.ConstraintClauses,
                    (CSS.ClassDeclarationSyntax c) => c.ConstraintClauses,
                    (CSS.DelegateDeclarationSyntax d) => d.ConstraintClauses,
                    _ => { throw new NotImplementedException($"{_.GetType().FullName} not implemented!"); }
                );
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
                        if (classOrInterfaceSymbol?.IsInterfaceType() == true)
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
                        if (arr?.Length > 0)
                            implements.Add(SyntaxFactory.ImplementsStatement(arr));
                        break;
                    case CS.SyntaxKind.InterfaceDeclaration:
                        arr = type.BaseList?.Types.Select(t => (TypeSyntax)t.Type.Accept(this)).ToArray();
                        if (arr?.Length > 0)
                            inherits.Add(SyntaxFactory.InheritsStatement(arr));
                        break;
                }
            }

            public override VisualBasicSyntaxNode VisitPredefinedType(CSS.PredefinedTypeSyntax node)
            {
                return SyntaxFactory.PredefinedType(SyntaxFactory.Token(ConvertToken(CS.CSharpExtensions.Kind(node.Keyword))));
            }

            public override VisualBasicSyntaxNode VisitNullableType(CSS.NullableTypeSyntax node)
            {
                return SyntaxFactory.NullableType((TypeSyntax)node.ElementType.Accept(this));
            }

            public override VisualBasicSyntaxNode VisitOmittedTypeArgument(CSS.OmittedTypeArgumentSyntax node)
            {
                return SyntaxFactory.ParseTypeName("");
            }

            #endregion

            #region NameSyntax

            public override VisualBasicSyntaxNode VisitIdentifierName(CSS.IdentifierNameSyntax node)
            {
                return WrapTypedNameIfNecessary(SyntaxFactory.IdentifierName(ConvertIdentifier(node.Identifier)), node);
            }

            public override VisualBasicSyntaxNode VisitGenericName(CSS.GenericNameSyntax node)
            {
                return WrapTypedNameIfNecessary(SyntaxFactory.GenericName(ConvertIdentifier(node.Identifier), (TypeArgumentListSyntax)node.TypeArgumentList.Accept(this)), node);
            }

            public override VisualBasicSyntaxNode VisitQualifiedName(CSS.QualifiedNameSyntax node)
            {
                return WrapTypedNameIfNecessary(SyntaxFactory.QualifiedName((NameSyntax)node.Left.Accept(this), (SimpleNameSyntax)node.Right.Accept(this)), node);
            }

            public override VisualBasicSyntaxNode VisitAliasQualifiedName(CSS.AliasQualifiedNameSyntax node)
            {
                return WrapTypedNameIfNecessary(SyntaxFactory.QualifiedName((NameSyntax)node.Alias.Accept(this), (SimpleNameSyntax)node.Name.Accept(this)), node);
            }

            public override VisualBasicSyntaxNode VisitTypeArgumentList(CSS.TypeArgumentListSyntax node)
            {
                return SyntaxFactory.TypeArgumentList(SyntaxFactory.SeparatedList(node.Arguments.Select(a => (TypeSyntax)a.Accept(this))));
            }

            VisualBasicSyntaxNode WrapTypedNameIfNecessary(ExpressionSyntax name, CSS.ExpressionSyntax originalName)
            {
                if (originalName.Parent is CSS.NameSyntax || originalName.Parent is CSS.AttributeSyntax || originalName.Parent is CSS.MemberAccessExpressionSyntax || originalName.Parent is CSS.MemberBindingExpressionSyntax) return name;
                if (originalName != null && originalName.Parent is CSS.InvocationExpressionSyntax)
                    return name;

                var symbolInfo = semanticModel.GetSymbolInfo(originalName);
                var symbol = symbolInfo.Symbol ?? symbolInfo.CandidateSymbols.FirstOrDefault();
                if (symbol.IsKind(SymbolKind.Method))
                    return SyntaxFactory.AddressOfExpression(name);


                return name;
            }

            #endregion
        }
    }
}
