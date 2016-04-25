using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace RefactoringEssentials.CSharp.Diagnostics
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class CallToObjectEqualsViaBaseAnalyzer : DiagnosticAnalyzer
    {
        static readonly DiagnosticDescriptor descriptor = new DiagnosticDescriptor(
            CSharpDiagnosticIDs.CallToObjectEqualsViaBaseAnalyzerID,
            GettextCatalog.GetString("Finds potentially erroneous calls to Object.Equals"),
            GettextCatalog.GetString("Call to base.Equals resolves to Object.Equals, which is reference equality"),
            DiagnosticAnalyzerCategories.CodeQualityIssues,
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            helpLinkUri: HelpLink.CreateFor(CSharpDiagnosticIDs.CallToObjectEqualsViaBaseAnalyzerID)
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
                new SyntaxKind[] { SyntaxKind.InvocationExpression }
            );
        }

        static bool TryGetDiagnostic(SyntaxNodeAnalysisContext nodeContext, out Diagnostic diagnostic)
        {
            diagnostic = default(Diagnostic);
            var node = nodeContext.Node as InvocationExpressionSyntax;
            if (node.ArgumentList.Arguments.Count != 1)
                return false;
            var memberExpression = node.Expression as MemberAccessExpressionSyntax;
            if (memberExpression == null || memberExpression.Name.Identifier.ToString() != "Equals" || !(memberExpression.Expression.IsKind(SyntaxKind.BaseExpression)))
                return false;

            var resolveResult = nodeContext.SemanticModel.GetSymbolInfo(node);
            if (resolveResult.Symbol == null || resolveResult.Symbol.ContainingType.SpecialType != SpecialType.System_Object)
                return false;

            diagnostic = Diagnostic.Create(descriptor, node.GetLocation());
            return true;
        }
    }
}