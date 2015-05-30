using System.Collections.Immutable;
using RefactoringEssentials;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace RefactoringEssentials.CSharp.Diagnostics
{
    /// <summary>
    /// Finds redundant base qualifier 
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class RedundantBaseQualifierAnalyzer : DiagnosticAnalyzer
    {
        static readonly DiagnosticDescriptor descriptor = new DiagnosticDescriptor(
            DiagnosticIDs.RedundantBaseQualifierAnalyzerID,
            GettextCatalog.GetString("'base.' is redundant and can safely be removed"),
            GettextCatalog.GetString("'base.' is redundant and can safely be removed"),
            DiagnosticAnalyzerCategories.RedundanciesInCode,
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            helpLinkUri: HelpLink.CreateFor(DiagnosticIDs.RedundantBaseQualifierAnalyzerID),
            customTags: DiagnosticCustomTags.Unnecessary
        );

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(descriptor);

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(
                nodeContext =>
                {
                    Diagnostic diagnostic;
                    if (TryGetDiagnostic(nodeContext, out diagnostic))
                    {
                        nodeContext.ReportDiagnostic(diagnostic);
                    }
                },
                new SyntaxKind[] { SyntaxKind.SimpleMemberAccessExpression }
            );
        }

        static bool TryGetDiagnostic(SyntaxNodeAnalysisContext nodeContext, out Diagnostic diagnostic)
        {
            diagnostic = default(Diagnostic);
            if (nodeContext.IsFromGeneratedCode())
                return false;

            var node = nodeContext.Node as MemberAccessExpressionSyntax;
            if (node.Expression.IsKind(SyntaxKind.BaseExpression))
            {
                var replacementNode = node.Name.WithLeadingTrivia(node.GetLeadingTrivia()).WithTrailingTrivia(node.GetTrailingTrivia());
                if (node.CanReplaceWithReducedName(replacementNode, nodeContext.SemanticModel, nodeContext.CancellationToken))
                {
                    diagnostic = Diagnostic.Create(descriptor, node.Expression.GetLocation(), additionalLocations: new[] { node.OperatorToken.GetLocation() });
                    return true;
                }
            }
            return false;
        }
    }
}