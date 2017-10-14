using System.Linq;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Text;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;

namespace RefactoringEssentials.CSharp.CodeRefactorings
{
    [ExportCodeRefactoringProvider(LanguageNames.CSharp, Name = "Remove #region/#endregion directives")]
    public class RemoveRegionEndRegionDirectivesCodeRefactoringProvider : CodeRefactoringProvider
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

            SyntaxTrivia directive;
            if (!TryGetDirective(root, span, out directive))
                return;

            context.RegisterRefactoring(
                CodeActionFactory.Create(
                    directive.Span,
                    DiagnosticSeverity.Info,
                    GettextCatalog.GetString("Remove region/endregion directives"),
                    async t2 =>
                    {
                        var structure = directive.GetStructure();
                        // var prev = directive.GetPreviousTrivia (model.SyntaxTree, cancellationToken, true);
                        var end = structure as DirectiveTriviaSyntax;
                        SourceText text = await document.GetTextAsync(cancellationToken).ConfigureAwait(false);
                        foreach (var e in end.GetRelatedDirectives().OrderByDescending(e => e.SpanStart))
                        {
                            var line = text.Lines.GetLineFromPosition(e.FullSpan.Start);
                            text = text.Replace(line.SpanIncludingLineBreak, "");
                        }
                        return document.WithText(text);
                    }
                )
            );
        }

        static bool TryGetDirective(SyntaxNode root, TextSpan span, out SyntaxTrivia directive)
        {
            directive = root.FindTrivia(span.Start);
            return directive.IsKind(SyntaxKind.RegionDirectiveTrivia) || directive.IsKind(SyntaxKind.EndRegionDirectiveTrivia);
        }
    }
}