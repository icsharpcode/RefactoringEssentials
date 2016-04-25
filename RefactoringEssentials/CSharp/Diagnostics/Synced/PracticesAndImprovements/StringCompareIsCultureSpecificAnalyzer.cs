using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;

namespace RefactoringEssentials.CSharp.Diagnostics
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    [NotPortedYet]
    public class StringCompareIsCultureSpecificAnalyzer : DiagnosticAnalyzer
    {
        static readonly DiagnosticDescriptor descriptor = new DiagnosticDescriptor(
            CSharpDiagnosticIDs.StringCompareIsCultureSpecificAnalyzerID,
            GettextCatalog.GetString("Warns when a culture-aware 'Compare' call is used by default"),
            GettextCatalog.GetString("'string.Compare' is culture-aware"),
            DiagnosticAnalyzerCategories.PracticesAndImprovements,
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            helpLinkUri: HelpLink.CreateFor(CSharpDiagnosticIDs.StringCompareIsCultureSpecificAnalyzerID)
        );

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(descriptor);

        //static readonly DiagnosticDescriptor Rule1 = new DiagnosticDescriptor (DiagnosticId, Description, "Use ordinal comparison", Category, DiagnosticSeverity.Warning, true, "'string.Compare' is culture-aware");
        //static readonly DiagnosticDescriptor Rule2 = new DiagnosticDescriptor (DiagnosticId, Description, "Use culture-aware comparison", Category, DiagnosticSeverity.Warning, true, "'string.Compare' is culture-aware");


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
            if (mre.Name.Identifier.ValueText != "Compare")
                return false;
            if (node.ArgumentList.Arguments.Count != 2 &&
                node.ArgumentList.Arguments.Count != 3 &&
                node.ArgumentList.Arguments.Count != 5 &&
                node.ArgumentList.Arguments.Count != 6)
                return false;

            var rr = nodeContext.SemanticModel.GetSymbolInfo(node, nodeContext.CancellationToken);
            if (rr.Symbol == null)
                return false;
            var symbol = rr.Symbol;
            if (!(symbol.ContainingType != null && symbol.ContainingType.SpecialType == SpecialType.System_String))
                return false;
            if (!symbol.IsStatic)
                return false;
            var parameters = symbol.GetParameters();
            var firstParameter = parameters.FirstOrDefault();
            if (firstParameter == null || firstParameter.Type.SpecialType != SpecialType.System_String)
                return false;   // First parameter not a string

            var lastParameter = parameters.Last();
            if (lastParameter.Type.Name == "StringComparison")
                return false; // already specifying a string comparison

            diagnostic = Diagnostic.Create(
                descriptor,
                node.GetLocation()
            );
            return true;
        }
    }
}