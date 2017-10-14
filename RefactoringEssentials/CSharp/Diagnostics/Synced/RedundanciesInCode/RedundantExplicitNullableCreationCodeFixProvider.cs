using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;

namespace RefactoringEssentials.CSharp.Diagnostics
{
	[ExportCodeFixProvider(LanguageNames.CSharp), System.Composition.Shared]
    public class RedundantExplicitNullableCreationCodeFixProvider : CodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds
        {
            get
            {
                return ImmutableArray.Create(CSharpDiagnosticIDs.RedundantExplicitNullableCreationAnalyzerID);
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
            var objectCreation = root.FindNode(context.Span, getInnermostNodeForTie: true) as ObjectCreationExpressionSyntax;
            var argumentListArgument = objectCreation.ArgumentList.Arguments.FirstOrDefault();
            if (argumentListArgument == null)
                return;

            context.RegisterCodeFix(CodeActionFactory.Create(objectCreation.Span, diagnostic.Severity, "Redundant explicit nullable type creation", token =>
            {
                var newRoot = root.ReplaceNode(objectCreation,
                    argumentListArgument.Expression.WithLeadingTrivia(objectCreation.GetLeadingTrivia()).WithAdditionalAnnotations(Formatter.Annotation));

                return Task.FromResult(document.WithSyntaxRoot(newRoot));
            }), diagnostic);
        }
    }
}