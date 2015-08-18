using Microsoft.CodeAnalysis;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CodeFixes;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeAnalysis.Formatting;

namespace RefactoringEssentials.CSharp.Diagnostics
{

    [ExportCodeFixProvider(LanguageNames.CSharp), System.Composition.Shared]
    public class RedundantIfElseBlockCodeFixProvider : CodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds
        {
            get
            {
                return ImmutableArray.Create(CSharpDiagnosticIDs.RedundantIfElseBlockAnalyzerID);
            }
        }

        public override FixAllProvider GetFixAllProvider()
        {
            return WellKnownFixAllProviders.BatchFixer;
        }

        public override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var document = context.Document;
            var cancellationToken = context.CancellationToken;
            var span = context.Span;
            var diagnostics = context.Diagnostics;
            var root = await document.GetSyntaxRootAsync(cancellationToken);
            var diagnostic = diagnostics.First();
            var node = root.FindNode(context.Span) as ElseClauseSyntax;
            var editor = await DocumentEditor.CreateAsync(document, cancellationToken);

            if (node == null)
                return;

            var newRoot = root.ReplaceNode(node, node.Statement
                 .WithLeadingTrivia(node.GetLeadingTrivia())
                 .WithAdditionalAnnotations(Formatter.Annotation));
            context.RegisterCodeFix(CodeActionFactory.Create(node.Span, diagnostic.Severity, "Remove redundant 'else'", document.WithSyntaxRoot(newRoot)), diagnostic);

            context.RegisterCodeFix(CodeAction.Create("Convert to 'return' statement", await token =>
            {
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
                var returnStatementToBeEliminated =
                    block.Statements.ElementAt(returnStatementAfterIfStatementIndex) as ReturnStatementSyntax;
                editor.RemoveNode(returnStatementToBeEliminated);
                var newDocument = editor.GetChangedDocument();

                return newDocument;
            }, string.Empty), diagnostic);
        }

        public async override Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var document = context.Document;
            var cancellationToken = context.CancellationToken;
            var span = context.Span;
            var diagnostics = context.Diagnostics;
            var root = await document.GetSyntaxRootAsync(cancellationToken);
            var diagnostic = diagnostics.First();
            var node = root.FindNode(context.Span) as ElseClauseSyntax;
            var editor = await DocumentEditor.CreateAsync(document, cancellationToken);


        }
    }
}