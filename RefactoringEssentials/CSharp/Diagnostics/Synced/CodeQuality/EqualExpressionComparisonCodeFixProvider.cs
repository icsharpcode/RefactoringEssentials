using Microsoft.CodeAnalysis;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CodeFixes;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;

namespace RefactoringEssentials.CSharp.Diagnostics
{

    [ExportCodeFixProvider(LanguageNames.CSharp), System.Composition.Shared]
    public class EqualExpressionComparisonCodeFixProvider : CodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds
        {
            get
            {
                return ImmutableArray.Create(CSharpDiagnosticIDs.EqualExpressionComparisonAnalyzerID);
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
            if (node is BinaryExpressionSyntax)
            {
                var newRoot = root.ReplaceNode(node, SyntaxFactory.LiteralExpression(node.IsKind(SyntaxKind.EqualsExpression) ? SyntaxKind.TrueLiteralExpression : SyntaxKind.FalseLiteralExpression));
                context.RegisterCodeFix(CodeActionFactory.Create(node.Span, diagnostic.Severity, diagnostic.GetMessage(), document.WithSyntaxRoot(newRoot)), diagnostic);
            } 
            else if (node is InvocationExpressionSyntax)
            {
                var newRoot = root.ReplaceNode(node, SyntaxFactory.LiteralExpression(SyntaxKind.TrueLiteralExpression));
                context.RegisterCodeFix(CodeActionFactory.Create(node.Span, diagnostic.Severity, diagnostic.GetMessage(), document.WithSyntaxRoot(newRoot)), diagnostic);
            } 
            else if (node is PrefixUnaryExpressionSyntax) 
            {
                var newRoot = root.ReplaceNode(node, SyntaxFactory.LiteralExpression(SyntaxKind.FalseLiteralExpression));
                context.RegisterCodeFix(CodeActionFactory.Create(node.Span, diagnostic.Severity, diagnostic.GetMessage(), document.WithSyntaxRoot(newRoot)), diagnostic);
            }
        }
    }
}