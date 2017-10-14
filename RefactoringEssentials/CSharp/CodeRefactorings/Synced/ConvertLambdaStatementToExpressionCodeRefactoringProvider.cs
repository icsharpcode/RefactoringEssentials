using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Formatting;

namespace RefactoringEssentials.CSharp.CodeRefactorings
{
    [ExportCodeRefactoringProvider(LanguageNames.CSharp, Name = "Converts statement of lambda body to expression")]
    public class ConvertLambdaStatementToExpressionCodeRefactoringProvider : CodeRefactoringProvider
    {
        internal static bool TryGetConvertableExpression(SyntaxNode body, out BlockSyntax blockStatement, out ExpressionSyntax expr)
        {
            expr = null;
            blockStatement = body as BlockSyntax;
            if (blockStatement == null || blockStatement.Statements.Count > 1)
                return false;
            var returnStatement = blockStatement.Statements.FirstOrDefault() as ReturnStatementSyntax;
            if (returnStatement != null)
            {
                expr = returnStatement.Expression;
            }
            else
            {
                var exprStatement = blockStatement.Statements.FirstOrDefault() as ExpressionStatementSyntax;
                if (exprStatement == null)
                    return false;
                expr = exprStatement.Expression;
            }
            return true;
        }

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

            var token = root.FindToken(span.Start);
            if (!token.IsKind(SyntaxKind.EqualsGreaterThanToken))
                return;
            var node = token.Parent;
            if (!node.IsKind(SyntaxKind.ParenthesizedLambdaExpression) && !node.IsKind(SyntaxKind.SimpleLambdaExpression))
                return;

            CSharpSyntaxNode body;
            if (node.IsKind(SyntaxKind.ParenthesizedLambdaExpression))
            {
                body = ((ParenthesizedLambdaExpressionSyntax)node).Body;
            }
            else
            {
                body = ((SimpleLambdaExpressionSyntax)node).Body;
            }
            if (body == null)
                return;

            BlockSyntax blockStatement;
            ExpressionSyntax expr;
            if (!TryGetConvertableExpression(body, out blockStatement, out expr))
                return;

            context.RegisterRefactoring(
                CodeActionFactory.Create(
                    token.Span,
                    DiagnosticSeverity.Info,
                    GettextCatalog.GetString("To lambda expression"),
                    t2 =>
                    {
                        SyntaxNode lambdaExpression;
                        if (node.IsKind(SyntaxKind.ParenthesizedLambdaExpression))
                        {
                            lambdaExpression = ((ParenthesizedLambdaExpressionSyntax)node).WithBody(expr);
                        }
                        else
                        {
                            lambdaExpression = ((SimpleLambdaExpressionSyntax)node).WithBody(expr);
                        }

                        var newRoot = root.ReplaceNode((SyntaxNode)node, lambdaExpression.WithAdditionalAnnotations(Formatter.Annotation));
                        return Task.FromResult(document.WithSyntaxRoot(newRoot));
                    }
                )
            );
        }
    }
}
