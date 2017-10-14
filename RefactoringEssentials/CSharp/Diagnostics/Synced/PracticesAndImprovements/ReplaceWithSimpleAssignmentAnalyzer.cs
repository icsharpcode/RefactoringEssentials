using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace RefactoringEssentials.CSharp.Diagnostics
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class ReplaceWithSimpleAssignmentAnalyzer : DiagnosticAnalyzer
    {
        static readonly DiagnosticDescriptor descriptor = new DiagnosticDescriptor(
            CSharpDiagnosticIDs.ReplaceWithSimpleAssignmentAnalyzerID,
            GettextCatalog.GetString("Replace with simple assignment"),
            GettextCatalog.GetString("Replace with simple assignment"),
            DiagnosticAnalyzerCategories.PracticesAndImprovements,
            DiagnosticSeverity.Info,
            isEnabledByDefault: true,
            helpLinkUri: HelpLink.CreateFor(CSharpDiagnosticIDs.ReplaceWithSimpleAssignmentAnalyzerID)
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
                SyntaxKind.OrAssignmentExpression,
                SyntaxKind.AndAssignmentExpression

            );
        }

        static bool TryGetDiagnostic(SyntaxNodeAnalysisContext nodeContext, out Diagnostic diagnostic)
        {
            diagnostic = default(Diagnostic);
            var node = nodeContext.Node as AssignmentExpressionSyntax;

            if (node.IsKind(SyntaxKind.OrAssignmentExpression))
            {
                LiteralExpressionSyntax right = node.Right as LiteralExpressionSyntax;
                //if right is true
                if ((right != null) && right.IsKind(SyntaxKind.TrueLiteralExpression))
                {
                    diagnostic = Diagnostic.Create(
                        descriptor,
                        node.GetLocation()
                    );
                    return true;
                }

            }
            else if (node.IsKind(SyntaxKind.AndAssignmentExpression))
            {
                LiteralExpressionSyntax right = node.Right as LiteralExpressionSyntax;
                //if right is false
                if ((right != null) && right.IsKind(SyntaxKind.FalseLiteralExpression))
                {
                    diagnostic = Diagnostic.Create(
                        descriptor,
                        node.GetLocation()
                    );
                    return true;
                }
            }
            return false;
        }
    }
}