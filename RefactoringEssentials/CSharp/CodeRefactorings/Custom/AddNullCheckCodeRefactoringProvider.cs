using System.Linq;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Formatting;

namespace RefactoringEssentials.CSharp
{
	/// <summary>
	/// Surround usage of a variable with a null check or add a null check to surrounding if block.
	/// </summary>
	[ExportCodeRefactoringProvider(LanguageNames.CSharp, Name = "Add null check")]
    public class AddNullCheckCodeRefactoringProvider : CodeRefactoringProvider
    {
        public override async Task ComputeRefactoringsAsync(CodeRefactoringContext context)
        {
            var document = context.Document;
            if (document.Project.Solution.Workspace.Kind == WorkspaceKind.MiscellaneousFiles)
                return;
            var span = context.Span;
            if (!span.IsEmpty)
                return;
            var cancellationToken = context.CancellationToken;
            if (cancellationToken.IsCancellationRequested)
                return;
            var model = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);
            if (model.IsFromGeneratedCode(cancellationToken))
                return;
            var root = await model.SyntaxTree.GetRootAsync(cancellationToken).ConfigureAwait(false);
            var token = root.FindToken(span.Start);

            ExpressionSyntax identifier = token.Parent as IdentifierNameSyntax;
            if (identifier == null)
                return;

            // If identifier is a type name, this might be a static member access or similar, don't suggest null checks on it
            var identifierSymbol = model.GetSymbolInfo(identifier).Symbol;
            if ((identifierSymbol == null) || (identifierSymbol.IsType()))
                return;

            // Identifier might be part of a MemberAccessExpression and we need to check it for null as a whole
            identifier = GetOuterMemberAccessExpression(identifier) ?? identifier;

            if ((identifier.Parent is ExpressionSyntax) && ConditionContainsNullCheck((ExpressionSyntax)identifier.Parent, identifier))
                return;

            var identifierAncestors = identifier.Ancestors();

            // Don't surround return statements with checks
            if (identifierAncestors.OfType<ReturnStatementSyntax>().Any())
                return;

            // If identifier is in a conditional ternary expression, skip refactoring is case of present null check in its condition
            var conditionalExprParent = identifierAncestors.OfType<ConditionalExpressionSyntax>().FirstOrDefault();
            if ((conditionalExprParent != null) && ConditionContainsNullCheck(conditionalExprParent.Condition, identifier))
                return;

            // Check identifier type, don't suggest null checks for value types!
            var identifierType = model.GetTypeInfo(identifier).Type;
            if ((identifierType == null) || (identifierType.IsValueType && !identifierType.IsNullableType()))
                return;

            var statementToWrap = identifierAncestors.OfType<StatementSyntax>().FirstOrDefault();
            if (statementToWrap == null)
                return;

            // No refactoring if statement is inside of a local variable declaration
            if ((statementToWrap is BlockSyntax) || (statementToWrap is LocalDeclarationStatementSyntax))
                return;

            SyntaxNode newWrappedStatement = null;

            // Check surrounding block
            var surroundingStatement = statementToWrap.Ancestors().OfType<StatementSyntax>().FirstOrDefault();
            if (surroundingStatement is BlockSyntax)
                surroundingStatement = surroundingStatement.Parent as StatementSyntax;
            if (surroundingStatement != null)
            {
                if (StatementWithConditionContainsNullCheck(surroundingStatement, identifier))
                    return;
                var surroundingIfStatement = surroundingStatement as IfStatementSyntax;
                if (surroundingIfStatement != null)
                {
                    statementToWrap = surroundingIfStatement;
                    newWrappedStatement = ExtendIfConditionWithNullCheck(surroundingIfStatement, identifier);
                }
            }
            else
            {
                if (statementToWrap is IfStatementSyntax)
                {
                    newWrappedStatement = ExtendIfConditionWithNullCheck((IfStatementSyntax)statementToWrap, identifier);
                }
                else
                {
                    newWrappedStatement = SyntaxFactory.IfStatement(
                            SyntaxFactory.BinaryExpression(SyntaxKind.NotEqualsExpression, identifier, SyntaxFactory.LiteralExpression(SyntaxKind.NullLiteralExpression)),
                            SyntaxFactory.Block(statementToWrap).WithLeadingTrivia(statementToWrap.GetLeadingTrivia()).WithTrailingTrivia(statementToWrap.GetTrailingTrivia())
                        ).WithAdditionalAnnotations(Formatter.Annotation);
                }
            }

