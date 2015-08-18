using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace RefactoringEssentials.CSharp.Diagnostics
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class RedundantIfElseBlockAnalyzer : DiagnosticAnalyzer
    {
        private static readonly DiagnosticDescriptor descriptor = new DiagnosticDescriptor(
            CSharpDiagnosticIDs.RedundantIfElseBlockAnalyzerID,
            GettextCatalog.GetString("Redundant 'else' keyword"),
            GettextCatalog.GetString("Redundant 'else' keyword"),
            DiagnosticAnalyzerCategories.RedundanciesInCode,
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            helpLinkUri: HelpLink.CreateFor(CSharpDiagnosticIDs.RedundantIfElseBlockAnalyzerID),
            customTags: DiagnosticCustomTags.Unnecessary
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
             SyntaxKind.ElseClause
            );
        }

        private static bool TryGetDiagnostic(SyntaxNodeAnalysisContext nodeContext, out Diagnostic diagnostic)
        {
            diagnostic = default(Diagnostic);
            if (nodeContext.IsFromGeneratedCode())
                return false;
            var node = nodeContext.Node as ElseClauseSyntax;
            if (node == null)
                return false;

            if (!ElseIsRedundantControlFlow(node, nodeContext))
                return false;

            diagnostic = Diagnostic.Create(descriptor, node.GetLocation());
            return true;
        }

        private static bool ElseIsRedundantControlFlow(ElseClauseSyntax elseClause, SyntaxNodeAnalysisContext syntaxNode)
        {
            var blockSyntax = elseClause.Statement as BlockSyntax;
            if (blockSyntax == null || !blockSyntax.Statements.Any())
                return true;

            var result = syntaxNode.SemanticModel.AnalyzeControlFlow(blockSyntax);
            return !result.EndPointIsReachable;
        }
    }
}