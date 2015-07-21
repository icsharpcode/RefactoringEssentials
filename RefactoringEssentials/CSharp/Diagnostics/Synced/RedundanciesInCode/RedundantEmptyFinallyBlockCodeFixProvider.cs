using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;

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
            var root = await document.GetSyntaxRootAsync(cancellationToken);
            var diagnostic = diagnostics.First();
            var node = root.FindNode(context.Span);
            //if (!node.IsKind(SyntaxKind.BaseList))
            //	continue;
            var tryStatement = (node as FinallyClauseSyntax).Parent as TryStatementSyntax;
            if (tryStatement != null && !tryStatement.Catches.Any())
            {
                FixEmptyFinallyWithoutCatchClause(node as FinallyClauseSyntax, tryStatement, diagnostic, context, root);
            }
            else if (tryStatement != null && tryStatement.Catches.Any())
            {
                FixEmptyFinallyWithCatchClause(node as FinallyClauseSyntax, diagnostic, context, root);
            }
            var newRoot = root.RemoveNode(node, SyntaxRemoveOptions.KeepNoTrivia);
            context.RegisterCodeFix(CodeActionFactory.Create(node.Span, diagnostic.Severity, "Remove 'finally'", document.WithSyntaxRoot(newRoot)), diagnostic);
        }

        public void FixEmptyFinallyWithoutCatchClause(FinallyClauseSyntax finallyClause, TryStatementSyntax tryStatement, Diagnostic diagnostic,
            CodeFixContext context, SyntaxNode root)
        {
            context.RegisterCodeFix(CodeActionFactory.Create(finallyClause.Span, diagnostic.Severity,
            "Remove redundant 'finally' ", token =>
            {
                var blockSyntax = tryStatement.Block;
                var newRoot = root.ReplaceNode(tryStatement, blockSyntax.WithoutLeadingTrivia());

                return Task.FromResult(context.Document.WithSyntaxRoot(newRoot));
            }), diagnostic);
        }

        public void FixEmptyFinallyWithCatchClause(FinallyClauseSyntax finallyClause, Diagnostic diagnostic,
            CodeFixContext context, SyntaxNode root)
        {
            context.RegisterCodeFix(CodeActionFactory.Create(finallyClause.Span, diagnostic.Severity,
            "Remove redundant 'finally' ", token =>
            {
                var newRoot = root.RemoveNode(finallyClause, SyntaxRemoveOptions.KeepNoTrivia);
                return Task.FromResult(context.Document.WithSyntaxRoot(newRoot));
            }), diagnostic);
        }
    }
}