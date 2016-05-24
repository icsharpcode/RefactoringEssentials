using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace RefactoringEssentials.CSharp.Diagnostics
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class StringEndsWithIsCultureSpecificAnalyzer : DiagnosticAnalyzer
    {
        static readonly DiagnosticDescriptor descriptor = new DiagnosticDescriptor(
            CSharpDiagnosticIDs.StringEndsWithIsCultureSpecificAnalyzerID,
            GettextCatalog.GetString("Warns when a culture-aware 'EndsWith' call is used by default."),
            GettextCatalog.GetString("'EndsWith' is culture-aware and missing a StringComparison argument"),
            DiagnosticAnalyzerCategories.PracticesAndImprovements,
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            helpLinkUri: HelpLink.CreateFor(CSharpDiagnosticIDs.StringEndsWithIsCultureSpecificAnalyzerID)
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
            MemberAccessExpressionSyntax mre = node.Expression as MemberAccessExpressionSyntax;
            if (mre == null)
                return false;
            if (mre.Name.Identifier.ValueText != "EndsWith")
                return false;

            var rr = nodeContext.SemanticModel.GetSymbolInfo(node, nodeContext.CancellationToken);
            if (rr.Symbol == null)
                return false;
            var symbol = rr.Symbol;
            if (!(symbol.ContainingType != null && symbol.ContainingType.SpecialType == SpecialType.System_String))
                return false;
            var parameters = symbol.GetParameters();
            // Ignore calls to EndsWith(String, bool, CultureInfo), there is no overload with StringComparison for them
            if (parameters.Length == 3)
                return false;
            var firstParameter = parameters.FirstOrDefault();
            if (firstParameter == null || firstParameter.Type.SpecialType != SpecialType.System_String)
                return false;   // First parameter not a string
            var lastParameter = parameters.Last();
            if (lastParameter.Type.Name == "StringComparison")
                return false;   // already specifying a string comparison
            diagnostic = Diagnostic.Create(
                descriptor,
                node.GetLocation()
            );
            return true;
        }
    }
}