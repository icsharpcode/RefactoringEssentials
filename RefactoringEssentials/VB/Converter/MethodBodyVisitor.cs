using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.VisualBasic;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
                StatementSyntax stmt;
                var elseIfBlocks = new List<ElseIfBlockSyntax>();
                ElseBlockSyntax elseBlock = null;
                CollectElseBlocks(node, elseIfBlocks, ref elseBlock);
                if (node.Statement is CSS.BlockSyntax)
                {
                    var b = (CSS.BlockSyntax)node.Statement;
                    stmt = SyntaxFactory.MultiLineIfBlock(
                        SyntaxFactory.IfStatement((ExpressionSyntax)node.Condition.Accept(nodesVisitor)).WithThenKeyword(SyntaxFactory.Token(SyntaxKind.ThenKeyword)),
                        SyntaxFactory.List(b.Statements.SelectMany(s => s.Accept(this))),
                        SyntaxFactory.List(elseIfBlocks),
                        elseBlock
                    );
                }
                else
                {
                    if (elseIfBlocks.Any())
                    {
                        stmt = SyntaxFactory.MultiLineIfBlock(
                             SyntaxFactory.IfStatement((ExpressionSyntax)node.Condition.Accept(nodesVisitor)).WithThenKeyword(SyntaxFactory.Token(SyntaxKind.ThenKeyword)),
                             node.Statement.Accept(this),
                             SyntaxFactory.List(elseIfBlocks),
                             elseBlock
                         );
                    }
                    else
                    {
                        stmt = SyntaxFactory.SingleLineIfStatement(
                            (ExpressionSyntax)node.Condition.Accept(nodesVisitor),
                            node.Statement.Accept(this),
                            elseBlock == null ? null : SyntaxFactory.SingleLineElseClause(elseBlock.Statements)
                        ).WithThenKeyword(SyntaxFactory.Token(SyntaxKind.ThenKeyword));
                    }
                }
                return SyntaxFactory.SingletonList<StatementSyntax>(stmt);
            }

            void CollectElseBlocks(CSS.IfStatementSyntax node, List<ElseIfBlockSyntax> elseIfBlocks, ref ElseBlockSyntax elseBlock)
            {
                if (node.Else == null) return;
                if (node.Else.Statement is CSS.IfStatementSyntax)
                {
                    var elseIf = (CSS.IfStatementSyntax)node.Else.Statement;
                    if (elseIf.Statement is CSS.BlockSyntax)
                    {
                        var block = (CSS.BlockSyntax)elseIf.Statement;
                        elseIfBlocks.Add(
                            SyntaxFactory.ElseIfBlock(
                                SyntaxFactory.ElseIfStatement((ExpressionSyntax)elseIf.Condition.Accept(nodesVisitor)).WithThenKeyword(SyntaxFactory.Token(SyntaxKind.ThenKeyword)),
                                SyntaxFactory.List(block.Statements.SelectMany(s => s.Accept(this)))
                            )
                        );
                    }
                    else
                    {
                        elseIfBlocks.Add(
                            SyntaxFactory.ElseIfBlock(
                                SyntaxFactory.ElseIfStatement((ExpressionSyntax)elseIf.Condition.Accept(nodesVisitor)).WithThenKeyword(SyntaxFactory.Token(SyntaxKind.ThenKeyword)),
                                elseIf.Statement.Accept(this)
                            )
                        );
                    }
                    CollectElseBlocks(elseIf, elseIfBlocks, ref elseBlock);
                }
                else if (node.Else.Statement is CSS.BlockSyntax)
                {
                    var block = (CSS.BlockSyntax)node.Else.Statement;
                    elseBlock = SyntaxFactory.ElseBlock(SyntaxFactory.List(block.Statements.SelectMany(s => s.Accept(this))));
                }
                else
                {
                    elseBlock = SyntaxFactory.ElseBlock(SyntaxFactory.List(node.Else.Statement.Accept(this)));
                }
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

            public override SyntaxList<StatementSyntax> VisitUsingStatement(CSS.UsingStatementSyntax node)
            {
                var stmt = SyntaxFactory.UsingStatement(
                    (ExpressionSyntax)node.Expression?.Accept(nodesVisitor),
                    RemodelVariableDeclaration(node.Declaration, nodesVisitor)
                );
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
                    .AddRange(node.Statement.Accept(this));
            }

            public override SyntaxList<StatementSyntax> VisitGotoStatement(CSS.GotoStatementSyntax node)
            {
                if (!(node.Expression is CSS.IdentifierNameSyntax))
                    throw new NotImplementedException();
                var stmt = SyntaxFactory.GoToStatement(SyntaxFactory.Label(SyntaxKind.IdentifierLabel, ConvertIdentifier(((CSS.IdentifierNameSyntax)node.Expression).Identifier)));
                return SyntaxFactory.SingletonList<StatementSyntax>(stmt);
            }

            SyntaxList<StatementSyntax> ConvertBlock(CSS.StatementSyntax node)
            {
                if (node is CSS.BlockSyntax)
                {
                    var b = (CSS.BlockSyntax)node;
                    return SyntaxFactory.List(b.Statements.SelectMany(s => s.Accept(this)));
                }
                else
                {
                    return node.Accept(this);
                }
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
                }
                return SyntaxFactory.SingletonList<StatementSyntax>(SyntaxFactory.ExitStatement(statementKind, SyntaxFactory.Token(keywordKind)));
            }

            public override SyntaxList<StatementSyntax> VisitCheckedStatement(CSS.CheckedStatementSyntax node)
            {
                return WrapInComment(Visit(node.Block), "Visual Basic does not support checked statements!");
            }

            private SyntaxList<StatementSyntax> WrapInComment(SyntaxList<StatementSyntax> nodes, string comment)
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
