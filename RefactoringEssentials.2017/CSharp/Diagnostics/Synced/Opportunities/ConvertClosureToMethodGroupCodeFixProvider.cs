using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CodeFixes;

namespace RefactoringEssentials.CSharp.Diagnostics
{
    [ExportCodeFixProvider(LanguageNames.CSharp), System.Composition.Shared]
    public class ConvertClosureToMethodGroupCodeFixProvider : CodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds
        {
            get
            {
                return ImmutableArray.Create(CSharpDiagnosticIDs.ConvertClosureToMethodDiagnosticID);
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
            if (node is ArgumentSyntax)
                node = ((ArgumentSyntax)node).Expression;
            var c1 = node as AnonymousMethodExpressionSyntax;
            var c2 = node as ParenthesizedLambdaExpressionSyntax;
            var c3 = node as SimpleLambdaExpressionSyntax;
            if (c1 == null && c2 == null && c3 == null)
                return;
            context.RegisterCodeFix(CodeActionFactory.Create(node.Span, diagnostic.Severity, GettextCatalog.GetString("Replace with method group"), token =>
            {
                InvocationExpressionSyntax invoke = null;
                if (c1 != null)
                    invoke = ConvertClosureToMethodGroupAnalyzer.AnalyzeBody(c1.Block);
                if (c2 != null)
                    invoke = ConvertClosureToMethodGroupAnalyzer.AnalyzeBody(c2.Body);
                if (c3 != null)
                    invoke = ConvertClosureToMethodGroupAnalyzer.AnalyzeBody(c3.Body);
                var newRoot = root.ReplaceNode((SyntaxNode)node, invoke.Expression.WithLeadingTrivia(node.GetLeadingTrivia()).WithTrailingTrivia(node.GetTrailingTrivia()));
                return Task.FromResult(document.WithSyntaxRoot(newRoot));
            }), diagnostic);
        }
    }
}