using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using RefactoringEssentials;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CodeFixes;

namespace RefactoringEssentials.CSharp.Diagnostics
{
    [ExportCodeFixProvider(LanguageNames.CSharp), System.Composition.Shared]
    public class RedundantBaseQualifierCodeFixProvider : CodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds
        {
            get
            {
                return ImmutableArray.Create(CSharpDiagnosticIDs.RedundantBaseQualifierAnalyzerID);
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
            var node = root.FindNode(context.Span);
            var parentMa = node.Parent as MemberAccessExpressionSyntax;
            if (parentMa != null)
            {
                var newRoot = root.ReplaceNode((SyntaxNode)parentMa,
                    parentMa.Name
                    .WithLeadingTrivia(parentMa.GetLeadingTrivia())
                    .WithTrailingTrivia(parentMa.GetTrailingTrivia()));
                context.RegisterCodeFix(CodeActionFactory.Create(parentMa.Span, diagnostic.Severity, "Remove 'base.'", document.WithSyntaxRoot(newRoot)), diagnostic);
            }
        }
    }
}