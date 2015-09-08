using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System.Linq;

namespace RefactoringEssentials.CSharp.Diagnostics
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class RewriteIfReturnToReturnAnalyzer : DiagnosticAnalyzer
    {
        private static readonly DiagnosticDescriptor descriptor = new DiagnosticDescriptor(
            CSharpDiagnosticIDs.RewriteIfReturnToReturnAnalyzerID,
            GettextCatalog.GetString("Convert 'if...return' to 'return'"),
            GettextCatalog.GetString("Convert to 'return' statement"),
            DiagnosticAnalyzerCategories.Opportunities,
            DiagnosticSeverity.Info,
            isEnabledByDefault: true,
            helpLinkUri: HelpLink.CreateFor(CSharpDiagnosticIDs.RewriteIfReturnToReturnAnalyzerID)
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
                }, SyntaxKind.IfStatement);
        }

        private static bool TryGetDiagnostic(SyntaxNodeAnalysisContext nodeContext, out Diagnostic diagnostic)
        {
            diagnostic = default(Diagnostic);
            if (nodeContext.IsFromGeneratedCode())
                return false;

            var node = nodeContext.Node as IfStatementSyntax;
            if (node == null)
                return false;

            var methodBody = node.Parent as BlockSyntax;
            if (methodBody == null)
                return false;

            var ifStatementIndex = methodBody.Statements.IndexOf(node);
            if (ifStatementIndex == methodBody.Statements.Count - 1)
                return false;

            if (node.Statement is ReturnStatementSyntax &&
                methodBody.Statements[ifStatementIndex + 1] is ReturnStatementSyntax)
            {
                diagnostic = Diagnostic.Create(descriptor, node.GetLocation());
                return true;
            }
            return false;
        }
    }
}