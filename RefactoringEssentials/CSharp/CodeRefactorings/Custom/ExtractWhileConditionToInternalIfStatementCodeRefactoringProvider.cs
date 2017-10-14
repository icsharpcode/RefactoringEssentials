using System.Linq;
using System.Threading;
using System.Collections.Generic;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.Text;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Formatting;

namespace RefactoringEssentials.CSharp.CodeRefactorings
{
    [ExportCodeRefactoringProvider(LanguageNames.CSharp, Name = "Extract field")]
    public class ExtractWhileConditionToInternalIfStatementCodeRefactoringProvider : SpecializedCodeRefactoringProvider<WhileStatementSyntax>
    {
        protected override IEnumerable<CodeAction> GetActions(Document document, SemanticModel semanticModel, SyntaxNode root, TextSpan span, WhileStatementSyntax node, CancellationToken cancellationToken)
        {
            if (!node.WhileKeyword.Span.Contains(span) || node.Statement == null || node.Condition == null)
                return Enumerable.Empty<CodeAction>();

            return new[] {
                CodeActionFactory.Create(
                    span,
                    DiagnosticSeverity.Info,
                    GettextCatalog.GetString ("Extract condition to internal 'if' statement"),
                    t2 => {
                        var ifStmt = SyntaxFactory.IfStatement(
                            CSharpUtil.InvertCondition(node.Condition),
                            SyntaxFactory.BreakStatement()
                        );

                        var statements = new List<StatementSyntax> ();
                        statements.Add(ifStmt);
                        var existingBlock = node.Statement as BlockSyntax;
                        if (existingBlock != null) {
                            statements.AddRange(existingBlock.Statements);
                        } else if (!node.Statement.IsKind(SyntaxKind.EmptyStatement)){
                            statements.Add(node.Statement);
                        }
                        var newNode = node.WithCondition(SyntaxFactory.LiteralExpression(SyntaxKind.TrueLiteralExpression))
                            .WithStatement(SyntaxFactory.Block(statements))
                            .WithAdditionalAnnotations(Formatter.Annotation);
                        var newRoot = root.ReplaceNode((SyntaxNode)node, newNode);
                        return Task.FromResult(document.WithSyntaxRoot(newRoot));
                    }
                )
            };
        }
    }
}
