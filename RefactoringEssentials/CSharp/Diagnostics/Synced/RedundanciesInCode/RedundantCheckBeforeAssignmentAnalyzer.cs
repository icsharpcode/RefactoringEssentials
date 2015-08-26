using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System.Linq;

namespace RefactoringEssentials.CSharp.Diagnostics
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class RedundantCheckBeforeAssignmentAnalyzer : DiagnosticAnalyzer
    {
        private static readonly DiagnosticDescriptor descriptor = new DiagnosticDescriptor(
            CSharpDiagnosticIDs.RedundantCheckBeforeAssignmentAnalyzerID,
            GettextCatalog.GetString("Check for inequality before assignment is redundant if (x != value) x = value;"),
            GettextCatalog.GetString("Redundant condition check before assignment"),
            DiagnosticAnalyzerCategories.RedundanciesInCode,
            DiagnosticSeverity.Info,
            isEnabledByDefault: true,
            helpLinkUri: HelpLink.CreateFor(CSharpDiagnosticIDs.RedundantCheckBeforeAssignmentAnalyzerID),
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
                SyntaxKind.IfStatement
            );
        }

        private static bool TryGetDiagnostic(SyntaxNodeAnalysisContext nodeContext, out Diagnostic diagnostic)
        {
            diagnostic = default(Diagnostic);
            if (nodeContext.IsFromGeneratedCode())
                return false;

            var node = nodeContext.Node as IfStatementSyntax;
            var check = node?.Condition as BinaryExpressionSyntax;
            if (check == null)
                return false;
            var block = node.Statement as BlockSyntax;
            if (block?.Statements.Count > 0)
            {
                var statement = block.Statements.ElementAt(0) as ExpressionStatementSyntax;
                var assignmentExpression = statement?.Expression as AssignmentExpressionSyntax;
                if (assignmentExpression == null ||
                    !(check.Left is IdentifierNameSyntax ||
                    !(assignmentExpression.Left is IdentifierNameSyntax)))
                    return false;

                var checkLeftSymbol = nodeContext.SemanticModel.GetSymbolInfo(check.Left).Symbol;
                var assignmentLeftSymbol = nodeContext.SemanticModel.GetSymbolInfo(assignmentExpression.Left).Symbol;
                
                if (checkLeftSymbol == null||
                    assignmentLeftSymbol == null||
                    !checkLeftSymbol.Name.Equals(assignmentLeftSymbol.Name))
                    return false;
                diagnostic = Diagnostic.Create(descriptor, check.GetLocation());
                return true;
            }
            else
            {
                var statement = node.Statement as ExpressionStatementSyntax;
                var expression = statement;
                var assignment = expression?.Expression as AssignmentExpressionSyntax;
                if (assignment == null)
                    return false;

                var checkLeftSymbol = nodeContext.SemanticModel.GetDeclaredSymbol(check.Left);
                var assignmentLeftSymbol = nodeContext.SemanticModel.GetDeclaredSymbol(assignment.Left);
                if ((checkLeftSymbol == null) || (assignmentLeftSymbol == null))
                    return false;

                if (!checkLeftSymbol.Name.Equals(assignmentLeftSymbol.Name))
                    return false;

                diagnostic = Diagnostic.Create(descriptor, check.GetLocation());
                return true;
            }
        }
    }
}