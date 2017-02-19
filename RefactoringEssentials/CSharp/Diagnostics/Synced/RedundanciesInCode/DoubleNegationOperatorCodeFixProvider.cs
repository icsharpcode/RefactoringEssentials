using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CodeFixes;

namespace RefactoringEssentials.CSharp.Diagnostics
{
    [ExportCodeFixProvider(LanguageNames.CSharp), System.Composition.Shared]
    public class DoubleNegationOperatorCodeFixProvider : CodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds
        {
            get
            {
                return ImmutableArray.Create(CSharpDiagnosticIDs.DoubleNegationOperatorAnalyzerID);
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
            var n = root.FindNode(context.Span, true, true);
            var node = n as PrefixUnaryExpressionSyntax;
            if (node == null)
                return;
            var innerUnaryOperatorExpr = node.Operand.SkipParens() as PrefixUnaryExpressionSyntax;
            if (innerUnaryOperatorExpr == null)
                return;
            var newRoot = root.ReplaceNode((SyntaxNode)node, innerUnaryOperatorExpr.Operand.SkipParens());
            context.RegisterCodeFix(CodeActionFactory.Create(node.Span, diagnostic.Severity, node.IsKind(SyntaxKind.LogicalNotExpression) ? "Remove '!!'" : "Remove '~~'", document.WithSyntaxRoot(newRoot)), diagnostic);
        }
    }

}
