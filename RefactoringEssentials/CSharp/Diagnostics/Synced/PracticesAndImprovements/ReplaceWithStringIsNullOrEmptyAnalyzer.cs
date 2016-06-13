using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using RefactoringEssentials;
using RefactoringEssentials.Util;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace RefactoringEssentials.CSharp.Diagnostics
{
    /// <summary>
    /// Checks for str == null &amp;&amp; str == " "
    /// Converts to: string.IsNullOrEmpty (str)
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class ReplaceWithStringIsNullOrEmptyAnalyzer : DiagnosticAnalyzer
    {
        /// <summary>
        /// The name of the property referred to by the <see cref="Diagnostic"/> for the replacement code.
        /// </summary>
        public static readonly string ReplacementPropertyName = "Replacement";

        static readonly DiagnosticDescriptor descriptor = new DiagnosticDescriptor(
            CSharpDiagnosticIDs.ReplaceWithStringIsNullOrEmptyAnalyzerID,
            GettextCatalog.GetString("Uses shorter string.IsNullOrEmpty call instead of a longer condition"),
            GettextCatalog.GetString("Expression can be replaced with '{0}'"),
            DiagnosticAnalyzerCategories.PracticesAndImprovements,
            DiagnosticSeverity.Info,
            isEnabledByDefault: true,
            helpLinkUri: HelpLink.CreateFor(CSharpDiagnosticIDs.ReplaceWithStringIsNullOrEmptyAnalyzerID)
        );

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(descriptor);

        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.RegisterSyntaxNodeAction(
                (nodeContext) =>
                {
                    Diagnostic diagnostic;
                    if (TryGetDiagnostic(nodeContext, out diagnostic))
                    {
                        nodeContext.ReportDiagnostic(diagnostic);
                    }
                },
                SyntaxKind.LogicalAndExpression,
                SyntaxKind.LogicalOrExpression
            );
        }

        static bool TryGetDiagnostic(SyntaxNodeAnalysisContext nodeContext, out Diagnostic diagnostic)
        {
            diagnostic = default(Diagnostic);

            var node = nodeContext.Node as BinaryExpressionSyntax;

            // Must be of binary expression of 2 binary expressions.
            if (!node.IsKind(SyntaxKind.LogicalAndExpression, SyntaxKind.LogicalOrExpression))
                return false;

            // Verify left is a binary expression.
            var left = SimplifySyntax(node.Left) as BinaryExpressionSyntax;
            if (left == null)
                return false;

            // Verify right is a binary expression.
            var right = SimplifySyntax(node.Right) as BinaryExpressionSyntax;
            if (right == null)
                return false;

            // Ensure left and right are binary and not assignment.
            if (!SyntaxFacts.IsBinaryExpression(left.OperatorToken.Kind()) || !SyntaxFacts.IsBinaryExpression(right.OperatorToken.Kind()))
                return false;

            // Test if left and right are suitable for replacement.
            var leftReplace = ShouldReplace(nodeContext, left);
            var rightReplace = ShouldReplace(nodeContext, right);

            // Test both are suitable for replacement.
            if (!leftReplace.ShouldReplace || !rightReplace.ShouldReplace)
                return false;

            // Test that both are either positive or negative tests.
            if (!(leftReplace.IsNegative == rightReplace.IsNegative))
                return false;

            // Ensure that one tests for null and the other tests for empty.
            var isNullOrEmptyTest = (leftReplace.IsNullTest && rightReplace.IsEmptyTest) || (leftReplace.IsEmptyTest && rightReplace.IsNullTest);

            if (!isNullOrEmptyTest)
                return false;

            // Ensure that both refer to the same identifier.
            // Good: foo != null && foo != ""
            // Bad: foo != null && bar != ""
            if (!string.Equals(leftReplace.IdentifierNode.ToString(),
                rightReplace.IdentifierNode.ToString(),
                StringComparison.OrdinalIgnoreCase))
                return false;

            // Generate replacement string and negate if necessary.
            // Used within diagnostic message and also passed down for replacement.
            var replacementString = string.Format("string.IsNullOrEmpty({0})", leftReplace.IdentifierNode);

            if (leftReplace.IsNegative)
                replacementString = "!" + replacementString;

            // We already did the work now pass it down to the code fix provider via a property.
            var props = new Dictionary<string, string>
                {
                    { ReplacementPropertyName, replacementString }
                };

            diagnostic = Diagnostic.Create(
                descriptor,
                node.GetLocation(),
                ImmutableDictionary.CreateRange(props),
                replacementString);
            return true;
        }

        /// <summary>
        /// Indicates whether a binary expression is suitable for replacement and info about it.
        /// </summary>
        class ShouldReplaceResult
        {
            /// <summary>
            /// Is the expression suitable for replacement.
            /// </summary>
            public bool ShouldReplace { get; set; } = false;

            /// <summary>
            /// Is the expression a test for null?
            /// </summary>
            public bool IsNullTest { get; set; } = false;

            /// <summary>
            /// Is the expression a test for empty?
            /// </summary>
            public bool IsEmptyTest { get; set; } = false;

            /// <summary>
            /// Is the expression negated?
            /// </summary>
            public bool IsNegative { get; set; } = false;

            /// <summary>
            /// What string symbol is being tested for null or empty?
            /// </summary>
            public ExpressionSyntax IdentifierNode { get; set; } = null;

        }

        /// <summary>
        /// Test whether a binary expression is suitable for replacement. 
        /// </summary>
        /// <returns>
        /// A <see cref="ShouldReplaceResult"/> indicating whether the node is suitable for replacement.
        /// </returns>
        static ShouldReplaceResult ShouldReplace(SyntaxNodeAnalysisContext nodeContext, BinaryExpressionSyntax node)
        {
            // input (left, right, operator) output Result
            var left = SimplifySyntax(node.Left);
            var right = SimplifySyntax(node.Right);

            // str ==
            if (IsStringSyntax(nodeContext, left))
            {
                return ShouldReplaceString(nodeContext, left, right, node.OperatorToken);
            }

            // == str
            if (IsStringSyntax(nodeContext, right))
            {
                return ShouldReplaceString(nodeContext, right, left, node.OperatorToken);
            }

            // str.Length ==
            if (IsStringLengthSyntax(nodeContext, left))
            {
                return ShouldReplaceStringLength(left as MemberAccessExpressionSyntax, right, node.OperatorToken);
            }

            // == str.Length
            if (IsStringLengthSyntax(nodeContext, right))
            {
                return ShouldReplaceStringLength(right as MemberAccessExpressionSyntax, left, node.OperatorToken);
            }

            // We did not find a suitable replacement.
            return new ShouldReplaceResult
            {
                ShouldReplace = false
            };
        }

        /// <summary>
        /// Determine whether a binary expression with a string expression is suitable for replacement.
        /// </summary>
        /// <param name="left">A node representing a string expression.</param>
        /// <param name="right">A node to be tested.</param>
        /// <param name="operatorToken">The operator separating the nodes.</param>
        /// <returns></returns>
        static ShouldReplaceResult ShouldReplaceString(SyntaxNodeAnalysisContext nodeContext, ExpressionSyntax left, ExpressionSyntax right, SyntaxToken operatorToken)
        {
            var result = new ShouldReplaceResult();
            result.ShouldReplace = false;

            // str == null or str != null
            if (IsNullSyntax(nodeContext, right))
            {
                result.IsNullTest = true;
                result.ShouldReplace = true;
            }
            // str == "" or str != ""
            // str == string.Empty or str != string.Empty
            else if (IsEmptySyntax(nodeContext, right))
            {
                result.IsEmptyTest = true;
                result.ShouldReplace = true;
            }

            if (result.ShouldReplace)
            {
                result.IdentifierNode = left;

                if (operatorToken.IsKind(SyntaxKind.ExclamationEqualsToken))
                {
                    result.IsNegative = true;
                }
            }

            return result;
        }

        /// <summary>
        /// Determines whether a binary expression with a string length expression is suitable for replacement.
        /// </summary>
        /// <param name="left">A node representing a string length expression.</param>
        /// <param name="right">A node to be tested.</param>
        /// <param name="operatorToken">The operator separating the nodes.</param>
        /// <returns></returns>
        static ShouldReplaceResult ShouldReplaceStringLength(MemberAccessExpressionSyntax left, ExpressionSyntax right, SyntaxToken operatorToken)
        {
            const string zeroLiteral = "0";
            const string oneLiteral = "1";

            var result = new ShouldReplaceResult();
            result.ShouldReplace = false;

            // str.Length == 0 or str.Length <= 0
            if (operatorToken.IsKind(SyntaxKind.EqualsEqualsToken, SyntaxKind.LessThanEqualsToken) && string.Equals(zeroLiteral, right.ToString()))
            {
                result.IsEmptyTest = true;
                result.ShouldReplace = true;
            }
            // str.Length < 1
            else if (operatorToken.IsKind(SyntaxKind.LessThanToken) && string.Equals(oneLiteral, right.ToString()))
            {
                result.IsEmptyTest = true;
                result.ShouldReplace = true;
            }
            // str.Length != 0 or str.Length > 0
            else if (operatorToken.IsKind(SyntaxKind.ExclamationEqualsToken, SyntaxKind.GreaterThanToken) && string.Equals(zeroLiteral, right.ToString()))
            {
                result.IsEmptyTest = true;
                result.IsNegative = true;
                result.ShouldReplace = true;
            }
            // str.Length >= 1
            else if (operatorToken.IsKind(SyntaxKind.GreaterThanEqualsToken) && string.Equals(oneLiteral, right.ToString()))
            {
                result.IsEmptyTest = true;
                result.IsNegative = true;
                result.ShouldReplace = true;
            }

            if (result.ShouldReplace)
            {
                result.IdentifierNode = left.Expression;
            }

            return result;

        }

        /// <summary>
        /// Does the expression look like a string type?
        /// </summary>
        static bool IsStringSyntax(SyntaxNodeAnalysisContext nodeContext, ExpressionSyntax node)
        {
            if (!IsStringType(nodeContext, node))
                return false;

            return node.IsKind(SyntaxKind.IdentifierName, SyntaxKind.InvocationExpression, SyntaxKind.SimpleMemberAccessExpression);
        }

        /// <summary>
        /// Does the expression look like a string length call?
        /// </summary>
        static bool IsStringLengthSyntax(SyntaxNodeAnalysisContext nodeContext, ExpressionSyntax node)
        {
            if (node.IsKind(SyntaxKind.SimpleMemberAccessExpression))
            {
                var smaNode = node as MemberAccessExpressionSyntax;

                if (smaNode.Name.Identifier.Text == "Length")
                {
                    if (!IsStringType(nodeContext, smaNode.Expression))
                        return false;

                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Does the expression look like a null?
        /// </summary>
        static bool IsNullSyntax(SyntaxNodeAnalysisContext nodeContext, ExpressionSyntax node)
        {
            if (!IsStringType(nodeContext, node))
                return false;

            return node.IsKind(SyntaxKind.NullLiteralExpression);
        }

        /// <summary>
        /// Does the expression look like a test for empty string ("" or string.Empty)?
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        static bool IsEmptySyntax(SyntaxNodeAnalysisContext nodeContext, ExpressionSyntax node)
        {
            if (!IsStringType(nodeContext, node))
                return false;

            if (node.IsKind(SyntaxKind.StringLiteralExpression))
            {
                if (string.Equals("\"\"", node.ToString()))
                    return true;
            }
            else if (node.IsKind(SyntaxKind.SimpleMemberAccessExpression))
            {
                var sma = node as MemberAccessExpressionSyntax;

                if (!string.Equals("string", sma.Expression.ToString(), StringComparison.OrdinalIgnoreCase))
                    return false;

                if (!string.Equals("Empty", sma.Name.ToString(), StringComparison.OrdinalIgnoreCase))
                    return false;

                return true;
            }

            return false;
        }

        /// <summary>
        /// Test if expression is a string type.
        /// </summary>
        static bool IsStringType(SyntaxNodeAnalysisContext nodeContext, ExpressionSyntax node)
        {
            var typeInfo = nodeContext.SemanticModel.GetTypeInfo(node);

            if (typeInfo.ConvertedType == null)
                return false;

            if (!string.Equals("String", typeInfo.ConvertedType.Name, StringComparison.OrdinalIgnoreCase))
                return false;

            return true;
        }

        /// <summary>
        /// Simplify an <see cref="ExpressionSyntax"/> by removing unecessary parenthesis.
        /// </summary>
        /// <returns>
        /// A simplified <see cref="ExpressionSyntax"/>.
        /// </returns>
        static ExpressionSyntax SimplifySyntax(ExpressionSyntax syntax)
        {
            if (syntax.IsKind(SyntaxKind.ParenthesizedExpression))
            {
                syntax = (syntax as ParenthesizedExpressionSyntax).Expression;

                syntax = SimplifySyntax(syntax);
            }

            return syntax;
        }
    }
}
