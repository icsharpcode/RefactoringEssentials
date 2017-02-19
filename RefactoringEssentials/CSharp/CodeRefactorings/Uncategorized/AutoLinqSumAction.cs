using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;

namespace RefactoringEssentials.CSharp.CodeRefactorings
{
    [ExportCodeRefactoringProvider(LanguageNames.CSharp, Name = "Convert loop to Linq expression")]
    public class AutoLinqSumAction : CodeRefactoringProvider
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
            var symbolInfo = model.GetSpeculativeSymbolInfo(span.Start, SyntaxFactory.ParseTypeName("Enumerable"), SpeculativeBindingOption.BindAsTypeOrNamespace);
            if (symbolInfo.Symbol == null || symbolInfo.Symbol.ContainingNamespace.ToDisplayString() != "System.Linq")
                return;
            var root = await model.SyntaxTree.GetRootAsync(cancellationToken).ConfigureAwait(false);
            var token = root.FindToken(span.Start);
            var foreachStmt = token.Parent as ForEachStatementSyntax;
            if (foreachStmt == null)
                return;
            var outputStatement = GetTransformedAssignmentExpression(model, foreachStmt);
            if (outputStatement == null)
                return;
            context.RegisterRefactoring(
                CodeActionFactory.Create(
                    token.Span,
                    DiagnosticSeverity.Info,
                    GettextCatalog.GetString("Convert foreach loop to LINQ expression"),
                    t2 => {
                        int indexOfSelf = foreachStmt.Parent.ChildNodes().IndexOf(n => foreachStmt == n);
                        var prevSibling = foreachStmt.Parent.ChildNodes().ElementAtOrDefault(indexOfSelf - 1) as StatementSyntax;

                        ExpressionSyntax leftSide = outputStatement.Left;
                        ExpressionSyntax rightSide = outputStatement.Right;
                        ExpressionSyntax expressionToReplace = GetExpressionToReplace(prevSibling, leftSide);

                        SyntaxNode newRoot;
                        if (expressionToReplace == null) {
                            newRoot = root.ReplaceNode(foreachStmt, SyntaxFactory.ExpressionStatement(outputStatement).WithAdditionalAnnotations(Formatter.Annotation));
                        } else {
                            ExpressionSyntax replacementExpression = rightSide;
                            if (!IsZeroPrimitive(expressionToReplace)) {
                                replacementExpression = SyntaxFactory.BinaryExpression(
                                    SyntaxKind.AddExpression,
                                    CSharpUtil.AddParensIfRequired(expressionToReplace),
                                    replacementExpression
                                ).WithAdditionalAnnotations(Formatter.Annotation);
                            }
                            newRoot = root.TrackNodes(foreachStmt, expressionToReplace);
                            newRoot = newRoot.ReplaceNode(newRoot.GetCurrentNode(expressionToReplace), replacementExpression);
                            newRoot = newRoot.RemoveNode(newRoot.GetCurrentNode(foreachStmt), SyntaxRemoveOptions.KeepNoTrivia);
                        }
                        return Task.FromResult(document.WithSyntaxRoot(newRoot));
                    }
                ));
        }

        AssignmentExpressionSyntax GetTransformedAssignmentExpression(SemanticModel context, ForEachStatementSyntax foreachStatement)
        {
            ExpressionSyntax leftExpression, rightExpression;
            if (!ExtractExpression(foreachStatement.Statement, out leftExpression, out rightExpression))
                return null;
            if (leftExpression == null || rightExpression == null)
                return null;

            if (!IsLinqSummableType(context.GetTypeInfo(leftExpression).Type))
                return null;

            if (rightExpression.DescendantNodesAndSelf().OfType<ExpressionSyntax>().Any(n => ExpressionNotAllowed(n))) {
                // Reject loops such as
                // int k = 0;
                // foreach (var x in y) { k += (z = 2); }
                // or
                // int k = 0;
                // foreach (var x in y) { k += (z++); }
                return null;
            }

            ExpressionSyntax baseExpression = foreachStatement.Expression;
            for (;;) {
                ConditionalExpressionSyntax condition = rightExpression as ConditionalExpressionSyntax;
                if (condition == null) break;

                if (EqualsLiteral(condition.WhenTrue, 0)) {
                    baseExpression = SyntaxFactory.InvocationExpression(
                        SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, baseExpression, (SimpleNameSyntax)SyntaxFactory.ParseName("Where")),
                        SyntaxFactory.ArgumentList(SyntaxFactory.SingletonSeparatedList(SyntaxFactory.Argument(BuildLambda(foreachStatement.Identifier, CSharpUtil.InvertCondition(condition.Condition)))))
                    );
                    rightExpression = condition.WhenFalse;
                    continue;
                }

                if (EqualsLiteral(condition.WhenFalse, 0)) {
                    baseExpression = SyntaxFactory.InvocationExpression(
                        SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, baseExpression, (SimpleNameSyntax)SyntaxFactory.ParseName("Where")),
                        SyntaxFactory.ArgumentList(SyntaxFactory.SingletonSeparatedList(SyntaxFactory.Argument(BuildLambda(foreachStatement.Identifier, condition.Condition))))
                    );
                    rightExpression = condition.WhenTrue;
                    continue;
                }
                break;
            }

            SimpleNameSyntax method;
            var arguments = new List<ArgumentSyntax>();
            if (EqualsLiteral(rightExpression, 1)) {
                method = (SimpleNameSyntax)SyntaxFactory.ParseName("Count");
            } else {
                method = (SimpleNameSyntax)SyntaxFactory.ParseName("Sum");
                if (rightExpression.SkipParens().ToString() != foreachStatement.Identifier.ToString()) {
                    var lambda = BuildLambda(foreachStatement.Identifier, rightExpression);
                    arguments.Add(SyntaxFactory.Argument(lambda));
                }
            }

            var rightSide = SyntaxFactory.InvocationExpression(
                SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, baseExpression, method),
                SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList(arguments))
            );

            return SyntaxFactory.AssignmentExpression(SyntaxKind.AddAssignmentExpression, leftExpression, rightSide);
        }

        bool EqualsLiteral<T>(ExpressionSyntax expr, T literal)
        {
            return object.Equals(literal, (expr as LiteralExpressionSyntax)?.Token.Value);
        }

        bool ExpressionNotAllowed(ExpressionSyntax expr)
        {
            switch (expr.Kind()) {
                case SyntaxKind.AddAssignmentExpression:
                case SyntaxKind.AndAssignmentExpression:
                case SyntaxKind.DivideAssignmentExpression:
                case SyntaxKind.ExclusiveOrAssignmentExpression:
                case SyntaxKind.LeftShiftAssignmentExpression:
                case SyntaxKind.ModuloAssignmentExpression:
                case SyntaxKind.MultiplyAssignmentExpression:
                case SyntaxKind.OrAssignmentExpression:
                case SyntaxKind.RightShiftAssignmentExpression:
                case SyntaxKind.SimpleAssignmentExpression:
                case SyntaxKind.SubtractAssignmentExpression:
                case SyntaxKind.PostIncrementExpression:
                case SyntaxKind.PreIncrementExpression:
                case SyntaxKind.PostDecrementExpression:
                case SyntaxKind.PreDecrementExpression:
                    return true;
            }
            return false;
        }

        bool IsZeroPrimitive(ExpressionSyntax expr)
        {
            //We want a very simple check -- no looking at constants, no constant folding, etc.
            //So 1+1 should return false, but (0) should return true
            expr = expr.SkipParens();
            return EqualsLiteral(expr, 0)
                || EqualsLiteral(expr, 0f)
                || EqualsLiteral(expr, 0d)
                || EqualsLiteral(expr, 0m);
        }

        ExpressionSyntax GetExpressionToReplace(SyntaxNode prevSibling, ExpressionSyntax requiredLeftSide)
        {
            if (prevSibling == null)
                return null;

            var declarationStatement = prevSibling as LocalDeclarationStatementSyntax;
            if (declarationStatement != null) {
                if (declarationStatement.Declaration?.Variables.Count != 1)
                    return null;

                var identifierExpr = requiredLeftSide as IdentifierNameSyntax;
                if (identifierExpr == null)
                    return null;

                var variableDecl = declarationStatement.Declaration.Variables.First();
                if (identifierExpr.Identifier.Text != variableDecl.Identifier.Text)
                    return null;

                return variableDecl.Initializer.Value;
            }

            var exprStatement = prevSibling as ExpressionStatementSyntax;
            if (exprStatement != null) {
                if (!exprStatement.Expression.IsKind(SyntaxKind.SimpleAssignmentExpression, SyntaxKind.AddAssignmentExpression))
                    return null;
                if (((AssignmentExpressionSyntax)exprStatement.Expression).Left.ToString() != requiredLeftSide.ToString())
                    return null;
                return ((AssignmentExpressionSyntax)exprStatement.Expression).Right;
            }

            return null;
        }

        static LambdaExpressionSyntax BuildLambda(SyntaxToken variableName, ExpressionSyntax expression)
        {
            return SyntaxFactory.SimpleLambdaExpression(
                SyntaxFactory.Parameter(variableName),
                expression
            );
        }

        bool IsLinqSummableType(ITypeSymbol type)
        {
            // Disabled for nullables, since int? x = 3; x += null; has result x = null,
            // but LINQ Sum behaves differently : nulls are treated as zero
            switch (type.SpecialType) {
                case SpecialType.System_UInt16:
                case SpecialType.System_Int16:
                case SpecialType.System_UInt32:
                case SpecialType.System_Int32:
                case SpecialType.System_UInt64:
                case SpecialType.System_Int64:
                case SpecialType.System_Single:
                case SpecialType.System_Double:
                case SpecialType.System_Decimal:
                    return true;
            }
            return false;
        }

        bool ExtractExpression(StatementSyntax statement, out ExpressionSyntax leftSide, out ExpressionSyntax rightSide)
        {
            leftSide = null;
            rightSide = null;

            if (statement == null || statement is EmptyStatementSyntax)
                return true;

            switch (statement.Kind()) {
                case SyntaxKind.ExpressionStatement:
                    var expression = ((ExpressionStatementSyntax)statement).Expression;
                    switch (expression?.Kind()) {
                        case SyntaxKind.AddAssignmentExpression:
                            leftSide = ((AssignmentExpressionSyntax)expression).Left;
                            rightSide = ((AssignmentExpressionSyntax)expression).Right;
                            return true;
                        case SyntaxKind.SubtractAssignmentExpression:
                            leftSide = ((AssignmentExpressionSyntax)expression).Left;
                            rightSide = SyntaxFactory.PrefixUnaryExpression(SyntaxKind.UnaryMinusExpression, ((AssignmentExpressionSyntax)expression).Right);
                            return true;
                        case SyntaxKind.PostDecrementExpression:
                        case SyntaxKind.PreDecrementExpression:
                            leftSide = expression.ExtractUnaryOperand();
                            rightSide = ComputeConstantValueCodeRefactoringProvider.GetLiteralExpression(-1);
                            return true;
                        case SyntaxKind.PostIncrementExpression:
                        case SyntaxKind.PreIncrementExpression:
                            leftSide = expression.ExtractUnaryOperand();
                            rightSide = ComputeConstantValueCodeRefactoringProvider.GetLiteralExpression(1);
                            return true;
                    }
                    return false;
                case SyntaxKind.Block:
                    var block = (BlockSyntax)statement;
                    foreach (StatementSyntax child in block.Statements) {
                        ExpressionSyntax newLeft, newRight;
                        if (!ExtractExpression(child, out newLeft, out newRight))
                            return false;

                        if (newLeft == null)
                            continue;

                        if (leftSide == null) {
                            leftSide = newLeft;
                            rightSide = newRight;
                        } else if (leftSide.IsEquivalentTo(newLeft)) {
                            rightSide = SyntaxFactory.BinaryExpression(
                                SyntaxKind.AddExpression,
                                CSharpUtil.AddParensIfRequired(rightSide),
                                CSharpUtil.AddParensIfRequired(newRight)
                            );
                        } else {
                            return false;
                        }
                    }
                    return true;
                case SyntaxKind.IfStatement:
                    var condition = (IfStatementSyntax)statement;
                    ExpressionSyntax ifLeft, ifRight;
                    if (!ExtractExpression(condition.Statement, out ifLeft, out ifRight))
                        return false;

                    ExpressionSyntax elseLeft, elseRight;
                    if (!ExtractExpression(condition.Else?.Statement, out elseLeft, out elseRight))
                        return false;

                    if (ifLeft == null && elseLeft == null)
                        return true;
                    if (ifLeft != null && elseLeft != null && !ifLeft.IsEquivalentTo(elseLeft))
                        return false;

                    ifRight = ifRight ?? ComputeConstantValueCodeRefactoringProvider.GetLiteralExpression(0);
                    elseRight = elseRight ?? ComputeConstantValueCodeRefactoringProvider.GetLiteralExpression(0);

                    leftSide = ifLeft ?? elseLeft;
                    rightSide = SyntaxFactory.ConditionalExpression(condition.Condition, ifRight, elseRight);
                    return true;
            }
            return false;
        }
    }
}

