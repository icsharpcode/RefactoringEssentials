using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace RefactoringEssentials.CSharp.Diagnostics
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class CompareOfFloatsByEqualityOperatorAnalyzer : DiagnosticAnalyzer
    {
        static readonly DiagnosticDescriptor descriptor = new DiagnosticDescriptor(
            CSharpDiagnosticIDs.CompareOfFloatsByEqualityOperatorAnalyzerID,
            GettextCatalog.GetString("Comparison of floating point numbers with equality operator"),
            GettextCatalog.GetString("{0}"),
            DiagnosticAnalyzerCategories.CodeQualityIssues,
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            helpLinkUri: HelpLink.CreateFor(CSharpDiagnosticIDs.CompareOfFloatsByEqualityOperatorAnalyzerID)
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
                        nodeContext.ReportDiagnostic(diagnostic);
                },
                SyntaxKind.EqualsExpression,
                SyntaxKind.NotEqualsExpression
            );
        }

        static bool TryGetDiagnostic(SyntaxNodeAnalysisContext nodeContext, out Diagnostic diagnostic)
        {
            diagnostic = default(Diagnostic);

            var node = nodeContext.Node as BinaryExpressionSyntax;
            var semanticModel = nodeContext.SemanticModel;

            string message = null, tag = null;

            if (IsNaN(semanticModel, node.Left))
            {
                message = node.IsKind(SyntaxKind.EqualsExpression) ?
                                  "NaN doesn't equal to any floating point number including to itself. Use 'IsNaN' instead." :
                                  "NaN doesn't equal to any floating point number including to itself. Use '!IsNaN' instead.";
                tag = "1";
            }
            else if (IsNaN(semanticModel, node.Right))
            {
                message = node.IsKind(SyntaxKind.EqualsExpression) ?
                                  "NaN doesn't equal to any floating point number including to itself. Use 'IsNaN' instead." :
                                  "NaN doesn't equal to any floating point number including to itself. Use '!IsNaN' instead.";
                tag = "2";
            }
            else if (IsPositiveInfinity(semanticModel, node.Left))
            {
                message = node.IsKind(SyntaxKind.EqualsExpression) ?
                                  "Comparison of floating point numbers with equality operator. Use 'IsPositiveInfinity' method." :
                                  "Comparison of floating point numbers with equality operator. Use '!IsPositiveInfinity' method.";
                tag = "3";
            }
            else if (IsPositiveInfinity(semanticModel, node.Right))
            {
                message = node.IsKind(SyntaxKind.EqualsExpression) ?
                                  "Comparison of floating point numbers with equality operator. Use 'IsPositiveInfinity' method." :
                                  "Comparison of floating point numbers with equality operator. Use '!IsPositiveInfinity' method.";
                tag = "4";
            }
            else if (IsNegativeInfinity(semanticModel, node.Left))
            {
                message = node.IsKind(SyntaxKind.EqualsExpression) ?
                                  "Comparison of floating point numbers with equality operator. Use 'IsNegativeInfinity' method." :
                                  "Comparison of floating point numbers with equality operator. Use '!IsNegativeInfinity' method.";
                tag = "5";
            }
            else if (IsNegativeInfinity(semanticModel, node.Right))
            {
                message = node.IsKind(SyntaxKind.EqualsExpression) ?
                                  "Comparison of floating point numbers with equality operator. Use 'IsNegativeInfinity' method." :
                                  "Comparison of floating point numbers with equality operator. Use '!IsNegativeInfinity' method.";
                tag = "6";
            }
            else if (IsFloatingPoint(semanticModel, node.Left) || IsFloatingPoint(semanticModel, node.Right))
            {
                if (IsConstantInfinity(semanticModel, node.Left) || IsConstantInfinity(semanticModel, node.Right))
                    return false;
                if (IsZero(node.Left))
                {
                    message = "Fix floating point number comparing. Compare a difference with epsilon.";
                    tag = "7";
                }
                else if (IsZero(node.Right))
                {
                    message = "Fix floating point number comparing. Compare a difference with epsilon.";
                    tag = "8";
                }
                else
                {
                    message = "Comparison of floating point numbers can be unequal due to the differing precision of the two values.";
                    tag = "9";
                }
            }

            if (message == null)
                return false;

            string floatType = GetFloatType(semanticModel, node.Left, node.Right);

            diagnostic = Diagnostic.Create(
                descriptor.Id,
                descriptor.Category,
                message,
                descriptor.DefaultSeverity,
                descriptor.DefaultSeverity,
                descriptor.IsEnabledByDefault,
                4,
                descriptor.Title,
                descriptor.Description,
                descriptor.HelpLinkUri,
                node.GetLocation(),
                null,
                new[] { tag, floatType }
            );
            return true;
        }

        internal static bool IsFloatingPointType(ITypeSymbol type)
        {
            if (type == null)
                return false;
            return type.SpecialType == SpecialType.System_Single || type.SpecialType == SpecialType.System_Double;
        }

        internal static bool IsFloatingPoint(SemanticModel semanticModel, SyntaxNode node)
        {
            return IsFloatingPointType(semanticModel.GetTypeInfo(node).Type);
        }

        internal static bool IsConstantInfinity(SemanticModel semanticModel, SyntaxNode node)
        {
            var rr = semanticModel.GetConstantValue(node);
            if (!rr.HasValue)
                return false;

            return rr.Value is double && double.IsInfinity((double)rr.Value) || rr.Value is float && float.IsInfinity((float)rr.Value);
        }

        internal static string GetFloatType(SemanticModel semanticModel, ExpressionSyntax left, ExpressionSyntax right)
        {
            var rr2 = semanticModel.GetTypeInfo(left);
            var rr1 = semanticModel.GetTypeInfo(right);
            if (rr1.Type != null && rr1.Type.SpecialType == SpecialType.System_Single &&
                rr2.Type != null && rr2.Type.SpecialType == SpecialType.System_Single)
                return "float";
            return "double";
        }

        internal static bool IsNaN(SemanticModel semanticModel, SyntaxNode node)
        {
            var rr = semanticModel.GetConstantValue(node);
            if (!rr.HasValue)
                return false;

            return rr.Value is double && double.IsNaN((double)rr.Value) || rr.Value is float && float.IsNaN((float)rr.Value);
        }

        internal static bool IsZero(SyntaxNode node)
        {
            var pe = node as LiteralExpressionSyntax;
            if (pe == null)
                return false;
            var token = pe.Token;
            if (token.Value is char || token.Value is string || token.Value is bool)
                return false;

            if (token.Value is double && (double)token.Value == 0d ||
                token.Value is float && (float)token.Value == 0f ||
                token.Value is decimal && (decimal)token.Value == 0m)
                return true;

            foreach (char c in token.ValueText)
            {
                if (char.IsDigit(c) && c != '0')
                    return false;
            }

            return true;
        }

        internal static bool IsNegativeInfinity(SemanticModel semanticModel, SyntaxNode node)
        {
            var rr = semanticModel.GetConstantValue(node);
            if (!rr.HasValue)
                return false;

            return rr.Value is double && double.IsNegativeInfinity((double)rr.Value) || rr.Value is float && float.IsNegativeInfinity((float)rr.Value);
        }

        internal static bool IsPositiveInfinity(SemanticModel semanticModel, SyntaxNode node)
        {
            var rr = semanticModel.GetConstantValue(node);
            if (!rr.HasValue)
                return false;

            return rr.Value is double && double.IsPositiveInfinity((double)rr.Value) || rr.Value is float && float.IsPositiveInfinity((float)rr.Value);
        }
    }
}