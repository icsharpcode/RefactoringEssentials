using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Formatting;

namespace RefactoringEssentials.CSharp.CodeRefactorings
{
    [ExportCodeRefactoringProvider(LanguageNames.CSharp, Name = "Invert logical expression")]
    public class InvertLogicalExpressionCodeRefactoringProvider : CodeRefactoringProvider
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
            ExpressionSyntax expr;
            SyntaxToken token;
            if (!NegateLogicalExpressionCodeRefactoringProvider.GetRelationalExpression(root, span, out expr, out token))
                return;
            if (expr.IsKind(SyntaxKind.LogicalNotExpression))
            {
                context.RegisterRefactoring(
                    CodeActionFactory.Create(
                        span,
                        DiagnosticSeverity.Info,
                        string.Format(GettextCatalog.GetString("Invert '{0}'"), expr),
                        t2 =>
                        {
                            var uOp = expr as PrefixUnaryExpressionSyntax;
                            var newRoot = root.ReplaceNode((SyntaxNode)
                                expr,
                                CSharpUtil.InvertCondition(uOp.Operand.SkipParens()).WithAdditionalAnnotations(Formatter.Annotation)
                            );
                            return Task.FromResult(document.WithSyntaxRoot(newRoot));
                        }
                    )
                );
            }

            if (expr.Parent is ParenthesizedExpressionSyntax && expr.Parent.Parent is PrefixUnaryExpressionSyntax)
            {
                var unaryOperatorExpression = expr.Parent.Parent as PrefixUnaryExpressionSyntax;
                if (unaryOperatorExpression.IsKind(SyntaxKind.LogicalNotExpression))
                {

                    context.RegisterRefactoring(
                        CodeActionFactory.Create(
                            span,
                            DiagnosticSeverity.Info,
                            string.Format(GettextCatalog.GetString("Invert '{0}'"), unaryOperatorExpression),
                            t2 =>
                            {
                                //var uOp = expr as PrefixUnaryExpressionSyntax;
                                var newRoot = root.ReplaceNode((SyntaxNode)
                                    unaryOperatorExpression,
                                    CSharpUtil.InvertCondition(expr).WithAdditionalAnnotations(Formatter.Annotation)
                                );
                                return Task.FromResult(document.WithSyntaxRoot(newRoot));
                            }
                        )
                    );
                }
            }

            context.RegisterRefactoring(
                CodeActionFactory.Create(
                    span,
                    DiagnosticSeverity.Info,
                    string.Format(GettextCatalog.GetString("Invert '{0}'"), expr),
                    t2 =>
                    {
                        var newRoot = root.ReplaceNode((SyntaxNode)
                            expr,
                            SyntaxFactory.PrefixUnaryExpression(
                                SyntaxKind.LogicalNotExpression,
                                SyntaxFactory.ParenthesizedExpression(CSharpUtil.InvertCondition(expr))
                            ).WithAdditionalAnnotations(Formatter.Annotation)
                        );
                        return Task.FromResult(document.WithSyntaxRoot(newRoot));
                    }
                )
            );
        }
    }
}