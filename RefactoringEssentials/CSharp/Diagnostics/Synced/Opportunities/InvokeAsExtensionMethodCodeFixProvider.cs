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
                return ImmutableArray.Create(NRefactoryDiagnosticIDs.InvokeAsExtensionMethodAnalyzerID);
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
            var root = await document.GetSyntaxRootAsync(cancellationToken);
            var diagnostic = diagnostics.First();
            var node = root.FindNode(context.Span).Parent.Parent as InvocationExpressionSyntax;
            if (node == null)
                return;
            var newRoot = root.ReplaceNode((SyntaxNode)node, node.WithArgumentList(node.ArgumentList.WithArguments(node.ArgumentList.Arguments.RemoveAt(0)))
                .WithExpression(((MemberAccessExpressionSyntax)node.Expression).WithExpression(node.ArgumentList.Arguments.First().Expression))
                .WithLeadingTrivia(node.GetLeadingTrivia()));
            context.RegisterCodeFix(CodeActionFactory.Create(node.Span, diagnostic.Severity, "Convert to extension method call", document.WithSyntaxRoot(newRoot)), diagnostic);
        }
    }
}