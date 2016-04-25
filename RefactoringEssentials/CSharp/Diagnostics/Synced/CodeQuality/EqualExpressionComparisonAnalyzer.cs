using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace RefactoringEssentials.CSharp.Diagnostics
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class EqualExpressionComparisonAnalyzer : DiagnosticAnalyzer
    {
        static readonly DiagnosticDescriptor descriptor = new DiagnosticDescriptor(
            CSharpDiagnosticIDs.EqualExpressionComparisonAnalyzerID,
            GettextCatalog.GetString("Comparing equal expression for equality is usually useless"),
            GettextCatalog.GetString("Replace with '{0}'"),
            DiagnosticAnalyzerCategories.CodeQualityIssues,
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            helpLinkUri: HelpLink.CreateFor(CSharpDiagnosticIDs.EqualExpressionComparisonAnalyzerID)
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
                    if (TryGetDiagnosticFromBinaryExpression(nodeContext, out diagnostic))
                    {
                        nodeContext.ReportDiagnostic(diagnostic);
                    }
                },
                new SyntaxKind[] {
                    SyntaxKind.EqualsExpression,
                    SyntaxKind.NotEqualsExpression,
                    SyntaxKind.GreaterThanExpression,
                    SyntaxKind.GreaterThanOrEqualExpression,
                    SyntaxKind.LessThanExpression,
                    SyntaxKind.LessThanOrEqualExpression
                }  
            );

            context.RegisterSyntaxNodeAction(
                (nodeContext) =>
                {
                    Diagnostic diagnostic;
                if (TryGetDiagnosticFromEqualsInvocation(nodeContext, out diagnostic))
                    {
                        nodeContext.ReportDiagnostic(diagnostic);
                    }
                },
                new SyntaxKind[] { SyntaxKind.InvocationExpression}  
            );

        }

        static bool TryGetDiagnosticFromBinaryExpression(SyntaxNodeAnalysisContext nodeContext, out Diagnostic diagnostic)
        {
            diagnostic = default(Diagnostic);
            var node = nodeContext.Node as BinaryExpressionSyntax;

            if (CSharpUtil.AreConditionsEqual(node.Left, node.Right))
            {
                diagnostic = Diagnostic.Create(descriptor, node.GetLocation(), node.IsKind(SyntaxKind.EqualsExpression) ? "true" : "false");
                return true;
            }

            return false;
        }

        static bool TryGetDiagnosticFromEqualsInvocation(SyntaxNodeAnalysisContext nodeContext, out Diagnostic diagnostic)
        {
            diagnostic = default(Diagnostic);
            var node = nodeContext.Node as InvocationExpressionSyntax;

            var info = nodeContext.SemanticModel.GetSymbolInfo(node);

            if (info.Symbol == null || !info.Symbol.IsKind(SymbolKind.Method) || info.Symbol.Name != "Equals" || info.Symbol.GetReturnType().SpecialType != SpecialType.System_Boolean)
                return false;

            var method = info.Symbol as IMethodSymbol;
            if (method.IsStatic) {
                if (method.Parameters.Length != 2 || node.ArgumentList.Arguments.Count != 2)
                    return false;
                if (CSharpUtil.AreConditionsEqual(node.ArgumentList.Arguments[0].Expression, node.ArgumentList.Arguments[1].Expression)) {
                    if (node.Parent.IsKind(SyntaxKind.LogicalNotExpression)) {
                        diagnostic = Diagnostic.Create(descriptor, node.Parent.GetLocation(), "false");
                    } else {
                        diagnostic = Diagnostic.Create(descriptor, node.GetLocation(), "true");
                    }
                    return true;
                }
            } else {
                if (method.Parameters.Length != 1 || node.ArgumentList.Arguments.Count != 1)
                    return false;
                var target = node.Expression as MemberAccessExpressionSyntax;
                if (target == null)
                    return false;
                if (CSharpUtil.AreConditionsEqual(node.ArgumentList.Arguments[0].Expression, target.Expression)) {
                    if (node.Parent.IsKind(SyntaxKind.LogicalNotExpression)) {
                        diagnostic = Diagnostic.Create(descriptor, node.Parent.GetLocation(), "false");
                    } else {
                        diagnostic = Diagnostic.Create(descriptor, node.GetLocation(), "true");
                    }
                    return true;
                }
            }
            return false;
        }
    }
}