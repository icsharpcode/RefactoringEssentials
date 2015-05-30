using System.Collections.Generic;
using ICSharpCode.NRefactory.PatternMatching;
using ICSharpCode.NRefactory.Refactoring;

namespace ICSharpCode.NRefactory6.CSharp.Diagnostics
{
    //	[IssueDescription("Else branch has some redundant if",
    //	                  Description = "One Else-if was checked before so is not be true",
    //	                  Category = IssueCategories.CodeQualityIssues,
    //	                  Severity = Severity.Warning,
    //	                  AnalysisDisableKeyword = "ConditionalTernaryEqualBranch")]
    public class DuplicateIfInIfChainDiagnosticAnalyzer : GatherVisitorCodeIssueProvider
    {
        protected override IGatherVisitor CreateVisitor(BaseSemanticModel context)
        {
            return new GatherVisitor(context);
        }

        class GatherVisitor : GatherVisitorBase<DuplicateIfInIfChainIssue>
        {
            public GatherVisitor(BaseSemanticModel ctx)
                : base(ctx)
            {
            }

            public override void VisitIfElseStatement(IfElseStatement ifStatement)
            {
                base.VisitIfElseStatement(ifStatement);
                var parentExpression = ifStatement.Parent as IfElseStatement;
                //handle only parent sequence
                if (parentExpression != null)
                    return;
                var expressions = GetExpressions(ifStatement);
                for (var i = 0; i < expressions.Count - 1; i++)
                {
                    for (var j = i + 1; j < expressions.Count; j++)
                    {
                        var leftCondition = expressions[i].Condition;
                        var rightIf = expressions[j];
                        var rightCondition = rightIf.Condition;

                        if (!leftCondition.IsMatch(rightCondition))
                            continue;
                        var action = new CodeAction(ctx.TranslateString("Remove redundant expression"),
                                                    script => RemoveRedundantIf(script, rightIf),
                                                    rightCondition);

                        AddDiagnosticAnalyzer(new CodeIssue(rightCondition,
                                 ctx.TranslateString(string.Format("The expression '{0}' is identical in the left branch",
                                rightCondition)), action)
                        { IssueMarker = IssueMarker.GrayOut });

                    }
                }
            }

            private static void RemoveRedundantIf(Script script, IfElseStatement expressionRight)
            {
                var parent = expressionRight.Parent as IfElseStatement;
                if (parent == null)
                { //should never happen!
                    return;
                }
                if (expressionRight.FalseStatement.IsNull)
                {
                    script.Remove(parent.ElseToken);
                    script.Remove(parent.FalseStatement);
                    script.FormatText(parent);
                }
                else
                {
                    script.Replace(parent.FalseStatement, expressionRight.FalseStatement.Clone());
                }
            }

            private static List<IfElseStatement> GetExpressions(IfElseStatement expression)
            {
                var baseExpression = expression;
                var falseExpression = baseExpression.FalseStatement as IfElseStatement;
                var expressions = new List<IfElseStatement>();
                while (falseExpression != null)
                {
                    expressions.Add(baseExpression);
                    baseExpression = falseExpression;
                    falseExpression = falseExpression.FalseStatement as IfElseStatement;
                }
                expressions.Add(baseExpression);
                return expressions;
            }
        }
    }

}
