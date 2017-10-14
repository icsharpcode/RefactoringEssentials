using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace RefactoringEssentials.CSharp.Diagnostics
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class PossibleMistakenCallToGetTypeAnalyzer : DiagnosticAnalyzer
    {
        static readonly DiagnosticDescriptor descriptor = new DiagnosticDescriptor(
            CSharpDiagnosticIDs.PossibleMistakenCallToGetTypeAnalyzerID,
            GettextCatalog.GetString("Possible mistaken call to 'object.GetType()'"),
            GettextCatalog.GetString("Possible mistaken call to 'object.GetType()'"),
            DiagnosticAnalyzerCategories.PracticesAndImprovements,
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            helpLinkUri: HelpLink.CreateFor(CSharpDiagnosticIDs.PossibleMistakenCallToGetTypeAnalyzerID)
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
                SyntaxKind.InvocationExpression
            );
        }

        static bool TryGetDiagnostic(SyntaxNodeAnalysisContext nodeContext, out Diagnostic diagnostic)
        {
            diagnostic = default(Diagnostic);
            var node = nodeContext.Node as InvocationExpressionSyntax;
            var memberExpr = node.Expression as MemberAccessExpressionSyntax;
            if (memberExpr == null || memberExpr.Name.Identifier.ValueText != "GetType")
                return false;
            var methodSymbol = nodeContext.SemanticModel.GetSymbolInfo(memberExpr);
            if (methodSymbol.Symbol == null || !IsSystemType(methodSymbol.Symbol.ContainingType) || methodSymbol.Symbol.IsStatic)
                return false;
            diagnostic = Diagnostic.Create(
                descriptor,
                node.GetLocation()
            );
            return true;
        }

        static bool IsSystemType(INamedTypeSymbol type)
        {
            return type.Name == "Type" && type.ContainingNamespace.ToDisplayString() == "System";
        }
    }
}