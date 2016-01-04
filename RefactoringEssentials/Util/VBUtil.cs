using System;
using RefactoringEssentials;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;
using Microsoft.CodeAnalysis.VisualBasic;
using Microsoft.CodeAnalysis;

namespace RefactoringEssentials
{
    static class VBUtil
    {
        /// <summary>
        /// Inverts a boolean condition. Note: The condition object can be frozen (from AST) it's cloned internally.
        /// </summary>
        /// <param name="condition">The condition to invert.</param>
        public static ExpressionSyntax InvertCondition(ExpressionSyntax condition)
        {
            if (condition is ParenthesizedExpressionSyntax)
            {
                return SyntaxFactory.ParenthesizedExpression(
                    InvertCondition(((ParenthesizedExpressionSyntax)condition).Expression));
            }

            if (condition is UnaryExpressionSyntax)
            {
                var uOp = (UnaryExpressionSyntax)condition;
                if (uOp.IsKind(SyntaxKind.NotExpression))
                {
                    return uOp.Operand;
                }
                return SyntaxFactory.UnaryExpression(
                    SyntaxKind.NotExpression, 
                    uOp.OperatorToken, 
                    uOp);
            }

            if (condition is BinaryExpressionSyntax)
            {
                var bOp = (BinaryExpressionSyntax)condition;

                if (bOp.IsKind(SyntaxKind.AndExpression) ||
                    bOp.IsKind(SyntaxKind.AndAlsoExpression) ||
                    bOp.IsKind(SyntaxKind.OrExpression) ||
                    bOp.IsKind(SyntaxKind.OrElseExpression))
                {
                    var kind = NegateConditionOperator(bOp.Kind());
                    return SyntaxFactory.BinaryExpression(
                        kind, 
                        InvertCondition(bOp.Left), 
                        SyntaxFactory.Token(GetBinaryExpressionOperatorTokenKind(kind)), 
                        InvertCondition(bOp.Right));
                }

                if (bOp.IsKind(SyntaxKind.EqualsExpression) ||
                    bOp.IsKind(SyntaxKind.NotEqualsExpression) ||
                    bOp.IsKind(SyntaxKind.GreaterThanExpression) ||
                    bOp.IsKind(SyntaxKind.GreaterThanOrEqualExpression) ||
                    bOp.IsKind(SyntaxKind.LessThanExpression) ||
                    bOp.IsKind(SyntaxKind.LessThanOrEqualExpression))
                {
                    var kind = NegateRelationalOperator(bOp.Kind());
                    return SyntaxFactory.BinaryExpression(
                        kind, 
                        bOp.Left, 
                        SyntaxFactory.Token(GetBinaryExpressionOperatorTokenKind(kind)), 
                        bOp.Right);
                }

                return SyntaxFactory.UnaryExpression(
                    SyntaxKind.NotExpression, 
                    SyntaxFactory.Token(SyntaxKind.NotKeyword), 
                    SyntaxFactory.ParenthesizedExpression(condition));
            }

            if (condition is TernaryConditionalExpressionSyntax)
            {
                var cEx = condition as TernaryConditionalExpressionSyntax;
                return cEx.WithCondition(InvertCondition(cEx.Condition));
            }

            if (condition is LiteralExpressionSyntax)
            {
                if (condition.Kind() == SyntaxKind.TrueLiteralExpression)
                {
                    return SyntaxFactory.LiteralExpression(
                        SyntaxKind.FalseLiteralExpression,
                        SyntaxFactory.Token(SyntaxKind.FalseKeyword));
                }
                if (condition.Kind() == SyntaxKind.FalseLiteralExpression)
                {
                    return SyntaxFactory.LiteralExpression(
                        SyntaxKind.TrueLiteralExpression,
                        SyntaxFactory.Token(SyntaxKind.TrueKeyword));
                }
            }

            return SyntaxFactory.UnaryExpression(
                SyntaxKind.NotExpression, 
                SyntaxFactory.Token(SyntaxKind.NotKeyword), 
                AddParensForUnaryExpressionIfRequired(condition));
        }

        public static SyntaxKind GetBinaryExpressionOperatorTokenKind(SyntaxKind op)
        {
            switch (op)
            {
                case SyntaxKind.EqualsExpression:
                    return SyntaxKind.EqualsToken;
                case SyntaxKind.NotEqualsExpression:
                    return SyntaxKind.LessThanGreaterThanToken;
                case SyntaxKind.GreaterThanExpression:
                    return SyntaxKind.GreaterThanToken;
                case SyntaxKind.GreaterThanOrEqualExpression:
                    return SyntaxKind.GreaterThanEqualsToken;
                case SyntaxKind.LessThanExpression:
                    return SyntaxKind.LessThanToken;
                case SyntaxKind.LessThanOrEqualExpression:
                    return SyntaxKind.LessThanEqualsToken;
                case SyntaxKind.OrExpression:
                    return SyntaxKind.OrKeyword;
                case SyntaxKind.OrElseExpression:
                    return SyntaxKind.OrElseKeyword;
                case SyntaxKind.AndExpression:
                    return SyntaxKind.AndKeyword;
                case SyntaxKind.AndAlsoExpression:
                    return SyntaxKind.AndAlsoKeyword;
                case SyntaxKind.AddExpression:
                    return SyntaxKind.PlusToken;
                case SyntaxKind.SubtractExpression:
                    return SyntaxKind.MinusToken;
			    // assignments
				case SyntaxKind.SimpleAssignmentStatement:
					return SyntaxKind.EqualsToken;
                case SyntaxKind.AddAssignmentStatement:
                    return SyntaxKind.PlusEqualsToken;
                case SyntaxKind.SubtractAssignmentStatement:
                    return SyntaxKind.MinusEqualsToken;
            }
            throw new ArgumentOutOfRangeException(nameof(op));
        }

        /// <summary>
        /// When negating an expression this is required, otherwise you would end up with
        /// a or b -> !a or b
        /// </summary>
        public static ExpressionSyntax AddParensForUnaryExpressionIfRequired(ExpressionSyntax expression)
        {
            if ((expression is BinaryExpressionSyntax) ||
                (expression is CastExpressionSyntax) ||
                (expression is LambdaExpressionSyntax))
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
                case SyntaxKind.OrExpression:
                    return SyntaxKind.AndExpression;
                case SyntaxKind.OrElseExpression:
                    return SyntaxKind.AndAlsoExpression;
                case SyntaxKind.AndExpression:
                    return SyntaxKind.OrExpression;
                case SyntaxKind.AndAlsoExpression:
                    return SyntaxKind.OrElseExpression;
            }
            throw new ArgumentOutOfRangeException(nameof(op));
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
                case SyntaxKind.OrExpression:
                    return SyntaxKind.AndExpression;
                case SyntaxKind.OrElseExpression:
                    return SyntaxKind.AndAlsoExpression;
                case SyntaxKind.AndExpression:
                    return SyntaxKind.OrExpression;
                case SyntaxKind.AndAlsoExpression:
                    return SyntaxKind.OrElseExpression;
            }
            throw new ArgumentOutOfRangeException(nameof(op));
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
                case SyntaxKind.OrExpression:
                case SyntaxKind.OrElseExpression:
                case SyntaxKind.AndExpression:
                case SyntaxKind.AndAlsoExpression:
                    return true;
            }
            return false;
        }
    }
}
