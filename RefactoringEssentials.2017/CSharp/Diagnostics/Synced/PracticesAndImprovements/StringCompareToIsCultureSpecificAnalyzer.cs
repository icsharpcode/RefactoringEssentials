using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;

namespace RefactoringEssentials.CSharp.Diagnostics
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class StringCompareToIsCultureSpecificAnalyzer : DiagnosticAnalyzer
    {
        static readonly DiagnosticDescriptor descriptor = new DiagnosticDescriptor(
            CSharpDiagnosticIDs.StringCompareToIsCultureSpecificAnalyzerID,
            GettextCatalog.GetString("Warns when a culture-aware 'string.CompareTo' call is used by default"),
            GettextCatalog.GetString("'string.CompareTo' is culture-aware"),
            DiagnosticAnalyzerCategories.PracticesAndImprovements,
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            helpLinkUri: HelpLink.CreateFor(CSharpDiagnosticIDs.StringCompareToIsCultureSpecificAnalyzerID)
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
            MemberAccessExpressionSyntax mre = node.Expression as MemberAccessExpressionSyntax;
            if (mre == null)
                return false;
            if (mre.Name.Identifier.ValueText != "CompareTo")
                return false;
            if (node.ArgumentList.Arguments.Count != 1)
                return false;

            var rr = nodeContext.SemanticModel.GetSymbolInfo(node, nodeContext.CancellationToken);
            if (rr.Symbol == null)
                return false;
            var symbol = rr.Symbol;
            if (!(symbol.ContainingType != null && symbol.ContainingType.SpecialType == SpecialType.System_String))
                return false;
            var parameters = symbol.GetParameters();
            var firstParameter = parameters.FirstOrDefault();
            if (firstParameter == null || firstParameter.Type.SpecialType != SpecialType.System_String)
                return false;   // First parameter not a string
            diagnostic = Diagnostic.Create(
                descriptor,
                node.GetLocation()
            );
            return true;
        }
    }
}