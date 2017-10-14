using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace RefactoringEssentials.CSharp.Diagnostics
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class BaseMemberHasParamsAnalyzer : DiagnosticAnalyzer
    {
        static readonly DiagnosticDescriptor descriptor = new DiagnosticDescriptor(
            CSharpDiagnosticIDs.BaseMemberHasParamsAnalyzerID,
            GettextCatalog.GetString("Base parameter has 'params' modifier, but missing in overrider"),
            GettextCatalog.GetString("Base method '{0}' has a 'params' modifier"),
            DiagnosticAnalyzerCategories.PracticesAndImprovements,
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            helpLinkUri: HelpLink.CreateFor(CSharpDiagnosticIDs.BaseMemberHasParamsAnalyzerID)
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
                SyntaxKind.MethodDeclaration
            );
        }

        static bool TryGetDiagnostic(SyntaxNodeAnalysisContext nodeContext, out Diagnostic diagnostic)
        {
            diagnostic = default(Diagnostic);
            var node = nodeContext.Node as MethodDeclarationSyntax;

            if (!node.Modifiers.Any(m => m.IsKind(SyntaxKind.OverrideKeyword)))
                return false;
            var lastParam = node.ParameterList.Parameters.LastOrDefault();
            if (lastParam == null || lastParam.Modifiers.Any(m => m.IsKind(SyntaxKind.ParamsKeyword)))
                return false;
            if (lastParam.Type == null || !lastParam.Type.IsKind(SyntaxKind.ArrayType))
                return false;
            var rr = nodeContext.SemanticModel.GetDeclaredSymbol(node);
            if (rr == null || !rr.IsOverride)
                return false;
            var baseMember = rr.OverriddenMethod;
            if (baseMember == null || baseMember.Parameters.Length == 0 || !baseMember.Parameters.Last().IsParams)
                return false;

            diagnostic = Diagnostic.Create(
                descriptor,
                lastParam.GetLocation(),
                baseMember.Name
            );
            return true;
        }
    }
}