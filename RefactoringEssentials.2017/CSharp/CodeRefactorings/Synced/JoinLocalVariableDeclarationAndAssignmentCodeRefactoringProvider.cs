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
    [ExportCodeRefactoringProvider(LanguageNames.CSharp, Name = "Join local variable declaration and assignment")]
    public class JoinLocalVariableDeclarationAndAssignmentCodeRefactoringProvider : SpecializedCodeRefactoringProvider<VariableDeclaratorSyntax>
    {
        protected override IEnumerable<CodeAction> GetActions(Document document, SemanticModel semanticModel, SyntaxNode root, TextSpan span, VariableDeclaratorSyntax node, CancellationToken cancellationToken)
        {
            var variableDecl = node.Parent.Parent as LocalDeclarationStatementSyntax;
            if (variableDecl == null || node.Initializer != null)
                yield break;
            var block = variableDecl.Parent as BlockSyntax;
            if (block == null)
                yield break;
            StatementSyntax nextStatement = null;
            for (int i = 0; i < block.Statements.Count; i++)
            {
                if (block.Statements[i] == variableDecl && i + 1 < block.Statements.Count)
                {
                    nextStatement = block.Statements[i + 1];
                    break;
                }
            }
            var expr = nextStatement as ExpressionStatementSyntax;
            if (expr == null)
                yield break;
            var assignment = expr.Expression as AssignmentExpressionSyntax;
            if (assignment == null || assignment.Left.ToString() != node.Identifier.ToString())
                yield break;

            yield return
                CodeActionFactory.Create(
                    span,
                    DiagnosticSeverity.Info,
                    GettextCatalog.GetString("Join declaration and assignment"),
                    t2 =>
                    {
                        var trackedRoot = root.TrackNodes(new SyntaxNode[] { node, nextStatement });
                        var newRoot = trackedRoot.ReplaceNode((SyntaxNode)
                            trackedRoot.GetCurrentNode(node),
                            node.WithInitializer(SyntaxFactory.EqualsValueClause(assignment.Right)).WithAdditionalAnnotations(Formatter.Annotation)
                        );
                        newRoot = newRoot.RemoveNode(newRoot.GetCurrentNode(nextStatement), SyntaxRemoveOptions.KeepNoTrivia);
                        return Task.FromResult(document.WithSyntaxRoot(newRoot));
                    }
                );
        }
    }
}