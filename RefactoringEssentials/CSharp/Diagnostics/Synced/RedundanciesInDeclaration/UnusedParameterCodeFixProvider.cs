using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Formatting;

namespace RefactoringEssentials.CSharp.Diagnostics
{
    [ExportCodeFixProvider(LanguageNames.CSharp), System.Composition.Shared]
    public class UnusedParameterCodeFixProvider : CodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds
        {
            get
            {
                return ImmutableArray.Create(CSharpDiagnosticIDs.UnusedParameterAnalyzerID);
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
            var node = root.FindToken(context.Span.Start).Parent as ParameterSyntax;
            if (node == null)
                return;
            var parent = node.Parent as ParameterListSyntax;
            var newRoot = root.ReplaceNode(parent, parent.WithParameters(SyntaxFactory.SeparatedList(parent.Parameters.Where(p => p != node).Select(p => p.WithAdditionalAnnotations(Formatter.Annotation)) )));
            context.RegisterCodeFix(CodeActionFactory.Create(node.Span, diagnostic.Severity, "Parameter '{0}' is never used", document.WithSyntaxRoot(newRoot)), diagnostic);
        }
    }
}
