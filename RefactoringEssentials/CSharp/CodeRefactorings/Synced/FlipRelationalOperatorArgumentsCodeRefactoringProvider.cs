using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Formatting;

namespace RefactoringEssentials.CSharp.CodeRefactorings
{
    [ExportCodeRefactoringProvider(LanguageNames.CSharp, Name = "Flip an relational operator operands")]
    public class FlipRelationalOperatorArgumentsCodeRefactoringProvider : CodeRefactoringProvider
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
            var binop = root.FindToken(span.Start).Parent as BinaryExpressionSyntax;

            if (binop == null || !binop.OperatorToken.Span.Contains(span))
                return;

            SyntaxKind flippedKind;
            string operatorText;
            if (TryFlip(binop, out flippedKind, out operatorText))
            {
                context.RegisterRefactoring(
                    CodeActionFactory.Create(
                        binop.OperatorToken.Span,
                        DiagnosticSeverity.Info,
                        string.Format(GettextCatalog.GetString("Flip '{0}' operator to '{1}'"), binop.OperatorToken, operatorText),
                        t2 =>
                        {
                            var newBinop = SyntaxFactory.BinaryExpression(flippedKind, binop.Right, binop.Left)
                                .WithAdditionalAnnotations(Formatter.Annotation);
                            var newRoot = root.ReplaceNode((SyntaxNode)binop, newBinop);
                            return Task.FromResult(document.WithSyntaxRoot(newRoot));
                        }
                    )
                );
                return;
            }
        }

        static bool TryFlip(BinaryExpressionSyntax expr, out SyntaxKind flippedKind, out string operatorText)
        {
            switch (expr.Kind())
            {
                case SyntaxKind.LessThanExpression:
                    flippedKind = SyntaxKind.GreaterThanExpression;
                    operatorText = ">";
                    return true;
                case SyntaxKind.LessThanOrEqualExpression:
                    flippedKind = SyntaxKind.GreaterThanOrEqualExpression;
                    operatorText = ">=";
                    return true;
                case SyntaxKind.GreaterThanExpression:
                    flippedKind = SyntaxKind.LessThanExpression;
                    operatorText = "<";
                    return true;
                case SyntaxKind.GreaterThanOrEqualExpression:
                    flippedKind = SyntaxKind.LessThanOrEqualExpression;
                    operatorText = "<=";
                    return true;
            }
            flippedKind = SyntaxKind.None;
            operatorText = null;
            return false;
        }
    }
}
