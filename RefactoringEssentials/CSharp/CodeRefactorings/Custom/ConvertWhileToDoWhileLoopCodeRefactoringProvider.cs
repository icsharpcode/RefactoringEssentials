using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Formatting;

namespace RefactoringEssentials.CSharp.CodeRefactorings
{
    /// <summary>
    /// Converts a while loop to a do...while loop.
    /// For instance: while (foo) {} becomes do { } while (foo);
    /// </summary>

    [ExportCodeRefactoringProvider(LanguageNames.CSharp, Name = "Convert while loop to do...while")]
    public class ConvertWhileToDoWhileLoopCodeRefactoringProvider : CodeRefactoringProvider
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

            var node = root.FindNode(span) as WhileStatementSyntax;
            if (node == null)
                return;
            context.RegisterRefactoring(
                CodeActionFactory.Create(
                    span,
                    DiagnosticSeverity.Info,
                    GettextCatalog.GetString("To 'do...while'"),
                    t2 => Task.FromResult(
                        document.WithSyntaxRoot(
                            root.ReplaceNode(
                                (SyntaxNode)node,
                                SyntaxFactory.DoStatement(node.Statement, node.Condition)
                                .WithAdditionalAnnotations(Formatter.Annotation)
                                .WithLeadingTrivia(node.GetLeadingTrivia())
                            )
                        )
                    )
                )
            );
        }
    }
}

