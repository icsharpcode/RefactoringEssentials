using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;

namespace RefactoringEssentials.CSharp.Diagnostics
{
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class PartialMethodParameterNameMismatchAnalyzer : DiagnosticAnalyzer
    {
        static readonly DiagnosticDescriptor descriptor = new DiagnosticDescriptor(
            CSharpDiagnosticIDs.PartialMethodParameterNameMismatchAnalyzerID,
            GettextCatalog.GetString("Parameter name differs in partial method definition"),
            GettextCatalog.GetString("Parameter name differs in partial method definition should be '{0}'"),
            DiagnosticAnalyzerCategories.CodeQualityIssues,
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            helpLinkUri: HelpLink.CreateFor(CSharpDiagnosticIDs.PartialMethodParameterNameMismatchAnalyzerID)
        );

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(descriptor);

        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.RegisterSyntaxNodeAction(
                AnalyzeMethod,
                new SyntaxKind[] { SyntaxKind.MethodDeclaration }
            );
        }

        static void AnalyzeMethod(SyntaxNodeAnalysisContext nodeContext)
        {
            var node = nodeContext.Node as MethodDeclarationSyntax;
            if (node == null || !node.Modifiers.Any(m => m.IsKind(SyntaxKind.PartialKeyword)))
                return;
            if (node.Body == null && node.ExpressionBody == null)
                return;

            var symbol = nodeContext.SemanticModel.GetDeclaredSymbol(node) as IMethodSymbol;
            if ((symbol == null) || (symbol.PartialDefinitionPart == null))
                return;
            for (int i = 0; i < symbol.PartialDefinitionPart.Parameters.Length && i < node.ParameterList.Parameters.Count; i++) {
                if (symbol.PartialDefinitionPart.Parameters[i].Name != node.ParameterList.Parameters[i].Identifier.ValueText) {
                    nodeContext.ReportDiagnostic(Diagnostic.Create(descriptor, node.ParameterList.Parameters[i].Identifier.GetLocation(), symbol.PartialDefinitionPart.Parameters[i].Name));
                }
            }
        }
    }
}