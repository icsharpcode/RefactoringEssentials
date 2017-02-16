using Microsoft.CodeAnalysis;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CodeFixes;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;

namespace RefactoringEssentials.CSharp.Diagnostics
{

	[ExportCodeFixProvider(LanguageNames.CSharp), System.Composition.Shared]
    public class RedundantDelegateCreationCodeFixProvider : CodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds
        {
            get
            {
                return ImmutableArray.Create(CSharpDiagnosticIDs.RedundantDelegateCreationAnalyzerID);
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
            var objectCreation = root.FindNode(context.Span) as ObjectCreationExpressionSyntax;
            var argument = objectCreation?.ArgumentList.Arguments[0];

            if (argument == null)
                return;

            var newRoot = root.ReplaceNode(objectCreation, 
                argument.Expression
                .WithoutLeadingTrivia()
                .WithAdditionalAnnotations(Formatter.Annotation));
            context.RegisterCodeFix(CodeActionFactory.Create(objectCreation.Span, diagnostic.Severity, "Remove redundant 'new'", document.WithSyntaxRoot(newRoot)), diagnostic);
        }

    }
}