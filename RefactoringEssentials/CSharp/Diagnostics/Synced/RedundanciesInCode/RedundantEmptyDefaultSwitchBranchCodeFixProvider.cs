using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CodeFixes;

namespace RefactoringEssentials.CSharp.Diagnostics
{
    [ExportCodeFixProvider(LanguageNames.CSharp), System.Composition.Shared]
    public class RedundantEmptyDefaultSwitchBranchCodeFixProvider : CodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds
        {
            get
            {
                return ImmutableArray.Create(CSharpDiagnosticIDs.RedundantEmptyDefaultSwitchBranchAnalyzerID);
            }
        }

        public override FixAllProvider GetFixAllProvider()
        {
            return WellKnownFixAllProviders.BatchFixer;
        }

        public async override Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var document = context.Document;
            var cancellationToken = context.CancellationToken;
            var span = context.Span;
            var diagnostics = context.Diagnostics;
            var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
            var diagnostic = diagnostics.First();
            var node = root.FindNode(context.Span).Parent as SwitchSectionSyntax;
            if (node == null)
                return;
            SyntaxNode newRoot = null;
            if (node.Labels.Count > 1)
            {
                // If there are multiple labels together with 'default' label, remove only 'default' label
                var defaultLabel = node.Labels.OfType<DefaultSwitchLabelSyntax>().LastOrDefault();
                if (defaultLabel == null)
                    return;
                newRoot = root.RemoveNode(defaultLabel, SyntaxRemoveOptions.KeepNoTrivia);
            }
            else
            {
                newRoot = root.RemoveNode(node, SyntaxRemoveOptions.KeepNoTrivia);
            }
            context.RegisterCodeFix(CodeActionFactory.Create(node.Span, diagnostic.Severity, "Remove redundant 'default' branch", document.WithSyntaxRoot(newRoot)), diagnostic);
        }
    }
}
