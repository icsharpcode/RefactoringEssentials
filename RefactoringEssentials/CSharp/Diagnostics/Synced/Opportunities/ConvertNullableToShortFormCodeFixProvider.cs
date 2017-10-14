using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Formatting;

namespace RefactoringEssentials.CSharp.Diagnostics
{
    [ExportCodeFixProvider(LanguageNames.CSharp), System.Composition.Shared]
    public class ConvertNullableToShortFormCodeFixProvider : CodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds
        {
            get
            {
                return ImmutableArray.Create(CSharpDiagnosticIDs.ConvertNullableToShortFormAnalyzerID);
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

            //if (!node.IsKind(SyntaxKind.GenericName))
            //	continue;

            var arg = ConvertNullableToShortFormAnalyzer.GetTypeArgument(node);

            var newRoot = root.ReplaceNode((SyntaxNode)node, SyntaxFactory.NullableType(arg)
                .WithAdditionalAnnotations(Formatter.Annotation)
                .WithLeadingTrivia(node.GetLeadingTrivia()));
            context.RegisterCodeFix(CodeActionFactory.Create(node.Span, diagnostic.Severity, string.Format("Rewrite to '{0}?'", arg.ToString()), document.WithSyntaxRoot(newRoot)), diagnostic);
        }
    }
}