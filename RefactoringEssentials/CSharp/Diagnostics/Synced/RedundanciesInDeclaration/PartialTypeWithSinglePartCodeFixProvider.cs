using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CodeFixes;

namespace RefactoringEssentials.CSharp.Diagnostics
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = "Partial class with single part"), System.Composition.Shared]
    public class PartialTypeWithSinglePartCodeFixProvider : CodeFixProvider
    {
        public sealed override ImmutableArray<string> FixableDiagnosticIds
        {
            get
            {
                return ImmutableArray.Create(CSharpDiagnosticIDs.PartialTypeWithSinglePartDiagnosticID);
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
            var node = root.FindToken(context.Span.Start);
            if (!node.IsKind(SyntaxKind.PartialKeyword))
                return;
            context.RegisterCodeFix(
                CodeActionFactory.Create(
                    node.Span,
                    diagnostic.Severity,
                    GettextCatalog.GetString("Remove 'partial'"),
                    delegate (CancellationToken token)
                    {
                        var oldClass = node.Parent;
                        var newClass = oldClass
                            .WithModifiers(SyntaxFactory.TokenList(node.Parent.GetModifiers().Where(t => !t.IsKind(SyntaxKind.PartialKeyword))))
                            .WithLeadingTrivia(oldClass.GetLeadingTrivia());
                        return Task.FromResult(document.WithSyntaxRoot(root.ReplaceNode(oldClass, newClass)));
                    }),
                diagnostic
            );
        }
    }
}