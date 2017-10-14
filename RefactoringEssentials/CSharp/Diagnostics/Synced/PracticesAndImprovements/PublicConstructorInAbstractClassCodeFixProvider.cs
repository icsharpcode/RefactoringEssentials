using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;

namespace RefactoringEssentials.CSharp.Diagnostics
{
    [ExportCodeFixProvider(LanguageNames.CSharp), System.Composition.Shared]
    public class PublicConstructorInAbstractClassCodeFixProvider : CodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds
        {
            get
            {
                return ImmutableArray.Create(CSharpDiagnosticIDs.PublicConstructorInAbstractClassAnalyzerID);
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
            var constructor = root.FindToken(span.Start).Parent.AncestorsAndSelf().OfType<ConstructorDeclarationSyntax>().First();
            context.RegisterCodeFix(CodeActionFactory.Create(span, diagnostic.Severity, "Make constructor protected", delegate
            {
                var publicToken = constructor.Modifiers.First(m => m.IsKind(SyntaxKind.PublicKeyword));
                var newConstructor = constructor.WithModifiers(constructor.Modifiers.Replace(publicToken, SyntaxFactory.Token(publicToken.LeadingTrivia, SyntaxKind.ProtectedKeyword,
                                                                                                                                 publicToken.TrailingTrivia)));
                var newRoot = root.ReplaceNode(constructor, newConstructor);
                return Task.FromResult(document.WithSyntaxRoot(newRoot));
            }), diagnostic);
            context.RegisterCodeFix(CodeActionFactory.Create(span, diagnostic.Severity, "Make constructor private", delegate
            {
                var publicToken = constructor.Modifiers.First(m => m.IsKind(SyntaxKind.PublicKeyword));
                var newConstructor = constructor.WithModifiers(constructor.Modifiers.Remove(publicToken));
                var newRoot = root.ReplaceNode(constructor, newConstructor);
                return Task.FromResult(document.WithSyntaxRoot(newRoot));
            }), diagnostic);
        }
    }
}