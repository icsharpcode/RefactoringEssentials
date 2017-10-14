using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace RefactoringEssentials.CSharp.Diagnostics
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class ConditionIsAlwaysTrueOrFalseAnalyzer : DiagnosticAnalyzer
    {
        static readonly DiagnosticDescriptor descriptor = new DiagnosticDescriptor(
            CSharpDiagnosticIDs.ConditionIsAlwaysTrueOrFalseAnalyzerID,
            GettextCatalog.GetString("Expression is always 'true' or always 'false'"),
            GettextCatalog.GetString("Expression is always '{0}'"),
            DiagnosticAnalyzerCategories.RedundanciesInCode,
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            helpLinkUri: HelpLink.CreateFor(CSharpDiagnosticIDs.ConditionIsAlwaysTrueOrFalseAnalyzerID),
            customTags: DiagnosticCustomTags.Unnecessary
        );

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(descriptor);

        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.RegisterSyntaxNodeAction(
                nodeContext =>
                {
                    Diagnostic diagnostic;
                    if (TryGetDiagnostic(nodeContext, out diagnostic))
                    {
                        nodeContext.ReportDiagnostic(diagnostic);
                    }
                },
                new SyntaxKind[] {
                    SyntaxKind.EqualsExpression,
                    SyntaxKind.NotEqualsExpression,
                    SyntaxKind.LessThanExpression,
                    SyntaxKind.LessThanOrEqualExpression,
                    SyntaxKind.GreaterThanExpression,
                    SyntaxKind.GreaterThanOrEqualExpression
                }
            );
            context.RegisterSyntaxNodeAction(
                nodeContext =>
                {
                    Diagnostic diagnostic;
                    if (TryGetDiagnostic2(nodeContext, out diagnostic))
                    {
                        nodeContext.ReportDiagnostic(diagnostic);
                    }
                },
                new SyntaxKind[] { SyntaxKind.LogicalNotExpression }
            );
        }

        static bool TryGetDiagnostic(SyntaxNodeAnalysisContext nodeContext, out Diagnostic diagnostic)
        {
            diagnostic = default(Diagnostic);

            var node = nodeContext.Node as BinaryExpressionSyntax;

            if (CheckConstant(nodeContext, node, ref diagnostic))
                return true;

            if (node.Left.SkipParens().IsKind(SyntaxKind.NullLiteralExpression))
            {
                if (CheckNullComparison(nodeContext, node, node.Right, node.Left, ref diagnostic))
                    return true;
            }
            else if (node.Right.SkipParens().IsKind(SyntaxKind.NullLiteralExpression))
            {
                if (CheckNullComparison(nodeContext, node, node.Left, node.Right, ref diagnostic))
                    return true;
            }
            return false;
        }

        static bool TryGetDiagnostic2(SyntaxNodeAnalysisContext nodeContext, out Diagnostic diagnostic)
        {
            diagnostic = default(Diagnostic);

            var node = nodeContext.Node as PrefixUnaryExpressionSyntax;

            if (CheckConstant(nodeContext, node, ref diagnostic))
                return true;
            return false;
        }


        static bool CheckNullComparison(SyntaxNodeAnalysisContext nodeContext, BinaryExpressionSyntax binaryOperatorExpression, ExpressionSyntax right, ExpressionSyntax nullNode, ref Diagnostic diagnostic)
        {
            if (!binaryOperatorExpression.IsKind(SyntaxKind.EqualsExpression) && !binaryOperatorExpression.IsKind(SyntaxKind.NotEqualsExpression))
                return false;
            // note null == null is checked by similiar expression comparison.
            var expr = right.SkipParens();

            var rr = nodeContext.SemanticModel.GetTypeInfo(expr);
            if (rr.Type == null)
                return false;
            var returnType = rr.Type;
            if (returnType != null && returnType.IsValueType && !returnType.IsPointerType())
            {
                // nullable check
                if (returnType.IsNullableType())
                    return false;

                var conversion = nodeContext.SemanticModel.GetConversion(nullNode);
                if (conversion.IsUserDefined)
                    return false;
                // check for user operators
                foreach (IMethodSymbol op in returnType.GetMembers().OfType<IMethodSymbol>().Where(m => m.MethodKind == MethodKind.UserDefinedOperator && m.Parameters.Length == 2))
                {
                    if (op.Parameters[0].Type.IsReferenceType == false && op.Parameters[1].Type.IsReferenceType == false)
                        continue;
                    if (binaryOperatorExpression.IsKind(SyntaxKind.EqualsExpression) && op.Name == "op_Equality")
                        return false;
                    if (binaryOperatorExpression.IsKind(SyntaxKind.NotEqualsExpression) && op.Name == "op_Inequality")
                        return false;
                }
                diagnostic = Diagnostic.Create(
                    descriptor,
                    binaryOperatorExpression.GetLocation(),
                    !binaryOperatorExpression.IsKind(SyntaxKind.EqualsExpression) ? "true" : "false"
                );
                return true;
            }
            return false;
        }

        static bool CheckConstant(SyntaxNodeAnalysisContext nodeContext, SyntaxNode expr, ref Diagnostic diagnostic)
        {
            var rr = nodeContext.SemanticModel.GetConstantValue(expr);
            if (rr.HasValue && rr.Value is bool)
            {
                var result = (bool)rr.Value;
                diagnostic = Diagnostic.Create(
                    descriptor,
                    expr.GetLocation(),
                    result ? "true" : "false"
                );
                return true;
            }
            return false;
        }
    }
}