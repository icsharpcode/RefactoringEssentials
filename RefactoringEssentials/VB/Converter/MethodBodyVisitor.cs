﻿using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.VisualBasic;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using CS = Microsoft.CodeAnalysis.CSharp;
using CSS = Microsoft.CodeAnalysis.CSharp.Syntax;

namespace RefactoringEssentials.VB.Converter
{
	public partial class CSharpConverter
    {
        class MethodBodyVisitor : CS.CSharpSyntaxVisitor<SyntaxList<StatementSyntax>>
        {
            SemanticModel semanticModel;
            NodesVisitor nodesVisitor;
            Stack<BlockInfo> blockInfo = new Stack<BlockInfo>(); // currently only works with switch blocks
            int switchCount = 0;
            public bool IsInterator { get; private set; }
            
            class BlockInfo
            {
                public readonly List<VisualBasicSyntaxNode> GotoCaseExpressions = new List<VisualBasicSyntaxNode>();
            }

            public MethodBodyVisitor(SemanticModel semanticModel, NodesVisitor nodesVisitor)
            {
                this.semanticModel = semanticModel;
                this.nodesVisitor = nodesVisitor;
            }

            public override SyntaxList<StatementSyntax> DefaultVisit(SyntaxNode node)
            {
                throw new NotImplementedException(node.GetType() + " not implemented!");
            }

            public override SyntaxList<StatementSyntax> VisitLocalDeclarationStatement(CSS.LocalDeclarationStatementSyntax node)
            {
                var modifiers = ConvertModifiers(node.Modifiers, TokenContext.Local);
                if (modifiers.Count == 0)
                    modifiers = modifiers.Add(SyntaxFactory.Token(SyntaxKind.DimKeyword));
                return SyntaxFactory.SingletonList<StatementSyntax>(
                    SyntaxFactory.LocalDeclarationStatement(
                        modifiers,
                        RemodelVariableDeclaration(node.Declaration, nodesVisitor)
                    )
                );
            }

            StatementSyntax ConvertSingleExpression(CSS.ExpressionSyntax node)
            {
                var exprNode = node.Accept(nodesVisitor);
                if (!(exprNode is StatementSyntax))
                    exprNode = SyntaxFactory.ExpressionStatement((ExpressionSyntax)exprNode);

                return (StatementSyntax)exprNode;
            }

            public override SyntaxList<StatementSyntax> VisitExpressionStatement(CSS.ExpressionStatementSyntax node)
            {
                return SyntaxFactory.SingletonList(ConvertSingleExpression(node.Expression));
            }

            public override SyntaxList<StatementSyntax> VisitIfStatement(CSS.IfStatementSyntax node)
            {
                IdentifierNameSyntax name;
                List<ArgumentSyntax> arguments = new List<ArgumentSyntax>();
                StatementSyntax stmt;
                if (node.Else == null && TryConvertRaiseEvent(node, out name, arguments))
                {
                    stmt = SyntaxFactory.RaiseEventStatement(name, SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList(arguments)));
                    return SyntaxFactory.SingletonList(stmt);
                }
                var elseIfBlocks = new List<ElseIfBlockSyntax>();
                ElseBlockSyntax elseBlock = null;
                CollectElseBlocks(node, elseIfBlocks, ref elseBlock);
                if (node.Statement is CSS.BlockSyntax)
                {
                    stmt = SyntaxFactory.MultiLineIfBlock(
                        SyntaxFactory.IfStatement((ExpressionSyntax)node.Condition.Accept(nodesVisitor)).WithThenKeyword(SyntaxFactory.Token(SyntaxKind.ThenKeyword)),
                        ConvertBlock(node.Statement),
                        SyntaxFactory.List(elseIfBlocks),
                        elseBlock
                    );
                }
                else
                {
                    if (elseIfBlocks.Any() || !IsSimpleStatement(node.Statement))
                    {
                        stmt = SyntaxFactory.MultiLineIfBlock(
                             SyntaxFactory.IfStatement((ExpressionSyntax)node.Condition.Accept(nodesVisitor)).WithThenKeyword(SyntaxFactory.Token(SyntaxKind.ThenKeyword)),
                             ConvertBlock(node.Statement),
                             SyntaxFactory.List(elseIfBlocks),
                             elseBlock
                         );
                    }
                    else
                    {
                        stmt = SyntaxFactory.SingleLineIfStatement(
                            (ExpressionSyntax)node.Condition.Accept(nodesVisitor),
                            ConvertBlock(node.Statement),
                            elseBlock == null ? null : SyntaxFactory.SingleLineElseClause(elseBlock.Statements)
                        ).WithThenKeyword(SyntaxFactory.Token(SyntaxKind.ThenKeyword));
                    }
                }
                return SyntaxFactory.SingletonList(stmt);
            }

