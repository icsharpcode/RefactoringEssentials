using System.Collections.Generic;
using System.Linq;
using ICSharpCode.NRefactory6.CSharp.Analysis;
using ICSharpCode.NRefactory.TypeSystem;
using ICSharpCode.NRefactory.Refactoring;

namespace ICSharpCode.NRefactory6.CSharp.Diagnostics
{
    [IssueDescription("Access to disposed closure variable",
                       Description = "Access to closure variable from anonymous method when the variable is" +
                                     " disposed externally",
                       Category = IssueCategories.CodeQualityIssues,
                       Severity = Severity.Warning,
                       AnalysisDisableKeyword = "AccessToDisposedClosure")]
    public class AccessToDisposedClosureDiagnosticAnalyzer : AccessToClosureIssue
    {
        public AccessToDisposedClosureIssue()
			: base ("Access to disposed closure")
		{
        }

        protected override bool IsTargetVariable(IVariable variable)
        {
            return variable.Type.GetAllBaseTypeDefinitions().Any(t => t.KnownTypeCode == KnownTypeCode.IDisposable);
        }

        protected override NodeKind GetNodeKind(AstNode node)
        {
            if (node.Parent is UsingStatement)
                return NodeKind.ReferenceAndModification;

            if (node.Parent is VariableDeclarationStatement && node.Parent.Parent is UsingStatement)
                return NodeKind.Modification;

            var memberRef = node.Parent as MemberReferenceExpression;
            if (memberRef != null && memberRef.Parent is InvocationExpression && memberRef.MemberName == "Dispose")
                return NodeKind.ReferenceAndModification;

            return NodeKind.Reference;
        }

        protected override bool CanReachModification(ControlFlowNode node, Statement start,
                                                   IDictionary<Statement, IList<Node>> modifications)
        {
            if (base.CanReachModification(node, start, modifications))
                return true;

            if (node.NextStatement != start)
            {
                var usingStatement = node.PreviousStatement as UsingStatement;
                if (usingStatement != null)
                {
                    if (modifications.ContainsKey(usingStatement))
                        return true;
                    if (usingStatement.ResourceAcquisition is Statement &&
                        modifications.ContainsKey((Statement)usingStatement.ResourceAcquisition))
                        return true;
                }
            }
            return false;
        }

        protected override IEnumerable<CodeAction> GetFixes(BaseSemanticModel context, Node env,
                                                             string variableName)
        {
            yield break;
        }
    }
}