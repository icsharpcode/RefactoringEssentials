using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace RefactoringEssentials.CSharp.Diagnostics
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class RedundantCaseLabelAnalyzer : DiagnosticAnalyzer
    {
        static readonly DiagnosticDescriptor descriptor = new DiagnosticDescriptor(
            CSharpDiagnosticIDs.RedundantCaseLabelAnalyzerID,
            GettextCatalog.GetString("Redundant case label"),
            GettextCatalog.GetString("'case' label is redundant"),
            DiagnosticAnalyzerCategories.RedundanciesInCode,
            DiagnosticSeverity.Hidden,
            isEnabledByDefault: true,
            helpLinkUri: HelpLink.CreateFor(CSharpDiagnosticIDs.RedundantCaseLabelAnalyzerID),
            customTags: DiagnosticCustomTags.Unnecessary
        );

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(descriptor);

        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.RegisterSyntaxNodeAction(
                nodeContext =>
                {
                    ScanDiagnostics(nodeContext);
                },
                new SyntaxKind[] { SyntaxKind.SwitchSection }
            );
        }

        static void ScanDiagnostics(SyntaxNodeAnalysisContext nodeContext)
        {
            var node = nodeContext.Node as SwitchSectionSyntax;
            if (node.Labels.Count < 2)
                return;
            if (!node.Labels.Any(l => l.IsKind(SyntaxKind.DefaultSwitchLabel)))
                return;
            foreach (var caseLabel in node.Labels)
            {
                if (caseLabel.IsKind(SyntaxKind.DefaultSwitchLabel))
                    continue;
                nodeContext.ReportDiagnostic(Diagnostic.Create(descriptor, caseLabel.GetLocation()));
            }
        }
    }
}