            bool IsSimpleStatement(CSS.StatementSyntax statement)
            {
                return statement is CSS.ExpressionStatementSyntax
                    || statement is CSS.BreakStatementSyntax
                    || statement is CSS.ContinueStatementSyntax
                    || statement is CSS.ReturnStatementSyntax
                    || statement is CSS.YieldStatementSyntax
                    || statement is CSS.ThrowStatementSyntax;
            }

            bool TryConvertRaiseEvent(CSS.IfStatementSyntax node, out IdentifierNameSyntax name, List<ArgumentSyntax> arguments)
            {
                name = null;
                var condition = node.Condition;
                while (condition is CSS.ParenthesizedExpressionSyntax)
                    condition = ((CSS.ParenthesizedExpressionSyntax)condition).Expression;
                if (!(condition is CSS.BinaryExpressionSyntax))
                    return false;
                var be = (CSS.BinaryExpressionSyntax)condition;
                if (!be.IsKind(CS.SyntaxKind.NotEqualsExpression) || (!be.Left.IsKind(CS.SyntaxKind.NullLiteralExpression) && !be.Right.IsKind(CS.SyntaxKind.NullLiteralExpression)))
                    return false;
                CSS.ExpressionStatementSyntax singleStatement;
                if (node.Statement is CSS.BlockSyntax)
                {
                    var block = ((CSS.BlockSyntax)node.Statement);
                    if (block.Statements.Count != 1)
                        return false;
                    singleStatement = block.Statements[0] as CSS.ExpressionStatementSyntax;
                }
                else
                {
                    singleStatement = node.Statement as CSS.ExpressionStatementSyntax;
                }
                if (singleStatement == null || !(singleStatement.Expression is CSS.InvocationExpressionSyntax))
                    return false;
                var possibleEventName = GetPossibleEventName(be.Left) ?? GetPossibleEventName(be.Right);
                if (possibleEventName == null)
                    return false;
                var invocation = (CSS.InvocationExpressionSyntax)singleStatement.Expression;
                var invocationName = GetPossibleEventName(invocation.Expression);
                if (possibleEventName != invocationName)
                    return false;
                name = SyntaxFactory.IdentifierName(possibleEventName);
                arguments.AddRange(invocation.ArgumentList.Arguments.Select(a => (ArgumentSyntax)a.Accept(nodesVisitor)));
                return true;
            }

            string GetPossibleEventName(CSS.ExpressionSyntax expression)
            {
                var ident = expression as CSS.IdentifierNameSyntax;
                if (ident != null)
                    return ident.Identifier.Text;
                var fre = expression as CSS.MemberAccessExpressionSyntax;
                if (fre != null && fre.Expression.IsKind(CS.SyntaxKind.ThisExpression))
                    return fre.Name.Identifier.Text;
                return null;
            }

            void CollectElseBlocks(CSS.IfStatementSyntax node, List<ElseIfBlockSyntax> elseIfBlocks, ref ElseBlockSyntax elseBlock)
            {
                if (node.Else == null) return;
                if (node.Else.Statement is CSS.IfStatementSyntax)
                {
                    var elseIf = (CSS.IfStatementSyntax)node.Else.Statement;
                    elseIfBlocks.Add(
                        SyntaxFactory.ElseIfBlock(
                            SyntaxFactory.ElseIfStatement((ExpressionSyntax)elseIf.Condition.Accept(nodesVisitor)).WithThenKeyword(SyntaxFactory.Token(SyntaxKind.ThenKeyword)),
                            ConvertBlock(elseIf.Statement)
                        )
                    );
                    CollectElseBlocks(elseIf, elseIfBlocks, ref elseBlock);
                }
                else
                    elseBlock = SyntaxFactory.ElseBlock(ConvertBlock(node.Else.Statement));
            }

