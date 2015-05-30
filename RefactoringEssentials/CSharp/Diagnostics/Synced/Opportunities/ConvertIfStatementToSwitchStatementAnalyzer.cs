using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using RefactoringEssentials.CSharp.CodeRefactorings;

namespace RefactoringEssentials.CSharp.Diagnostics
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class ConvertIfStatementToSwitchStatementAnalyzer : DiagnosticAnalyzer
    {
        static readonly DiagnosticDescriptor descriptor = new DiagnosticDescriptor(
            NRefactoryDiagnosticIDs.ConvertIfStatementToSwitchStatementAnalyzerID,
            GettextCatalog.GetString("'if' statement can be re-written as 'switch' statement"),
            GettextCatalog.GetString("Convert to 'switch' statement"),
            DiagnosticAnalyzerCategories.Opportunities,
            DiagnosticSeverity.Info,
            isEnabledByDefault: true,
            helpLinkUri: HelpLink.CreateFor(NRefactoryDiagnosticIDs.ConvertIfStatementToSwitchStatementAnalyzerID)
        );

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(descriptor);

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(
                (nodeContext) =>
                {
                    Diagnostic diagnostic;
                    if (TryGetDiagnostic(nodeContext, out diagnostic))
                    {
                        nodeContext.ReportDiagnostic(diagnostic);
                    }
                },
                new SyntaxKind[] { SyntaxKind.IfStatement }
            );
        }

        static bool TryGetDiagnostic(SyntaxNodeAnalysisContext nodeContext, out Diagnostic diagnostic)
        {
            var node = nodeContext.Node as IfStatementSyntax;
            var semanticModel = nodeContext.SemanticModel;
            var cancellationToken = nodeContext.CancellationToken;

            diagnostic = default(Diagnostic);
            if (nodeContext.IsFromGeneratedCode())
                return false;
            if (node.Parent is IfStatementSyntax || node.Parent is ElseClauseSyntax)
                return false;

            var switchExpr = ConvertIfStatementToSwitchStatementCodeRefactoringProvider.GetSwitchExpression(semanticModel, node.Condition);
            if (switchExpr == null)
                return false;
            var switchSections = new List<SwitchSectionSyntax>();
            if (!ConvertIfStatementToSwitchStatementCodeRefactoringProvider.CollectSwitchSections(switchSections, semanticModel, node, switchExpr))
                return false;
            if (switchSections.Count(s => !s.Labels.OfType<DefaultSwitchLabelSyntax>().Any()) <= 2)
                return false;

            diagnostic = Diagnostic.Create(
                descriptor,
                node.IfKeyword.GetLocation()
            );
            return true;
        }
    }
}