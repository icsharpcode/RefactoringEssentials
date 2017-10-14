using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace RefactoringEssentials.CSharp.Diagnostics
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class OptionalParameterRefOutAnalyzer : DiagnosticAnalyzer
    {
        static readonly DiagnosticDescriptor descriptor = new DiagnosticDescriptor(
            CSharpDiagnosticIDs.OptionalParameterRefOutAnalyzerID,
            GettextCatalog.GetString("C# doesn't support optional 'ref' or 'out' parameters"),
            GettextCatalog.GetString("C# doesn't support optional 'ref' or 'out' parameters"),
            DiagnosticAnalyzerCategories.CodeQualityIssues,
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            helpLinkUri: HelpLink.CreateFor(CSharpDiagnosticIDs.OptionalParameterRefOutAnalyzerID)
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
                SyntaxKind.Parameter
            );
        }

        static bool TryGetDiagnostic(SyntaxNodeAnalysisContext nodeContext, out Diagnostic diagnostic)
        {
            diagnostic = default(Diagnostic);
            var node = nodeContext.Node as ParameterSyntax;
            if (!node.Modifiers.Any(m => m.IsKind(SyntaxKind.RefKeyword) || m.IsKind(SyntaxKind.OutKeyword)))
                return false;
            foreach (var attributeLists in node.AttributeLists)
            {
                foreach (var attribute in attributeLists.Attributes)
                {
                    var attrSymbol = nodeContext.SemanticModel.GetTypeInfo(attribute).Type;
                    if (attrSymbol == null)
                        continue;
                    if (attrSymbol.Name == "OptionalAttribute" && attrSymbol.ContainingNamespace.Name == "InteropServices")
                    {
                        diagnostic = Diagnostic.Create(
                            descriptor,
                            node.GetLocation()
                        );
                        return true;
                    }
                }
            }
            return false;
        }
    }
}