            public override SyntaxList<StatementSyntax> VisitSwitchStatement(CSS.SwitchStatementSyntax node)
            {
                StatementSyntax stmt;
                blockInfo.Push(new BlockInfo());
                try
                {
                    var blocks = node.Sections.Select(ConvertSwitchSection).ToArray();
                    stmt = SyntaxFactory.SelectBlock(
                        SyntaxFactory.SelectStatement((ExpressionSyntax)node.Expression.Accept(nodesVisitor)).WithCaseKeyword(SyntaxFactory.Token(SyntaxKind.CaseKeyword)),
                        SyntaxFactory.List(AddLabels(blocks, blockInfo.Peek().GotoCaseExpressions))
                    );
                    switchCount++;
                }
                finally
                {
                    blockInfo.Pop();
                }
                return SyntaxFactory.SingletonList(stmt);
            }

            IEnumerable<CaseBlockSyntax> AddLabels(CaseBlockSyntax[] blocks, List<VisualBasicSyntaxNode> gotoLabels)
            {
                foreach (var _block in blocks)
                {
                    var block = _block;
                    foreach (var caseClause in block.CaseStatement.Cases)
                    {
                        var expression = caseClause is ElseCaseClauseSyntax ? (VisualBasicSyntaxNode)caseClause : ((SimpleCaseClauseSyntax)caseClause).Value;
                        if (gotoLabels.Any(label => label.IsEquivalentTo(expression)))
                            block = block.WithStatements(block.Statements.Insert(0, SyntaxFactory.LabelStatement(MakeGotoSwitchLabel(expression))));
                    }
                    yield return block;
                }
            }

            CaseBlockSyntax ConvertSwitchSection(CSS.SwitchSectionSyntax section)
            {
                if (section.Labels.OfType<CSS.DefaultSwitchLabelSyntax>().Any())
                    return SyntaxFactory.CaseElseBlock(SyntaxFactory.CaseElseStatement(SyntaxFactory.ElseCaseClause()), ConvertSwitchSectionBlock(section));
                return SyntaxFactory.CaseBlock(SyntaxFactory.CaseStatement(SyntaxFactory.SeparatedList(section.Labels.OfType<CSS.CaseSwitchLabelSyntax>().Select(ConvertSwitchLabel))), ConvertSwitchSectionBlock(section));
            }

            SyntaxList<StatementSyntax> ConvertSwitchSectionBlock(CSS.SwitchSectionSyntax section)
            {
                List<StatementSyntax> statements = new List<StatementSyntax>();
                var lastStatement = section.Statements.LastOrDefault();
                foreach (var s in section.Statements)
                {
                    if (s == lastStatement && s is CSS.BreakStatementSyntax)
                        continue;
                    statements.AddRange(ConvertBlock(s));
                }
                return SyntaxFactory.List(statements);
            }

            CaseClauseSyntax ConvertSwitchLabel(CSS.CaseSwitchLabelSyntax label)
            {
                return SyntaxFactory.SimpleCaseClause((ExpressionSyntax)label.Value.Accept(nodesVisitor));
            }

            public override SyntaxList<StatementSyntax> VisitDoStatement(CSS.DoStatementSyntax node)
            {
                var condition = (ExpressionSyntax)node.Condition.Accept(nodesVisitor);
                var stmt = ConvertBlock(node.Statement);
                var block = SyntaxFactory.DoLoopWhileBlock(
                    SyntaxFactory.DoStatement(SyntaxKind.SimpleDoStatement),
                    stmt,
                    SyntaxFactory.LoopStatement(SyntaxKind.LoopWhileStatement, SyntaxFactory.WhileClause(condition))
                );

                return SyntaxFactory.SingletonList<StatementSyntax>(block);
            }

            public override SyntaxList<StatementSyntax> VisitWhileStatement(CSS.WhileStatementSyntax node)
            {
                var condition = (ExpressionSyntax)node.Condition.Accept(nodesVisitor);
                var stmt = ConvertBlock(node.Statement);
                var block = SyntaxFactory.WhileBlock(
                    SyntaxFactory.WhileStatement(condition),
                    stmt
                );

                return SyntaxFactory.SingletonList<StatementSyntax>(block);
            }

