using System.Linq;
using System.Collections.Generic;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;
using Microsoft.CodeAnalysis.VisualBasic;
using Microsoft.CodeAnalysis.Formatting;
using System;

namespace RefactoringEssentials.VB.CodeRefactorings
{
    [ExportCodeRefactoringProvider(LanguageNames.VisualBasic, Name = "Convert 'Select Case' to 'If'")]
    public class ConvertSelectCaseToIfCodeRefactoringProvider : CodeRefactoringProvider
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
            var selectStatement = root.FindNode(span) as SelectStatementSyntax;
            if (selectStatement == null)
                return;
            var node = selectStatement.Parent as SelectBlockSyntax;

            if (node == null || node.CaseBlocks.Count == 0)
                return;

            if (node.CaseBlocks.Count == 1 && node.CaseBlocks[0].CaseStatement.Cases.OfType<ElseCaseClauseSyntax>().Any())
                return;

            foreach (var block in node.CaseBlocks)
            {
                var lastStatement = block.Statements.LastOrDefault() as ExitStatementSyntax;
                if (HasNonTrailingExitSelects(block, lastStatement != null && lastStatement.BlockKeyword.IsKind(SyntaxKind.SelectKeyword) ? lastStatement : null))
                    return;
            }

            context.RegisterRefactoring(
                CodeActionFactory.Create(
                    span,
                    DiagnosticSeverity.Info,
                    GettextCatalog.GetString("To 'if'"),
                    t2 =>
                    {
                        MultiLineIfBlockSyntax ifBlock = null;
                        List<ElseIfBlockSyntax> elseIfs = new List<ElseIfBlockSyntax>();
                        ElseBlockSyntax defaultElse = null;

                        foreach (var block in node.CaseBlocks)
                        {
                            var condition = CollectCondition(node.SelectStatement.Expression, block.CaseStatement.Cases.ToArray());
                            var statements = block.Statements;
                            var lastStatement = block.Statements.LastOrDefault() as ExitStatementSyntax;
                            if (lastStatement != null && lastStatement.BlockKeyword.IsKind(SyntaxKind.SelectKeyword))
                                statements = SyntaxFactory.List(statements.Take(statements.Count - 1));
                            if (condition == null)
                            {
                                defaultElse = SyntaxFactory.ElseBlock()
                                    .AddStatements(statements.ToArray())
                                    .WithLeadingTrivia(block.GetLeadingTrivia())
                                    .WithTrailingTrivia(block.GetTrailingTrivia());
                                break;
                            }
                            else if (ifBlock == null)
                            {
                                ifBlock = SyntaxFactory.MultiLineIfBlock(SyntaxFactory.IfStatement(condition).WithThenKeyword(SyntaxFactory.Token(SyntaxKind.ThenKeyword)))
                                    .WithStatements(statements);
                            }
                            else
                            {
                                elseIfs.Add(SyntaxFactory.ElseIfBlock(SyntaxFactory.ElseIfStatement(condition).WithThenKeyword(SyntaxFactory.Token(SyntaxKind.ThenKeyword)), statements));
                            }
                        }

                        ifBlock = ifBlock.WithElseIfBlocks(SyntaxFactory.List(elseIfs));
                        if (defaultElse != null)
                            ifBlock = ifBlock.WithElseBlock(defaultElse);
                        ifBlock = ifBlock.WithLeadingTrivia(node.GetLeadingTrivia().Concat(ifBlock.GetLeadingTrivia())).WithTrailingTrivia(node.GetTrailingTrivia())
                            .WithElseBlock(defaultElse).WithAdditionalAnnotations(Formatter.Annotation);

                        return Task.FromResult(document.WithSyntaxRoot(root.ReplaceNode(node, ifBlock)));
                    })
            );
        }

        ExpressionSyntax CollectCondition(ExpressionSyntax expressionSyntax, CaseClauseSyntax[] cases)
        {
            //default
            if (cases.Length == 0 || cases.OfType<ElseCaseClauseSyntax>().Any())
                return null;

            var conditionList = cases.Select(c => TranslateCaseToCondition(expressionSyntax, c).NormalizeWhitespace()).ToList();

            if (conditionList.Count == 1)
                return conditionList.First();

            ExpressionSyntax condition = conditionList[0];
            for (int i = 1; i < conditionList.Count; ++i)
            {
                condition = SyntaxFactory.BinaryExpression(SyntaxKind.OrElseExpression, condition, SyntaxFactory.Token(SyntaxKind.OrElseKeyword), conditionList[i]);
            }
            return condition.WithAdditionalAnnotations(Formatter.Annotation);
        }

        private BinaryExpressionSyntax TranslateCaseToCondition(ExpressionSyntax expressionSyntax, CaseClauseSyntax c)
        {
            var simple = c as SimpleCaseClauseSyntax;
            if (simple != null)
                return SyntaxFactory.BinaryExpression(SyntaxKind.EqualsExpression, expressionSyntax, SyntaxFactory.Token(SyntaxKind.EqualsToken), simple.Value.WithoutTrivia());
            var range = c as RangeCaseClauseSyntax;
            if (range != null)
                return SyntaxFactory.BinaryExpression(
                    SyntaxKind.AndAlsoExpression,
                    SyntaxFactory.BinaryExpression(SyntaxKind.GreaterThanOrEqualExpression, expressionSyntax, SyntaxFactory.Token(SyntaxKind.GreaterThanEqualsToken), range.LowerBound.WithoutTrivia()),
                    SyntaxFactory.Token(SyntaxKind.AndAlsoKeyword),
                    SyntaxFactory.BinaryExpression(SyntaxKind.LessThanOrEqualExpression, expressionSyntax, SyntaxFactory.Token(SyntaxKind.LessThanEqualsToken), range.UpperBound.WithoutTrivia())
                );
            var relational = c as RelationalCaseClauseSyntax;
            if (relational != null)
                return SyntaxFactory.BinaryExpression(SyntaxKind.EqualsExpression, expressionSyntax, relational.OperatorToken, relational.Value.WithoutTrivia());
            throw new NotSupportedException();
        }

        internal bool HasNonTrailingExitSelects(SyntaxNode node, ExitStatementSyntax trailing)
        {
            var exit = node as ExitStatementSyntax;
            if ((exit != null && exit.BlockKeyword.IsKind(SyntaxKind.SelectKeyword)) && (trailing == null || !node.GetLocation().Equals(trailing.GetLocation())))
                return true;
            return node.DescendantNodes().Any(n => HasNonTrailingExitSelects(n, trailing));
        }
    }
}
