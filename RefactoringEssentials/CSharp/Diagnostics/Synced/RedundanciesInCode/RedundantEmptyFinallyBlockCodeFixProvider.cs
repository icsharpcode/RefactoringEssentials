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
    public class RedundantEmptyFinallyBlockCodeFixProvider : CodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds
        {
            get
            {
                return ImmutableArray.Create(CSharpDiagnosticIDs.RedundantEmptyFinallyBlockAnalyzerID);
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
            var node = root.FindToken(context.Span.Start).Parent as FinallyClauseSyntax;
            if (node == null)
                return;
            var tryStatement = node.Parent as TryStatementSyntax;
            if (tryStatement == null)
                return;
            SyntaxNode newRoot;

            if (tryStatement.Catches.Count > 0)
            {
                newRoot = root.ReplaceNode(tryStatement, tryStatement.WithFinally(null).WithAdditionalAnnotations(Formatter.Annotation));
            }
            else {
                newRoot = root.ReplaceNode(tryStatement, tryStatement.Block.Statements.Select(s => s.WithAdditionalAnnotations(Formatter.Annotation)));
            }

            context.RegisterCodeFix(CodeActionFactory.Create(node.Span, diagnostic.Severity, "Remove 'finally'", document.WithSyntaxRoot(newRoot)), diagnostic);
        }
    }
}