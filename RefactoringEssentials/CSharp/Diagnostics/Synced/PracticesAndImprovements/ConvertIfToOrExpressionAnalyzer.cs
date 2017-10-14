using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace RefactoringEssentials.CSharp.Diagnostics
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class ConvertIfToOrExpressionAnalyzer : DiagnosticAnalyzer
    {
        static readonly DiagnosticDescriptor descriptor = new DiagnosticDescriptor(
            CSharpDiagnosticIDs.ConvertIfToOrExpressionAnalyzerID,
            GettextCatalog.GetString("Convert 'if' to '||' expression"),
            GettextCatalog.GetString("{0}"),
            DiagnosticAnalyzerCategories.PracticesAndImprovements,
            DiagnosticSeverity.Info,
            isEnabledByDefault: true,
            helpLinkUri: HelpLink.CreateFor(CSharpDiagnosticIDs.ConvertIfToOrExpressionAnalyzerID)
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
                SyntaxKind.IfStatement
            );
        }

        static bool TryGetDiagnostic(SyntaxNodeAnalysisContext nodeContext, out Diagnostic diagnostic)
        {
            diagnostic = default(Diagnostic);
            var node = nodeContext.Node as IfStatementSyntax;

            ExpressionSyntax target;
            SyntaxTriviaList assignmentTrailingTriviaList;
            if (MatchIfElseStatement(node, SyntaxKind.TrueLiteralExpression, out target, out assignmentTrailingTriviaList))
            {
                var varDeclaration = FindPreviousVarDeclaration(node);
                if (varDeclaration != null)
                {
                    var targetIdentifier = target as IdentifierNameSyntax;
                    if (targetIdentifier == null)
                        return false;
                    var declaredVarName = varDeclaration.Declaration.Variables.First().Identifier.Value;
                    var assignedVarName = targetIdentifier.Identifier.Value;
                    if (declaredVarName != assignedVarName)
                        return false;
                    if (!CheckTarget(targetIdentifier, node.Condition))
                        return false;
                    diagnostic = Diagnostic.Create(
                        descriptor,
                        node.IfKeyword.GetLocation(),
                        "Convert to '||' expression"
                    );
                    return true;
                }
                else
                {
                    if (!CheckTarget(target, node.Condition))
                        return false;
                    diagnostic = Diagnostic.Create(
                        descriptor,
                        node.IfKeyword.GetLocation(),
                        "Replace with '|='"
                    );
                    return true;
                }
            }

            return false;
        }

        internal static bool MatchIfElseStatement(IfStatementSyntax ifStatement, SyntaxKind assignmentLiteralExpressionType, out ExpressionSyntax assignmentTarget, out SyntaxTriviaList assignmentTrailingTriviaList)
        {
            assignmentTarget = null;
            assignmentTrailingTriviaList = SyntaxFactory.TriviaList(SyntaxFactory.SyntaxTrivia(SyntaxKind.DisabledTextTrivia, ""));

            if (ifStatement.Else != null)
                return false;

            var trueExpression = ifStatement.Statement as ExpressionStatementSyntax;
            if (trueExpression != null)
            {
                return CheckForAssignmentOfLiteral(trueExpression, assignmentLiteralExpressionType, out assignmentTarget, out assignmentTrailingTriviaList);
            }

            var blockExpression = ifStatement.Statement as BlockSyntax;
            if (blockExpression != null)
            {
                if (blockExpression.Statements.Count != 1)
                    return false;
                return CheckForAssignmentOfLiteral(blockExpression.Statements[0], assignmentLiteralExpressionType, out assignmentTarget, out assignmentTrailingTriviaList);
            }

            return false;
        }

        internal static bool CheckForAssignmentOfLiteral(StatementSyntax statement, SyntaxKind literalExpressionType, out ExpressionSyntax assignmentTarget, out SyntaxTriviaList assignmentTrailingTriviaList)
        {
            assignmentTarget = null;
            assignmentTrailingTriviaList = SyntaxFactory.TriviaList(SyntaxFactory.SyntaxTrivia(SyntaxKind.DisabledTextTrivia, ""));
            var expressionStatement = statement as ExpressionStatementSyntax;
            if (expressionStatement == null)
                return false;
            var assignmentExpression = expressionStatement.Expression as AssignmentExpressionSyntax;
            if ((assignmentExpression == null) || !assignmentExpression.IsKind(SyntaxKind.SimpleAssignmentExpression))
                return false;
            assignmentTarget = assignmentExpression.Left as IdentifierNameSyntax;
            assignmentTrailingTriviaList = assignmentExpression.OperatorToken.TrailingTrivia;
            if (assignmentTarget == null)
                assignmentTarget = assignmentExpression.Left as MemberAccessExpressionSyntax;
            var rightAssignment = assignmentExpression.Right as LiteralExpressionSyntax;
            return (assignmentTarget != null) && (rightAssignment != null) && (rightAssignment.IsKind(literalExpressionType));
        }

        internal static LocalDeclarationStatementSyntax FindPreviousVarDeclaration(StatementSyntax statement)
        {
            var siblingStatements = statement.Parent.ChildNodes().OfType<StatementSyntax>();
            StatementSyntax lastSibling = null;
            foreach (var sibling in siblingStatements)
            {
                if (sibling == statement)
                {
                    return lastSibling as LocalDeclarationStatementSyntax;
                }
                lastSibling = sibling;
            }

            return null;
        }

        internal static bool CheckTarget(ExpressionSyntax target, ExpressionSyntax expr)
        {
            if (target.IsKind(SyntaxKind.IdentifierName))
                return !expr.DescendantNodesAndSelf().OfType<IdentifierNameSyntax>().Any(n => ((IdentifierNameSyntax)target).Identifier.ValueText == n.Identifier.ValueText);
            if (target.IsKind(SyntaxKind.SimpleMemberAccessExpression))
            {
                var descendantTargetNodes = target.DescendantNodesAndSelf();
                return !expr.DescendantNodesAndSelf().Any(
                        n =>
                        {
                            // If n is a simple idenifier, try to find it in target expression as well
                            if (n.IsKind(SyntaxKind.IdentifierName))
                                return descendantTargetNodes.Any(tn =>
                                    tn.IsKind(SyntaxKind.IdentifierName) && (((IdentifierNameSyntax)tn).Identifier.ValueText == ((IdentifierNameSyntax)n).Identifier.ValueText));
                            // StartsWith() is a very simple solution, but should be enough in usual cases
                            if (n.IsKind(SyntaxKind.SimpleMemberAccessExpression))
                                return ((MemberAccessExpressionSyntax)target).Expression.ToString().StartsWith(((MemberAccessExpressionSyntax)n).Expression.ToString());
                            return false;
                        }
                    );
            }
            return false;
        }

        internal static ExpressionSyntax AddParensToComplexExpression(ExpressionSyntax condition)
        {
            var binaryExpression = condition as BinaryExpressionSyntax;
            if (binaryExpression == null)
                return condition;

            if (binaryExpression.IsKind(SyntaxKind.LogicalOrExpression)
                || binaryExpression.IsKind(SyntaxKind.LogicalAndExpression))
                return SyntaxFactory.ParenthesizedExpression(binaryExpression.SkipParens());

            return condition;
        }
    }
}