            public override SyntaxList<StatementSyntax> VisitForStatement(CSS.ForStatementSyntax node)
            {
                StatementSyntax block;
                if (!ConvertForToSimpleForNext(node, out block))
                {
                    var stmts = ConvertBlock(node.Statement)
                        .AddRange(node.Incrementors.Select(ConvertSingleExpression));
                    var condition = node.Condition == null ? Literal(true) : (ExpressionSyntax)node.Condition.Accept(nodesVisitor);
                    block = SyntaxFactory.WhileBlock(
                        SyntaxFactory.WhileStatement(condition),
                        stmts
                    );
                    return SyntaxFactory.List(node.Initializers.Select(ConvertSingleExpression)).Add(block);
                }
                return SyntaxFactory.SingletonList(block);
            }

            bool ConvertForToSimpleForNext(CSS.ForStatementSyntax node, out StatementSyntax block)
            {
                //   ForStatement -> ForNextStatement when for-loop is simple

                // only the following forms of the for-statement are allowed:
                // for (TypeReference name = start; name < oneAfterEnd; name += step)
                // for (name = start; name < oneAfterEnd; name += step)
                // for (TypeReference name = start; name <= end; name += step)
                // for (name = start; name <= end; name += step)
                // for (TypeReference name = start; name > oneAfterEnd; name -= step)
                // for (name = start; name > oneAfterEnd; name -= step)
                // for (TypeReference name = start; name >= end; name -= step)
                // for (name = start; name >= end; name -= step)

                block = null;

                // check if the form is valid and collect TypeReference, name, start, end and step
                bool hasVariable = node.Declaration != null && node.Declaration.Variables.Count == 1;
                if (!hasVariable && node.Initializers.Count != 1)
                    return false;
                if (node.Incrementors.Count != 1)
                    return false;
                var iterator = node.Incrementors.FirstOrDefault()?.Accept(nodesVisitor) as AssignmentStatementSyntax;
                if (iterator == null || !iterator.IsKind(SyntaxKind.AddAssignmentStatement, SyntaxKind.SubtractAssignmentStatement))
                    return false;
                var iteratorIdentifier = iterator.Left as IdentifierNameSyntax;
                if (iteratorIdentifier == null)
                    return false;
                var stepExpression = iterator.Right as LiteralExpressionSyntax;
                if (stepExpression == null || !(stepExpression.Token.Value is int))
                    return false;
                int step = (int)stepExpression.Token.Value;
                if (iterator.OperatorToken.IsKind(SyntaxKind.MinusToken))
                    step = -step;

                var condition = node.Condition as CSS.BinaryExpressionSyntax;
                if (condition == null || !(condition.Left is CSS.IdentifierNameSyntax))
                    return false;
                if (((CSS.IdentifierNameSyntax)condition.Left).Identifier.IsEquivalentTo(iteratorIdentifier.Identifier))
                    return false;

                ExpressionSyntax end;
                if (iterator.IsKind(SyntaxKind.SubtractAssignmentStatement))
                {
                    if (condition.IsKind(CS.SyntaxKind.GreaterThanOrEqualExpression))
                        end = (ExpressionSyntax)condition.Right.Accept(nodesVisitor);
                    else if (condition.IsKind(CS.SyntaxKind.GreaterThanExpression))
                        end = SyntaxFactory.BinaryExpression(SyntaxKind.AddExpression, (ExpressionSyntax)condition.Right.Accept(nodesVisitor), SyntaxFactory.Token(SyntaxKind.PlusToken), Literal(1));
                    else return false;
                }
                else
                {
                    if (condition.IsKind(CS.SyntaxKind.LessThanOrEqualExpression))
                        end = (ExpressionSyntax)condition.Right.Accept(nodesVisitor);
                    else if (condition.IsKind(CS.SyntaxKind.LessThanExpression))
                        end = SyntaxFactory.BinaryExpression(SyntaxKind.SubtractExpression, (ExpressionSyntax)condition.Right.Accept(nodesVisitor), SyntaxFactory.Token(SyntaxKind.MinusToken), Literal(1));
                    else return false;
                }

                VisualBasicSyntaxNode variable;
                ExpressionSyntax start;
                if (hasVariable)
                {
                    var v = node.Declaration.Variables[0];
                    start = (ExpressionSyntax)v.Initializer?.Value.Accept(nodesVisitor);
                    if (start == null)
                        return false;
                    variable = SyntaxFactory.VariableDeclarator(
                        SyntaxFactory.SingletonSeparatedList(SyntaxFactory.ModifiedIdentifier(ConvertIdentifier(v.Identifier))),
                        node.Declaration.Type.IsVar ? null : SyntaxFactory.SimpleAsClause((TypeSyntax)node.Declaration.Type.Accept(nodesVisitor)),
                        null
                    );
                }
                else
                {
                    var initializer = node.Initializers.FirstOrDefault() as CSS.AssignmentExpressionSyntax;
                    if (initializer == null || !initializer.IsKind(CS.SyntaxKind.SimpleAssignmentExpression))
                        return false;
                    if (!(initializer.Left is CSS.IdentifierNameSyntax))
                        return false;
                    if (((CSS.IdentifierNameSyntax)initializer.Left).Identifier.IsEquivalentTo(iteratorIdentifier.Identifier))
                        return false;
                    variable = initializer.Left.Accept(nodesVisitor);
                    start = (ExpressionSyntax)initializer.Right.Accept(nodesVisitor);
                }

                block = SyntaxFactory.ForBlock(
                    SyntaxFactory.ForStatement(variable, start, end, step == 1 ? null : SyntaxFactory.ForStepClause(Literal(step))),
                    ConvertBlock(node.Statement),
                    SyntaxFactory.NextStatement()
                );
                return true;
            }

