using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace RefactoringEssentials.CSharp.Diagnostics
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class CompareNonConstrainedGenericWithNullAnalyzer : DiagnosticAnalyzer
    {
        static readonly DiagnosticDescriptor descriptor = new DiagnosticDescriptor(
            CSharpDiagnosticIDs.CompareNonConstrainedGenericWithNullAnalyzerID,
            GettextCatalog.GetString("Possible compare of value type with 'null'"),
            GettextCatalog.GetString("Possible compare of value type with 'null'"),
            DiagnosticAnalyzerCategories.CodeQualityIssues,
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            helpLinkUri: HelpLink.CreateFor(CSharpDiagnosticIDs.CompareNonConstrainedGenericWithNullAnalyzerID)
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
            var left = node.Left.SkipParens();
            var right = node.Right.SkipParens();
            ExpressionSyntax expr = null, highlightExpr = null;
            if (left.IsKind(SyntaxKind.NullLiteralExpression))
            {
                expr = right;
                highlightExpr = node.Left;
            }
            if (right.IsKind(SyntaxKind.NullLiteralExpression))
            {
                expr = left;
                highlightExpr = node.Right;
            }
            if (expr == null)
                return false;
            var type = nodeContext.SemanticModel.GetTypeInfo(expr).Type;
            if ((type == null) || (type.TypeKind != TypeKind.TypeParameter) || type.IsReferenceType)
                return false;
            diagnostic = Diagnostic.Create(
                descriptor,
                highlightExpr.GetLocation()
            );
            return true;
        }
    }
}