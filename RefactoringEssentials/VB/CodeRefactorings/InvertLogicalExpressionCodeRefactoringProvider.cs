using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.VisualBasic;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.Text;
using RefactoringEssentials.Util;

namespace RefactoringEssentials.VB.CodeRefactorings
{
    [ExportCodeRefactoringProvider(LanguageNames.VisualBasic, Name = "Invert logical expression")]
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
            if (!GetRelationalExpression(root, span, out expr, out token))
                return;
            if (expr.IsKind(SyntaxKind.NotExpression))
            {
                context.RegisterRefactoring(
                    CodeActionFactory.Create(
                        span,
                        DiagnosticSeverity.Info,
                        string.Format(GettextCatalog.GetString("Invert '{0}'"), expr),
                        t2 =>
                        {
                            var uOp = expr as UnaryExpressionSyntax;
                            var convertedExpr = VBUtil.InvertCondition(uOp.Operand.SkipParens()).WithAdditionalAnnotations(Formatter.Annotation);
                            var newRoot = root.ReplaceNode(
                                expr,
                                VBUtil.InvertCondition(uOp.Operand.SkipParens()).WithAdditionalAnnotations(Formatter.Annotation)
                            );
                            return Task.FromResult(document.WithSyntaxRoot(newRoot));
                        }
                    )
                );
            }
            else if (expr.Parent is ParenthesizedExpressionSyntax && expr.Parent.Parent is UnaryExpressionSyntax)
            {
                var unaryOperatorExpression = expr.Parent.Parent as UnaryExpressionSyntax;
                if (unaryOperatorExpression.IsKind(SyntaxKind.NotExpression))
                {

                    context.RegisterRefactoring(
                        CodeActionFactory.Create(
                            span,
                            DiagnosticSeverity.Info,
                            string.Format(GettextCatalog.GetString("Invert '{0}'"), unaryOperatorExpression),
                            t2 =>
                            {
                                //var uOp = expr as PrefixUnaryExpressionSyntax;
                                var newRoot = root.ReplaceNode(
                                    unaryOperatorExpression,
                                    VBUtil.InvertCondition(expr).WithAdditionalAnnotations(Formatter.Annotation)
                                );
                                return Task.FromResult(document.WithSyntaxRoot(newRoot));
                            }
                        )
                    );
                }
            }
            else
            {
                context.RegisterRefactoring(
                    CodeActionFactory.Create(
                        span,
                        DiagnosticSeverity.Info,
                        string.Format(GettextCatalog.GetString("Invert '{0}'"), expr),
                        t2 =>
                        {
                            var newRoot = root.ReplaceNode((SyntaxNode)
                                expr,
                                SyntaxFactory.UnaryExpression(
                                    SyntaxKind.NotExpression,
                                    SyntaxFactory.Token(SyntaxKind.NotKeyword),
                                    SyntaxFactory.ParenthesizedExpression(VBUtil.InvertCondition(expr))
                                ).WithAdditionalAnnotations(Formatter.Annotation)
                            );
                            return Task.FromResult(document.WithSyntaxRoot(newRoot));
                        }
                    )
                );
            }
        }

        internal static bool GetRelationalExpression(SyntaxNode root, TextSpan span, out ExpressionSyntax expr, out SyntaxToken token)
        {
            expr = null;
            token = default(SyntaxToken);
            var bOp = root.FindNode(span).SkipArgument() as BinaryExpressionSyntax;
            if (bOp != null && bOp.OperatorToken.Span.Contains(span) && VBUtil.IsRelationalOperator(bOp.Kind()))
            {
                expr = bOp;
                token = bOp.OperatorToken;
                return true;
            }

            var uOp = root.FindNode(span).SkipArgument() as UnaryExpressionSyntax;
            if (uOp != null && uOp.OperatorToken.Span.Contains(span) && uOp.IsKind(SyntaxKind.NotExpression))
            {
                expr = uOp;
                token = uOp.OperatorToken;
                return true;
            }
            return false;
        }
    }
}