using System.Linq;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Formatting;

namespace RefactoringEssentials.CSharp.CodeRefactorings
{
    [ExportCodeRefactoringProvider(LanguageNames.CSharp, Name = "Convert expression body member to statement body")]
    public class ConvertExpressionBodyToStatementBodyCodeRefactoringProvider : CodeRefactoringProvider
    {
        public override async Task ComputeRefactoringsAsync(CodeRefactoringContext context)
        {
            var document = context.Document;
            if (document.Project.Solution.Workspace.Kind == WorkspaceKind.MiscellaneousFiles)
                return;
            var span = context.Span;
            if (!span.IsEmpty)
                return;
            var cancellationToken = context.CancellationToken;
            if (cancellationToken.IsCancellationRequested)
                return;
            var model = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);
            if (model.IsFromGeneratedCode(cancellationToken))
                return;
            var root = await model.SyntaxTree.GetRootAsync(cancellationToken).ConfigureAwait(false);
            var parseOptions = root.SyntaxTree.Options as CSharpParseOptions;
            if (parseOptions != null && parseOptions.LanguageVersion < LanguageVersion.CSharp6)
                return;

            var token = root.FindToken(span.Start);
            var method = GetDeclaration<MethodDeclarationSyntax>(token);
            if (method != null)
                HandleMethodCase(context, model, root, token, method);

            var property = GetDeclaration<PropertyDeclarationSyntax>(token);
            if (property != null)
                HandlePropertyCase(context, root, token, property);
        }

        static bool IsExpressionBody(BlockSyntax body, ArrowExpressionClauseSyntax arrowExpr, out ExpressionSyntax returnedExpression)
        {
            if (body != null ||
                arrowExpr == null)
            {
                returnedExpression = null;
                return false;
            }
            returnedExpression = arrowExpr.Expression;
            return true;
        }

        static T GetDeclaration<T>(SyntaxToken token) where T : MemberDeclarationSyntax
        {
            if (token.IsKind(SyntaxKind.IdentifierToken))
                return token.Parent as T;
            if (token.IsKind(SyntaxKind.ArrowExpressionClause))
                return token.Parent.GetAncestors().OfType<T>().FirstOrDefault();
            return null;
        }

        static void HandleMethodCase(CodeRefactoringContext context, SemanticModel model, SyntaxNode root, SyntaxToken token, MethodDeclarationSyntax method)
        {
            ExpressionSyntax expr;
            if (!IsExpressionBody(method.Body, method.ExpressionBody, out expr))
                return;

            var methodSymbol = model.GetDeclaredSymbol(method);
            if (methodSymbol == null)
                return;
            
            BlockSyntax methodBody =  methodSymbol.ReturnsVoid ?
                SyntaxFactory.Block(SyntaxFactory.ExpressionStatement(expr)) :
                SyntaxFactory.Block(SyntaxFactory.ReturnStatement(expr));

            context.RegisterRefactoring(
                CodeActionFactory.Create(
                    token.Span,
                    DiagnosticSeverity.Info,
                    GettextCatalog.GetString("To statement body"),
                    t2 =>
                    {
                        var newRoot = root.ReplaceNode((SyntaxNode)
                            method,
                            method
                            .WithBody(methodBody)
                            .WithExpressionBody(null)
                            .WithSemicolonToken(SyntaxFactory.MissingToken(SyntaxKind.SemicolonToken))
                            .WithAdditionalAnnotations(Formatter.Annotation)
                            .WithTrailingTrivia(method.GetTrailingTrivia())

                        );
                        return Task.FromResult(context.Document.WithSyntaxRoot(newRoot));
                    }
                )
            );

        }

        static void HandlePropertyCase(CodeRefactoringContext context, SyntaxNode root, SyntaxToken token, PropertyDeclarationSyntax property)
        {
            ExpressionSyntax expr;
            if (!IsExpressionBody(null, property.ExpressionBody, out expr))
                return;
            context.RegisterRefactoring(
                CodeActionFactory.Create(
                    token.Span,
                    DiagnosticSeverity.Info,
                    GettextCatalog.GetString("To statement body"),
                    t2 =>
                    {
                        var accessor =
                            SyntaxFactory
                                .AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
                                .WithBody(SyntaxFactory.Block(SyntaxFactory.ReturnStatement(expr)))
                                .WithAdditionalAnnotations(Formatter.Annotation);
                        var accessorDeclList = new SyntaxList<AccessorDeclarationSyntax>();
                        accessorDeclList = accessorDeclList.Add(accessor);

                        var newRoot = root.ReplaceNode((SyntaxNode)
                            property,
                            property
                            .WithAccessorList(SyntaxFactory.AccessorList(accessorDeclList))
                            .WithExpressionBody(null)
                            .WithSemicolonToken(SyntaxFactory.MissingToken(SyntaxKind.SemicolonToken))
                            .WithAdditionalAnnotations(Formatter.Annotation)
                            .WithTrailingTrivia(property.GetTrailingTrivia())

                        );
                        return Task.FromResult(context.Document.WithSyntaxRoot(newRoot));
                    }
                )
            );
        }
    }
}
