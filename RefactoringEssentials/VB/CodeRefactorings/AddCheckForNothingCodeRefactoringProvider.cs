using System.Linq;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;
using Microsoft.CodeAnalysis.VisualBasic;
using Microsoft.CodeAnalysis.Formatting;
using RefactoringEssentials.Util;

namespace RefactoringEssentials.VB
{
	/// <summary>
	/// Surround usage of a variable with an "IsNot Nothing" check or adds it to surrounding If block.
	/// </summary>
	[ExportCodeRefactoringProvider(LanguageNames.VisualBasic, Name = "Add check for Nothing")]
    public class AddCheckForNothingCodeRefactoringProvider : CodeRefactoringProvider
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

            // Don't surround Return statements with checks
            if (identifierAncestors.OfType<ReturnStatementSyntax>().Any())
                return;

            // If identifier is in a conditional ternary expression, skip refactoring is case of present null check in its condition
            var conditionalExprParent = identifierAncestors.OfType<TernaryConditionalExpressionSyntax>().FirstOrDefault();
            if ((conditionalExprParent != null) && ConditionContainsNullCheck(conditionalExprParent.Condition, identifier))
                return;

            // Check identifier type, don't suggest null checks for value types!
            var identifierType = model.GetTypeInfo(identifier).Type;
            if ((identifierType == null) || (identifierType.IsValueType && !identifierType.IsNullableType()))
                return;

            SyntaxNode statementToWrap = identifierAncestors.OfType<ExecutableStatementSyntax>().FirstOrDefault();
            if (statementToWrap == null)
                return;

            // No refactoring if statement is inside of a local variable declaration
            if (statementToWrap is LocalDeclarationStatementSyntax)
                return;

            bool wrapWithSingleLineIfStatement = false;
            SyntaxNode newWrappedStatement = null;

            var wrappedStatementAncestors = statementToWrap.Ancestors();
            if (wrappedStatementAncestors.OfType<SingleLineLambdaExpressionSyntax>().Any())
            {
                // Inside of a single-line lambda => wrap with single line If statement
                wrapWithSingleLineIfStatement = true;
            }

            // Check surrounding block
            var surroundingElseIfBlock = wrappedStatementAncestors.FirstOrDefault() as ElseIfBlockSyntax;
            if (surroundingElseIfBlock != null)
            {
                // Special handling for extension of Else If blocks
                if (StatementWithConditionContainsNullCheck(surroundingElseIfBlock, identifier))
                    return;
                statementToWrap = surroundingElseIfBlock;
                newWrappedStatement = ExtendIfConditionWithNullCheck(surroundingElseIfBlock, identifier);
            }
            else
            {
                var surroundingStatement = wrappedStatementAncestors.OfType<ExecutableStatementSyntax>().FirstOrDefault();
                if (surroundingStatement != null)
                {
                    if (StatementWithConditionContainsNullCheck(surroundingStatement, identifier))
                        return;
                    if ((surroundingStatement is MultiLineIfBlockSyntax) || (surroundingStatement is SingleLineIfStatementSyntax))
                    {
                        statementToWrap = surroundingStatement;
                        newWrappedStatement = ExtendIfConditionWithNullCheck(surroundingStatement, identifier);
                    }
                }
                else
                {
                    if (StatementWithConditionContainsNullCheck(statementToWrap, identifier))
                        return;
                    if ((statementToWrap is MultiLineIfBlockSyntax) || (statementToWrap is SingleLineIfStatementSyntax))
                    {
                        newWrappedStatement = ExtendIfConditionWithNullCheck(statementToWrap, identifier);
                    }
                }
            }

            if (newWrappedStatement == null)
            {
                if (wrapWithSingleLineIfStatement)
                {
                    newWrappedStatement = SyntaxFactory.SingleLineIfStatement(
                            SyntaxFactory.Token(SyntaxKind.IfKeyword),
                            CreateIsNotNothingBinaryExpression(identifier),
                            SyntaxFactory.Token(SyntaxKind.ThenKeyword),
                            SyntaxFactory.List<StatementSyntax>(new[] { ((StatementSyntax)statementToWrap).WithoutLeadingTrivia().WithoutTrailingTrivia() }),
                            null
                        ).WithLeadingTrivia(statementToWrap.GetLeadingTrivia()).WithTrailingTrivia(statementToWrap.GetTrailingTrivia()).WithAdditionalAnnotations(Formatter.Annotation);
                }
                else
                {
                    newWrappedStatement = SyntaxFactory.MultiLineIfBlock(
                        SyntaxFactory.IfStatement(
                            SyntaxFactory.Token(SyntaxKind.IfKeyword),
                            CreateIsNotNothingBinaryExpression(identifier),
                            SyntaxFactory.Token(SyntaxKind.ThenKeyword)),
                            SyntaxFactory.List<StatementSyntax>(new[] { ((StatementSyntax) statementToWrap).WithoutLeadingTrivia().WithoutTrailingTrivia() }),
                            SyntaxFactory.List<ElseIfBlockSyntax>(),
                            null
                        ).WithLeadingTrivia(statementToWrap.GetLeadingTrivia()).WithTrailingTrivia(statementToWrap.GetTrailingTrivia()).WithAdditionalAnnotations(Formatter.Annotation);
                }
            }

