using System.Linq;
using System.Collections.Generic;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Simplification;
using Microsoft.CodeAnalysis.Formatting;

namespace RefactoringEssentials.CSharp.CodeRefactorings
{
    [ExportCodeRefactoringProvider(LanguageNames.CSharp, Name = "Make abstract member virtual")]
    public class ToAbstractVirtualNonVirtualConversionCodeRefactoringProvider : CodeRefactoringProvider
    {
        public override async Task ComputeRefactoringsAsync(CodeRefactoringContext context)
        {
            var document = context.Document;
            var span = context.Span;
            var cancellationToken = context.CancellationToken;
            var model = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);
            if (model.IsFromGeneratedCode(cancellationToken))
                return;
            var root = await model.SyntaxTree.GetRootAsync(cancellationToken).ConfigureAwait(false);

            var token = root.FindToken(span.Start);

            if (!token.IsKind(SyntaxKind.IdentifierToken, SyntaxKind.AbstractKeyword, SyntaxKind.VirtualKeyword, SyntaxKind.ThisKeyword))
                return;
            MemberDeclarationSyntax declaration;
            ISymbol symbol = null;
            if (token.IsKind(SyntaxKind.IdentifierToken)) {
                if (token.Parent?.Parent?.IsKind(SyntaxKind.VariableDeclaration) == true) {
                    declaration = token.Parent?.Parent?.Parent as MemberDeclarationSyntax;
                    symbol = model.GetDeclaredSymbol(token.Parent);
                } else {
                    declaration = token.Parent as MemberDeclarationSyntax;
                    if (declaration != null)
                        symbol = model.GetDeclaredSymbol(declaration);
                }
            } else {
                declaration = token.Parent as MemberDeclarationSyntax;
                if (declaration != null)
                    symbol = model.GetDeclaredSymbol(declaration);
            }
            if (declaration == null || symbol == null
                || declaration is BaseTypeDeclarationSyntax
                || declaration is ConstructorDeclarationSyntax
                || declaration is DestructorDeclarationSyntax)
                return;
            var modifiers = declaration.GetModifiers();
            if (modifiers.Any(m => m.IsKind(SyntaxKind.OverrideKeyword, SyntaxKind.ExternKeyword)))
                return;

            var containingType = symbol.ContainingType;
            if (symbol.DeclaredAccessibility == Accessibility.Private || containingType.IsInterfaceType())
                return;

            var explicitInterface = declaration.GetExplicitInterfaceSpecifierSyntax();
            if (explicitInterface != null)
                return;

            if (containingType.IsAbstract)
            {
                if (modifiers.Any(m => m.IsKind(SyntaxKind.AbstractKeyword)))
                {
                    context.RegisterRefactoring(CodeActionFactory.Create(
                        token.Span,
                        DiagnosticSeverity.Info,
                        GettextCatalog.GetString("To non-abstract"),
                        t2 =>
                        {
                            var newRoot = root.ReplaceNode((SyntaxNode)declaration, ImplementAbstractDeclaration(declaration).WithAdditionalAnnotations(Formatter.Annotation));
                            return Task.FromResult(document.WithSyntaxRoot(newRoot));
                        }
                    )
                    );
                }
                else
                {
                    if (CheckBody(declaration))
                    {
                        context.RegisterRefactoring(CodeActionFactory.Create(
                            token.Span,
                            DiagnosticSeverity.Info,
                            GettextCatalog.GetString("To abstract"),
                            t2 =>
                            {
                                var newRoot = root.ReplaceNode((SyntaxNode)declaration, MakeAbstractDeclaration(declaration).WithAdditionalAnnotations(Formatter.Annotation));
                                return Task.FromResult(document.WithSyntaxRoot(newRoot));
                            }
                        )
                        );
                    }
                }
            }

