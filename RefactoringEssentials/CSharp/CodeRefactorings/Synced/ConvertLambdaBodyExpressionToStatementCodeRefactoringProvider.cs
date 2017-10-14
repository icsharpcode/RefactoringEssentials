using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Formatting;

namespace RefactoringEssentials.CSharp.CodeRefactorings
{
    [ExportCodeRefactoringProvider(LanguageNames.CSharp, Name = "Converts expression of lambda body to statement")]
    public class ConvertLambdaBodyExpressionToStatementCodeRefactoringProvider : CodeRefactoringProvider
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

            var token = root.FindToken(span.Start);
            if (!token.IsKind(SyntaxKind.EqualsGreaterThanToken))
                return;
            var node = token.Parent;
            if (!node.IsKind(SyntaxKind.ParenthesizedLambdaExpression) && !node.IsKind(SyntaxKind.SimpleLambdaExpression))
                return;

            ExpressionSyntax bodyExpr = null;
            if (node.IsKind(SyntaxKind.ParenthesizedLambdaExpression))
            {
                bodyExpr = ((ParenthesizedLambdaExpressionSyntax)node).Body as ExpressionSyntax;
            }
            else
            {
                bodyExpr = ((SimpleLambdaExpressionSyntax)node).Body as ExpressionSyntax;
            }
            if (bodyExpr == null)
                return;

            context.RegisterRefactoring(
                CodeActionFactory.Create(
                    token.Span,
                    DiagnosticSeverity.Info,
                    GettextCatalog.GetString("To lambda statement"),
                    t2 =>
                    {
                        SyntaxNode lambdaExpression;
                        if (node.IsKind(SyntaxKind.ParenthesizedLambdaExpression))
                        {
                            lambdaExpression = ((ParenthesizedLambdaExpressionSyntax)node).WithBody(SyntaxFactory.Block(
                                RequireReturnStatement(model, node) ? (StatementSyntax)SyntaxFactory.ReturnStatement(bodyExpr) : SyntaxFactory.ExpressionStatement(bodyExpr)
                            ));
                        }
                        else
                        {
                            lambdaExpression = ((SimpleLambdaExpressionSyntax)node).WithBody(SyntaxFactory.Block(
                                RequireReturnStatement(model, node) ? (StatementSyntax)SyntaxFactory.ReturnStatement(bodyExpr) : SyntaxFactory.ExpressionStatement(bodyExpr)
                            ));
                        }

                        var newRoot = root.ReplaceNode((SyntaxNode)node, lambdaExpression.WithAdditionalAnnotations(Formatter.Annotation));
                        return Task.FromResult(document.WithSyntaxRoot(newRoot));
                    }
                )
            );
        }

        internal static bool RequireReturnStatement(SemanticModel model, SyntaxNode lambda)
        {
            var typeInfo = model.GetTypeInfo(lambda);
            var type = typeInfo.ConvertedType ?? typeInfo.Type;
            if (type == null || !type.IsDelegateType())
                return false;
            var returnType = type.GetDelegateInvokeMethod().GetReturnType();
            return returnType != null && returnType.SpecialType != SpecialType.System_Void;
        }
    }
}
