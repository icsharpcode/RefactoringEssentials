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
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace RefactoringEssentials.Util.Analysis
{
    /// <summary>
    /// Statement reachability analysis.
    /// </summary>
    public sealed class ReachabilityAnalysis
    {
        HashSet<StatementSyntax> reachableStatements = new HashSet<StatementSyntax>();
        HashSet<StatementSyntax> reachableEndPoints = new HashSet<StatementSyntax>();
        HashSet<ControlFlowNode> visitedNodes = new HashSet<ControlFlowNode>();
        Stack<ControlFlowNode> stack = new Stack<ControlFlowNode>();
        RecursiveDetectorVisitor recursiveDetectorVisitor = null;

        private ReachabilityAnalysis() {}

        public static ReachabilityAnalysis Create(StatementSyntax statement, SemanticModel resolver = null, RecursiveDetectorVisitor recursiveDetectorVisitor = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            var cfgBuilder = new ControlFlowGraphBuilder();
            var cfg = cfgBuilder.BuildControlFlowGraph(statement, resolver, cancellationToken);
            return Create(cfg, recursiveDetectorVisitor, cancellationToken);
        }

        internal static ReachabilityAnalysis Create(StatementSyntax statement, SemanticModel typeResolveContext, CancellationToken cancellationToken)
        {
            var cfgBuilder = new ControlFlowGraphBuilder();
            var cfg = cfgBuilder.BuildControlFlowGraph(statement, typeResolveContext, cancellationToken);
            return Create(cfg, null, cancellationToken);
        }

        public static ReachabilityAnalysis Create(IList<ControlFlowNode> controlFlowGraph, RecursiveDetectorVisitor recursiveDetectorVisitor = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (controlFlowGraph == null)
                throw new ArgumentNullException("controlFlowGraph");
            ReachabilityAnalysis ra = new ReachabilityAnalysis();
            ra.recursiveDetectorVisitor = recursiveDetectorVisitor;
            // Analysing a null node can result in an empty control flow graph
            if (controlFlowGraph.Count > 0) {
                ra.stack.Push(controlFlowGraph[0]);
                while (ra.stack.Count > 0) {
                    cancellationToken.ThrowIfCancellationRequested();
                    ra.MarkReachable(ra.stack.Pop());
                }
            }
            ra.stack = null;
            ra.visitedNodes = null;
            return ra;
        }

        void MarkReachable(ControlFlowNode node)
        {
            if (node.PreviousStatement != null) {
                if (node.PreviousStatement is LabeledStatementSyntax) {
                    reachableStatements.Add(node.PreviousStatement);
                }
                reachableEndPoints.Add(node.PreviousStatement);
            }
            if (node.NextStatement != null) {
                reachableStatements.Add(node.NextStatement);
                if (IsRecursive(node.NextStatement)) {
                    return;
                }
            }
            foreach (var edge in node.Outgoing) {
                if (visitedNodes.Add(edge.To))
                    stack.Push(edge.To);
            }
        }

        bool IsRecursive(StatementSyntax statement)
        {
            return recursiveDetectorVisitor != null && recursiveDetectorVisitor.Visit(statement);
        }

        public IEnumerable<StatementSyntax> ReachableStatements {
            get { return reachableStatements; }
        }

        public bool IsReachable(StatementSyntax statement)
        {
            return reachableStatements.Contains(statement);
        }

        public bool IsEndpointReachable(StatementSyntax statement)
        {
            return reachableEndPoints.Contains(statement);
        }

        public class RecursiveDetectorVisitor : CSharpSyntaxVisitor<bool>
        {
            public override bool VisitConditionalExpression(ConditionalExpressionSyntax node)
            {
                if (Visit(node.Condition))
                    return true;
                if (!Visit(node.WhenTrue))
                    return false;
                return Visit(node.WhenFalse);
            }

            public override bool VisitConditionalAccessExpression(ConditionalAccessExpressionSyntax node)
            {
                if (!Visit(node.Expression))
                    return false;
                return Visit(node.WhenNotNull);
            }

            public override bool VisitBinaryExpression(BinaryExpressionSyntax node)
            {
                if (node.IsKind(SyntaxKind.CoalesceExpression)) {
                    return Visit(node.Left);
                }
                return base.VisitBinaryExpression(node);
            }

            public override bool VisitIfStatement(IfStatementSyntax node)
            {
                if (Visit(node.Condition))
                    return true;
                if (!Visit(node.Statement))
                    return false;
                //No need to worry about null ast nodes, since AcceptVisitor will just
                //return false in those cases
                return Visit(node.Else);
            }

            public override bool VisitForEachStatement(ForEachStatementSyntax node)
            {
                //Even if the body is always recursive, the function may stop if the collection
                // is empty.
                return Visit(node.Expression);
            }

            public override bool VisitForStatement(ForStatementSyntax node)
            {
                if (node.Initializers.Any(initializer => Visit(initializer)))
                    return true;
                return Visit(node.Condition);
            }

            public override bool VisitSwitchStatement(SwitchStatementSyntax node)
            {
                if (Visit(node.Expression))
                    return true;

                bool foundDefault = false;
                foreach (var section in node.Sections) {
                    foundDefault = foundDefault || section.Labels.Any(label => label.Keyword.IsKind(SyntaxKind.DefaultKeyword));
                    if (!Visit(section))
                        return false;
                }
                return foundDefault;
            }

            public override bool VisitExpressionStatement(ExpressionStatementSyntax node)
            {
                return Visit(node.Expression);
            }

            public override bool VisitReturnStatement(ReturnStatementSyntax node)
            {
                return Visit(node.Expression);
            }

            public override bool VisitBlock(BlockSyntax node)
            {
                //If the block has a recursive statement, then that statement will be visited
                //individually by the CFG construction algorithm later.
                return false;
            }

            //protected override bool VisitChildren(AstNode node)
            //{
            //    return VisitNodeList(node.Children);
            //}

            //bool VisitNodeList(IEnumerable<AstNode> nodes) {
            //    return nodes.Any(node => node.AcceptVisitor(this));
            //}

            public override bool VisitQueryExpression(QueryExpressionSyntax node)
            {
                //We only care about the first from clause because:
                //in "from x in Method() select x", Method() might be recursive
                //but in "from x in Bar() from y in Method() select x + y", even if Method() is recursive
                //Bar might still be empty.
                var queryFromClause = node.FromClause;
                if (queryFromClause == null)
                    return true;
                return Visit(queryFromClause);
            }
        }
    }
}