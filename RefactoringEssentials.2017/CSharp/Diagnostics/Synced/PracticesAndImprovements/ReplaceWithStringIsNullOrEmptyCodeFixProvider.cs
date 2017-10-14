using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;

namespace RefactoringEssentials.CSharp.Diagnostics
{

    [ExportCodeFixProvider(LanguageNames.CSharp), System.Composition.Shared]
    public class ReplaceWithStringIsNullOrEmptyCodeFixProvider : CodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds
        {
            get
            {
                return ImmutableArray.Create(CSharpDiagnosticIDs.ReplaceWithStringIsNullOrEmptyAnalyzerID);
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

            // Get the replacement code from the diagnostic and fix the node.
            string replacementCodeString = diagnostic.Properties[ReplaceWithStringIsNullOrEmptyAnalyzer.ReplacementPropertyName];
            var newNode = SyntaxFactory.ParseExpression(replacementCodeString);

            var newRoot = root.ReplaceNode(node, newNode);

            var codeAction = CodeActionFactory.Create(
                node.Span,
                diagnostic.Severity,
                string.Format("Use '{0}'", replacementCodeString),
                document.WithSyntaxRoot(newRoot));

            context.RegisterCodeFix(codeAction, diagnostic);
        }
    }
}