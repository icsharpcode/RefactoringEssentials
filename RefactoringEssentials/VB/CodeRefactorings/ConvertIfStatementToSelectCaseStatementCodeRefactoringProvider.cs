using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;
using Microsoft.CodeAnalysis.VisualBasic;
using Microsoft.CodeAnalysis.Formatting;
using RefactoringEssentials.Util;

namespace RefactoringEssentials.VB.CodeRefactorings
{
	[ExportCodeRefactoringProvider(LanguageNames.VisualBasic, Name = "Convert 'If' to 'Select Case'")]
    public class ConvertIfStatementToSelectCaseStatementCodeRefactoringProvider : CodeRefactoringProvider
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
            var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
            var model = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);
            if (model.IsFromGeneratedCode(cancellationToken))
                return;
            var node = root.FindNode(span) as IfStatementSyntax;

            if (node == null || !(node.Parent is MultiLineIfBlockSyntax))
                return;

			var ifBlock = node.Parent as MultiLineIfBlockSyntax;

            var selectCaseExpression = GetSelectCaseExpression(model, node.Condition);
            if (selectCaseExpression == null)
                return;

            var caseBlocks = new List<CaseBlockSyntax>();
            if (!CollectCaseBlocks(caseBlocks, model, ifBlock, selectCaseExpression))
                return;

            context.RegisterRefactoring(
                CodeActionFactory.Create(
                    span,
                    DiagnosticSeverity.Info,
                    GettextCatalog.GetString("To 'Select Case'"),
                    ct =>
                    {
                        var selectCaseStatement = SyntaxFactory.SelectBlock(SyntaxFactory.SelectStatement(selectCaseExpression).WithCaseKeyword(SyntaxFactory.Token(SyntaxKind.CaseKeyword)),
                            new SyntaxList<CaseBlockSyntax>().AddRange(caseBlocks))
							.NormalizeWhitespace();
                        return Task.FromResult(document.WithSyntaxRoot(root.ReplaceNode(
                            ifBlock, selectCaseStatement
                            .WithLeadingTrivia(ifBlock.GetLeadingTrivia())
							.WithTrailingTrivia(ifBlock.GetTrailingTrivia())
                            .WithAdditionalAnnotations(Formatter.Annotation))));
                    })
            );
        }

        internal static ExpressionSyntax GetSelectCaseExpression(SemanticModel context, ExpressionSyntax expr)
        {
            var binaryOp = expr as BinaryExpressionSyntax;
            if (binaryOp == null)
                return null;

            if (binaryOp.OperatorToken.IsKind(SyntaxKind.OrElseKeyword, SyntaxKind.OrKeyword))
                return GetSelectCaseExpression(context, binaryOp.Left);

            if (binaryOp.OperatorToken.IsKind(SyntaxKind.EqualsToken))
            {
                ExpressionSyntax switchExpr = null;
                if (IsConstantExpression(context, binaryOp.Right))
                    switchExpr = binaryOp.Left;
                if (IsConstantExpression(context, binaryOp.Left))
                    switchExpr = binaryOp.Right;
                if (switchExpr != null && IsValidSwitchType(context.GetTypeInfo(switchExpr).Type))
                    return switchExpr;
            }

            return null;
        }

        static bool IsConstantExpression(SemanticModel context, ExpressionSyntax expr)
        {
            if (expr is LiteralExpressionSyntax)
                return true;
            return context.GetConstantValue(expr).HasValue;
        }

        static readonly SpecialType[] validTypes = {
            SpecialType.System_String, SpecialType.System_Boolean, SpecialType.System_Char,
            SpecialType.System_Byte, SpecialType.System_SByte,
            SpecialType.System_Int16, SpecialType.System_Int32, SpecialType.System_Int64,
            SpecialType.System_UInt16, SpecialType.System_UInt32, SpecialType.System_UInt64
        };

        static bool IsValidSwitchType(ITypeSymbol type)
        {
            if (type == null || type is IErrorTypeSymbol)
                return false;
            if (type.TypeKind == TypeKind.Enum)
                return true;

            if (type.IsNullableType())
            {
                type = type.GetNullableUnderlyingType();
                if (type == null || type is IErrorTypeSymbol)
                    return false;
            }
            return Array.IndexOf(validTypes, type.SpecialType) != -1;
        }

        internal static bool CollectCaseBlocks(List<CaseBlockSyntax> result, SemanticModel context, MultiLineIfBlockSyntax ifBlock, ExpressionSyntax switchExpr)
        {
            // if
            var labels = new List<CaseClauseSyntax>();
            if (!CollectCaseLabels(labels, context, ifBlock.IfStatement.Condition, switchExpr))
                return false;

            result.Add(SyntaxFactory.CaseBlock(SyntaxFactory.CaseStatement(labels.ToArray()), ifBlock.Statements));

            foreach (var block in ifBlock.ElseIfBlocks)
            {
				labels = new List<CaseClauseSyntax>();
				if (!CollectCaseLabels(labels, context, block.ElseIfStatement.Condition, switchExpr))
					return false;

				result.Add(SyntaxFactory.CaseBlock(SyntaxFactory.CaseStatement(labels.ToArray()), block.Statements));
			}

			// else
			if (ifBlock.ElseBlock != null) {
				result.Add(SyntaxFactory.CaseElseBlock(SyntaxFactory.CaseElseStatement(SyntaxFactory.ElseCaseClause()), ifBlock.ElseBlock.Statements));
			}
			return true;
        }

        static bool CollectCaseLabels(List<CaseClauseSyntax> result, SemanticModel context,
                                       ExpressionSyntax condition, ExpressionSyntax switchExpr)
        {
            if (condition is ParenthesizedExpressionSyntax)
                return CollectCaseLabels(result, context, ((ParenthesizedExpressionSyntax)condition).Expression, switchExpr);

            var binaryOp = condition as BinaryExpressionSyntax;
            if (binaryOp == null)
                return false;

            if (binaryOp.IsKind(SyntaxKind.OrExpression) || binaryOp.IsKind(SyntaxKind.OrElseExpression))
                return CollectCaseLabels(result, context, binaryOp.Left, switchExpr) &&
                       CollectCaseLabels(result, context, binaryOp.Right, switchExpr);

            if (binaryOp.IsKind(SyntaxKind.EqualsExpression, SyntaxKind.IsExpression))
            {
                if (switchExpr.IsEquivalentTo(binaryOp.Left, true))
                {
                    if (IsConstantExpression(context, binaryOp.Right))
                    {
                        result.Add(SyntaxFactory.SimpleCaseClause(binaryOp.Right));
                        return true;
                    }
                }
                else if (switchExpr.IsEquivalentTo(binaryOp.Right, true))
                {
                    if (IsConstantExpression(context, binaryOp.Left))
                    {
                        result.Add(SyntaxFactory.SimpleCaseClause(binaryOp.Left));
                        return true;
                    }
                }
            }

            return false;
        }
    }
}
