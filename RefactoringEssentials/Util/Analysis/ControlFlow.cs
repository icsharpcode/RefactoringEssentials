// Copyright (c) 2010-2013 AlphaSierraPapa for the SharpDevelop Team
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this
// software and associated documentation files (the "Software"), to deal in the Software
// without restriction, including without limitation the rights to use, copy, modify, merge,
// publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons
// to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or
// substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED,
// INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR
// PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE
// FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR
// OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;

namespace RefactoringEssentials.Util.Analysis
{
	/// <summary>
	/// Represents a node in the control flow graph of a C# method.
	/// </summary>
	public class ControlFlowNode
    {
        public readonly StatementSyntax PreviousStatement;
        public readonly StatementSyntax NextStatement;

        public readonly ControlFlowNodeType Type;

        public readonly List<ControlFlowEdge> Outgoing = new List<ControlFlowEdge>();
        public readonly List<ControlFlowEdge> Incoming = new List<ControlFlowEdge>();

        public ControlFlowNode(StatementSyntax previousStatement, StatementSyntax nextStatement, ControlFlowNodeType type)
        {
            if (previousStatement == null && nextStatement == null)
                throw new ArgumentException("previousStatement and nextStatement must not be both null");
            this.PreviousStatement = previousStatement;
            this.NextStatement = nextStatement;
            this.Type = type;
        }
    }

    public enum ControlFlowNodeType
    {
        /// <summary>
        /// Unknown node type
        /// </summary>
        None,
        /// <summary>
        /// Node in front of a statement
        /// </summary>
        StartNode,
        /// <summary>
        /// Node between two statements
        /// </summary>
        BetweenStatements,
        /// <summary>
        /// Node at the end of a statement list
        /// </summary>
        EndNode,
        /// <summary>
        /// Node representing the position before evaluating the condition of a loop.
        /// </summary>
        LoopCondition
    }

    public class ControlFlowEdge
    {
        public readonly ControlFlowNode From;
        public readonly ControlFlowNode To;
        public readonly ControlFlowEdgeType Type;

        List<TryStatementSyntax> jumpOutOfTryFinally;

        public ControlFlowEdge(ControlFlowNode from, ControlFlowNode to, ControlFlowEdgeType type)
        {
            if (from == null)
                throw new ArgumentNullException("from");
            if (to == null)
                throw new ArgumentNullException("to");
            this.From = from;
            this.To = to;
            this.Type = type;
        }

        internal void AddJumpOutOfTryFinally(TryStatementSyntax tryFinally)
        {
            if (jumpOutOfTryFinally == null)
                jumpOutOfTryFinally = new List<TryStatementSyntax>();
            jumpOutOfTryFinally.Add(tryFinally);
        }

        /// <summary>
        /// Gets whether this control flow edge is leaving any try-finally statements.
        /// </summary>
        public bool IsLeavingTryFinally {
            get { return jumpOutOfTryFinally != null; }
        }

        /// <summary>
        /// Gets the try-finally statements that this control flow edge is leaving.
        /// </summary>
        public IEnumerable<TryStatementSyntax> TryFinallyStatements {
            get { return jumpOutOfTryFinally ?? Enumerable.Empty<TryStatementSyntax>(); }
        }
    }

    public enum ControlFlowEdgeType
    {
        /// <summary>
        /// Regular control flow.
        /// </summary>
        Normal,
        /// <summary>
        /// Conditional control flow (edge taken if condition is true)
        /// </summary>
        ConditionTrue,
        /// <summary>
        /// Conditional control flow (edge taken if condition is false)
        /// </summary>
        ConditionFalse,
        /// <summary>
        /// A jump statement (goto, goto case, break or continue)
        /// </summary>
        Jump
    }

    /// <summary>
    /// Constructs the control flow graph for C# statements.
    /// </summary>
    public class ControlFlowGraphBuilder
    {
        // Written according to the reachability rules in the C# spec (§8.1 End points and reachability)

        protected virtual ControlFlowNode CreateNode(StatementSyntax previousStatement, StatementSyntax nextStatement, ControlFlowNodeType type)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return new ControlFlowNode(previousStatement, nextStatement, type);
        }

