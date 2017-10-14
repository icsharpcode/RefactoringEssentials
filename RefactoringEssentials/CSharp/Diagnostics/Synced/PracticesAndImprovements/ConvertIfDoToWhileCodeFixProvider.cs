using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Formatting;

namespace RefactoringEssentials.CSharp.Diagnostics
{
    [ExportCodeFixProvider(LanguageNames.CSharp), System.Composition.Shared]
    public class ConvertIfDoToWhileCodeFixProvider : CodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds
        {
            get
            {
                return ImmutableArray.Create(CSharpDiagnosticIDs.ConvertIfDoToWhileAnalyzerID);
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
            var node = root.FindNode(context.Span) as IfStatementSyntax;
            if (node == null)
                return;

            context.RegisterCodeFix(CodeActionFactory.Create(node.Span, diagnostic.Severity, "Replace with 'while'", token =>
            {
                var newNode = SyntaxFactory.WhileStatement(
                    node.Condition,
                    ConvertIfDoToWhileAnalyzer.GetEmbeddedDoStatement(node.Statement).Statement
                );
                var newRoot = root.ReplaceNode((SyntaxNode)node, newNode.WithLeadingTrivia(node.GetLeadingTrivia()).WithAdditionalAnnotations(Formatter.Annotation));
                return Task.FromResult(document.WithSyntaxRoot(newRoot));
            }), diagnostic);
        }
    }
}