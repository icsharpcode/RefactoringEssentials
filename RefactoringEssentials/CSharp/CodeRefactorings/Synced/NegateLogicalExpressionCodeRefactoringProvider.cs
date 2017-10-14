using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Text;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Formatting;

namespace RefactoringEssentials.CSharp.CodeRefactorings
{
    [ExportCodeRefactoringProvider(LanguageNames.CSharp, Name = "Negate a relational expression")]
    public class NegateLogicalExpressionCodeRefactoringProvider : CodeRefactoringProvider
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
            context.RegisterRefactoring(
                CodeActionFactory.Create(
                    span,
                    DiagnosticSeverity.Info,
                    string.Format(GettextCatalog.GetString("Negate '{0}'"), expr),
                    t2 =>
                    {
                        var newRoot = root.ReplaceNode((SyntaxNode)
                            expr,
                            CSharpUtil.InvertCondition(expr).WithAdditionalAnnotations(Formatter.Annotation)
                        );
                        return Task.FromResult(document.WithSyntaxRoot(newRoot));
                    }
                )
            );
        }

        internal static bool GetRelationalExpression(SyntaxNode root, TextSpan span, out ExpressionSyntax expr, out SyntaxToken token)
        {
            expr = null;
            token = default(SyntaxToken);
            var bOp = root.FindNode(span).SkipArgument() as BinaryExpressionSyntax;
            if (bOp != null && bOp.OperatorToken.Span.Contains(span) && CSharpUtil.IsRelationalOperator(bOp.Kind()))
            {
                expr = bOp;
                token = bOp.OperatorToken;
                return true;
            }

            var uOp = root.FindNode(span).SkipArgument() as PrefixUnaryExpressionSyntax;
            if (uOp != null && uOp.OperatorToken.Span.Contains(span) && uOp.IsKind(SyntaxKind.LogicalNotExpression))
            {
                expr = uOp;
                token = uOp.OperatorToken;
                return true;
            }
            return false;
        }
    }
}
