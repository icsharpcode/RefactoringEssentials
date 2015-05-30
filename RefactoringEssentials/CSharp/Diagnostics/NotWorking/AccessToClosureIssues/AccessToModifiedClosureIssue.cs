using System;
using System.Collections.Generic;
using System.Linq;
using ICSharpCode.NRefactory.Refactoring;

namespace ICSharpCode.NRefactory6.CSharp.Diagnostics
{
    [IssueDescription("Access to modified closure variable",
                       Description = "Access to closure variable from anonymous method when the variable is modified " +
                                     "externally",
                       Category = IssueCategories.CodeQualityIssues,
                       Severity = Severity.Warning,
                       AnalysisDisableKeyword = "AccessToModifiedClosure")]
    public class AccessToModifiedClosureDiagnosticAnalyzer : AccessToClosureIssue
    {
        public AccessToModifiedClosureIssue()
			: base ("Access to modified closure")
		{
        }

        protected override NodeKind GetNodeKind(AstNode node)
        {
            var assignment = node.GetParent<AssignmentExpression>();
            if (assignment != null && assignment.Left == node)
            {
                if (assignment.Operator == AssignmentOperatorType.Assign)
                    return NodeKind.Modification;
                return NodeKind.ReferenceAndModification;
            }
            var unaryExpr = node.GetParent<UnaryOperatorExpression>();
            if (unaryExpr != null && unaryExpr.Expression == node &&
                (unaryExpr.Operator == UnaryOperatorType.Increment ||
                 unaryExpr.Operator == UnaryOperatorType.PostIncrement ||
                 unaryExpr.Operator == UnaryOperatorType.Decrement ||
                 unaryExpr.Operator == UnaryOperatorType.PostDecrement))
            {
                return NodeKind.ReferenceAndModification;
            }
            if (node.Parent is ForeachStatement)
                return NodeKind.Modification;

            return NodeKind.Reference;
        }

        protected override IEnumerable<CodeAction> GetFixes(BaseSemanticModel context, Node env,
                                                             string variableName)
        {
            var containingStatement = env.ContainingStatement;

            // we don't give a fix for these cases since the general fix may not work
            // lambda in while/do-while/for condition
            if (containingStatement is WhileStatement || containingStatement is DoWhileStatement ||
                containingStatement is ForStatement)
                yield break;
            // lambda in for initializer/iterator
            if (containingStatement.Parent is ForStatement &&
                ((ForStatement)containingStatement.Parent).EmbeddedStatement != containingStatement)
                yield break;

            Action<Script> action = script =>
            {
                var newName = LocalVariableNamePicker.PickSafeName(
                    containingStatement.GetParent<EntityDeclaration>(),
                    Enumerable.Range(1, 100).Select(i => variableName + i));

                var variableDecl = new VariableDeclarationStatement(new SimpleType("var"), newName,
                                                                     new IdentifierExpression(variableName));

                if (containingStatement.Parent is BlockStatement || containingStatement.Parent is SwitchSection)
                {
                    script.InsertBefore(containingStatement, variableDecl);
                }
                else
                {
                    var offset = script.GetCurrentOffset(containingStatement.StartLocation);
                    script.InsertBefore(containingStatement, variableDecl);
                    script.InsertText(offset, "{");
                    script.InsertText(script.GetCurrentOffset(containingStatement.EndLocation), "}");
                    script.FormatText(containingStatement.Parent);
                }

                var textNodes = new List<AstNode>();
                textNodes.Add(variableDecl.Variables.First().NameToken);

                foreach (var reference in env.GetAllReferences())
                {
                    var identifier = new IdentifierExpression(newName);
                    script.Replace(reference.AstNode, identifier);
                    textNodes.Add(identifier);
                }
                script.Link(textNodes.ToArray());
            };
            yield return new CodeAction(context.TranslateString("Copy to local variable"), action, env.AstNode);
        }
    }
}
