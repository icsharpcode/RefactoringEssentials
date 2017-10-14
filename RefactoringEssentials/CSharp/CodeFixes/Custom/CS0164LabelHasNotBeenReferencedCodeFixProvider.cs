using Microsoft.CodeAnalysis;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CodeFixes;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;

namespace RefactoringEssentials.CSharp.CodeFixes
{
    [ExportCodeFixProvider(LanguageNames.CSharp), System.Composition.Shared]
    public class CS0164LabelHasNotBeenReferencedCodeFixProvider : CodeFixProvider
    {
        const string CS0164 = "CS0164"; // CS0164: A label was declared but never used.

        public sealed override ImmutableArray<string> FixableDiagnosticIds
        {
            get { return ImmutableArray.Create(CS0164); }
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
            var node = root.FindNode(context.Span) as LabeledStatementSyntax;
            if (node == null)
                return;
            context.RegisterCodeFix(CodeActionFactory.Create(
                node.Span,
                diagnostic.Severity,
                GettextCatalog.GetString("Remove unused label"),
                token =>
                {
                    var newRoot = root.ReplaceNode(node, node.Statement.WithLeadingTrivia(node.GetLeadingTrivia()).WithTrailingTrivia(node.GetTrailingTrivia()));
                    return Task.FromResult(document.WithSyntaxRoot(newRoot));
                }), diagnostic);

        }
    }
}

