using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System.Threading;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace RefactoringEssentials.CSharp.Diagnostics
{
    /// <summary>
    /// Checks for "a != null ? a : other"<expr>
    /// Converts to: "a ?? other"<expr>
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class ConvertConditionalTernaryToNullCoalescingAnalyzer : DiagnosticAnalyzer
    {
        static readonly DiagnosticDescriptor descriptor = new DiagnosticDescriptor(
            CSharpDiagnosticIDs.ConvertConditionalTernaryToNullCoalescingAnalyzerID,
            GettextCatalog.GetString("'?:' expression can be converted to '??' expression"),
            GettextCatalog.GetString("'?:' expression can be converted to '??' expression"),
            DiagnosticAnalyzerCategories.Opportunities,
            DiagnosticSeverity.Info,
            isEnabledByDefault: true,
            helpLinkUri: HelpLink.CreateFor(CSharpDiagnosticIDs.ConvertConditionalTernaryToNullCoalescingAnalyzerID)
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
                new SyntaxKind[] { SyntaxKind.ConditionalExpression }
            );
        }

        static bool TryGetDiagnostic(SyntaxNodeAnalysisContext nodeContext, out Diagnostic diagnostic)
        {
            var node = nodeContext.Node as ConditionalExpressionSyntax;
            var semanticModel = nodeContext.SemanticModel;
            var cancellationToken = nodeContext.CancellationToken;

            diagnostic = default(Diagnostic);
            var obj = AnalyzeBinaryExpression(node.Condition);
            if (obj == null)
                return false;
            if (node.Condition.SkipParens().IsKind(SyntaxKind.NotEqualsExpression))
            {
                var whenTrue = UnpackNullableValueAccess(semanticModel, node.WhenTrue, cancellationToken);
                if (!CanBeNull(semanticModel, whenTrue, cancellationToken))
                    return false;
                if (obj.SkipParens().IsEquivalentTo(whenTrue.SkipParens(), true))
                {
                    diagnostic = Diagnostic.Create(
                        descriptor,
                        node.GetLocation()
                    );
                    return true;
                }
                var cast = whenTrue as CastExpressionSyntax;
                if (cast != null && cast.Expression != null && obj.SkipParens().IsEquivalentTo(cast.Expression.SkipParens(), true))
                {
                    diagnostic = Diagnostic.Create(
                        descriptor,
                        node.GetLocation()
                    );
                    return true;
                }
            }
            else
            {
                var whenFalse = UnpackNullableValueAccess(semanticModel, node.WhenFalse, cancellationToken);
                if (!CanBeNull(semanticModel, whenFalse, cancellationToken))
                    return false;
                if (obj.SkipParens().IsEquivalentTo(whenFalse.SkipParens(), true))
                {
                    diagnostic = Diagnostic.Create(
                        descriptor,
                        node.GetLocation()
                    );
                    return true;
                }
            }
            return false;
        }

        static ExpressionSyntax AnalyzeBinaryExpression(ExpressionSyntax node)
        {
            var bOp = node.SkipParens() as BinaryExpressionSyntax;
            if (bOp == null)
                return null;
            if (bOp.IsKind(SyntaxKind.NotEqualsExpression) || bOp.IsKind(SyntaxKind.EqualsExpression))
            {
                if (bOp.Left != null && bOp.Left.SkipParens().IsKind(SyntaxKind.NullLiteralExpression))
                    return bOp.Right;
                if (bOp.Right != null && bOp.Right.SkipParens().IsKind(SyntaxKind.NullLiteralExpression))
                    return bOp.Left;
            }
            return null;
        }

        static bool CanBeNull(SemanticModel semanticModel, ExpressionSyntax expression, CancellationToken cancellationToken)
        {
            var info = semanticModel.GetTypeInfo(expression, cancellationToken);
            if (info.ConvertedType == null)
                return false;
            if (info.ConvertedType.IsReferenceType || info.ConvertedType.IsNullableType())
                return true;
            return false;
        }

        internal static ExpressionSyntax UnpackNullableValueAccess(SemanticModel semanticModel, ExpressionSyntax expression, CancellationToken cancellationToken)
        {
            var expr = expression.SkipParens();
            if (!expr.IsKind(SyntaxKind.SimpleMemberAccessExpression))
                return expression;
            var info = semanticModel.GetTypeInfo(((MemberAccessExpressionSyntax)expr).Expression, cancellationToken);
            if (info.ConvertedType != null && !info.ConvertedType.IsNullableType())
                return expression;
            return ((MemberAccessExpressionSyntax)expr).Expression;
        }
    }
}