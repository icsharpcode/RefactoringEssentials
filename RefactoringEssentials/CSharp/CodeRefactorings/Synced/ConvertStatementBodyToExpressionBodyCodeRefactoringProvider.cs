using System.Linq;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Formatting;

namespace RefactoringEssentials.CSharp.CodeRefactorings
{
    [ExportCodeRefactoringProvider(LanguageNames.CSharp, Name = "Convert statement body member to expression body")]
    public class ConvertStatementBodyToExpressionBodyCodeRefactoringProvider : CodeRefactoringProvider
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
                HandleMethodCase(context, root, token, method);

            var property = GetDeclaration<PropertyDeclarationSyntax>(token);
            if (property != null)
                HandlePropertyCase(context, root, token, property);

            var indexer = GetDeclaration<IndexerDeclarationSyntax>(token);
            if (indexer != null)
                HandleIndexerCase(context, root, token, indexer);
        }

        static bool IsSimpleReturn(BlockSyntax body, out ExpressionSyntax returnedExpression)
        {
            if (body == null ||
                body.Statements.Count != 1 ||
                !body.Statements[0].IsKind(SyntaxKind.ReturnStatement))
            {
                returnedExpression = null;
                return false;
            }
            returnedExpression = ((ReturnStatementSyntax)body.Statements[0]).Expression;
            return true;
        }

        static bool IsSimpleExpressionInVoidMethod(MethodDeclarationSyntax method, out ExpressionSyntax returnedExpression)
        {
            var voidReturnType = method.ReturnType as PredefinedTypeSyntax;
            if (method.Body == null ||
                method.Body.Statements.Count != 1 ||
                !(method.Body.Statements[0] is ExpressionStatementSyntax) ||
                voidReturnType == null ||
                !voidReturnType.Keyword.IsKind(SyntaxKind.VoidKeyword))
            {
                returnedExpression = null;
                return false;
            }
            returnedExpression = ((ExpressionStatementSyntax)method.Body.Statements[0]).Expression;
            return (returnedExpression != null);
        }

        static T GetDeclaration<T>(SyntaxToken token) where T : MemberDeclarationSyntax
        {
            if (token.IsKind(SyntaxKind.IdentifierToken))
                return token.Parent as T;
            if (token.IsKind(SyntaxKind.ThisKeyword))
                return token.Parent as T;
            if (token.IsKind(SyntaxKind.ReturnKeyword))
                return token.Parent.GetAncestors().OfType<T>().FirstOrDefault();
            return null;
        }

        static void HandleMethodCase(CodeRefactoringContext context, SyntaxNode root, SyntaxToken token, MethodDeclarationSyntax method)
        {
            ExpressionSyntax expr;
            if (!IsSimpleReturn(method.Body, out expr))
                if (!IsSimpleExpressionInVoidMethod(method, out expr))
                    return;
            context.RegisterRefactoring(
                CodeActionFactory.Create(
                    token.Span,
                    DiagnosticSeverity.Info,
                    GettextCatalog.GetString("To expression body"),
                    t2 =>
                    {
                        var newRoot = root.ReplaceNode((SyntaxNode)
                            method,
                            method
                            .WithBody(null)
                            .WithExpressionBody(SyntaxFactory.ArrowExpressionClause(expr))
                            .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken))
                            .WithTrailingTrivia(expr.Parent.GetTrailingTrivia())
                            .WithAdditionalAnnotations(Formatter.Annotation)
                        );
                        return Task.FromResult(context.Document.WithSyntaxRoot(newRoot));
                    }
                )
            );
        }

        static void HandlePropertyCase(CodeRefactoringContext context, SyntaxNode root, SyntaxToken token, PropertyDeclarationSyntax property)
        {
            var getter = property.AccessorList?.Accessors.FirstOrDefault(acc => acc.IsKind(SyntaxKind.GetAccessorDeclaration));
            ExpressionSyntax expr;
            if (getter == null || property.AccessorList.Accessors.Count != 1 || !IsSimpleReturn(getter.Body, out expr))
                return;
            context.RegisterRefactoring(
                CodeActionFactory.Create(
                    token.Span,
                    DiagnosticSeverity.Info,
                    GettextCatalog.GetString("To expression body"),
                    t2 =>
                    {
                        var newRoot = root.ReplaceNode((SyntaxNode)
                            property,
                            property
                            .WithAccessorList(null)
                            .WithExpressionBody(SyntaxFactory.ArrowExpressionClause(expr))
                            .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken))
                            .WithTrailingTrivia(expr.Parent.GetTrailingTrivia())
                            .WithAdditionalAnnotations(Formatter.Annotation)
                        );
                        return Task.FromResult(context.Document.WithSyntaxRoot(newRoot));
                    }
                )
            );
        }

        static void HandleIndexerCase(CodeRefactoringContext context, SyntaxNode root, SyntaxToken token, IndexerDeclarationSyntax indexer)
        {
            var getter = indexer.AccessorList.Accessors.FirstOrDefault(acc => acc.IsKind(SyntaxKind.GetAccessorDeclaration));
            ExpressionSyntax expr;
            if (getter == null || indexer.AccessorList.Accessors.Count != 1 || !IsSimpleReturn(getter.Body, out expr))
                return;
            context.RegisterRefactoring(
                CodeActionFactory.Create(
                    token.Span,
                    DiagnosticSeverity.Info,
                    GettextCatalog.GetString("To expression body"),
                    t2 =>
                    {
                        var newRoot = root.ReplaceNode((SyntaxNode)
                            indexer,
                            indexer
                            .WithAccessorList(null)
                            .WithExpressionBody(SyntaxFactory.ArrowExpressionClause(expr))
                            .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken))
                            .WithTrailingTrivia(expr.Parent.GetTrailingTrivia())
                            .WithAdditionalAnnotations(Formatter.Annotation)
                        );
                        return Task.FromResult(context.Document.WithSyntaxRoot(newRoot));
                    }
                )
            );
        }
    }
}
