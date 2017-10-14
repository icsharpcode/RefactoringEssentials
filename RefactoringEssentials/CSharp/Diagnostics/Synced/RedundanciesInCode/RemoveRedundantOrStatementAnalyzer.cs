using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace RefactoringEssentials.CSharp.Diagnostics
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class RemoveRedundantOrStatementAnalyzer : DiagnosticAnalyzer
    {
        static readonly DiagnosticDescriptor descriptor = new DiagnosticDescriptor(
            CSharpDiagnosticIDs.RemoveRedundantOrStatementAnalyzerID,
            GettextCatalog.GetString("Remove redundant statement"),
            GettextCatalog.GetString("Statement is redundant"),
            DiagnosticAnalyzerCategories.RedundanciesInCode,
            DiagnosticSeverity.Hidden,
            isEnabledByDefault: true,
            helpLinkUri: HelpLink.CreateFor(CSharpDiagnosticIDs.RemoveRedundantOrStatementAnalyzerID),
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
                    Diagnostic diagnostic;
                    if (TryGetDiagnostic(nodeContext, out diagnostic))
                    {
                        nodeContext.ReportDiagnostic(diagnostic);
                    }
                },
                new SyntaxKind[] { SyntaxKind.OrAssignmentExpression, SyntaxKind.AndAssignmentExpression }
            );
        }

        static bool TryGetDiagnostic(SyntaxNodeAnalysisContext nodeContext, out Diagnostic diagnostic)
        {
            diagnostic = default(Diagnostic);

            var assignment = nodeContext.Node as AssignmentExpressionSyntax;
            if (assignment == null || !(assignment.Parent is ExpressionStatementSyntax))
                return false;

            //check redundant foo |= false
            var literalRight = assignment.Right as LiteralExpressionSyntax;
            if (literalRight == null)
                return false;

            bool isOrWithFalse = assignment.IsKind(SyntaxKind.OrAssignmentExpression) && literalRight.IsKind(SyntaxKind.FalseLiteralExpression);
            bool isAndWithTrue = assignment.IsKind(SyntaxKind.AndAssignmentExpression) && literalRight.IsKind(SyntaxKind.TrueLiteralExpression);
            if (isOrWithFalse || isAndWithTrue)
            {
                diagnostic = Diagnostic.Create(descriptor, assignment.GetLocation());
                return true;
            }
            return false;
        }
    }
}