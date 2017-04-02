using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CodeFixes;

namespace RefactoringEssentials.CSharp.Diagnostics
{
    [ExportCodeFixProvider(LanguageNames.CSharp), System.Composition.Shared]
    public class InvokeAsExtensionMethodCodeFixProvider : CodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds
        {
            get
            {
                return ImmutableArray.Create(CSharpDiagnosticIDs.InvokeAsExtensionMethodAnalyzerID);
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
            var node = root.FindNode(context.Span).Parent.Parent as InvocationExpressionSyntax;
            if (node == null)
                return;
            var invocationTarget = ((MemberAccessExpressionSyntax)node.Expression)
                .WithExpression(CSharpUtil.AddParensIfRequired(node.ArgumentList.Arguments.First().Expression));
            var newNode = node
                .WithArgumentList(node.ArgumentList.WithArguments(node.ArgumentList.Arguments.RemoveAt(0)))
                .WithExpression(invocationTarget)
                .WithLeadingTrivia(node.GetLeadingTrivia());
            var newRoot = root.ReplaceNode(node, newNode);
            context.RegisterCodeFix(CodeActionFactory.Create(node.Span, diagnostic.Severity, "Convert to extension method call", document.WithSyntaxRoot(newRoot)), diagnostic);
        }
    }
}