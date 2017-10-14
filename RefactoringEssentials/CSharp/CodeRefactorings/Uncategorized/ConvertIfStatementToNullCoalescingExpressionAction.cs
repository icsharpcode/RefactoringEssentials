using System.Linq;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Formatting;

namespace RefactoringEssentials.CSharp.CodeRefactorings
{
    [ExportCodeRefactoringProvider(LanguageNames.CSharp, Name = "Convert 'if' to '??' expression")]
    public class ConvertIfStatementToNullCoalescingExpressionAction : CodeRefactoringProvider
    {
        public override async Task ComputeRefactoringsAsync(CodeRefactoringContext context)
        {
            var document = context.Document;
            var span = context.Span;
            var cancellationToken = context.CancellationToken;
            var model = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);
            if (model.IsFromGeneratedCode(cancellationToken))
                return;
            var root = await model.SyntaxTree.GetRootAsync(cancellationToken).ConfigureAwait(false);

            var node = root.FindNode(span) as IfStatementSyntax;
            if (node == null || !node.IfKeyword.Span.Contains(span))
                return;

            ExpressionSyntax rightSide;
            var comparedNode = CheckNode(node, out rightSide);
            if (comparedNode == null)
                return;

            context.RegisterRefactoring(
                CodeActionFactory.Create(
                    span,
                    DiagnosticSeverity.Info,
                    GettextCatalog.GetString("Replace with '??'"),
                    t2 =>
                    {
                        //get the node prior to this
                        var previousNode = node.Parent.ChildThatContainsPosition(node.GetLeadingTrivia().Min(t => t.SpanStart) - 1).AsNode();
                        var previousDecl = previousNode as VariableDeclarationSyntax;
                        var prevVarDecl = previousNode as LocalDeclarationStatementSyntax;
                        if (prevVarDecl != null)
                            previousDecl = prevVarDecl.Declaration;
                        SyntaxNode newRoot = null;
                        if (previousDecl != null && previousDecl.Variables.Count == 1)
                        {
                            var variable = previousDecl.Variables.First();
                            var comparedNodeIdentifierExpression = comparedNode as IdentifierNameSyntax;
                            if (comparedNodeIdentifierExpression != null && comparedNodeIdentifierExpression.Identifier.ValueText == variable.Identifier.ValueText)
                            {
                                var initialiser = variable.WithInitializer(SyntaxFactory.EqualsValueClause(SyntaxFactory.BinaryExpression(SyntaxKind.CoalesceExpression, variable.Initializer.Value, rightSide)));
                                newRoot = root.ReplaceNode((SyntaxNode)variable, initialiser.WithAdditionalAnnotations(Formatter.Annotation));
                            }
                        }
                        else
                        {
                            var previousExpressionStatement = previousNode as ExpressionStatementSyntax;
                            if (previousExpressionStatement != null)
                            {
                                var previousAssignment = previousExpressionStatement.Expression as AssignmentExpressionSyntax;
                                if (previousAssignment != null && comparedNode.IsEquivalentTo(previousAssignment.Left, true))
                                {
                                    newRoot = root.ReplaceNode((SyntaxNode)previousAssignment.Right, SyntaxFactory.BinaryExpression(SyntaxKind.CoalesceExpression, previousAssignment.Right, rightSide).WithAdditionalAnnotations(Formatter.Annotation));
                                }
                            }
                            else
                            {
                                newRoot = root.ReplaceNode((SyntaxNode)node,
                                    SyntaxFactory.ExpressionStatement(SyntaxFactory.AssignmentExpression(SyntaxKind.SimpleAssignmentExpression, comparedNode, SyntaxFactory.BinaryExpression(SyntaxKind.CoalesceExpression, comparedNode, rightSide)))
                                    .WithAdditionalAnnotations(Formatter.Annotation)
                                );
                            }
                        }
                        var ifStatementToRemove = newRoot.DescendantNodes().OfType<IfStatementSyntax>().FirstOrDefault(m => m.IsEquivalentTo(node));
                        if (ifStatementToRemove != null)
                            newRoot = newRoot.RemoveNode(ifStatementToRemove, SyntaxRemoveOptions.KeepNoTrivia);

                        return Task.FromResult(document.WithSyntaxRoot(newRoot));
                    }
                )
            );
        }

        static ExpressionSyntax CheckNode(IfStatementSyntax node, out ExpressionSyntax rightSide)
        {
            rightSide = null;
            var condition = node.Condition as BinaryExpressionSyntax;
            if (condition == null || !(condition.IsKind(SyntaxKind.EqualsExpression) || condition.IsKind(SyntaxKind.NotEqualsExpression)))
                return null;
            var nullSide = condition.Right as LiteralExpressionSyntax;
            bool nullIsRight = true;
            if (nullSide == null || !nullSide.IsKind(SyntaxKind.NullLiteralExpression))
            {
                nullSide = condition.Left as LiteralExpressionSyntax;
                nullIsRight = false;
                if (nullSide == null || !nullSide.IsKind(SyntaxKind.NullLiteralExpression))
                    return null;
            }
            bool isEquality = condition.IsKind(SyntaxKind.EqualsExpression);
            StatementSyntax contentStatement;
            if (isEquality)
            {
                contentStatement = node.Statement;
                if (node.Else != null && !IsEmpty(node.Else.Statement))
                    return null;
            }
            else
            {
                contentStatement = node.Else != null ? node.Else.Statement : null;
                if (!IsEmpty(node.Statement))
                    return null;
            }

            contentStatement = GetSimpleStatement(contentStatement);
            if (contentStatement == null)
                return null;

            var assignExpr = (contentStatement as ExpressionStatementSyntax)?.Expression as AssignmentExpressionSyntax;
            if (assignExpr == null || !assignExpr.Left.IsEquivalentTo(nullIsRight ? condition.Left : condition.Right, true))
                return null;
            rightSide = assignExpr.Right;
            return nullIsRight ? condition.Left : condition.Right;
        }

        internal static StatementSyntax GetSimpleStatement(StatementSyntax statement)
        {
            BlockSyntax block;
            while ((block = statement as BlockSyntax) != null)
            {
                var statements = block.DescendantNodes().OfType<StatementSyntax>().Where(desc => !IsEmpty(desc)).ToList();

                if (statements.Count != 1)
                    return null;
                statement = statements.First();
            }
            return statement;
        }

        internal static bool IsEmpty(SyntaxNode node)
        {
            //don't include self
            return node == null || node.DescendantNodesAndSelf().All(d => (d is EmptyStatementSyntax || d is BlockSyntax));
        }
    }
}
