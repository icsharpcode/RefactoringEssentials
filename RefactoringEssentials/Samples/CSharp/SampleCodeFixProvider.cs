using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using RefactoringEssentials.CSharp.Diagnostics;

namespace RefactoringEssentials.Samples.CSharp
{
	// PLEASE UNCOMMENT THIS LINE TO REGISTER CODE FIX IN IDE.
	//[ExportCodeFixProvider(LanguageNames.CSharp), System.Composition.Shared]
	public class SampleCodeFixProvider : CodeFixProvider
    {
        // A CodeFixProvider is a complement of an analyzer providing a fix for the
        // potential code issue. To link to the correct analyzer its diagnostic ID must
        // be returned by the overridden FixableDiagnosticIds property.

        public override ImmutableArray<string> FixableDiagnosticIds
        {
            get
            {
                return ImmutableArray.Create(CSharpDiagnosticIDs.SampleAnalyzerID);
            }
        }

        // Overriding GetFixAllProvider() is optional, but can be used to
        // allow to apply the fix to all occurrences of this code issue
        // in document, project or whole solution.
        public override FixAllProvider GetFixAllProvider()
        {
            return WellKnownFixAllProviders.BatchFixer;
        }

        public async override Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            // Get some initial variables needed later
            var document = context.Document;
            var cancellationToken = context.CancellationToken;
            var span = context.Span;
            var diagnostics = context.Diagnostics;
            var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);

            // This is an instance of Diagnostic instance created by corresponding analyzer
            var diagnostic = diagnostics.First();

            // Get the node which has been marked by analyzer.
            // In this example that must have been the class name identifier (see SampleAnalyzer.cs).
            // But since the identifier is a SyntaxToken, FindNode() returns the innermost SyntaxNode, which is the ClassDeclarationSyntax.
            var node = root.FindNode(context.Span) as ClassDeclarationSyntax;
            if (node == null)
                return;

            // New name does not contain the "C" prefix (just cut it)
            string newName = node.Identifier.ValueText.Substring(1);
            context.RegisterCodeFix(CodeActionFactory.Create(node.Span, diagnostic.Severity, "Sample code fix: Remove 'C' prefix", token =>
            {
                // Applying a fix means replacing parts of syntax tree with our new elements.

                // Note: Every call to any .With...() method creates a completely new syntax tree object,
                // which is not part of current document anymore! Finally we create a completely new document
                // based on a new syntax tree containing the result of our refactoring.

                // In our sample we replace the class declaration with a new class declaration containing the new name.
                // Note: Always try to preserve formatting when replacing nodes: Copy leading and trailing trivia from original node to the new one.
                var newRoot = root.ReplaceNode(node, node.WithIdentifier(
                    SyntaxFactory.Identifier(newName).WithLeadingTrivia(node.Identifier.LeadingTrivia).WithTrailingTrivia(node.Identifier.TrailingTrivia)));
                return Task.FromResult(document.WithSyntaxRoot(newRoot));
            }), diagnostic);
        }
    }
}