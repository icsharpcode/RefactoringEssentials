using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CodeFixes;

namespace RefactoringEssentials.CSharp.Diagnostics
{
    [ExportCodeFixProvider(LanguageNames.CSharp), System.Composition.Shared]
    public class NonPublicMethodWithTestAttributeCodeFixProvider : CodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds
        {
            get
            {
                return ImmutableArray.Create(CSharpDiagnosticIDs.NonPublicMethodWithTestAttributeAnalyzerID);
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
            var node = root.FindNode(context.Span) as MethodDeclarationSyntax;
            if (node == null)
                return;

            Func<SyntaxToken, bool> isModifierToRemove =
                m => (m.IsKind(SyntaxKind.PrivateKeyword) || m.IsKind(SyntaxKind.ProtectedKeyword) || m.IsKind(SyntaxKind.InternalKeyword));

            // Get trivia for new modifier
            var leadingTrivia = SyntaxTriviaList.Empty;
            var trailingTrivia = SyntaxTriviaList.Create(SyntaxFactory.Space);
            var removedModifiers = node.Modifiers.Where(isModifierToRemove);
            if (removedModifiers.Any())
            {
                leadingTrivia = removedModifiers.First().LeadingTrivia;
            }
            else
            {
                // Method begins directly with return type, use its leading trivia
                leadingTrivia = node.ReturnType.GetLeadingTrivia();
            }

            var newMethod = node.WithModifiers(SyntaxFactory.TokenList(new SyntaxTokenList()
                .Add(SyntaxFactory.Token(leadingTrivia, SyntaxKind.PublicKeyword, trailingTrivia))
                .AddRange(node.Modifiers.ToArray().Where(m => !isModifierToRemove(m)))))
                .WithReturnType(node.ReturnType.WithoutLeadingTrivia());
            var newRoot = root.ReplaceNode(node, newMethod);
            context.RegisterCodeFix(CodeActionFactory.Create(node.Span, diagnostic.Severity, "Make method public", document.WithSyntaxRoot(newRoot)), diagnostic);
        }
    }
}