using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CodeFixes;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;
using System.Linq;

namespace RefactoringEssentials.CSharp.Diagnostics
{

    [ExportCodeFixProvider(LanguageNames.CSharp), System.Composition.Shared]
    public class CallToObjectEqualsViaBaseCodeFixProvider : CodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds
        {
            get
            {
                return ImmutableArray.Create(CSharpDiagnosticIDs.CallToObjectEqualsViaBaseAnalyzerID);
            }
        }

        // TODO : not sure if this makes sense
        // Test and enable or add comment with reason
        //public override FixAllProvider GetFixAllProvider()
        //{
        //	return WellKnownFixAllProviders.BatchFixer;
        //}

        public async override Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var document = context.Document;
            var cancellationToken = context.CancellationToken;
            var span = context.Span;
            var diagnostics = context.Diagnostics;
            var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
            var diagnostic = diagnostics.First();
            var node = root.FindNode(context.Span) as InvocationExpressionSyntax;
            if (node == null)
                return;

            context.RegisterCodeFix(CodeActionFactory.Create(node.Span, diagnostic.Severity, "Change invocation to call 'object.ReferenceEquals'", arg =>
            {
                var arguments = new SeparatedSyntaxList<ArgumentSyntax>();
                arguments = arguments.Add(SyntaxFactory.Argument(SyntaxFactory.ThisExpression()));
                arguments = arguments.Add(node.ArgumentList.Arguments[0]);

                return Task.FromResult(document.WithSyntaxRoot(
                    root.ReplaceNode((SyntaxNode)
                        node,
                        SyntaxFactory.InvocationExpression(
                            SyntaxFactory.ParseExpression("object.ReferenceEquals"),
                            SyntaxFactory.ArgumentList(arguments)
                        )
                            .WithLeadingTrivia(node.GetLeadingTrivia())
                            .WithAdditionalAnnotations(Formatter.Annotation))
                    )
                );
            }), diagnostic);

            context.RegisterCodeFix(CodeActionFactory.Create(node.Span, diagnostic.Severity, "Remove 'base.'", arg =>
            {
                return Task.FromResult(document.WithSyntaxRoot(root.ReplaceNode((SyntaxNode)node, node.WithExpression(SyntaxFactory.IdentifierName("Equals")))));
            }), diagnostic);
        }
    }
}