            if (modifiers.Any(m => m.IsKind(SyntaxKind.VirtualKeyword)))
            {
                context.RegisterRefactoring(CodeActionFactory.Create(
                    token.Span,
                    DiagnosticSeverity.Info,
                    GettextCatalog.GetString("To non-virtual"),
                    t2 =>
                    {
                        var newRoot = root.ReplaceNode((SyntaxNode)declaration, RemoveVirtualModifier(declaration));
                        return Task.FromResult(document.WithSyntaxRoot(newRoot));
                    }
                )
                );
            }
            else
            {
                if (modifiers.Any(m => m.IsKind(SyntaxKind.AbstractKeyword)))
                {
                    context.RegisterRefactoring(CodeActionFactory.Create(
                        token.Span,
                        DiagnosticSeverity.Info,
                        GettextCatalog.GetString("To virtual"),
                        t2 =>
                        {
                            var newRoot = root.ReplaceNode((SyntaxNode)declaration, ImplementAbstractDeclaration(declaration, true).WithAdditionalAnnotations(Formatter.Annotation));
                            return Task.FromResult(document.WithSyntaxRoot(newRoot));
                        }
                    )
                    );
                }
                else if (!containingType.IsStatic)
                {
                    context.RegisterRefactoring(CodeActionFactory.Create(
                        token.Span,
                        DiagnosticSeverity.Info,
                        GettextCatalog.GetString("To virtual"),
                        t2 =>
                        {
                            var newRoot = root.ReplaceNode((SyntaxNode)declaration, AddModifier(declaration, SyntaxKind.VirtualKeyword).WithAdditionalAnnotations(Formatter.Annotation));
                            return Task.FromResult(document.WithSyntaxRoot(newRoot));
                        }
                    )
                    );
                }
            }
        }

        internal static BlockSyntax CreateNotImplementedBody()
        {
            var throwStatement = SyntaxFactory.ThrowStatement(
                SyntaxFactory.ParseExpression("new System.NotImplementedException()").WithAdditionalAnnotations(Simplifier.Annotation)
            );
            return SyntaxFactory.Block(throwStatement);
        }

        static SyntaxNode ImplementAbstractDeclaration(MemberDeclarationSyntax abstractDeclaration, bool implementAsVirtual = false)
        {
            var method = abstractDeclaration as MethodDeclarationSyntax;

            var modifier = abstractDeclaration.GetModifiers();
            var newMods = modifier.Where(m => !m.IsKind(SyntaxKind.AbstractKeyword) && !m.IsKind(SyntaxKind.StaticKeyword));
            if (implementAsVirtual)
            {
                newMods = newMods.Concat(
                    new[] { SyntaxFactory.Token(SyntaxKind.VirtualKeyword) }
                );
            }

            var newModifier = SyntaxFactory.TokenList(newMods);

            if (method != null)
            {
                return SyntaxFactory.MethodDeclaration(
                    method.AttributeLists,
                    newModifier,
                    method.ReturnType,
                    method.ExplicitInterfaceSpecifier,
                    method.Identifier,
                    method.TypeParameterList,
                    method.ParameterList,
                    method.ConstraintClauses,
                    CreateNotImplementedBody(),
                    method.ExpressionBody);
            }

            var property = abstractDeclaration as PropertyDeclarationSyntax;
            if (property != null)
            {
                var accessors = new List<AccessorDeclarationSyntax>();
                foreach (var accessor in property.AccessorList.Accessors)
                {
                    accessors.Add(SyntaxFactory.AccessorDeclaration(accessor.Kind(), accessor.AttributeLists, accessor.Modifiers, CreateNotImplementedBody()));
                }
                return SyntaxFactory.PropertyDeclaration(
                    property.AttributeLists,
                    newModifier,
                    property.Type,
                    property.ExplicitInterfaceSpecifier,
                    property.Identifier,
                    SyntaxFactory.AccessorList(SyntaxFactory.List<AccessorDeclarationSyntax>(accessors)),
                    property.ExpressionBody,
                    property.Initializer);
            }

            var indexer = abstractDeclaration as IndexerDeclarationSyntax;
            if (indexer != null)
            {
                var accessors = new List<AccessorDeclarationSyntax>();
                foreach (var accessor in indexer.AccessorList.Accessors)
                {
                    accessors.Add(SyntaxFactory.AccessorDeclaration(accessor.Kind(), accessor.AttributeLists, accessor.Modifiers, CreateNotImplementedBody()));
                }
                return SyntaxFactory.IndexerDeclaration(
                    indexer.AttributeLists,
                    newModifier,
                    indexer.Type,
                    indexer.ExplicitInterfaceSpecifier,
                    indexer.ParameterList,
                    SyntaxFactory.AccessorList(SyntaxFactory.List<AccessorDeclarationSyntax>(accessors)),
                    indexer.ExpressionBody);
            }

            var evt = abstractDeclaration as EventDeclarationSyntax;
            if (evt != null)
            {
                var accessors = new List<AccessorDeclarationSyntax>();
                foreach (var accessor in evt.AccessorList.Accessors)
                {
                    accessors.Add(SyntaxFactory.AccessorDeclaration(accessor.Kind(), CreateNotImplementedBody()));
                }
                return SyntaxFactory.EventDeclaration(
                    evt.AttributeLists,
                    newModifier,
                    evt.Type,
                    evt.ExplicitInterfaceSpecifier,
                    evt.Identifier,
                    SyntaxFactory.AccessorList(SyntaxFactory.List<AccessorDeclarationSyntax>(accessors))
                );
            }

            var evtField = abstractDeclaration as EventFieldDeclarationSyntax;
            if (evtField != null)
            {
                return SyntaxFactory.EventFieldDeclaration(
                    evtField.AttributeLists,
                    newModifier,
                    evtField.Declaration
                );
            }

            return null;
        }

