using Microsoft.CodeAnalysis;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CodeFixes;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace RefactoringEssentials.CSharp.Diagnostics
{

	[ExportCodeFixProvider(LanguageNames.CSharp), System.Composition.Shared]
    public class RedundantToStringCallCodeFixProvider : CodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds
        {
            get
            {
                return ImmutableArray.Create(CSharpDiagnosticIDs.RedundantToStringCallAnalyzerID, CSharpDiagnosticIDs.RedundantToStringCallAnalyzer_ValueTypesID);
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
            var parent = root.FindToken(context.Span.Start).Parent;
            var node = parent as MemberAccessExpressionSyntax;
            if (node == null)
                return;
            var newRoot = root.ReplaceNode(node.Parent, node.Expression.WithTrailingTrivia(node.Parent.GetTrailingTrivia()).WithLeadingTrivia(node.Parent.GetLeadingTrivia()));
            context.RegisterCodeFix(CodeActionFactory.Create(node.Span, diagnostic.Severity, "Remove redundant '.ToString()'", document.WithSyntaxRoot(newRoot)), diagnostic);
        }
    }
}