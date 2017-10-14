using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;

namespace RefactoringEssentials.CSharp.Diagnostics
{
    [ExportCodeFixProvider(LanguageNames.CSharp), System.Composition.Shared]
    public class ArrayCreationCanBeReplacedWithArrayInitializerCodeFixProvider : CodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds
        {
            get
            {
                return ImmutableArray.Create(CSharpDiagnosticIDs.ArrayCreationCanBeReplacedWithArrayInitializerAnalyzerID);
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
            var text = await document.GetTextAsync(cancellationToken).ConfigureAwait(false);
            var diagnostic = diagnostics.First();
            var sourceSpan = context.Span;
            context.RegisterCodeFix(CodeActionFactory.Create(sourceSpan, diagnostic.Severity, "Use array initializer", document.WithText(text.Replace(sourceSpan, ""))), diagnostic);
        }
    }
}