        protected virtual ControlFlowEdge CreateEdge(ControlFlowNode from, ControlFlowNode to, ControlFlowEdgeType type)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return new ControlFlowEdge(from, to, type);
        }

        StatementSyntax rootStatement;
        SemanticModel typeResolveContext;
        Func<SyntaxNode, CancellationToken, ISymbol> resolver;
        List<ControlFlowNode> nodes;
        Dictionary<string, ControlFlowNode> labels;
        List<GotoStatementSyntax> gotoStatements;
        CancellationToken cancellationToken;


        public IList<ControlFlowNode> BuildControlFlowGraph(StatementSyntax statement, SemanticModel resolver, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (statement == null)
                throw new ArgumentNullException("statement");
            if (resolver == null)
                throw new ArgumentNullException("resolver");
            return BuildControlFlowGraph(statement, resolver, (sn, token) => resolver.GetSymbolInfo(sn, token).Symbol, cancellationToken);
        }

        internal IList<ControlFlowNode> BuildControlFlowGraph(StatementSyntax statement, SemanticModel typeResolveContext, Func<SyntaxNode, CancellationToken, ISymbol> resolver, CancellationToken cancellationToken)
        {
            NodeCreationVisitor nodeCreationVisitor = new NodeCreationVisitor();
            nodeCreationVisitor.builder = this;
            try {
                this.nodes = new List<ControlFlowNode>();
                this.labels = new Dictionary<string, ControlFlowNode>();
                this.gotoStatements = new List<GotoStatementSyntax>();
                this.rootStatement = statement;
                this.resolver = resolver;
                this.typeResolveContext = typeResolveContext;
                this.cancellationToken = cancellationToken;
                // TODO:
                nodeCreationVisitor.curNode = CreateStartNode(statement);
                nodeCreationVisitor.Visit(statement);

                // Resolve goto statements:
                foreach (var gotoStmt in gotoStatements) {
                    string label = ((GotoStatementSyntax)gotoStmt).Expression?.ToString();
                    ControlFlowNode labelNode;
                    if (labels.TryGetValue(label, out labelNode))
                        nodeCreationVisitor.Connect(nodeCreationVisitor.curNode, labelNode, ControlFlowEdgeType.Jump);
                }

                AnnotateLeaveEdgesWithTryFinallyBlocks();

                return nodes;
            } finally {
                this.nodes = null;
                this.labels = null;
                this.gotoStatements = null;
                this.rootStatement = null;
                this.resolver = null;
                this.typeResolveContext = null;
                this.cancellationToken = CancellationToken.None;
            }
        }

        void AnnotateLeaveEdgesWithTryFinallyBlocks()
        {
            foreach (ControlFlowEdge edge in nodes.SelectMany(n => n.Outgoing)) {
                if (edge.Type != ControlFlowEdgeType.Jump) {
                    // Only jumps are potential candidates for leaving try-finally blocks.
                    // Note that the regular edges leaving try or catch blocks are already annotated by the visitor.
                    continue;
                }
                var gotoStatement = edge.From.NextStatement;
                // Debug.Assert(gotoStatement is GotoStatementSyntax || gotoStatement is BreakStatementSyntax || gotoStatement is ContinueStatementSyntax);
				var targetStatement = edge.To.PreviousStatement ?? edge.To.NextStatement;
				if (gotoStatement == null || targetStatement == null || gotoStatement.Parent == targetStatement.Parent)
                    continue;
                var targetParentTryCatch = new HashSet<TryStatementSyntax>(targetStatement.Ancestors().OfType<TryStatementSyntax>());
                for (SyntaxNode node = gotoStatement.Parent; node != null; node = node.Parent) {
                    var leftTryCatch = node as TryStatementSyntax;
                    if (leftTryCatch != null) {
                        if (targetParentTryCatch.Contains(leftTryCatch))
                            break;
                        if (leftTryCatch.Finally != null)
                            edge.AddJumpOutOfTryFinally(leftTryCatch);
                    }
                }
            }
        }

        #region Create*Node
        ControlFlowNode CreateStartNode(StatementSyntax statement)
        {
            if (statement == null)
                return null;
            ControlFlowNode node = CreateNode(null, statement, ControlFlowNodeType.StartNode);
            nodes.Add(node);
            return node;
        }

        ControlFlowNode CreateSpecialNode(StatementSyntax statement, ControlFlowNodeType type, bool addToNodeList = true)
        {
            ControlFlowNode node = CreateNode(null, statement, type);
            if (addToNodeList)
                nodes.Add(node);
            return node;
        }

        ControlFlowNode CreateEndNode(StatementSyntax statement, bool addToNodeList = true)
        {
            StatementSyntax nextStatement;
            if (statement == rootStatement) {
                nextStatement = null;
            } else {
                // Find the next statement in the same role:
                var list = statement.Parent.ChildNodes().ToList();
                var idx = list.IndexOf(statement);
                nextStatement = idx >= 0 && idx + 1 < list.Count ? list[idx + 1] as StatementSyntax : null;
            }
            ControlFlowNodeType type = nextStatement != null ? ControlFlowNodeType.BetweenStatements : ControlFlowNodeType.EndNode;
            ControlFlowNode node = CreateNode(statement, nextStatement, type);
            if (addToNodeList)
                nodes.Add(node);
            return node;
        }
        #endregion

        #region Constant evaluation
        /// <summary>
        /// Gets/Sets whether to handle only primitive expressions as constants (no complex expressions like "a + b").
        /// </summary>
        public bool EvaluateOnlyPrimitiveConstants { get; set; }

        /// <summary>
        /// Evaluates an expression.
        /// </summary>
        /// <returns>The constant value of the expression; or null if the expression is not a constant.</returns>
        Optional<object> EvaluateConstant(ExpressionSyntax expr)
        {
            if (expr == null)
                return new SymbolInfo ();
            if (EvaluateOnlyPrimitiveConstants) {
                if (!(expr is LiteralExpressionSyntax /*|| expr is NullReferenceExpression*/))
                    return null;
            }

            return typeResolveContext.GetConstantValue(expr);
        }

        /// <summary>
        /// Evaluates an expression.
        /// </summary>
        /// <returns>The value of the constant boolean expression; or null if the value is not a constant boolean expression.</returns>
        bool? EvaluateCondition(ExpressionSyntax expr)
        {
            var rr = EvaluateConstant(expr);
            if (rr.HasValue)
                return rr.Value as bool?;
            else
                return null;
        }

        bool AreEqualConstants(Optional<object> c1, Optional<object> c2)
        {
            if (!c1.HasValue || !c2.HasValue)
                return false;
            return c1.Value != null && c2.Value != null && c1.Value.Equals(c2.Value);
        }
        #endregion

        sealed class NodeCreationVisitor : CSharpSyntaxVisitor
        {
            // 'data' parameter: input control flow node (start of statement being visited)
            // Return value: result control flow node (end of statement being visited)

            internal ControlFlowGraphBuilder builder;
            Stack<ControlFlowNode> breakTargets = new Stack<ControlFlowNode>();
            Stack<ControlFlowNode> continueTargets = new Stack<ControlFlowNode>();
            List<ControlFlowNode> gotoCaseOrDefault = new List<ControlFlowNode>();
            internal ControlFlowNode curNode;

            internal ControlFlowEdge Connect(ControlFlowNode from, ControlFlowNode to, ControlFlowEdgeType type = ControlFlowEdgeType.Normal)
            {
                if (from == null || to == null)
                    return null;
                ControlFlowEdge edge = builder.CreateEdge(from, to, type);
                from.Outgoing.Add(edge);
                to.Incoming.Add(edge);
                return edge;
            }

            /// <summary>
            /// Creates an end node for <c>stmt</c> and connects <c>from</c> with the new node.
            /// </summary>
            void CreateConnectedEndNode(StatementSyntax stmt)
            {
                ControlFlowNode newNode = builder.CreateEndNode(stmt);
                Connect(curNode, newNode);
                curNode = newNode;
            }

            public override void DefaultVisit(SyntaxNode node)
            {
                // We have overrides for all possible statements and should visit statements only.
                throw new NotSupportedException();
            }

            public override void VisitBlock(BlockSyntax node)
            {
                // C# 4.0 spec: §8.2 Blocks
                curNode = HandleStatementList(node.Statements, curNode);
                CreateConnectedEndNode(node);
            }

            ControlFlowNode HandleStatementList(IEnumerable<StatementSyntax> statements, ControlFlowNode source)
            {
                var oldCurNode = curNode;
                ControlFlowNode childNode = null;
                foreach (var stmt in statements) {
                    if (childNode == null) {
                        curNode = childNode = builder.CreateStartNode(stmt);
                        if (source != null)
                            Connect(source, childNode);
                    }
                    // Debug.Assert(childNode.NextStatement == stmt);
                    Visit(stmt);
                    childNode = curNode;
                    // Debug.Assert(childNode.PreviousStatement == stmt);
                }
                curNode = oldCurNode;
                return childNode ?? source;
            }

            public override void VisitEmptyStatement(EmptyStatementSyntax node)
            {
                CreateConnectedEndNode(node);
            }

            public override void VisitLabeledStatement(LabeledStatementSyntax node)
            {
                CreateConnectedEndNode(node);
                builder.labels[node.Identifier.ValueText] = curNode;
            }

            public override void VisitLocalDeclarationStatement(LocalDeclarationStatementSyntax node)
            {
                CreateConnectedEndNode(node);
            }

            public override void VisitExpressionStatement(ExpressionStatementSyntax node)
            {
                CreateConnectedEndNode(node);
            }

            public override void VisitIfStatement(IfStatementSyntax node)
            {
                bool? cond = builder.EvaluateCondition(node.Condition);

                var startNode = curNode;
                ControlFlowNode trueBegin = builder.CreateStartNode(node.Statement);
                if (cond != false)
                    Connect(startNode, trueBegin, ControlFlowEdgeType.ConditionTrue);

                curNode = trueBegin;
                Visit(node.Statement);
                ControlFlowNode trueEnd = curNode;

                ControlFlowNode falseEnd = null;

                if (node.Else?.Statement != null)
                {
                    ControlFlowNode falseBegin = builder.CreateStartNode(node.Else?.Statement);
                    if (cond != true)
                        Connect(startNode, falseBegin, ControlFlowEdgeType.ConditionFalse);

                    curNode = trueBegin;
                    Visit(node.Else?.Statement);

                    falseEnd = curNode;
                }
                // (if no else statement exists, both falseBegin and falseEnd will be null)

                ControlFlowNode end = builder.CreateEndNode(node);
                Connect(trueEnd, end);
                if (falseEnd != null) {
                    Connect(falseEnd, end);
                } else if (cond != true) {
                    Connect(startNode, end, ControlFlowEdgeType.ConditionFalse);
                }
                curNode = end;
            }

            public override void VisitSwitchStatement(SwitchStatementSyntax node)
            {
                // First, figure out which switch section will get called (if the expression is constant):
                var constant = builder.EvaluateConstant(node.Expression);
                SwitchSectionSyntax defaultSection = null;
                SwitchSectionSyntax sectionMatchedByConstant = null;
                foreach (var section in node.Sections) {
                    foreach (var label in section.Labels) {
                        
                        if (label.IsKind(SyntaxKind.DefaultSwitchLabel)) {
                            defaultSection = section;
                        } else if (constant.HasValue /*&& constant.IsCompileTimeConstant*/) {
                            var scl = label as CaseSwitchLabelSyntax;
                            var labelConstant = builder.EvaluateConstant(scl.Value);
                            if (builder.AreEqualConstants(constant, labelConstant))
                                sectionMatchedByConstant = section;
                        }
                    }
                }
                if (constant.HasValue && sectionMatchedByConstant == null)
                    sectionMatchedByConstant = defaultSection;

                int gotoCaseOrDefaultInOuterScope = gotoCaseOrDefault.Count;
                List<ControlFlowNode> sectionStartNodes = new List<ControlFlowNode>();

                ControlFlowNode end = builder.CreateEndNode(node, addToNodeList: false);
                breakTargets.Push(end);
                var myEntryPoint = curNode;
                foreach (var section in node.Sections) {
                    int sectionStartNodeID = builder.nodes.Count;
                    if (!constant.HasValue || section == sectionMatchedByConstant) {
                        curNode = myEntryPoint;
                        HandleStatementList(section.Statements, myEntryPoint);
                    } else {
                        // This section is unreachable: pass null to HandleStatementList.
                        curNode = null;
                        HandleStatementList(section.Statements, null);
                    }
                    // Don't bother connecting the ends of the sections: the 'break' statement takes care of that.

                    // Store the section start node for 'goto case' statements.
                    sectionStartNodes.Add(sectionStartNodeID < builder.nodes.Count ? builder.nodes[sectionStartNodeID] : null);
                }
                breakTargets.Pop();
                if (defaultSection == null && sectionMatchedByConstant == null) {
                    Connect(myEntryPoint, end);
                }

                if (gotoCaseOrDefault.Count > gotoCaseOrDefaultInOuterScope) {
                    // Resolve 'goto case' statements:
                    for (int i = gotoCaseOrDefaultInOuterScope; i < gotoCaseOrDefault.Count; i++) {
                        ControlFlowNode gotoCaseNode = gotoCaseOrDefault[i];
                        var gotoCaseStatement = gotoCaseNode.NextStatement as GotoStatementSyntax;
                        Optional<object> gotoCaseConstant = null;
                        if (gotoCaseStatement != null) {
                            gotoCaseConstant = builder.EvaluateConstant(gotoCaseStatement.Expression);
                        }
                        int targetSectionIndex = -1;
                        int currentSectionIndex = 0;
                        foreach (var section in node.Sections) {
                            foreach (var label in section.Labels) {
                                if (label is CaseSwitchLabelSyntax) {
                                    var scl = label as CaseSwitchLabelSyntax;

                                    // goto case
                                    if (scl.Value != null) {
                                        var labelConstant = builder.EvaluateConstant(scl.Value);
                                        if (builder.AreEqualConstants(gotoCaseConstant, labelConstant))
                                            targetSectionIndex = currentSectionIndex;
                                    }
                                } else {
                                    // goto default
                                    if (label.IsKind(SyntaxKind.DefaultSwitchLabel))
                                        targetSectionIndex = currentSectionIndex;
                                }
                            }
                            currentSectionIndex++;
                        }
                        if (targetSectionIndex >= 0 && sectionStartNodes[targetSectionIndex] != null)
                            Connect(gotoCaseNode, sectionStartNodes[targetSectionIndex], ControlFlowEdgeType.Jump);
                        else
                            Connect(gotoCaseNode, end, ControlFlowEdgeType.Jump);
                    }
                    gotoCaseOrDefault.RemoveRange(gotoCaseOrDefaultInOuterScope, gotoCaseOrDefault.Count - gotoCaseOrDefaultInOuterScope);
                }

                builder.nodes.Add(end);
                curNode = end;
            }


            public override void VisitWhileStatement(WhileStatementSyntax node)
            {
                // <data> <condition> while (cond) { <bodyStart> embeddedStmt; <bodyEnd> } <end>
                ControlFlowNode end = builder.CreateEndNode(node, addToNodeList: false);
                ControlFlowNode conditionNode = builder.CreateSpecialNode(node, ControlFlowNodeType.LoopCondition);
                breakTargets.Push(end);
                continueTargets.Push(conditionNode);

                Connect(curNode, conditionNode);

                bool? cond = builder.EvaluateCondition(node.Condition);
                ControlFlowNode bodyStart = builder.CreateStartNode(node.Statement);
                if (cond != false)
                    Connect(conditionNode, bodyStart, ControlFlowEdgeType.ConditionTrue);
                Visit(node.Statement);
                Connect(curNode, conditionNode);
                if (cond != true)
                    Connect(conditionNode, end, ControlFlowEdgeType.ConditionFalse);

                breakTargets.Pop();
                continueTargets.Pop();
                builder.nodes.Add(end);
                curNode = end;
            }

            public override void VisitDoStatement(DoStatementSyntax node)
            {
                // <data> do { <bodyStart> embeddedStmt; <bodyEnd>} <condition> while(cond); <end>
                ControlFlowNode end = builder.CreateEndNode(node, addToNodeList: false);
                ControlFlowNode conditionNode = builder.CreateSpecialNode(node, ControlFlowNodeType.LoopCondition, addToNodeList: false);
                breakTargets.Push(end);
                continueTargets.Push(conditionNode);

                ControlFlowNode bodyStart = builder.CreateStartNode(node.Statement);
                Connect(curNode, bodyStart);
                Visit(node.Statement);
                Connect(curNode, conditionNode);

                bool? cond = builder.EvaluateCondition(node.Condition);
                if (cond != false)
                    Connect(conditionNode, bodyStart, ControlFlowEdgeType.ConditionTrue);
                if (cond != true)
                    Connect(conditionNode, end, ControlFlowEdgeType.ConditionFalse);

                breakTargets.Pop();
                continueTargets.Pop();
                builder.nodes.Add(conditionNode);
                builder.nodes.Add(end);
                curNode = end;
            }


            public override void VisitForStatement(ForStatementSyntax forStatement)
            {
                // Initializers/Iterators ?  -> difference between NR5/Roslyn -> they're not statements anymore.
                // HandleStatementList(forStatement.Initializers, curNode);

                // for (initializers <data>; <condition>cond; <iteratorStart>iterators<iteratorEnd>) { <bodyStart> embeddedStmt; <bodyEnd> } <end>
                ControlFlowNode end = builder.CreateEndNode(forStatement, addToNodeList: false);
                ControlFlowNode conditionNode = builder.CreateSpecialNode(forStatement, ControlFlowNodeType.LoopCondition);
                Connect(curNode, conditionNode);

                int iteratorStartNodeID = builder.nodes.Count;
                ControlFlowNode iteratorEnd = null; // HandleStatementList(forStatement.Incrementors, null);
                ControlFlowNode iteratorStart;
                if (iteratorEnd != null) {
                    iteratorStart = builder.nodes[iteratorStartNodeID];
                    Connect(iteratorEnd, conditionNode);
                } else {
                    iteratorStart = conditionNode;
                }

                breakTargets.Push(end);
                continueTargets.Push(iteratorStart);

                ControlFlowNode bodyStart = builder.CreateStartNode(forStatement.Statement);
                Visit (forStatement.Statement);
                Connect(curNode, iteratorStart);

                breakTargets.Pop();
                continueTargets.Pop();

                bool? cond = forStatement.Condition == null ? true : builder.EvaluateCondition(forStatement.Condition);
                if (cond != false)
                    Connect(conditionNode, bodyStart, ControlFlowEdgeType.ConditionTrue);
                if (cond != true)
                    Connect(conditionNode, end, ControlFlowEdgeType.ConditionFalse);

                builder.nodes.Add(end);
                curNode = end;
            }

            void HandleEmbeddedStatement(StatementSyntax embeddedStatement, ControlFlowNode source)
            {
                if (embeddedStatement == null)
                {
                    curNode = source;
                }
                ControlFlowNode bodyStart = builder.CreateStartNode(embeddedStatement);
                if (source != null)
                    Connect(source, bodyStart);
                Visit(embeddedStatement); 
            }

            public override void VisitForEachStatement(ForEachStatementSyntax node)
            {
                // <data> foreach (<condition>...) { <bodyStart>embeddedStmt<bodyEnd> } <end>
                ControlFlowNode end = builder.CreateEndNode(node, addToNodeList: false);
                ControlFlowNode conditionNode = builder.CreateSpecialNode(node, ControlFlowNodeType.LoopCondition);
                Connect(curNode, conditionNode);

                breakTargets.Push(end);
                continueTargets.Push(conditionNode);

                HandleEmbeddedStatement(node.Statement, conditionNode);
                Connect(curNode, conditionNode);

                breakTargets.Pop();
                continueTargets.Pop();

                Connect(conditionNode, end);
                builder.nodes.Add(end);
                curNode = end;
            }

            public override void VisitBreakStatement(BreakStatementSyntax node)
            {
                if (breakTargets.Count > 0)
                {
                    var target = breakTargets.Peek();
                    Connect(curNode, target, ControlFlowEdgeType.Jump);
                    curNode = target;
                }
                curNode = builder.CreateEndNode(node);
            }


            public override void VisitContinueStatement(ContinueStatementSyntax node)
            {
                if (continueTargets.Count > 0)
                {
                    var target = continueTargets.Peek();
                    Connect(curNode, target, ControlFlowEdgeType.Jump);
                    curNode = target;
                }
                curNode = builder.CreateEndNode(node);
            }

            public override void VisitGotoStatement(GotoStatementSyntax node)
            {
                if (node.IsKind(SyntaxKind.GotoDefaultStatement) || node.IsKind(SyntaxKind.GotoCaseStatement)) {
                    gotoCaseOrDefault.Add(curNode);
                } else {
                    builder.gotoStatements.Add(node);
                }
                curNode = builder.CreateEndNode(node);
            }

            public override void VisitReturnStatement(ReturnStatementSyntax node)
            {
                // end not connected with data
                curNode = builder.CreateEndNode(node);
            }

            public override void VisitThrowStatement(ThrowStatementSyntax node)
            {
                // end not connected with data
                curNode = builder.CreateEndNode(node);
            }

            public override void VisitTryStatement(TryStatementSyntax node)
            {
                ControlFlowNode end = builder.CreateEndNode(node, addToNodeList: false);
                HandleEmbeddedStatement(node.Block, curNode);
                var edge = Connect(curNode, end);
                if (node.Finally?.Block != null)
                    edge.AddJumpOutOfTryFinally(node);
                var tryEndPoint = curNode;
                foreach (var cc in node.Catches) {
                    HandleEmbeddedStatement(cc.Block, tryEndPoint);
                    edge = Connect(tryEndPoint, end);
                    if (node.Finally?.Block != null)
                        edge.AddJumpOutOfTryFinally(node);
                }
                if (node.Finally?.Block != null) {
                    // Don't connect the end of the try-finally block to anything.
                    // Consumers of the CFG will have to special-case try-finally.
                    HandleEmbeddedStatement(node.Finally.Block, curNode);
                }
                builder.nodes.Add(end);
                curNode = end;
            }

            public override void VisitCheckedStatement(CheckedStatementSyntax node)
            {
                HandleEmbeddedStatement(node.Block, curNode);
                CreateConnectedEndNode(node);
            }


            //public override ControlFlowNode VisitUncheckedStatement(UncheckedStatement uncheckedStatement, ControlFlowNode data)
            //{
            //    ControlFlowNode bodyEnd = HandleEmbeddedStatement(uncheckedStatement.Body, data);
            //    return CreateConnectedEndNode(uncheckedStatement, bodyEnd);
            //}

            public override void VisitLockStatement(LockStatementSyntax node)
            {
                HandleEmbeddedStatement(node.Statement, curNode);
                CreateConnectedEndNode(node);
            }

            public override void VisitUsingStatement(UsingStatementSyntax node)
            {
                // ResourceAcquisition ?
                HandleEmbeddedStatement(node.Statement, curNode);
                CreateConnectedEndNode(node);
            }

            public override void VisitYieldStatement(YieldStatementSyntax node)
            {
                if (node.IsKind(SyntaxKind.YieldBreakStatement))
                {
                    // end not connected with data
                    curNode = builder.CreateEndNode(node);
                }
                else {
                    CreateConnectedEndNode(node);
                }
            }

            public override void VisitUnsafeStatement(UnsafeStatementSyntax node)
            {
                HandleEmbeddedStatement(node.Block, curNode);
                CreateConnectedEndNode(node);
            }

            public override void VisitFixedStatement(FixedStatementSyntax node)
            {
                HandleEmbeddedStatement(node.Statement, curNode);
                CreateConnectedEndNode(node);
            }
        }

        ///// <summary>
        ///// Debugging helper that exports a control flow graph.
        ///// </summary>
        //public static GraphVizGraph ExportGraph(IList<ControlFlowNode> nodes)
        //{
        //    GraphVizGraph g = new GraphVizGraph();
        //    GraphVizNode[] n = new GraphVizNode[nodes.Count];
        //    Dictionary<ControlFlowNode, int> dict = new Dictionary<ControlFlowNode, int>();
        //    for (int i = 0; i < n.Length; i++) {
        //        dict.Add(nodes[i], i);
        //        n[i] = new GraphVizNode(i);
        //        string name = "#" + i + " = ";
        //        switch (nodes[i].Type) {
        //            case ControlFlowNodeType.StartNode:
        //            case ControlFlowNodeType.BetweenStatements:
        //                name += nodes[i].NextStatement.DebugToString();
        //            break;
        //            case ControlFlowNodeType.EndNode:
        //                name += "End of " + nodes[i].PreviousStatement.DebugToString();
        //            break;
        //            case ControlFlowNodeType.LoopCondition:
        //                name += "Condition in " + nodes[i].NextStatement.DebugToString();
        //            break;
        //            default:
        //                name += "?";
        //            break;
        //        }
        //        n[i].label = name;
        //        g.AddNode(n[i]);
        //    }
        //    for (int i = 0; i < n.Length; i++) {
        //        foreach (ControlFlowEdge edge in nodes[i].Outgoing) {
        //            GraphVizEdge ge = new GraphVizEdge(i, dict[edge.To]);
        //            if (edge.IsLeavingTryFinally)
        //                ge.style = "dashed";
        //            switch (edge.Type) {
        //                case ControlFlowEdgeType.ConditionTrue:
        //                    ge.color = "green";
        //                break;
        //                case ControlFlowEdgeType.ConditionFalse:
        //                    ge.color = "red";
        //                break;
        //                case ControlFlowEdgeType.Jump:
        //                    ge.color = "blue";
        //                break;
        //            }
        //            g.AddEdge(ge);
        //        }
        //    }
        //    return g;
        //}
    }
}
