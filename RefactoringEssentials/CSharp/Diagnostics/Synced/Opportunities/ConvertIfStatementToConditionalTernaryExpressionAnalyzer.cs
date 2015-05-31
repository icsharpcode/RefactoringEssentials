using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using RefactoringEssentials.CSharp.CodeRefactorings;

namespace RefactoringEssentials.CSharp.Diagnostics
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class ConvertIfStatementToConditionalTernaryExpressionAnalyzer : DiagnosticAnalyzer
    {
        static readonly DiagnosticDescriptor descriptor = new DiagnosticDescriptor(
            CSharpDiagnosticIDs.ConvertIfStatementToConditionalTernaryExpressionAnalyzerID,
            GettextCatalog.GetString("Convert 'if' to '?:'"),
            GettextCatalog.GetString("Convert to '?:' expression"),
            DiagnosticAnalyzerCategories.Opportunities,
            DiagnosticSeverity.Info,
            isEnabledByDefault: true,
            helpLinkUri: HelpLink.CreateFor(CSharpDiagnosticIDs.ConvertIfStatementToConditionalTernaryExpressionAnalyzerID)
        );

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(descriptor);

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(
                (nodeContext) =>
                {
                    Diagnostic diagnostic;
                    if (TryGetDiagnostic(nodeContext, out diagnostic))
                    {
                        nodeContext.ReportDiagnostic(diagnostic);
                    }
                },
                new SyntaxKind[] { SyntaxKind.IfStatement }
            );
        }

        static bool TryGetDiagnostic(SyntaxNodeAnalysisContext nodeContext, out Diagnostic diagnostic)
        {
            var node = nodeContext.Node as IfStatementSyntax;
            var semanticModel = nodeContext.SemanticModel;
            var cancellationToken = nodeContext.CancellationToken;

            diagnostic = default(Diagnostic);
            if (nodeContext.IsFromGeneratedCode())
                return false;
            ExpressionSyntax condition, target;
            AssignmentExpressionSyntax trueAssignment, falseAssignment;
            if (!ConvertIfStatementToConditionalTernaryExpressionCodeRefactoringProvider.ParseIfStatement(node, out condition, out target, out trueAssignment, out falseAssignment))
                return false;
            if (IsComplexCondition(condition) || IsComplexExpression(trueAssignment.Right) || IsComplexExpression(falseAssignment.Right))
                return false;

            diagnostic = Diagnostic.Create(
                descriptor,
                node.IfKeyword.GetLocation()
            );
            return true;
        }

        public static bool IsComplexExpression(ExpressionSyntax expr)
        {
            var loc = expr.GetLocation().GetLineSpan();
            return loc.StartLinePosition.Line != loc.EndLinePosition.Line ||
                expr is ConditionalExpressionSyntax ||
                expr is BinaryExpressionSyntax;
        }

        public static bool IsComplexCondition(ExpressionSyntax expr)
        {
            var loc = expr.GetLocation().GetLineSpan();
            if (loc.StartLinePosition.Line != loc.EndLinePosition.Line)
                return true;

            if (expr is LiteralExpressionSyntax || expr is IdentifierNameSyntax || expr is MemberAccessExpressionSyntax || expr is InvocationExpressionSyntax)
                return false;

            var pexpr = expr as ParenthesizedExpressionSyntax;
            if (pexpr != null)
                return IsComplexCondition(pexpr.Expression);

            var uOp = expr as PrefixUnaryExpressionSyntax;
            if (uOp != null)
                return IsComplexCondition(uOp.Operand);

            var bop = expr as BinaryExpressionSyntax;
            if (bop == null)
                return true;
            return !(bop.IsKind(SyntaxKind.GreaterThanExpression) ||
                bop.IsKind(SyntaxKind.GreaterThanOrEqualExpression) ||
                bop.IsKind(SyntaxKind.EqualsExpression) ||
                bop.IsKind(SyntaxKind.NotEqualsExpression) ||
                bop.IsKind(SyntaxKind.LessThanExpression) ||
                bop.IsKind(SyntaxKind.LessThanOrEqualExpression));
        }
    }
}