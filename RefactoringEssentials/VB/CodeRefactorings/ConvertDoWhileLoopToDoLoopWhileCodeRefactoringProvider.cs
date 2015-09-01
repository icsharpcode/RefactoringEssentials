using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.VisualBasic;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;

namespace RefactoringEssentials.VB.CodeRefactorings
{
    /// <summary>
    /// Convert Do While/Until...Loop to Do...Loop While/Until.
    /// </summary>
    [ExportCodeRefactoringProvider(LanguageNames.VisualBasic, Name = "Convert Do While/Until...Loop to Do...Loop While/Until")]
    public class ConvertDoWhileLoopToDoLoopWhileCodeRefactoringProvider : CodeRefactoringProvider
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
            SyntaxKind loopKindAfterConversion;
            if (parentBlockNode.Kind() == SyntaxKind.DoWhileLoopBlock)
            {
                description = "To 'Do ... Loop While'";
                blockKindAfterConversion = SyntaxKind.DoLoopWhileBlock;
                loopKindAfterConversion = SyntaxKind.LoopWhileStatement;
            }
            else if (parentBlockNode.Kind() == SyntaxKind.DoUntilLoopBlock)
            {
                description = "To 'Do ... Loop Until'";
                blockKindAfterConversion = SyntaxKind.DoLoopUntilBlock;
                loopKindAfterConversion = SyntaxKind.LoopUntilStatement;
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
                                SyntaxFactory.SimpleDoStatement(),
                                parentBlockNode.Statements,
                                SyntaxFactory.LoopStatement(loopKindAfterConversion, parentBlockNode.DoStatement.WhileOrUntilClause))
                            .WithLeadingTrivia(parentBlockNode.GetLeadingTrivia())
                            .WithAdditionalAnnotations(Formatter.Annotation));
                        return Task.FromResult(document.WithSyntaxRoot(newRoot));
                    }
                )
            );
        }
    }
}