            public override SyntaxList<StatementSyntax> VisitForEachStatement(CSS.ForEachStatementSyntax node)
            {
                VisualBasicSyntaxNode variable;
                if (node.Type.IsVar)
                {
                    variable = SyntaxFactory.IdentifierName(ConvertIdentifier(node.Identifier));
                }
                else
                {
                    variable = SyntaxFactory.VariableDeclarator(
                        SyntaxFactory.SingletonSeparatedList(SyntaxFactory.ModifiedIdentifier(ConvertIdentifier(node.Identifier))),
                        SyntaxFactory.SimpleAsClause((TypeSyntax)node.Type.Accept(nodesVisitor)),
                        null
                    );
                }
                var expression = (ExpressionSyntax)node.Expression.Accept(nodesVisitor);
                var stmt = ConvertBlock(node.Statement);
                var block = SyntaxFactory.ForEachBlock(
                    SyntaxFactory.ForEachStatement(variable, expression),
                    stmt,
                    SyntaxFactory.NextStatement()
                );
                return SyntaxFactory.SingletonList<StatementSyntax>(block);
            }

            public override SyntaxList<StatementSyntax> VisitTryStatement(CSS.TryStatementSyntax node)
            {
                var block = SyntaxFactory.TryBlock(
                    ConvertBlock(node.Block),
                    SyntaxFactory.List(node.Catches.IndexedSelect(ConvertCatchClause)),
                    node.Finally == null ? null : SyntaxFactory.FinallyBlock(ConvertBlock(node.Finally.Block))
                );

                return SyntaxFactory.SingletonList<StatementSyntax>(block);
            }

            CatchBlockSyntax ConvertCatchClause(int index, CSS.CatchClauseSyntax catchClause)
            {
                var statements = ConvertBlock(catchClause.Block);
                if (catchClause.Declaration == null)
                    return SyntaxFactory.CatchBlock(SyntaxFactory.CatchStatement(), statements);
                var type = (TypeSyntax)catchClause.Declaration.Type.Accept(nodesVisitor);
                string simpleTypeName;
                if (type is QualifiedNameSyntax)
                    simpleTypeName = ((QualifiedNameSyntax)type).Right.ToString();
                else
                    simpleTypeName = type.ToString();
                return SyntaxFactory.CatchBlock(
                    SyntaxFactory.CatchStatement(
                        SyntaxFactory.IdentifierName(catchClause.Declaration.Identifier.IsKind(CS.SyntaxKind.None) ? SyntaxFactory.Identifier($"__unused{simpleTypeName}{index + 1}__") : ConvertIdentifier(catchClause.Declaration.Identifier)),
                        SyntaxFactory.SimpleAsClause(type),
                        catchClause.Filter == null ? null : SyntaxFactory.CatchFilterClause((ExpressionSyntax)catchClause.Filter.FilterExpression.Accept(nodesVisitor))
                    ), statements
                );
            }

