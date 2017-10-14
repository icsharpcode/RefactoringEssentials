using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Formatting;

namespace RefactoringEssentials.CSharp.CodeRefactorings
{
    [ExportCodeRefactoringProvider(LanguageNames.CSharp, Name = "Flip an operator operands")]
    public class FlipOperatorArgumentsCodeRefactoringProvider : CodeRefactoringProvider
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

            if (binop.IsKind(SyntaxKind.EqualsExpression) || binop.IsKind(SyntaxKind.NotEqualsExpression))
            {
                context.RegisterRefactoring(
                    CodeActionFactory.Create(
                        binop.OperatorToken.Span,
                        DiagnosticSeverity.Info,
                        string.Format(GettextCatalog.GetString("Flip '{0}' operands"), binop.OperatorToken),
                        t2 =>
                        {
                            var newBinop = SyntaxFactory.BinaryExpression(binop.Kind(), binop.Right, binop.Left)
                                .WithAdditionalAnnotations(Formatter.Annotation);
                            var newRoot = root.ReplaceNode((SyntaxNode)binop, newBinop);
                            return Task.FromResult(document.WithSyntaxRoot(newRoot));
                        }
                    )
                );
                return;
            }
        }
    }
}
