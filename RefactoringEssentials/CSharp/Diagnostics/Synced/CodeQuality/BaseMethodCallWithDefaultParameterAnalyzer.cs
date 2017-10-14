using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace RefactoringEssentials.CSharp.Diagnostics
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class BaseMethodCallWithDefaultParameterAnalyzer : DiagnosticAnalyzer
    {
        static readonly DiagnosticDescriptor descriptor = new DiagnosticDescriptor(
            CSharpDiagnosticIDs.BaseMethodCallWithDefaultParameterDiagnosticID,
            GettextCatalog.GetString("Call to base member with implicit default parameters"),
            GettextCatalog.GetString("Call to base member with implicit default parameters"),
            DiagnosticAnalyzerCategories.CodeQualityIssues,
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            helpLinkUri: HelpLink.CreateFor(CSharpDiagnosticIDs.ConvertClosureToMethodDiagnosticID)
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
                new SyntaxKind[] { SyntaxKind.InvocationExpression, SyntaxKind.ElementAccessExpression }
            );
        }

        static bool TryGetDiagnostic(SyntaxNodeAnalysisContext nodeContext, out Diagnostic diagnostic)
        {
            diagnostic = default(Diagnostic);

            var invocationExpr = nodeContext.Node as InvocationExpressionSyntax;
            if (invocationExpr != null)
            {
                var mr = invocationExpr.Expression as MemberAccessExpressionSyntax;
                if (mr == null || !mr.Expression.IsKind(SyntaxKind.BaseExpression))
                    return false;

                var invocationRR = nodeContext.SemanticModel.GetSymbolInfo(invocationExpr);
                if (invocationRR.Symbol == null)
                    return false;

                var parentEntity = invocationExpr.FirstAncestorOrSelf<MethodDeclarationSyntax>();
                if (parentEntity == null)
                    return false;
                var rr = nodeContext.SemanticModel.GetDeclaredSymbol(parentEntity);
                if (rr == null || rr.OverriddenMethod != invocationRR.Symbol)
                    return false;

                var parameters = invocationRR.Symbol.GetParameters();
                if (invocationExpr.ArgumentList.Arguments.Count >= parameters.Length ||
                        parameters.Length == 0 ||
                        !parameters.Last().IsOptional)
                    return false;

                diagnostic = Diagnostic.Create(
                    descriptor,
                    invocationExpr.GetLocation()
                );
                return true;
            }

            var elementAccessExpr = nodeContext.Node as ElementAccessExpressionSyntax;
            if (elementAccessExpr != null)
            {
                var mr = elementAccessExpr.Expression;
                if (mr == null || !mr.IsKind(SyntaxKind.BaseExpression))
                    return false;

                var invocationRR = nodeContext.SemanticModel.GetSymbolInfo(elementAccessExpr);
                if (invocationRR.Symbol == null)
                    return false;

                var parentEntity = elementAccessExpr.FirstAncestorOrSelf<IndexerDeclarationSyntax>();
                if (parentEntity == null)
                    return false;

                var rr = nodeContext.SemanticModel.GetDeclaredSymbol(parentEntity);
                if (rr == null || rr.OverriddenProperty != invocationRR.Symbol)
                    return false;

                var parameters = invocationRR.Symbol.GetParameters();
                if (elementAccessExpr.ArgumentList.Arguments.Count >= parameters.Length ||
                        parameters.Length == 0 ||
                        !parameters.Last().IsOptional)
                    return false;

                diagnostic = Diagnostic.Create(
                    descriptor,
                    elementAccessExpr.GetLocation()
                );
                return true;
            }
            return false;
        }
    }
}