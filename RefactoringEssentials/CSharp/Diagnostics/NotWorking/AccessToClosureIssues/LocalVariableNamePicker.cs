using System.Collections.Generic;
using System.Linq;

namespace ICSharpCode.NRefactory6.CSharp.Diagnostics
{
    public static class LocalVariableNamePicker
    {
        public static string PickSafeName(AstNode node, IEnumerable<string> candidates)
        {
            var existingNames = new VariableNameCollector().Collect(node);
            return candidates.FirstOrDefault(name => !existingNames.Contains(name));
        }

        class VariableNameCollector : DepthFirstAstVisitor
        {
            private ISet<string> variableNames = new HashSet<string>();

            public ISet<string> Collect(AstNode node)
            {
                variableNames.Clear();
                node.AcceptVisitor(this);
                return variableNames;
            }

            public override void VisitParameterDeclaration(ParameterDeclaration parameterDeclaration)
            {
                variableNames.Add(parameterDeclaration.Name);
                base.VisitParameterDeclaration(parameterDeclaration);
            }

            public override void VisitVariableInitializer(VariableInitializer variableInitializer)
            {
                variableNames.Add(variableInitializer.Name);
                base.VisitVariableInitializer(variableInitializer);
            }

            public override void VisitForeachStatement(ForeachStatement foreachStatement)
            {
                variableNames.Add(foreachStatement.VariableName);
                base.VisitForeachStatement(foreachStatement);
            }
        }
    }
}
