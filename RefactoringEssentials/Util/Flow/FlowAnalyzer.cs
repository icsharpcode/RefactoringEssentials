// (c) by Matt Warren. Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.
// From https://github.com/mattwar/nullaby

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Nullaby
{
	/// <summary>
	/// Analyzises a block of code for state transitions across code flow
	/// </summary>
	public class FlowAnalyzer<TState> : CSharpSyntaxWalker
        where TState : FlowState
    {
        protected readonly SemanticModel Model;
        private readonly TState initialState;

        private TState lexicalState;

        private ImmutableDictionary<FlowLocation, TState> joinStates
            = ImmutableDictionary<FlowLocation, TState>.Empty;

        private FlowLocation breakLocation;
        private FlowLocation continueLocation;

        private Dictionary<string, FlowLocation> labelLocations
            = new Dictionary<string, FlowLocation>();

        public FlowAnalyzer(SemanticModel model, TState initialState)
        {
            this.Model = model;
            this.initialState = initialState;
        }

        public FlowAnalysis<TState> Analyze(SyntaxNode node)
        {
            if (node.Parent != null)
            {
                // for some nodes, start analysis at parent node instead
                switch (node.Parent.Kind())
                {
                    case SyntaxKind.EqualsValueClause:
                    case SyntaxKind.NameEquals:
                    case SyntaxKind.VariableDeclarator:
                    case SyntaxKind.Parameter:
                    case SyntaxKind.ArrowExpressionClause:
                        this.Analyze(node.Parent);
                        break;
                }
            }

            this.SetState(node.StartLocation(), this.initialState);

            // do initial visit to find join points
            this.DoAnalysis(node);

            // if we have joins, repeat visits until we have reached steady-state
            int revists = 0;
            while (this.joinStates.Count > 0)
            {
                var lastJoinStates = this.joinStates;
                this.DoAnalysis(node);
                if (lastJoinStates == this.joinStates || (revists++) < 10)
                {
                    break;
                }
            }

            return new FlowAnalysis<TState>(this.joinStates);
        }

        private void DoAnalysis(SyntaxNode node)
        {
            this.lexicalState = this.initialState;
            this.Visit(node);
        }

        public override void Visit(SyntaxNode node)
        {
            base.Visit(node);

            if (this.lexicalState.IsConditional(node))
            {
                FlowState trueState;
                FlowState falseState;
                this.lexicalState.AfterConditional(node, out trueState, out falseState);
                if (!trueState.Equals(this.lexicalState) || !falseState.Equals(this.lexicalState))
                {
                    var joinedState = (TState)trueState.Join(falseState);
                    this.SetState(node.EndLocation(), joinedState);
                }
            }
            else
            {
                var afterState = this.lexicalState.After(node);
                if (afterState != this.lexicalState)
                {
                    this.SetState(node.EndLocation(), (TState)afterState);
                }
            }
        }

        public override void VisitBinaryExpression(BinaryExpressionSyntax node)
        {
            switch (node.Kind())
            {
                case SyntaxKind.LogicalOrExpression:
                case SyntaxKind.LogicalAndExpression:
                    TState trueState;
                    TState falseState;
                    this.VisitCondition(node, out trueState, out falseState);
                    this.SetState(node.EndLocation(), (TState)trueState.Join(falseState));
                    break;

                default:
                    base.VisitBinaryExpression(node);
                    break;
            }
        }

        public override void VisitPrefixUnaryExpression(PrefixUnaryExpressionSyntax node)
        {
            switch (node.Kind())
            {
                case SyntaxKind.LogicalNotExpression:
                    TState trueState;
                    TState falseState;
                    this.VisitCondition(node, out trueState, out falseState);
                    this.SetState(node.EndLocation(), (TState)trueState.Join(falseState));
                    break;

                default:
                    base.VisitPrefixUnaryExpression(node);
                    break;
            }
        }

        private void VisitCondition(ExpressionSyntax expression, out TState trueState, out TState falseState)
        {
            switch (expression.Kind())
            {
                case SyntaxKind.LogicalAndExpression:
                    this.VisitLogicalAnd((BinaryExpressionSyntax)expression, out trueState, out falseState);
                    break;

                case SyntaxKind.LogicalOrExpression:
                    this.VisitLogicalOr((BinaryExpressionSyntax)expression, out trueState, out falseState);
                    break;

                case SyntaxKind.LogicalNotExpression:
                    this.VisitLogicalNot((PrefixUnaryExpressionSyntax)expression, out trueState, out falseState);
                    break;

                default:
                    this.Visit(expression);
                    if (this.lexicalState.IsConditional(expression))
                    {
                        FlowState trueX;
                        FlowState falseX;
                        this.lexicalState.AfterConditional(expression, out trueX, out falseX);
                        trueState = (TState)trueX;
                        falseState = (TState)falseX;
                    }
                    else
                    {
                        trueState = this.lexicalState;
                        falseState = this.lexicalState;
                    }
                    break;
            }
        }

        private void VisitLogicalNot(PrefixUnaryExpressionSyntax node, out TState trueState, out TState falseState)
        {
            this.VisitCondition(node.Operand, out falseState, out trueState);
        }

        private void VisitLogicalAnd(BinaryExpressionSyntax binop, out TState trueState, out TState falseState)
        {
            TState leftTrueState;
            TState leftFalseState;
            this.VisitCondition(binop.Left, out leftTrueState, out leftFalseState);

            // right only executes on true
            this.SetState(binop.Right.StartLocation(), leftTrueState);
            TState rightTrueState;
            TState rightFalseState;
            this.VisitCondition(binop.Right, out rightTrueState, out rightFalseState);

            trueState = rightTrueState; // both executed true path
            falseState = (TState)leftFalseState.Join(rightFalseState); // either executed

            this.SetState(binop.EndLocation(), (TState)trueState.Join(falseState));
        }

        private void VisitLogicalOr(BinaryExpressionSyntax binop, out TState trueState, out TState falseState)
        {
            TState leftTrueState;
            TState leftFalseState;
            this.VisitCondition(binop.Left, out leftTrueState, out leftFalseState);
            var afterLeft = this.lexicalState;

            // right only executes on false
            this.SetState(binop.Right.StartLocation(), leftFalseState);
            TState rightTrueState;
            TState rightFalseState;
            this.VisitCondition(binop.Right, out rightTrueState, out rightFalseState);
            var afterRight = this.lexicalState;

            trueState = (TState)leftTrueState.Join(rightTrueState); // either executed
            falseState = rightFalseState; // both executed false path

            this.SetState(binop.EndLocation(), (TState)trueState.Join(falseState));
        }

        public override void VisitIfStatement(IfStatementSyntax node)
        {
            TState trueState;
            TState falseState;
            this.VisitCondition(node.Condition, out trueState, out falseState);

            // statement only executes on true
            this.SetState(node.Statement.StartLocation(), trueState);
            this.Visit(node.Statement);
            var statementEndState = this.lexicalState;

            var elseEndState = falseState;

            if (node.Else != null)
            {
                // else only executes on false
                this.SetState(node.Else.StartLocation(), falseState);
                this.Visit(node.Else);
                elseEndState = this.lexicalState;
            }

            if (!Exits(node.Statement))
            {
                this.SetState(node.EndLocation(), elseEndState);
            }
            else if (node.Else != null && !Exits(node.Else.Statement))
            {
                this.SetState(node.EndLocation(), statementEndState);
            }
            else
            {
                this.SetState(node.EndLocation(), (TState)statementEndState.Join(elseEndState));
            }
        }

        public override void VisitWhileStatement(WhileStatementSyntax node)
        {
            var saveContinueLocation = this.continueLocation;
            var saveBreakLocation = this.breakLocation;
               
            // merge with any states that branch back to start (continue)
            this.JoinState(node.StartLocation(), this.lexicalState);

            TState trueState;
            TState falseState;
            this.VisitCondition(node.Condition, out trueState, out falseState);

            this.continueLocation = node.StartLocation();
            this.breakLocation = node.EndLocation();

            // statement only executes when condition is true
            this.SetState(node.Statement.StartLocation(), trueState);
            this.Visit(node.Statement);

            // at end of loop, branch back to top
            this.BranchToLocation(this.continueLocation, this.lexicalState);

            // merge with any states that branch to end (break & natural exit)
            this.JoinState(node.EndLocation(), falseState);

            this.continueLocation = saveContinueLocation;
            this.breakLocation = saveBreakLocation;
        }

        public override void VisitBreakStatement(BreakStatementSyntax node)
        {
            if (this.breakLocation != default(FlowLocation))
            {
                this.BranchToLocation(this.breakLocation, this.lexicalState);
            }
        }

        public override void VisitContinueStatement(ContinueStatementSyntax node)
        {
            if (this.continueLocation != default(FlowLocation))
            {
                this.BranchToLocation(this.continueLocation, this.lexicalState);
            }
        }

        public override void VisitLabeledStatement(LabeledStatementSyntax node)
        {
            var location = node.StartLocation();
            this.labelLocations[node.Identifier.ValueText] = location;
            this.JoinState(location, this.lexicalState);
            base.VisitLabeledStatement(node);
        }

        public override void VisitGotoStatement(GotoStatementSyntax node)
        {
            if (node.CaseOrDefaultKeyword == default(SyntaxToken) &&
                node.Expression.IsKind(SyntaxKind.IdentifierName))
            {
                var name = ((IdentifierNameSyntax)node.Expression);

                FlowLocation location;
                if (this.labelLocations.TryGetValue(name.Identifier.ValueText, out location))
                {
                    this.BranchToLocation(location, this.lexicalState);
                }
            }

            // TODO: handle goto case and goto default
        }

        private bool Exits(StatementSyntax statement)
        {
            var flow = this.Model.AnalyzeControlFlow(statement);
            return flow.EndPointIsReachable; // || flow.ExitPoints.Any();
        }

        private bool BranchesOut(StatementSyntax statement)
        {
            var flow = this.Model.AnalyzeControlFlow(statement);
            return flow.ExitPoints.Any();
        }

        private void BranchToLocation(FlowLocation location, FlowState state)
        {
            TState existingState;
            if (this.joinStates.TryGetValue(location, out existingState))
            {
                this.joinStates = this.joinStates.SetItem(location, (TState)existingState.Join(this.lexicalState));
            }
            else
            {
                this.joinStates = this.joinStates.SetItem(location, this.lexicalState);
            }
        }

        public void JoinState(FlowLocation location, TState state)
        {
            TState existingState;
            if (this.joinStates.TryGetValue(location, out existingState))
            {
                this.lexicalState = (TState)existingState.Join(state);
            }
            else
            {
                this.lexicalState = state;
            }

            this.joinStates = this.joinStates.SetItem(location, this.lexicalState);
        }

        public void SetState(FlowLocation location, TState state)
        {
            this.lexicalState = state;
            this.joinStates = this.joinStates.SetItem(location, state);
        }
    }
}