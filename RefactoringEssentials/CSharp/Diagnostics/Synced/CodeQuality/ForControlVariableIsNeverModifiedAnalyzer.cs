using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;

namespace RefactoringEssentials.CSharp.Diagnostics
{
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class ForControlVariableIsNeverModifiedAnalyzer : DiagnosticAnalyzer
    {
        static readonly DiagnosticDescriptor descriptor = new DiagnosticDescriptor(
            CSharpDiagnosticIDs.ForControlVariableIsNeverModifiedAnalyzerID,
            GettextCatalog.GetString("'for' loop control variable is never modified"),
            GettextCatalog.GetString("'for' loop control variable is never modified"),
            DiagnosticAnalyzerCategories.CodeQualityIssues,
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            helpLinkUri: HelpLink.CreateFor(CSharpDiagnosticIDs.ForControlVariableIsNeverModifiedAnalyzerID)
        );

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(descriptor);

        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.RegisterSyntaxNodeAction(
                AnalyzeForStatement, 
                new SyntaxKind[] { SyntaxKind.ForStatement }
            );
        }

        void AnalyzeForStatement(SyntaxNodeAnalysisContext nodeContext)
        {
            var node = nodeContext.Node as ForStatementSyntax;
            if (node?.Declaration?.Variables == null)
                return;
            foreach (var variable in node.Declaration.Variables) {
                var local = nodeContext.SemanticModel.GetDeclaredSymbol(variable);
                if (local == null)
                    return;
                if ((node.Condition == null) || !node.Condition.DescendantNodesAndSelf().OfType<IdentifierNameSyntax>().Any(n => n.Identifier.ValueText == local.Name))
                    continue;
                bool wasModified = false;
                foreach (var identifier in node.DescendantNodesAndSelf().OfType<IdentifierNameSyntax>()) {
                    if (identifier.Identifier.ValueText != local.Name)
                        continue;

                    if (!IsModified(identifier))
                        continue;

                    if (nodeContext.SemanticModel.GetSymbolInfo(identifier).Symbol.Equals(local)) {
                        wasModified = true;
                        break;
                    }
                }

                if (!wasModified) {
                    nodeContext.ReportDiagnostic(Diagnostic.Create(descriptor, variable.Identifier.GetLocation()));
                }
            }
        }

        bool IsModified(IdentifierNameSyntax identifier)
        {
            if (identifier.Parent.IsKind(SyntaxKind.PreDecrementExpression) ||
                identifier.Parent.IsKind(SyntaxKind.PostDecrementExpression) ||
                identifier.Parent.IsKind(SyntaxKind.PreIncrementExpression) || 
                identifier.Parent.IsKind(SyntaxKind.PostIncrementExpression))
                return true;

            var assignment = identifier.Parent as AssignmentExpressionSyntax;
            if (assignment != null && assignment.Left == identifier)
                return true;

            var arg = identifier.Parent as ArgumentSyntax;
            if (arg != null && (arg.RefOrOutKeyword.IsKind(SyntaxKind.RefKeyword)  || arg.RefOrOutKeyword.IsKind(SyntaxKind.OutKeyword)))
                return true;
            
            return false;
        }
    }
}
