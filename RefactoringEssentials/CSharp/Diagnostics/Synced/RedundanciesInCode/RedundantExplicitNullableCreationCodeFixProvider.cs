using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
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
            var root = await document.GetSyntaxRootAsync(cancellationToken);
            var diagnostic = diagnostics.First();
            var objectCreation = root.FindNode(context.Span) as ObjectCreationExpressionSyntax;
            var argumentList = objectCreation.ChildNodes().OfType<ArgumentListSyntax>().FirstOrDefault();

            if (argumentList == null)
                return;

            context.RegisterCodeFix(CodeActionFactory.Create(objectCreation.Span, diagnostic.Severity, "Redundant explicit nullable type creation", token =>
            {
                var newNode = SyntaxFactory.ObjectCreationExpression(objectCreation.NewKeyword, objectCreation.Type,
                    argumentList, objectCreation.Initializer);

                var newRoot = root.ReplaceNode(objectCreation,
                    newNode.WithLeadingTrivia(objectCreation.GetLeadingTrivia())
                        .WithAdditionalAnnotations(Formatter.Annotation));

                return Task.FromResult(document.WithSyntaxRoot(newRoot));
            }), diagnostic);
        }
    }
}