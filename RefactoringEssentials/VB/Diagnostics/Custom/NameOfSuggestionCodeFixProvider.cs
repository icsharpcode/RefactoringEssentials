using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;
using Microsoft.CodeAnalysis.VisualBasic;

namespace RefactoringEssentials.VB.Diagnostics
{
    [ExportCodeFixProvider(LanguageNames.VisualBasic), System.Composition.Shared]
    public class NameOfSuggestionCodeFixProvider : CodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds
        {
            get
            {
                return ImmutableArray.Create(VBDiagnosticIDs.NameOfSuggestionAnalyzerID);
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
            var node = root.FindToken(context.Span.Start).Parent as LiteralExpressionSyntax;
            if (node == null)
                return;

            context.RegisterCodeFix(
                CodeActionFactory.Create(
                    node.Span,
                    diagnostic.Severity,
                    string.Format(GettextCatalog.GetString("To 'NameOf({0})'"), node.Token.ValueText),
                    (token) =>
                    {
                        var newRoot = root.ReplaceNode(node, SyntaxFactory.ParseExpression("NameOf(" + node.Token.ValueText + ")").WithAdditionalAnnotations(Formatter.Annotation));
                        return Task.FromResult(document.WithSyntaxRoot(newRoot));
                    }
                ),
                diagnostic
            );
        }
    }
}