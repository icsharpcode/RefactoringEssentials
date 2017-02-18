using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;

namespace RefactoringEssentials.CSharp.CodeFixes
{
    [ExportCodeFixProvider(LanguageNames.CSharp), System.Composition.Shared]
    public class CS0168LocalVariableNotUsedCodeFixProvider : CodeFixProvider
    {
        public const string CS0168 = "CS0168"; // The variable 'var' is declared but never used

        public override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(CS0168);

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
            var node = root.FindNode(context.Span) as VariableDeclaratorSyntax;
            if (node == null)
                return;
            var variableDeclarationSyntax = node.Parent as VariableDeclarationSyntax;
            if (variableDeclarationSyntax == null)
                return;
            // This is a workaround to avoid working with implausible syntax tree on syntax errors
            var block = variableDeclarationSyntax.Parent?.Parent as BlockSyntax;
            if (block == null)
                return;
            SyntaxNode nodeToRemove;
            if (variableDeclarationSyntax.Variables.Count == 1) {
                nodeToRemove = node.Parent.Parent;
            } else {
                nodeToRemove = node;
            }
            var newRoot = root.RemoveNode(nodeToRemove, SyntaxRemoveOptions.KeepNoTrivia);
            context.RegisterCodeFix(CodeActionFactory.Create(node.Span, diagnostic.Severity, "Remove unused local variable", document.WithSyntaxRoot(newRoot)), diagnostic);
        }
    }
}
