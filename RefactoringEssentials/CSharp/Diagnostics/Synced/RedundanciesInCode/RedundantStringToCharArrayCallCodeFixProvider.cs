using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Formatting;

namespace RefactoringEssentials.CSharp.Diagnostics
{
	[ExportCodeFixProvider(LanguageNames.CSharp), System.Composition.Shared]
    public class RedundantStringToCharArrayCallCodeFixProvider : CodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds
        {
            get
            {
                return ImmutableArray.Create(CSharpDiagnosticIDs.RedundantStringToCharArrayCallAnalyzerID);
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
            var node = root.FindNode(context.Span).Parent;

            if (node is MemberAccessExpressionSyntax)
            {
                FixStringToCharInvocation(node as MemberAccessExpressionSyntax, diagnostic, context, root);
            }
        }

        public void FixStringToCharInvocation(MemberAccessExpressionSyntax toCharArrayInvocation, Diagnostic diagnostic, CodeFixContext context, SyntaxNode root)
        {
            context.RegisterCodeFix(CodeActionFactory.Create(toCharArrayInvocation.Span, diagnostic.Severity, "Remove redundant 'string.ToCharArray()' call",
            token =>
            {
                var newRoot = root.ReplaceNode(toCharArrayInvocation.Parent, toCharArrayInvocation.Expression
                    .WithAdditionalAnnotations(Formatter.Annotation));
                return Task.FromResult(context.Document.WithSyntaxRoot(newRoot));
            }), diagnostic);
        }
    }
}