        static bool CheckBody(MemberDeclarationSyntax node)
        {
            var property = node as BasePropertyDeclarationSyntax;
            if (property != null) {
                if (property.AccessorList == null || property.AccessorList.Accessors.Any(acc => !IsValidBody(acc.Body)))
                    return false;
            }

            var m = node as MethodDeclarationSyntax;
            if (m != null && !IsValidBody(m.Body))
                return false;
            return true;
        }

        static bool IsValidBody(BlockSyntax body)
        {
            if (body == null)
                return true;
            var first = body.Statements.FirstOrDefault();
            if (first == null)
                return true;
            //			if (first.GetNextSibling(s => s.Role == BlockStatement.StatementRole) != null)
            //				return false;
            return first is EmptyStatementSyntax || first is ThrowStatementSyntax;
        }

        static SyntaxNode MakeAbstractDeclaration(MemberDeclarationSyntax abstractDeclaration)
        {
            var method = abstractDeclaration as MethodDeclarationSyntax;

            var modifier = abstractDeclaration.GetModifiers();
            var newModifier = SyntaxFactory.TokenList(modifier.Where(m => !m.IsKind(SyntaxKind.VirtualKeyword) && !m.IsKind(SyntaxKind.StaticKeyword) && !m.IsKind(SyntaxKind.SealedKeyword)).Concat(
                new[] { SyntaxFactory.Token(SyntaxKind.AbstractKeyword) }
            ));

            if (method != null)
            {
                return SyntaxFactory.MethodDeclaration(
                    method.AttributeLists,
                    newModifier,
                    method.ReturnType,
                    method.ExplicitInterfaceSpecifier,
                    method.Identifier,
                    method.TypeParameterList,
                    method.ParameterList,
                    method.ConstraintClauses,
                    null,
                    method.ExpressionBody,
                    SyntaxFactory.Token(SyntaxKind.SemicolonToken));
            }

            var property = abstractDeclaration as PropertyDeclarationSyntax;
            if (property != null)
            {
                var accessors = new List<AccessorDeclarationSyntax>();
                foreach (var accessor in property.AccessorList.Accessors)
                {
                    accessors.Add(SyntaxFactory.AccessorDeclaration(accessor.Kind(), accessor.AttributeLists, accessor.Modifiers, accessor.Keyword, (BlockSyntax) null, SyntaxFactory.Token(SyntaxKind.SemicolonToken)));
                }
                return SyntaxFactory.PropertyDeclaration(
                    property.AttributeLists,
                    newModifier,
                    property.Type,
                    property.ExplicitInterfaceSpecifier,
                    property.Identifier,
                    SyntaxFactory.AccessorList(SyntaxFactory.List<AccessorDeclarationSyntax>(accessors)),
                    property.ExpressionBody,
                    property.Initializer);
            }

            var indexer = abstractDeclaration as IndexerDeclarationSyntax;
            if (indexer != null)
            {
                var accessors = new List<AccessorDeclarationSyntax>();
                foreach (var accessor in indexer.AccessorList.Accessors)
                {
                    accessors.Add(SyntaxFactory.AccessorDeclaration(accessor.Kind(), accessor.AttributeLists, accessor.Modifiers, accessor.Keyword, (BlockSyntax) null, SyntaxFactory.Token(SyntaxKind.SemicolonToken)));
                }
                return SyntaxFactory.IndexerDeclaration(
                    indexer.AttributeLists,
                    newModifier,
                    indexer.Type,
                    indexer.ExplicitInterfaceSpecifier,
                    indexer.ParameterList,
                    SyntaxFactory.AccessorList(SyntaxFactory.List<AccessorDeclarationSyntax>(accessors)),
                    indexer.ExpressionBody);
            }
            var evt = abstractDeclaration as EventDeclarationSyntax;
            if (evt != null)
            {
                return SyntaxFactory.EventFieldDeclaration(
                    evt.AttributeLists,
                    newModifier,
                    SyntaxFactory.VariableDeclaration(
                        evt.Type,
                        SyntaxFactory.SeparatedList<VariableDeclaratorSyntax>(
                            new[] {
                                SyntaxFactory.VariableDeclarator(evt.Identifier)
                            }
                        )
                    )
                );
            }
            var evt2 = abstractDeclaration as EventFieldDeclarationSyntax;
            if (evt2 != null)
            {
                return evt2.WithModifiers(newModifier);
            }
            return null;
        }

