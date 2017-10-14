using Microsoft.CodeAnalysis;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CodeFixes;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;

namespace RefactoringEssentials.CSharp.CodeFixes
{
    [ExportCodeFixProvider(LanguageNames.CSharp), System.Composition.Shared]
    public class CS0152DuplicateCaseLabelValueCodeFixProvider : CodeFixProvider
    {
        const string CS0152 = "CS0152"; // CS0152: The label 'label' already occurs in this switch statement

        public sealed override ImmutableArray<string> FixableDiagnosticIds
        {
            get { return ImmutableArray.Create(CS0152); }
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
            var model = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);
            if (model.IsFromGeneratedCode(cancellationToken))
                return;
            var root = await model.SyntaxTree.GetRootAsync(cancellationToken).ConfigureAwait(false);

            var diagnostic = diagnostics.First();
            var node = root.FindNode(context.Span) as CaseSwitchLabelSyntax;
            if (node == null)
                return;
            var switchSection = node.Parent as SwitchSectionSyntax;
            if (switchSection.Labels.Count == 1)
            {
                var switchStatement = switchSection.Parent as SwitchStatementSyntax;
                if (switchStatement.Sections.Count(sect => sect.IsEquivalentTo(switchSection)) <= 1)
                    return;
                context.RegisterCodeFix(CodeActionFactory.Create(
                    node.Span,
                    diagnostic.Severity,
                    string.Format(GettextCatalog.GetString("Remove 'case {0}' switch section"), node.Value),
                    token =>
                    {
                        var newRoot = root.RemoveNode(node.Parent, SyntaxRemoveOptions.KeepNoTrivia);
                        return Task.FromResult(document.WithSyntaxRoot(newRoot));
                    }), diagnostic);
            }
            else
            {
                if (switchSection.Labels.Count(label => label.IsEquivalentTo(node)) > 1)
                {
                    context.RegisterCodeFix(CodeActionFactory.Create(
                    node.Span,
                    diagnostic.Severity,
                    string.Format(GettextCatalog.GetString("Remove 'case {0}' label"), node.Value),
                    token =>
                    {
                        var newRoot = root.RemoveNode(node, SyntaxRemoveOptions.KeepNoTrivia);
                        return Task.FromResult(document.WithSyntaxRoot(newRoot));
                    }), diagnostic);
                }
            }
        }
    }
}