using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CodeFixes;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;

namespace RefactoringEssentials.CSharp.Diagnostics
{
    [ExportCodeFixProvider(LanguageNames.CSharp), System.Composition.Shared]
    public class ConvertToStaticTypeCodeFixProvider : CodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds
        {
            get
            {
                return ImmutableArray.Create(CSharpDiagnosticIDs.ConvertToStaticTypeAnalyzerID);
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
            var node = root.FindNode(context.Span) as ClassDeclarationSyntax;
            if (node == null)
                return;
            var sealedMod = node.Modifiers.FirstOrDefault(m => m.IsKind(SyntaxKind.SealedKeyword));
            var newRoot = root.ReplaceNode((SyntaxNode)node, node.WithModifiers(node.Modifiers.Remove(sealedMod)
                .Add(SyntaxFactory.Token(SyntaxKind.StaticKeyword).WithTrailingTrivia(SyntaxFactory.Whitespace(" "))))
                .WithLeadingTrivia(node.GetLeadingTrivia()));
            context.RegisterCodeFix(CodeActionFactory.Create(node.Span, diagnostic.Severity, "Make class static", document.WithSyntaxRoot(newRoot)), diagnostic);
        }
    }
}