        static SyntaxNode AddModifier(MemberDeclarationSyntax abstractDeclaration, SyntaxKind token)
        {
            var method = abstractDeclaration as MethodDeclarationSyntax;

            var modifier = abstractDeclaration.GetModifiers();
            var newMods = modifier.Where(m => !m.IsKind(SyntaxKind.AbstractKeyword) && !m.IsKind(SyntaxKind.StaticKeyword));

            newMods = newMods.Concat(
                new[] { SyntaxFactory.Token(token) }
            );

            var newModifier = SyntaxFactory.TokenList(newMods);

            if (method != null)
            {
                return method.WithModifiers(newModifier);
            }

            var property = abstractDeclaration as PropertyDeclarationSyntax;
            if (property != null)
            {
                return property.WithModifiers(newModifier);
            }

            var indexer = abstractDeclaration as IndexerDeclarationSyntax;
            if (indexer != null)
            {
                return indexer.WithModifiers(newModifier);
            }

            var evt = abstractDeclaration as EventDeclarationSyntax;
            if (evt != null)
            {
                return evt.WithModifiers(newModifier);
            }

            var evt2 = abstractDeclaration as EventFieldDeclarationSyntax;
            if (evt2 != null)
            {
                return evt2.WithModifiers(newModifier);
            }
            return null;
        }

        static SyntaxNode RemoveVirtualModifier(MemberDeclarationSyntax abstractDeclaration)
        {
            var method = abstractDeclaration as MethodDeclarationSyntax;

            if (method != null)
            {
                return method.WithModifiers(SyntaxFactory.TokenList(method.Modifiers.Where(m => !m.IsKind(SyntaxKind.VirtualKeyword))));
            }

            var property = abstractDeclaration as PropertyDeclarationSyntax;
            if (property != null)
            {
                return property.WithModifiers(SyntaxFactory.TokenList(property.Modifiers.Where(m => !m.IsKind(SyntaxKind.VirtualKeyword))));
            }

            var indexer = abstractDeclaration as IndexerDeclarationSyntax;
            if (indexer != null)
            {
                return indexer.WithModifiers(SyntaxFactory.TokenList(indexer.Modifiers.Where(m => !m.IsKind(SyntaxKind.VirtualKeyword))));
            }

            var evt = abstractDeclaration as EventDeclarationSyntax;
            if (evt != null)
            {
                return evt.WithModifiers(SyntaxFactory.TokenList(evt.Modifiers.Where(m => !m.IsKind(SyntaxKind.VirtualKeyword))));
            }

            var evt2 = abstractDeclaration as EventFieldDeclarationSyntax;
            if (evt2 != null)
            {
                return evt2.WithModifiers(SyntaxFactory.TokenList(evt2.Modifiers.Where(m => !m.IsKind(SyntaxKind.VirtualKeyword))));
            }
            return abstractDeclaration;
        }
    }
}