            public override SyntaxList<StatementSyntax> VisitUsingStatement(CSS.UsingStatementSyntax node)
            {
                UsingStatementSyntax stmt;
                if (node.Declaration == null)
                {
                    stmt = SyntaxFactory.UsingStatement(
                        (ExpressionSyntax)node.Expression?.Accept(nodesVisitor),
                        SyntaxFactory.SeparatedList<VariableDeclaratorSyntax>()
                    );
                }
                else
                {
                    stmt = SyntaxFactory.UsingStatement(null, RemodelVariableDeclaration(node.Declaration, nodesVisitor));
                }
                return SyntaxFactory.SingletonList<StatementSyntax>(SyntaxFactory.UsingBlock(stmt, ConvertBlock(node.Statement)));
            }

            public override SyntaxList<StatementSyntax> VisitLockStatement(CSS.LockStatementSyntax node)
            {
                var stmt = SyntaxFactory.SyncLockStatement(
                    (ExpressionSyntax)node.Expression?.Accept(nodesVisitor)
                );
                return SyntaxFactory.SingletonList<StatementSyntax>(SyntaxFactory.SyncLockBlock(stmt, ConvertBlock(node.Statement)));
            }

            public override SyntaxList<StatementSyntax> VisitLabeledStatement(CSS.LabeledStatementSyntax node)
            {
                return SyntaxFactory.SingletonList<StatementSyntax>(SyntaxFactory.LabelStatement(ConvertIdentifier(node.Identifier)))
                    .AddRange(ConvertBlock(node.Statement));
            }

            string MakeGotoSwitchLabel(VisualBasicSyntaxNode expression)
            {
                string expressionText;
                if (expression is ElseCaseClauseSyntax)
                    expressionText = "Default";
                else
                    expressionText = expression.ToString();
                return $"_Select{switchCount}_Case{expressionText}";
            }

            public override SyntaxList<StatementSyntax> VisitGotoStatement(CSS.GotoStatementSyntax node)
            {
                LabelSyntax label;
                if (node.IsKind(CS.SyntaxKind.GotoCaseStatement, CS.SyntaxKind.GotoDefaultStatement))
                {
                    if (blockInfo.Count == 0)
                        throw new InvalidOperationException("goto case/goto default outside switch is illegal!");
                    var labelExpression = node.Expression?.Accept(nodesVisitor) ?? SyntaxFactory.ElseCaseClause();
                    blockInfo.Peek().GotoCaseExpressions.Add(labelExpression);
                    label = SyntaxFactory.Label(SyntaxKind.IdentifierLabel, MakeGotoSwitchLabel(labelExpression));
                }
                else
                {
                    label = SyntaxFactory.Label(SyntaxKind.IdentifierLabel, ConvertIdentifier(((CSS.IdentifierNameSyntax)node.Expression).Identifier));
                }
                return SyntaxFactory.SingletonList<StatementSyntax>(SyntaxFactory.GoToStatement(label));
            }

            SyntaxList<StatementSyntax> ConvertBlock(CSS.StatementSyntax node)
            {
                if (node is CSS.BlockSyntax)
                {
                    var b = (CSS.BlockSyntax)node;
                    return SyntaxFactory.List(b.Statements.Where(s => !(s is CSS.EmptyStatementSyntax)).SelectMany(s => s.Accept(this)));
                }
                if (node is CSS.EmptyStatementSyntax)
                {
                    return SyntaxFactory.List<StatementSyntax>();
                }
                return node.Accept(this);
            }

            public override SyntaxList<StatementSyntax> VisitReturnStatement(CSS.ReturnStatementSyntax node)
            {
                StatementSyntax stmt;
                if (node.Expression == null)
                    stmt = SyntaxFactory.ReturnStatement();
                else
                    stmt = SyntaxFactory.ReturnStatement((ExpressionSyntax)node.Expression.Accept(nodesVisitor));
                return SyntaxFactory.SingletonList(stmt);
            }

