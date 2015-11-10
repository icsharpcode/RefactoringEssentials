using System;
using RefactoringEssentials;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis;

namespace RefactoringEssentials
{
    class ReflectionNamespaces
    {
        public const string WorkspacesAsmName = ", Microsoft.CodeAnalysis.Workspaces";
        public const string CSWorkspacesAsmName = ", Microsoft.CodeAnalysis.CSharp.Workspaces";
        public const string CAAsmName = ", Microsoft.CodeAnalysis";
        public const string CACSharpAsmName = ", Microsoft.CodeAnalysis.CSharp";
    }

    static class CSharpUtil
    {
        /// <summary>
        /// Inverts a boolean condition. Note: The condition object can be frozen (from AST) it's cloned internally.
        /// </summary>
        /// <param name="condition">The condition to invert.</param>
        public static ExpressionSyntax InvertCondition(ExpressionSyntax condition)
        {
            return InvertConditionInternal(condition);
        }

        static ExpressionSyntax InvertConditionInternal(ExpressionSyntax condition)
        {
            if (condition is ParenthesizedExpressionSyntax)
            {
                return SyntaxFactory.ParenthesizedExpression(InvertCondition(((ParenthesizedExpressionSyntax)condition).Expression));
            }

            if (condition is PrefixUnaryExpressionSyntax)
            {
                var uOp = (PrefixUnaryExpressionSyntax)condition;
                if (uOp.IsKind(SyntaxKind.LogicalNotExpression))
                {
                    if (!(uOp.Parent is ExpressionSyntax))
                        return uOp.Operand.SkipParens();
                    return uOp.Operand;
                }
                return SyntaxFactory.PrefixUnaryExpression(SyntaxKind.LogicalNotExpression, uOp);
            }

            if (condition is BinaryExpressionSyntax)
            {
                var bOp = (BinaryExpressionSyntax)condition;

                if (bOp.IsKind(SyntaxKind.LogicalAndExpression) || bOp.IsKind(SyntaxKind.LogicalOrExpression))
                    return SyntaxFactory.BinaryExpression(NegateConditionOperator(bOp.Kind()), InvertCondition(bOp.Left), InvertCondition(bOp.Right));

                if (bOp.IsKind(SyntaxKind.EqualsExpression) ||
                    bOp.IsKind(SyntaxKind.NotEqualsExpression) ||
                    bOp.IsKind(SyntaxKind.GreaterThanExpression) ||
                    bOp.IsKind(SyntaxKind.GreaterThanOrEqualExpression) ||
                    bOp.IsKind(SyntaxKind.LessThanExpression) ||
                    bOp.IsKind(SyntaxKind.LessThanOrEqualExpression))
                    return SyntaxFactory.BinaryExpression(NegateRelationalOperator(bOp.Kind()), bOp.Left, bOp.Right);

                return SyntaxFactory.PrefixUnaryExpression(SyntaxKind.LogicalNotExpression, SyntaxFactory.ParenthesizedExpression(condition));
            }

            if (condition is ConditionalExpressionSyntax)
            {
                var cEx = condition as ConditionalExpressionSyntax;
                return cEx.WithCondition(InvertCondition(cEx.Condition));
            }

            if (condition is LiteralExpressionSyntax)
            {
                if (condition.Kind() == SyntaxKind.TrueLiteralExpression)
                    return SyntaxFactory.LiteralExpression(SyntaxKind.FalseLiteralExpression);
                if (condition.Kind() == SyntaxKind.FalseLiteralExpression)
                    return SyntaxFactory.LiteralExpression(SyntaxKind.TrueLiteralExpression);
            }

            return SyntaxFactory.PrefixUnaryExpression(SyntaxKind.LogicalNotExpression, AddParensIfRequired(condition, false));
        }

        /// <summary>
        /// When negating an expression this is required, otherwise you would end up with
        /// a or b -> !a or b
        /// </summary>
        public static ExpressionSyntax AddParensIfRequired(ExpressionSyntax expression, bool parenthesesRequiredForUnaryExpressions = true)
        {
            if ((expression is BinaryExpressionSyntax) ||
                (expression is AssignmentExpressionSyntax) ||
                (expression is CastExpressionSyntax) ||
                (expression is ParenthesizedLambdaExpressionSyntax) ||
                (expression is SimpleLambdaExpressionSyntax) ||
                (expression is ConditionalExpressionSyntax))
            {
                return SyntaxFactory.ParenthesizedExpression(expression);
            }

            if (parenthesesRequiredForUnaryExpressions &&
                ((expression is PostfixUnaryExpressionSyntax) ||
                (expression is PrefixUnaryExpressionSyntax)))
            {
                return SyntaxFactory.ParenthesizedExpression(expression);
            }

            return expression;
        }

        /// <summary>
        /// Get negation of the specified relational operator
        /// </summary>
        /// <returns>
        /// negation of the specified relational operator, or BinaryOperatorType.Any if it's not a relational operator
        /// </returns>
        public static SyntaxKind NegateRelationalOperator(SyntaxKind op)
        {
            switch (op)
            {
                case SyntaxKind.EqualsExpression:
                    return SyntaxKind.NotEqualsExpression;
                case SyntaxKind.NotEqualsExpression:
                    return SyntaxKind.EqualsExpression;
                case SyntaxKind.GreaterThanExpression:
                    return SyntaxKind.LessThanOrEqualExpression;
                case SyntaxKind.GreaterThanOrEqualExpression:
                    return SyntaxKind.LessThanExpression;
                case SyntaxKind.LessThanExpression:
                    return SyntaxKind.GreaterThanOrEqualExpression;
                case SyntaxKind.LessThanOrEqualExpression:
                    return SyntaxKind.GreaterThanExpression;
                case SyntaxKind.LogicalOrExpression:
                    return SyntaxKind.LogicalAndExpression;
                case SyntaxKind.LogicalAndExpression:
                    return SyntaxKind.LogicalOrExpression;
            }
            throw new ArgumentOutOfRangeException("op");
        }

        /// <summary>
        /// Returns true, if the specified operator is a relational operator
        /// </summary>
        public static bool IsRelationalOperator(SyntaxKind op)
        {
            switch (op)
            {
                case SyntaxKind.EqualsExpression:
                case SyntaxKind.NotEqualsExpression:
                case SyntaxKind.GreaterThanExpression:
                case SyntaxKind.GreaterThanOrEqualExpression:
                case SyntaxKind.LessThanExpression:
                case SyntaxKind.LessThanOrEqualExpression:
                case SyntaxKind.LogicalOrExpression:
                case SyntaxKind.LogicalAndExpression:
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Get negation of the condition operator
        /// </summary>
        /// <returns>
        /// negation of the specified condition operator, or BinaryOperatorType.Any if it's not a condition operator
        /// </returns>
        public static SyntaxKind NegateConditionOperator(SyntaxKind op)
        {
            switch (op)
            {
                case SyntaxKind.LogicalOrExpression:
                    return SyntaxKind.LogicalAndExpression;
                case SyntaxKind.LogicalAndExpression:
                    return SyntaxKind.LogicalOrExpression;
            }
            throw new ArgumentOutOfRangeException("op");
        }

        public static bool AreConditionsEqual(ExpressionSyntax cond1, ExpressionSyntax cond2)
        {
            if (cond1 == null || cond2 == null)
                return false;
            return cond1.SkipParens().IsEquivalentTo(cond2.SkipParens(), true);
        }
    }
}
