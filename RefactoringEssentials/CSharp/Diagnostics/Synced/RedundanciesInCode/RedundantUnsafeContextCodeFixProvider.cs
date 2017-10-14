using Microsoft.CodeAnalysis;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CodeFixes;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;

namespace RefactoringEssentials.CSharp.Diagnostics
{

    [ExportCodeFixProvider(LanguageNames.CSharp), System.Composition.Shared]
    public class RedundantUnsafeContextCodeFixProvider : CodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds
        {
            get
            {
                return ImmutableArray.Create(CSharpDiagnosticIDs.RedundantUnsafeContextAnalyzerID);
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
            var token = root.FindToken(context.Span.Start);
            var node = token.Parent;
            if (node.IsKind(SyntaxKind.ClassDeclaration)) {
                var decl = node as ClassDeclarationSyntax;
                var newRoot = root.ReplaceNode(decl, decl.WithModifiers(SyntaxFactory.TokenList(decl.Modifiers.Where(m => !m.IsKind(SyntaxKind.UnsafeKeyword)))));
                context.RegisterCodeFix(CodeActionFactory.Create(token.Span, diagnostic.Severity, "Remove redundant 'unsafe' modifier", document.WithSyntaxRoot(newRoot)), diagnostic);
            }
            if (node.IsKind(SyntaxKind.StructDeclaration)) {
                var decl = node as StructDeclarationSyntax;
                var newRoot = root.ReplaceNode(decl, decl.WithModifiers(SyntaxFactory.TokenList(decl.Modifiers.Where(m => !m.IsKind(SyntaxKind.UnsafeKeyword)))));
                context.RegisterCodeFix(CodeActionFactory.Create(token.Span, diagnostic.Severity, "Remove redundant 'unsafe' modifier", document.WithSyntaxRoot(newRoot)), diagnostic);
            }
            if (node.IsKind(SyntaxKind.UnsafeStatement)) {
                var decl = node as UnsafeStatementSyntax;
                var newRoot = root.ReplaceNode(decl, decl.Block.Statements.Select(s => s.WithAdditionalAnnotations(Formatter.Annotation)));
                context.RegisterCodeFix(CodeActionFactory.Create(token.Span, diagnostic.Severity, "Replace 'unsafe' statement with its body", document.WithSyntaxRoot(newRoot)), diagnostic);
            }
        }
    }
}