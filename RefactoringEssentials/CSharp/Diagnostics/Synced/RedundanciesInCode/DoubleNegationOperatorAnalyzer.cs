using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace RefactoringEssentials.CSharp.Diagnostics
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class DoubleNegationOperatorAnalyzer : DiagnosticAnalyzer
    {
        static readonly DiagnosticDescriptor descriptor = new DiagnosticDescriptor(
            CSharpDiagnosticIDs.DoubleNegationOperatorAnalyzerID,
            GettextCatalog.GetString("Double negation is redundant"),
            GettextCatalog.GetString("Double negation is redundant"),
            DiagnosticAnalyzerCategories.RedundanciesInCode,
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            helpLinkUri: HelpLink.CreateFor(CSharpDiagnosticIDs.DoubleNegationOperatorAnalyzerID),
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
                new SyntaxKind[] { SyntaxKind.LogicalNotExpression, SyntaxKind.BitwiseNotExpression }
            );
        }

        static bool TryGetDiagnostic(SyntaxNodeAnalysisContext nodeContext, out Diagnostic diagnostic)
        {
            diagnostic = default(Diagnostic);

            var node = nodeContext.Node as PrefixUnaryExpressionSyntax;

            if (node.IsKind(SyntaxKind.LogicalNotExpression))
            {
                var innerUnaryOperatorExpr = node.Operand.SkipParens() as PrefixUnaryExpressionSyntax;
                if (innerUnaryOperatorExpr == null || !innerUnaryOperatorExpr.IsKind(SyntaxKind.LogicalNotExpression))
                    return false;
                diagnostic = Diagnostic.Create(descriptor, node.GetLocation());
                return true;

            }

            if (node.IsKind(SyntaxKind.BitwiseNotExpression))
            {
                var innerUnaryOperatorExpr = node.Operand.SkipParens() as PrefixUnaryExpressionSyntax;
                if (innerUnaryOperatorExpr == null || !innerUnaryOperatorExpr.IsKind(SyntaxKind.BitwiseNotExpression))
                    return false;
                diagnostic = Diagnostic.Create(descriptor, node.GetLocation());
                return true;
            }
            return false;
        }
    }
}
