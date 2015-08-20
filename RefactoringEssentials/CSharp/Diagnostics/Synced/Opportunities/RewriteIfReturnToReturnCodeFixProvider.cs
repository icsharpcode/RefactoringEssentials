using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.Editing;

namespace RefactoringEssentials.CSharp.Diagnostics
{
    [ExportCodeFixProvider(LanguageNames.CSharp), System.Composition.Shared]
    public class RewriteIfReturnToReturnCodeFixProvider : CodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds
        {
            get
            {
                return ImmutableArray.Create(CSharpDiagnosticIDs.RewriteIfReturnToReturnAnalyzerID);
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
            if (node == null)
                return;

            context.RegisterCodeFix(CodeAction.Create("Convert to 'return' statement", async token =>
            {
                var editor = await DocumentEditor.CreateAsync(document, cancellationToken);
                var statementCondition = (node as IfStatementSyntax)?.Condition;
                var newReturn = SyntaxFactory.ReturnStatement(SyntaxFactory.Token(SyntaxKind.ReturnKeyword),
                    statementCondition, SyntaxFactory.Token(SyntaxKind.SemicolonToken));
                editor.ReplaceNode(node as IfStatementSyntax, newReturn
                    .WithLeadingTrivia(node.GetLeadingTrivia())
                    .WithAdditionalAnnotations(Formatter.Annotation));


                var block = node.Parent as BlockSyntax;
                if (block == null)
                    return null;

                var returnStatementAfterIfStatementIndex = block.Statements.IndexOf(node as IfStatementSyntax) + 1;
                var returnStatementToBeEliminated = block.Statements.ElementAt(returnStatementAfterIfStatementIndex) as ReturnStatementSyntax;
                editor.RemoveNode(returnStatementToBeEliminated);
                var newDocument = editor.GetChangedDocument();

                return newDocument;
            }, string.Empty), diagnostic);
        }
    }
}