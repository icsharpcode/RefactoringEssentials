using Microsoft.CodeAnalysis.Diagnostics;

namespace RefactoringEssentials.CSharp.Diagnostics
{
    public abstract class VariableOnlyAssignedAnalyzer : DiagnosticAnalyzer
    {
        //
        //		protected static bool TestOnlyAssigned(BaseSemanticModel ctx, AstNode rootNode, IVariable variable)
        //		{
        //			var assignment = false;
        //			var nonAssignment = false;
        //			foreach (var result in ctx.FindReferences(rootNode, variable)) {
        //				var node = result.Node;
        //				if (node is ParameterDeclaration)
        //					continue;
        //
        //				if (node is VariableInitializer) {
        //					if (!(node as VariableInitializer).Initializer.IsNull)
        //						assignment = true;
        //					continue;
        //				}
        //
        //				if (node is IdentifierExpression) {
        //					var parent = node.Parent;
        //					if (parent is AssignmentExpression) {
        //						if (((AssignmentExpression)parent).Left == node) {
        //							assignment = true;
        //							continue;
        //						}
        //					} else if (parent is UnaryOperatorExpression) {
        //						var op = ((UnaryOperatorExpression)parent).Operator;
        //						switch (op) {
        //							case UnaryOperatorType.Increment:
        //							case UnaryOperatorType.PostIncrement:
        //							case UnaryOperatorType.Decrement:
        //							case UnaryOperatorType.PostDecrement:
        //								assignment = true;
        //								if (!(parent.Parent is ExpressionStatement))
        //									nonAssignment = true;
        //								continue;
        //						}
        //					} else if (parent is DirectionExpression) {
        //						if (((DirectionExpression)parent).FieldDirection == FieldDirection.Out) {
        //							assignment = true;
        //							// Using dummy variables is necessary for ignoring
        //							// out-arguments, so we don't want to warn for those.
        //							nonAssignment = true;
        //							continue;
        //						}
        //					}
        //				}
        //				nonAssignment = true;
        //			}
        //			return assignment && !nonAssignment;
        //		}

    }
}
