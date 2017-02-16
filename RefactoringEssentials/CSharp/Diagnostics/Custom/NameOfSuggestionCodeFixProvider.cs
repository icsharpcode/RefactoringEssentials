using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;

namespace RefactoringEssentials.CSharp.Diagnostics
{
    [ExportCodeFixProvider(LanguageNames.CSharp), System.Composition.Shared]
    public class NameOfSuggestionCodeFixProvider : CodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds
        {
            get
            {
                return ImmutableArray.Create(CSharpDiagnosticIDs.NameOfSuggestionAnalyzerID);
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
                    string.Format(GettextCatalog.GetString("To 'nameof({0})'"), node.Token.ValueText),
                    (token) =>
                    {
                        var newRoot = root.ReplaceNode(node, SyntaxFactory.ParseExpression("nameof(" + node.Token.ValueText + ")").WithAdditionalAnnotations(Formatter.Annotation));
                        return Task.FromResult(document.WithSyntaxRoot(newRoot));
                    }
                ),
                diagnostic
            );
        }
    }
}