            context.RegisterRefactoring(CodeActionFactory.Create(token.Span, DiagnosticSeverity.Info, GettextCatalog.GetString("Add null check"), t2 =>
            {
                var newRoot = root.ReplaceNode(statementToWrap, newWrappedStatement);
                return Task.FromResult(document.WithSyntaxRoot(newRoot));
            }));
        }

        IfStatementSyntax ExtendIfConditionWithNullCheck(IfStatementSyntax ifStatement, ExpressionSyntax identifier)
        {
            return ifStatement.WithCondition(
                    SyntaxFactory.BinaryExpression(SyntaxKind.LogicalAndExpression,
                        ParenthizeIfNeeded(SyntaxFactory.BinaryExpression(SyntaxKind.NotEqualsExpression, identifier, SyntaxFactory.LiteralExpression(SyntaxKind.NullLiteralExpression))),
                        ParenthizeIfNeeded(ifStatement.Condition)
                    ).WithAdditionalAnnotations(Formatter.Annotation)
                );
        }

        bool StatementWithConditionContainsNullCheck(SyntaxNode statement, ExpressionSyntax identifierToCheck)
        {
            if (statement is IfStatementSyntax)
                return ConditionContainsNullCheck(((IfStatementSyntax)statement).Condition, identifierToCheck);
            if (statement is WhileStatementSyntax)
                return ConditionContainsNullCheck(((WhileStatementSyntax)statement).Condition, identifierToCheck);
            if (statement is ForStatementSyntax)
                return ConditionContainsNullCheck(((ForStatementSyntax)statement).Condition, identifierToCheck);
            if (statement is ConditionalExpressionSyntax)
                return ConditionContainsNullCheck(((ConditionalExpressionSyntax)statement).Condition, identifierToCheck);

            return false;
        }

        bool ConditionContainsNullCheck(ExpressionSyntax condition, ExpressionSyntax identifierToCheck)
        {
            var identifierNameToCheck = identifierToCheck as IdentifierNameSyntax;
            var memberAccessExpressionToCheck = identifierToCheck as MemberAccessExpressionSyntax;

            return condition.DescendantNodesAndSelf().Any(n =>
            {
                var binaryExpr = n as BinaryExpressionSyntax;
                if (binaryExpr != null)
                {

                    IdentifierNameSyntax identifierName = binaryExpr.Left as IdentifierNameSyntax;
                    if ((identifierName != null) && (identifierNameToCheck != null) && (identifierName.Identifier.ValueText == identifierNameToCheck.Identifier.ValueText))
                        return binaryExpr.IsKind(SyntaxKind.NotEqualsExpression) && binaryExpr.Right.IsKind(SyntaxKind.NullLiteralExpression);
                    MemberAccessExpressionSyntax memberAccessExpressionSyntax = binaryExpr.Left as MemberAccessExpressionSyntax;
                    if ((memberAccessExpressionSyntax != null) && (memberAccessExpressionToCheck != null) && (memberAccessExpressionSyntax.ToString() == identifierToCheck.ToString()))
                        return binaryExpr.IsKind(SyntaxKind.NotEqualsExpression) && binaryExpr.Right.IsKind(SyntaxKind.NullLiteralExpression);

                    identifierName = binaryExpr.Right as IdentifierNameSyntax;
                    if ((identifierName != null) && (identifierNameToCheck != null) && (identifierName.Identifier.ValueText == identifierNameToCheck.Identifier.ValueText))
                        return binaryExpr.IsKind(SyntaxKind.NotEqualsExpression) && binaryExpr.Left.IsKind(SyntaxKind.NullLiteralExpression);
                    memberAccessExpressionSyntax = binaryExpr.Right as MemberAccessExpressionSyntax;
                    if ((memberAccessExpressionSyntax != null) && (memberAccessExpressionToCheck != null) && (memberAccessExpressionSyntax.ToString() == identifierToCheck.ToString()))
                        return binaryExpr.IsKind(SyntaxKind.NotEqualsExpression) && binaryExpr.Left.IsKind(SyntaxKind.NullLiteralExpression);
                }

                return false;
            });
        }

        ExpressionSyntax ParenthizeIfNeeded(ExpressionSyntax expression)
        {
            if (expression is BinaryExpressionSyntax)
                return SyntaxFactory.ParenthesizedExpression(expression);
            return expression;
        }

        MemberAccessExpressionSyntax GetOuterMemberAccessExpression(ExpressionSyntax identifier)
        {
            var parent = identifier.Parent as MemberAccessExpressionSyntax;
            if ((parent == null) || (parent.Name != identifier))
                return null;
            return parent;
        }
    }
}

