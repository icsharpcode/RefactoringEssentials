using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace RefactoringEssentials.CSharp.Diagnostics
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class RedundantEmptyFinallyBlockAnalyzer : DiagnosticAnalyzer
    {
        static readonly DiagnosticDescriptor descriptor = new DiagnosticDescriptor(
            CSharpDiagnosticIDs.RedundantEmptyFinallyBlockAnalyzerID,
            GettextCatalog.GetString("Redundant empty finally block"),
            GettextCatalog.GetString("Redundant empty finally block"),
            DiagnosticAnalyzerCategories.RedundanciesInCode,
            DiagnosticSeverity.Info,
            isEnabledByDefault: true,
            helpLinkUri: HelpLink.CreateFor(CSharpDiagnosticIDs.RedundantEmptyFinallyBlockAnalyzerID),
            customTags: DiagnosticCustomTags.Unnecessary
        );

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(descriptor);

        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.RegisterSyntaxNodeAction(
            	(nodeContext) => {
            		Diagnostic diagnostic;
            		if (TryGetDiagnostic (nodeContext, out diagnostic)) {
            			nodeContext.ReportDiagnostic(diagnostic);
            		}
            	}, 
                new SyntaxKind[] { SyntaxKind.FinallyClause }
            );
        }

        static bool TryGetDiagnostic(SyntaxNodeAnalysisContext nodeContext, out Diagnostic diagnostic)
        {
            diagnostic = default(Diagnostic);
            var node = nodeContext.Node as FinallyClauseSyntax;
            if (node.Block != null && node.Block.Statements.Count == 0)
            {
                diagnostic = Diagnostic.Create(descriptor, node.FinallyKeyword.GetLocation());
                return true;
            }
            return false;
        }
    }
}