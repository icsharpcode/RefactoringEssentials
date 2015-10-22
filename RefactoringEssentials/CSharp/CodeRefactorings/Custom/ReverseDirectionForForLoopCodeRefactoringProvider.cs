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
    [ExportCodeRefactoringProvider(LanguageNames.CSharp, Name = "Reverse the direction of a for ")]
    public class ReverseDirectionForForLoopCodeRefactoringProvider : SpecializedCodeRefactoringProvider<ForStatementSyntax>
    {
        protected override IEnumerable<CodeAction> GetActions(Document document, SemanticModel semanticModel, SyntaxNode root, TextSpan span, ForStatementSyntax node, CancellationToken cancellationToken)
        {
            if (!node.ForKeyword.Span.Contains(span.Start))
                yield break;

            if (node.Initializers.Any() || node.Declaration == null || node.Declaration.Variables.Count != 1)
                yield break;
            var initializer = node.Declaration.Variables[0];
            if (initializer == null)
                yield break;
            if (node.Incrementors.Count != 1)
                yield break;

            var type = semanticModel.GetSymbolInfo(node.Declaration.Type).Symbol as ITypeSymbol;
            if (type == null || type.SpecialType != SpecialType.System_Int32)
                yield break;

            var iterator = node.Incrementors.First();
            ExpressionSyntax step;
            var direction = IsForward(iterator, initializer.Identifier.ToString(), out step);

            ExpressionSyntax newInitializer;
            var newCondition = GetNewCondition(node.Condition.SkipParens(), initializer, direction, step.SkipParens(), out newInitializer);
            if (newCondition == null)
                yield break;

            yield return CodeActionFactory.Create(
                node.Span,
                DiagnosticSeverity.Info,
                GettextCatalog.GetString("Reverse 'for' loop'"),
                t2 =>
                {
                    var newRoot = root.ReplaceNode(
                        node,
                        node
                            .WithDeclaration(
                                SyntaxFactory.VariableDeclaration(
                                    node.Declaration.Type,
                                    SyntaxFactory.SeparatedList<VariableDeclaratorSyntax>(
                                        new[] {
                                            SyntaxFactory.VariableDeclarator(initializer.Identifier, null, SyntaxFactory.EqualsValueClause (newInitializer))
                                        }
                                    )
                                )
                            )
                            .WithCondition(newCondition)
                            .WithIncrementors(SyntaxFactory.SeparatedList<ExpressionSyntax>(
                                new[] {
                                    CreateIterator(initializer.Identifier.ToString (), !direction, step)
                                }
                            ))
                            .WithAdditionalAnnotations(Formatter.Annotation)
                    );
                    return Task.FromResult(document.WithSyntaxRoot(newRoot));
                }
            );
        }

        static bool? IsForward(ExpressionSyntax expression, string name, out ExpressionSyntax step)
        {
            step = null;
            if (expression == null)
                return null;

            // forward
            if (expression.IsKind(SyntaxKind.PreIncrementExpression))
            {
                step = null;
                return true;
            }
            else if (expression.IsKind(SyntaxKind.PostIncrementExpression))
            {
                step = null;
                return true;
            }
            else if (expression.IsKind(SyntaxKind.AddAssignmentExpression))
            {
                var assignment = ((AssignmentExpressionSyntax)expression);
                if (assignment.Left.ToString() != name)
                    return null;
                step = assignment.Right;
                return true;
            }

            // backward
            if (expression.IsKind(SyntaxKind.PreDecrementExpression))
            {
                step = null;
                return false;
            }
            else if (expression.IsKind(SyntaxKind.PostDecrementExpression))
            {
                step = null;
                return false;
            }
            else if (expression.IsKind(SyntaxKind.SubtractAssignmentExpression))
            {
                var assignment = ((AssignmentExpressionSyntax)expression);
                if (assignment.Left.SkipParens().ToString() != name)
                    return null;
                step = assignment.Right;
                return false;
            }

            return null;
        }

        static ExpressionSyntax GetNewCondition(ExpressionSyntax condition, VariableDeclaratorSyntax initializer, bool? direction, ExpressionSyntax step, out ExpressionSyntax newInitializer)
        {
            var name = initializer.Identifier.ToString();
            newInitializer = null;
            var bOp = condition as BinaryExpressionSyntax;
            if (bOp == null || direction == null)
                return null;

            ExpressionSyntax bound = null;
            if (direction == true)
            {
                if (bOp.IsKind(SyntaxKind.LessThanExpression))
                {
                    if (bOp.Left.SkipParens().ToString() == name)
                        bound = bOp.Right.SkipParens();
                }
                else if (bOp.IsKind(SyntaxKind.GreaterThanExpression))
                {
                    if (bOp.Right.SkipParens().ToString() == name)
                        bound = bOp.Left.SkipParens();
                }
                if (bound != null)
                {
                    newInitializer = direction == true ? Subtract(bound, step) : bound;
                    return GetNewBound(name, false, initializer.Initializer.Value, step);
                }

                if (condition.IsKind(SyntaxKind.LessThanOrEqualExpression))
                {
                    if (bOp.Left.SkipParens().ToString() == name)
                        bound = bOp.Right.SkipParens();
                }
                else if (condition.IsKind(SyntaxKind.GreaterThanOrEqualExpression))
                {
                    if (bOp.Right.SkipParens().ToString() == name)
                        bound = bOp.Left.SkipParens();
                }

                if (bound != null)
                {
                    newInitializer = bound;
                    return GetNewBound(name, false, initializer.Initializer.Value, step);
                }
            }

            if (condition.IsKind(SyntaxKind.GreaterThanOrEqualExpression))
            {
                if (bOp.Left.SkipParens().ToString() == name)
                    bound = bOp.Right.SkipParens();
            }
            else if (condition.IsKind(SyntaxKind.LessThanOrEqualExpression))
            {
                if (bOp.Right.SkipParens().ToString() == name)
                    bound = bOp.Left.SkipParens();
            }

            if (bound == null)
                return null;
            newInitializer = direction == true ? Subtract(bound, step) : bound;
            return GetNewBound(name, true, initializer.Initializer.Value, step);
        }

        static ExpressionSyntax Subtract(ExpressionSyntax expr, ExpressionSyntax step)
        {
            if (step != null && expr.SkipParens().IsEquivalentTo(step.SkipParens()))
                return SyntaxFactory.ParseExpression("0");

            var pe = expr as LiteralExpressionSyntax;
            if (pe != null)
            {
                if (step == null)
                    return SyntaxFactory.ParseExpression(((int)pe.Token.Value - 1).ToString());
                var stepExpr = step as LiteralExpressionSyntax;
                if (stepExpr != null)
                    return SyntaxFactory.ParseExpression(((int)pe.Token.Value - (int)stepExpr.Token.Value).ToString());
            }

            var bOp = expr as BinaryExpressionSyntax;
            if (bOp != null)
            {
                if (bOp.IsKind(SyntaxKind.SubtractExpression))
                {
                    var right = Add(bOp.Right, step);
                    if (right.ToString() == "0")
                        return bOp.Left;
                    return SyntaxFactory.BinaryExpression(SyntaxKind.SubtractExpression, bOp.Left, right);
                }
                if (bOp.IsKind(SyntaxKind.AddExpression))
                {
                    var right = Subtract(bOp.Right, step);
                    if (right.ToString() == "0")
                        return bOp.Left;
                    return SyntaxFactory.BinaryExpression(SyntaxKind.AddExpression, bOp.Left, right);
                }
            }
            if (step == null)
                return SyntaxFactory.BinaryExpression(SyntaxKind.SubtractExpression, expr, SyntaxFactory.ParseExpression("1"));

            return SyntaxFactory.BinaryExpression(SyntaxKind.SubtractExpression, expr, CSharpUtil.AddParensIfRequired(step, false));
        }

        static ExpressionSyntax Add(ExpressionSyntax expr, ExpressionSyntax step)
        {
            var pe = expr as LiteralExpressionSyntax;
            if (pe != null)
            {
                if (step == null)
                    return SyntaxFactory.ParseExpression(((int)pe.Token.Value + 1).ToString());
                var stepExpr = step as LiteralExpressionSyntax;
                if (stepExpr != null)
                    return SyntaxFactory.ParseExpression(((int)pe.Token.Value + (int)stepExpr.Token.Value).ToString());
            }

            var bOp = expr as BinaryExpressionSyntax;
            if (bOp != null)
            {
                if (bOp.IsKind(SyntaxKind.AddExpression))
                {
                    var right = Add(bOp.Right, step);
                    if (right.ToString() == "0")
                        return bOp.Left;
                    return SyntaxFactory.BinaryExpression(SyntaxKind.AddExpression, bOp.Left, right);
                }
                if (bOp.IsKind(SyntaxKind.SubtractExpression))
                {
                    var right = Subtract(bOp.Right, step);
                    if (right.ToString() == "0")
                        return bOp.Left;
                    return SyntaxFactory.BinaryExpression(SyntaxKind.SubtractExpression, bOp.Left, right);
                }
            }
            if (step == null)
                return SyntaxFactory.BinaryExpression(SyntaxKind.AddExpression, expr, SyntaxFactory.ParseExpression("1"));

            return SyntaxFactory.BinaryExpression(SyntaxKind.AddExpression, expr, CSharpUtil.AddParensIfRequired(step, false));
        }

        static ExpressionSyntax GetNewBound(string name, bool? direction, ExpressionSyntax initializer, ExpressionSyntax step)
        {
            if (initializer == null)
                return null;
            return SyntaxFactory.BinaryExpression(
                direction == true ? SyntaxKind.LessThanExpression : SyntaxKind.GreaterThanOrEqualExpression,
                SyntaxFactory.IdentifierName(name),
                direction == true ? Add(initializer, step) : initializer);
        }

        static ExpressionSyntax CreateIterator(string name, bool? direction, ExpressionSyntax step)
        {
            if (direction == true)
            {
                if (step == null || step.SkipParens().ToString() == "1")
                {
                    return SyntaxFactory.PostfixUnaryExpression(SyntaxKind.PostIncrementExpression, SyntaxFactory.IdentifierName(name));
                }
                return SyntaxFactory.AssignmentExpression(SyntaxKind.AddAssignmentExpression, SyntaxFactory.IdentifierName(name), step);
            }
            if (step == null || step.SkipParens().ToString() == "1")
            {
                return SyntaxFactory.PostfixUnaryExpression(SyntaxKind.PostDecrementExpression, SyntaxFactory.IdentifierName(name));
            }
            return SyntaxFactory.AssignmentExpression(SyntaxKind.SubtractAssignmentExpression, SyntaxFactory.IdentifierName(name), step);
        }
    }
}