            public override SyntaxList<StatementSyntax> VisitYieldStatement(CSS.YieldStatementSyntax node)
            {
                IsInterator = true;
                StatementSyntax stmt;
                if (node.Expression == null)
                    stmt = SyntaxFactory.ReturnStatement();
                else
                    stmt = SyntaxFactory.YieldStatement((ExpressionSyntax)node.Expression.Accept(nodesVisitor));
                return SyntaxFactory.SingletonList(stmt);
            }

            public override SyntaxList<StatementSyntax> VisitThrowStatement(CSS.ThrowStatementSyntax node)
            {
                StatementSyntax stmt;
                if (node.Expression == null)
                    stmt = SyntaxFactory.ThrowStatement();
                else
                    stmt = SyntaxFactory.ThrowStatement((ExpressionSyntax)node.Expression.Accept(nodesVisitor));
                return SyntaxFactory.SingletonList(stmt);
            }

            public override SyntaxList<StatementSyntax> VisitContinueStatement(CSS.ContinueStatementSyntax node)
            {
                var statementKind = SyntaxKind.None;
                var keywordKind = SyntaxKind.None;
                foreach (var stmt in node.GetAncestors<CSS.StatementSyntax>())
                {
                    if (stmt is CSS.DoStatementSyntax)
                    {
                        statementKind = SyntaxKind.ContinueDoStatement;
                        keywordKind = SyntaxKind.DoKeyword;
                        break;
                    }
                    if (stmt is CSS.WhileStatementSyntax)
                    {
                        statementKind = SyntaxKind.ContinueWhileStatement;
                        keywordKind = SyntaxKind.WhileKeyword;
                        break;
                    }
                    if (stmt is CSS.ForStatementSyntax || stmt is CSS.ForEachStatementSyntax)
                    {
                        statementKind = SyntaxKind.ContinueForStatement;
                        keywordKind = SyntaxKind.ForKeyword;
                        break;
                    }
                }
                return SyntaxFactory.SingletonList<StatementSyntax>(SyntaxFactory.ContinueStatement(statementKind, SyntaxFactory.Token(keywordKind)));
            }

            public override SyntaxList<StatementSyntax> VisitBreakStatement(CSS.BreakStatementSyntax node)
            {
                var statementKind = SyntaxKind.None;
                var keywordKind = SyntaxKind.None;
                foreach (var stmt in node.GetAncestors<CSS.StatementSyntax>())
                {
                    if (stmt is CSS.DoStatementSyntax)
                    {
                        statementKind = SyntaxKind.ExitDoStatement;
                        keywordKind = SyntaxKind.DoKeyword;
                        break;
                    }
                    if (stmt is CSS.WhileStatementSyntax)
                    {
                        statementKind = SyntaxKind.ExitWhileStatement;
                        keywordKind = SyntaxKind.WhileKeyword;
                        break;
                    }
                    if (stmt is CSS.ForStatementSyntax || stmt is CSS.ForEachStatementSyntax)
                    {
                        statementKind = SyntaxKind.ExitForStatement;
                        keywordKind = SyntaxKind.ForKeyword;
                        break;
                    }
                    if (stmt is CSS.SwitchStatementSyntax)
                    {
                        statementKind = SyntaxKind.ExitSelectStatement;
                        keywordKind = SyntaxKind.SelectKeyword;
                        break;
                    }
                }
                return SyntaxFactory.SingletonList<StatementSyntax>(SyntaxFactory.ExitStatement(statementKind, SyntaxFactory.Token(keywordKind)));
            }

            public override SyntaxList<StatementSyntax> VisitCheckedStatement(CSS.CheckedStatementSyntax node)
            {
                return WrapInComment(ConvertBlock(node.Block), "Visual Basic does not support checked statements!");
            }

            SyntaxList<StatementSyntax> WrapInComment(SyntaxList<StatementSyntax> nodes, string comment)
            {
                if (nodes.Count > 0)
                {
                    nodes = nodes.Replace(nodes[0], nodes[0].WithPrependedLeadingTrivia(SyntaxFactory.CommentTrivia("BEGIN TODO : " + comment)));
                    nodes = nodes.Replace(nodes.Last(), nodes.Last().WithAppendedTrailingTrivia(SyntaxFactory.CommentTrivia("END TODO : " + comment)));
                }

                return nodes;
            }
        }
    }
}
