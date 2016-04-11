using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis.Formatting;

namespace RefactoringEssentials.CSharp.CodeRefactorings
{
    [ExportCodeRefactoringProvider(LanguageNames.CSharp, Name = "Convert 'return' to 'if'")]
    public class ConvertTernaryExpressionToIfStatementCodeRefactoringProvider : CodeRefactoringProvider
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
            var syntaxNode = root.FindNode(span);
            var returnStatement = syntaxNode as ReturnStatementSyntax;
            if (returnStatement != null)
            {
                if (!(returnStatement.Expression is ConditionalExpressionSyntax) && !(returnStatement.Expression is BinaryExpressionSyntax))
                    return;
                if (returnStatement.Expression is BinaryExpressionSyntax && !returnStatement.Expression.IsKind(SyntaxKind.CoalesceExpression))
                    return;

                context.RegisterRefactoring(
                    CodeActionFactory.Create(
                        span,
                        DiagnosticSeverity.Info,
                        GettextCatalog.GetString("Replace with 'if' statement"),
                        t2 =>
                        {

                            StatementSyntax statement = null;
                            ReturnStatementSyntax returnAfter = null;
                            if (returnStatement.Expression is ConditionalExpressionSyntax)
                            {
                                var condition = (ConditionalExpressionSyntax)returnStatement.Expression;
                                statement = SyntaxFactory.IfStatement(condition.Condition, SyntaxFactory.ReturnStatement(condition.WhenTrue));
                                returnAfter = SyntaxFactory.ReturnStatement(condition.WhenFalse);
                            }
                            else
                            {
                                var bOp = returnStatement.Expression as BinaryExpressionSyntax;
                                if (bOp != null && bOp.IsKind(SyntaxKind.CoalesceExpression))
                                {
                                    statement = SyntaxFactory.IfStatement(SyntaxFactory.BinaryExpression(SyntaxKind.NotEqualsExpression, bOp.Left, SyntaxFactory.LiteralExpression(SyntaxKind.NullLiteralExpression)), SyntaxFactory.ReturnStatement(bOp.Left));
                                    returnAfter = SyntaxFactory.ReturnStatement(bOp.Right);
                                }
                            }

                            var newRoot = root.ReplaceNode((SyntaxNode)returnStatement, new SyntaxNode[] {
                                statement.WithAdditionalAnnotations(Formatter.Annotation).WithLeadingTrivia(returnStatement.GetLeadingTrivia()),
                                returnAfter.WithAdditionalAnnotations(Formatter.Annotation)
                            });
                            return Task.FromResult(document.WithSyntaxRoot(newRoot));
                        }
                    )
                );
            }

            var assignExpr = syntaxNode as AssignmentExpressionSyntax;
            if (assignExpr != null)
            {
                if (assignExpr.Right.IsKind(SyntaxKind.ConditionalExpression))
                {
                    context.RegisterRefactoring(
                        CodeActionFactory.Create(
                            span,
                            DiagnosticSeverity.Info,
                            GettextCatalog.GetString("Replace with 'if' statement"),
                            t2 =>
                            {
                                var ifStatement = CreateForConditionalExpression(assignExpr, (ConditionalExpressionSyntax)assignExpr.Right);
                                return Task.FromResult(document.WithSyntaxRoot(root.ReplaceNode((SyntaxNode)assignExpr.Parent, ifStatement.WithAdditionalAnnotations(Formatter.Annotation))));
                            }
                        )
                    );
                }
                if (assignExpr.Right.IsKind(SyntaxKind.CoalesceExpression))
                {
                    context.RegisterRefactoring(
                        CodeActionFactory.Create(
                            span,
                            DiagnosticSeverity.Info,
                            GettextCatalog.GetString("Replace with 'if' statement"),
                            t2 =>
                            {
                                var ifStatement = CreateForNullCoalescingExpression(assignExpr, (BinaryExpressionSyntax)assignExpr.Right);
                                return Task.FromResult(document.WithSyntaxRoot(root.ReplaceNode((SyntaxNode)assignExpr.Parent, ifStatement.WithAdditionalAnnotations(Formatter.Annotation))));
                            }
                        )
                    );
                }
            }
        }

        static IfStatementSyntax CreateForConditionalExpression(AssignmentExpressionSyntax expr, ConditionalExpressionSyntax conditional)
        {
            return SyntaxFactory.IfStatement(
                conditional.Condition,
                SyntaxFactory.ExpressionStatement(
                    SyntaxFactory.AssignmentExpression(expr.Kind(), expr.Left, conditional.WhenTrue)
                ),
                SyntaxFactory.ElseClause(
                    SyntaxFactory.ExpressionStatement(
                        SyntaxFactory.AssignmentExpression(expr.Kind(), expr.Left, conditional.WhenFalse)
                    )
                )
            );
        }

        static IfStatementSyntax CreateForNullCoalescingExpression(AssignmentExpressionSyntax expr, BinaryExpressionSyntax bOp)
        {
            return SyntaxFactory.IfStatement(SyntaxFactory.BinaryExpression(SyntaxKind.NotEqualsExpression, bOp.Left, SyntaxFactory.LiteralExpression(SyntaxKind.NullLiteralExpression)),
                SyntaxFactory.ExpressionStatement(SyntaxFactory.AssignmentExpression(expr.Kind(), expr.Left, bOp.Left)),
                SyntaxFactory.ElseClause(SyntaxFactory.ExpressionStatement(SyntaxFactory.AssignmentExpression(expr.Kind(), expr.Left, bOp.Right))));
        }
    }

}

