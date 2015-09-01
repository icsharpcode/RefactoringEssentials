using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.VisualBasic;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;

namespace RefactoringEssentials.VB.CodeRefactorings
{
    /// <summary>
    /// Convert Do...Loop While/Until to Do While/Until...Loop.
    /// </summary>
    [ExportCodeRefactoringProvider(LanguageNames.VisualBasic, Name = "Convert Do...Loop While/Until to Do While/Until...Loop")]
    public class ConvertDoLoopWhileToDoWhileLoopCodeRefactoringProvider : CodeRefactoringProvider
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

            var node = root.FindNode(span);
            if (node == null)
                return;

            DoLoopBlockSyntax parentBlockNode = null;
            if ((node is DoStatementSyntax) || (node is LoopStatementSyntax))
            {
                parentBlockNode = node.Parent as DoLoopBlockSyntax;
            }
            else
            {
                return;
            }

            string description;
            SyntaxKind blockKindAfterConversion;
            SyntaxKind doKindAfterConversion;
            if (parentBlockNode.Kind() == SyntaxKind.DoLoopWhileBlock)
            {
                description = "To 'Do While ... Loop'";
                blockKindAfterConversion = SyntaxKind.DoWhileLoopBlock;
                doKindAfterConversion = SyntaxKind.DoWhileStatement;
            }
            else if (parentBlockNode.Kind() == SyntaxKind.DoLoopUntilBlock)
            {
                description = "To 'Do Until ... Loop'";
                blockKindAfterConversion = SyntaxKind.DoUntilLoopBlock;
                doKindAfterConversion = SyntaxKind.DoUntilStatement;
            }
            else
            {
                return;
            }

            context.RegisterRefactoring(
                CodeActionFactory.Create(
                    span,
                    DiagnosticSeverity.Info,
                    GettextCatalog.GetString(GettextCatalog.GetString(description)),
                    t2 =>
                    {
                        var newRoot = root.ReplaceNode(
                            (SyntaxNode)parentBlockNode,
                            SyntaxFactory.DoLoopBlock(blockKindAfterConversion,
                                SyntaxFactory.DoStatement(doKindAfterConversion, parentBlockNode.LoopStatement.WhileOrUntilClause),
                                parentBlockNode.Statements, SyntaxFactory.SimpleLoopStatement().WithTrailingTrivia(parentBlockNode.LoopStatement.GetTrailingTrivia()))
                            .WithLeadingTrivia(parentBlockNode.GetLeadingTrivia())
                            .WithAdditionalAnnotations(Formatter.Annotation));
                        return Task.FromResult(document.WithSyntaxRoot(newRoot));
                    }
                )
            );
        }
    }
}