            context.RegisterRefactoring(CodeActionFactory.Create(token.Span, DiagnosticSeverity.Info, GettextCatalog.GetString("Add check for Nothing"), t2 =>
            {
                var newRoot = root.ReplaceNode<SyntaxNode>(statementToWrap, newWrappedStatement);
                return Task.FromResult(document.WithSyntaxRoot(newRoot));
            }));
        }

        SyntaxNode ExtendIfConditionWithNullCheck(SyntaxNode statement, ExpressionSyntax identifier)
        {
            MultiLineIfBlockSyntax multiLineIfStatement = statement as MultiLineIfBlockSyntax;
            if (multiLineIfStatement != null)
            {
                return multiLineIfStatement.WithIfStatement(
                    multiLineIfStatement.IfStatement.WithCondition(
                        SyntaxFactory.BinaryExpression(SyntaxKind.AndAlsoExpression,
                            ParenthizeIfNeeded(CreateIsNotNothingBinaryExpression(identifier)),
                            SyntaxFactory.Token(SyntaxKind.AndAlsoKeyword),
                            ParenthizeIfNeeded(multiLineIfStatement.IfStatement.Condition)
                        ).WithAdditionalAnnotations(Formatter.Annotation)
                    ));
            }
            SingleLineIfStatementSyntax singleLineIfStatement = statement as SingleLineIfStatementSyntax;
            if (singleLineIfStatement != null)
            {
                return singleLineIfStatement.WithCondition(
                        SyntaxFactory.BinaryExpression(SyntaxKind.AndAlsoExpression,
                            ParenthizeIfNeeded(CreateIsNotNothingBinaryExpression(identifier)),
                            SyntaxFactory.Token(SyntaxKind.AndAlsoKeyword),
                            ParenthizeIfNeeded(singleLineIfStatement.Condition)
                        ).WithAdditionalAnnotations(Formatter.Annotation)
                    );
            }
            ElseIfBlockSyntax elseIfBlock = statement as ElseIfBlockSyntax;
            if (elseIfBlock != null)
            {
                return elseIfBlock.WithElseIfStatement(
                    elseIfBlock.ElseIfStatement.WithCondition(
                        SyntaxFactory.BinaryExpression(SyntaxKind.AndAlsoExpression,
                            ParenthizeIfNeeded(CreateIsNotNothingBinaryExpression(identifier)),
                            SyntaxFactory.Token(SyntaxKind.AndAlsoKeyword),
                            ParenthizeIfNeeded(elseIfBlock.ElseIfStatement.Condition)
                        ).WithAdditionalAnnotations(Formatter.Annotation)
                    ));
            }

            return null;
        }

        BinaryExpressionSyntax CreateIsNotNothingBinaryExpression(ExpressionSyntax identifier)
        {
            return SyntaxFactory.BinaryExpression(SyntaxKind.IsNotExpression, identifier.WithoutTrailingTrivia(), SyntaxFactory.Token(SyntaxKind.IsNotKeyword), SyntaxFactory.LiteralExpression(SyntaxKind.NothingLiteralExpression, SyntaxFactory.Token(SyntaxKind.NothingKeyword)));
        }

        bool StatementWithConditionContainsNullCheck(SyntaxNode statement, ExpressionSyntax identifierToCheck)
        {
            if (statement is SingleLineIfStatementSyntax)
                return ConditionContainsNullCheck(((SingleLineIfStatementSyntax)statement).Condition, identifierToCheck);
            if (statement is MultiLineIfBlockSyntax)
                return ConditionContainsNullCheck(((MultiLineIfBlockSyntax)statement).IfStatement.Condition, identifierToCheck);
            if (statement is ElseIfBlockSyntax)
                return ConditionContainsNullCheck(((ElseIfBlockSyntax)statement).ElseIfStatement.Condition, identifierToCheck);
            if (statement is WhileBlockSyntax)
                return ConditionContainsNullCheck(((WhileBlockSyntax)statement).WhileStatement.Condition, identifierToCheck);
            if (statement is DoLoopBlockSyntax)
                return ConditionContainsNullCheck(((DoLoopBlockSyntax)statement).DoStatement.WhileOrUntilClause.Condition, identifierToCheck);
            if (statement is TernaryConditionalExpressionSyntax)
                return ConditionContainsNullCheck(((TernaryConditionalExpressionSyntax)statement).Condition, identifierToCheck);

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
                        return binaryExpr.IsKind(SyntaxKind.IsNotExpression) && binaryExpr.Right.IsKind(SyntaxKind.NothingLiteralExpression);
                    MemberAccessExpressionSyntax memberAccessExpressionSyntax = binaryExpr.Left as MemberAccessExpressionSyntax;
                    if ((memberAccessExpressionSyntax != null) && (memberAccessExpressionToCheck != null) && (memberAccessExpressionSyntax.ToString() == identifierToCheck.ToString()))
                        return binaryExpr.IsKind(SyntaxKind.IsNotExpression) && binaryExpr.Right.IsKind(SyntaxKind.NothingLiteralExpression);

                    identifierName = binaryExpr.Right as IdentifierNameSyntax;
                    if ((identifierName != null) && (identifierNameToCheck != null) && (identifierName.Identifier.ValueText == identifierNameToCheck.Identifier.ValueText))
                        return binaryExpr.IsKind(SyntaxKind.IsNotExpression) && binaryExpr.Left.IsKind(SyntaxKind.NothingLiteralExpression);
                    memberAccessExpressionSyntax = binaryExpr.Right as MemberAccessExpressionSyntax;
                    if ((memberAccessExpressionSyntax != null) && (memberAccessExpressionToCheck != null) && (memberAccessExpressionSyntax.ToString() == identifierToCheck.ToString()))
                        return binaryExpr.IsKind(SyntaxKind.IsNotExpression) && binaryExpr.Left.IsKind(SyntaxKind.NothingLiteralExpression);
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

