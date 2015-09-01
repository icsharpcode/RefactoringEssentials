using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Editing;

namespace RefactoringEssentials.CSharp.Diagnostics
{
    [ExportCodeFixProvider(LanguageNames.CSharp), System.Composition.Shared]
    public class RedundantCheckBeforeAssignmentCodeFixProvider : CodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds
        {
            get
            {
                return ImmutableArray.Create(CSharpDiagnosticIDs.RedundantCheckBeforeAssignmentAnalyzerID);
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
            var node = root.FindNode(context.Span) as BinaryExpressionSyntax;
            if (node == null)
                return;
            ExpressionStatementSyntax expression = null;
            var ifStatement = node.Parent as IfStatementSyntax;
            if (ifStatement?.Statement is BlockSyntax)
            {
                var block = (BlockSyntax)ifStatement.Statement;
                if (((ExpressionStatementSyntax)block.Statements[0])?.Expression is AssignmentExpressionSyntax)
                {
                    expression = (ExpressionStatementSyntax)block.Statements[0];
                }
            }
            else if (ifStatement?.Statement is ExpressionStatementSyntax)
            {
                expression = (ExpressionStatementSyntax)ifStatement.Statement;
            }

            if (expression == null)
                return;

            context.RegisterCodeFix(CodeAction.Create("Remove redundant check", async token =>
            {
                var editor = await DocumentEditor.CreateAsync(document, cancellationToken);
                editor.InsertBefore(ifStatement,expression);
                editor.RemoveNode(ifStatement);

                var newDocument = editor.GetChangedDocument();
                return newDocument;
            }, string.Empty), diagnostic);
        }
    }
}