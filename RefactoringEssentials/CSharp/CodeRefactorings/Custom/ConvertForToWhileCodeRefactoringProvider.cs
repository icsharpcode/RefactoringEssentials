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
    [ExportCodeRefactoringProvider(LanguageNames.CSharp, Name = "Convert 'for' loop to 'while'")]
    public class ConvertForToWhileCodeRefactoringProvider : SpecializedCodeRefactoringProvider<ForStatementSyntax>
    {
        protected override IEnumerable<CodeAction> GetActions(Document document, SemanticModel semanticModel, SyntaxNode root, TextSpan span, ForStatementSyntax node, CancellationToken cancellationToken)
        {
            if (!node.ForKeyword.Span.Contains(span))
                return Enumerable.Empty<CodeAction>();

            return new[] { CodeActionFactory.Create(
                node.Span,
                DiagnosticSeverity.Info,
                GettextCatalog.GetString ("To 'while'"),
                t2 => {
                    var statements = new List<StatementSyntax>();
                    var blockSyntax = node.Statement as BlockSyntax;
                    if (blockSyntax != null) {
                        statements.AddRange(blockSyntax.Statements);
                    } else {
                        statements.Add(node.Statement);
                    }
                    statements.AddRange(node.Incrementors.Select(i => SyntaxFactory.ExpressionStatement(i)));

                    var whileStatement = SyntaxFactory.WhileStatement(
                            node.Condition ?? SyntaxFactory.LiteralExpression(SyntaxKind.TrueLiteralExpression),
                            SyntaxFactory.Block(statements));
                    var replaceStatements = new List<StatementSyntax>();
                    if (node.Declaration != null)
                        replaceStatements.Add(SyntaxFactory.LocalDeclarationStatement(node.Declaration).WithAdditionalAnnotations(Formatter.Annotation));

                    foreach (var init in node.Initializers) {
                        replaceStatements.Add(SyntaxFactory.ExpressionStatement(init).WithAdditionalAnnotations(Formatter.Annotation));
                    }
                    replaceStatements.Add (whileStatement.WithAdditionalAnnotations(Formatter.Annotation));
                    replaceStatements[0] = replaceStatements[0].WithLeadingTrivia(node.GetLeadingTrivia());

                    var newRoot = root.ReplaceNode((SyntaxNode)node, replaceStatements);
                    return Task.FromResult(document.WithSyntaxRoot(newRoot));
                }
            )};
        }
    }
}
