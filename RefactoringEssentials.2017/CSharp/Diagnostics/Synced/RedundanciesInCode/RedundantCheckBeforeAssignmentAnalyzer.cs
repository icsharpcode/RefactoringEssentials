using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace RefactoringEssentials.CSharp.Diagnostics
{
	/// <summary>
	/// The analyzer checks patterns like the following:
	/// <code>
	/// if (var != value)
	///		var = value;
	/// </code>
	/// In addition to that, it works with if-statements using block-syntax (containing a single statement),
	/// and empty else-blocks or empty-statement else-blocks.
	/// 
	/// The Fix removes the redundant check. This works on locals, parameters, fields and properties.
	/// If a property setter does something expensive, it is recommended to do such a check *inside*
	/// the property setter, *not* in the calling code. Hence, the check is redundant anyway.
	/// </summary>
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class RedundantCheckBeforeAssignmentAnalyzer : DiagnosticAnalyzer
    {
        private static readonly DiagnosticDescriptor descriptor = new DiagnosticDescriptor(
            CSharpDiagnosticIDs.RedundantCheckBeforeAssignmentAnalyzerID,
            GettextCatalog.GetString("Check for inequality before assignment is redundant if (x != value) x = value;"),
            GettextCatalog.GetString("Redundant condition check before assignment"),
            DiagnosticAnalyzerCategories.RedundanciesInCode,
            DiagnosticSeverity.Hidden,
            isEnabledByDefault: true,
            helpLinkUri: HelpLink.CreateFor(CSharpDiagnosticIDs.RedundantCheckBeforeAssignmentAnalyzerID),
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
                    if (TryGetDiagnostic(nodeContext, out diagnostic))
                    {
                        nodeContext.ReportDiagnostic(diagnostic);
                    }
                },
                SyntaxKind.IfStatement
            );
        }

        static bool TryGetDiagnostic(SyntaxNodeAnalysisContext nodeContext, out Diagnostic diagnostic)
        {
            diagnostic = default(Diagnostic);

            var ifStatement = nodeContext.Node as IfStatementSyntax;

            if (ifStatement.Else != null)
            {
                var elseStmt = ifStatement.Else?.Statement;
                var block = elseStmt as BlockSyntax;
                if (block != null)
                {
                    if (block.Statements.Count > 0)
                        return false;
                }
                else
                {
                    if (!(elseStmt is EmptyStatementSyntax))
                        return false;
                }
            }

            var condition = ifStatement?.Condition as BinaryExpressionSyntax;
            if (condition == null || !condition.OperatorToken.IsKind(SyntaxKind.ExclamationEqualsToken))
                return false;
            AssignmentExpressionSyntax assignment;
            if (ifStatement.Statement is BlockSyntax)
            {
                var block = (BlockSyntax)ifStatement.Statement;
                if (block.Statements.Count != 1)
                    return false;
                var statement = (block.Statements[0] as ExpressionStatementSyntax)?.Expression;
                assignment = statement as AssignmentExpressionSyntax;
            }
            else
            {
                var statement = (ifStatement.Statement as ExpressionStatementSyntax)?.Expression;
                assignment = statement as AssignmentExpressionSyntax;
            }
            if (!CheckConditionAndAssignment(nodeContext, assignment, condition))
                return false;
            diagnostic = Diagnostic.Create(descriptor, condition.GetLocation());
            return true;
        }

        static bool CheckConditionAndAssignment(SyntaxNodeAnalysisContext nodeContext, AssignmentExpressionSyntax assignment, BinaryExpressionSyntax condition)
        {
            if (assignment == null)
                return false;

            var assignmentTarget = nodeContext.SemanticModel.GetSymbolInfo(assignment.Left).Symbol;
            if (assignmentTarget == null)
                return false;

            var condLeftSymbol = nodeContext.SemanticModel.GetSymbolInfo(condition.Left).Symbol;
            var condRightSymbol = nodeContext.SemanticModel.GetSymbolInfo(condition.Right).Symbol;

            var assignmentValue = nodeContext.SemanticModel.GetSymbolInfo(assignment.Right).Symbol;
            var constant = nodeContext.SemanticModel.GetConstantValue(assignment.Right);

            bool constantAssignment = assignmentValue == null && constant.HasValue;

            if (assignmentTarget.Equals(condLeftSymbol))
            {
                if (constantAssignment)
                {
                    var condRightValue = nodeContext.SemanticModel.GetConstantValue(condition.Right);
                    if (!condRightValue.HasValue)
                        return false;

                    return condRightValue.Value == constant.Value;
                }
                else
                {
                    if ((assignmentValue == null) || !assignmentValue.Equals(condRightSymbol))
                        return false;
                }
                return true;
            }

            // flipped operands
            if (assignmentTarget.Equals(condRightSymbol))
            {
                if (constantAssignment)
                {
                    var condLeftValue = nodeContext.SemanticModel.GetConstantValue(condition.Left);
                    if (condLeftValue.HasValue)
                        return condLeftValue.Value == constant.Value;
                }
                else
                {
                    if ((assignmentValue == null) || !assignmentValue.Equals(condLeftSymbol))
                        return false;
                }
                return true;
            }

            return false;
        }
    }
}