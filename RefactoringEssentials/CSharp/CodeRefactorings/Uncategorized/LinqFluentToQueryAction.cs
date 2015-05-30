using System.Threading;
using System.Collections.Generic;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.Text;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace RefactoringEssentials.CSharp.CodeRefactorings
{
    [ExportCodeRefactoringProvider(LanguageNames.CSharp, Name = "Convert LINQ query to fluent syntax")]
    [NotPortedYet]
    public class LinqFluentToQueryAction : SpecializedCodeRefactoringProvider<InvocationExpressionSyntax>
    {
        //		static readonly List<string> LinqQueryMethods = new List<string>() {
        //			"Select", "SelectMany", "GroupBy",
        //			"OrderBy", "OrderByDescending", "ThenBy", "ThenByDescending",
        //			"Where", "Cast",
        //			"Join", "GroupJoin"
        //		};
        //
        protected override IEnumerable<CodeAction> GetActions(Document document, SemanticModel semanticModel, SyntaxNode root, TextSpan span, InvocationExpressionSyntax node, CancellationToken cancellationToken)
        {
            yield break;
        }
        //		protected override CodeAction GetAction(SemanticModel context, InvocationExpression node)
        //		{
        //			if (!IsLinqMethodInvocation(node)) {
        //				return null;
        //			}
        //
        //			while (node.Parent is MemberReferenceExpression) {
        //				var parentInvocation = ((MemberReferenceExpression)node.Parent).Parent;
        //				if (!IsLinqMethodInvocation(parentInvocation)) {
        //					break;
        //				}
        //				node = (InvocationExpression) parentInvocation;
        //			}
        //
        //			IntroduceQueryExpressions queryExpressionIntroducer = new IntroduceQueryExpressions();
        //			CombineQueryExpressions queryExpressionCombiner = new CombineQueryExpressions();
        //			Expression newNode = queryExpressionIntroducer.ConvertFluentToQuery(node);
        //
        //			queryExpressionCombiner.CombineQuery(newNode);
        //
        //			if (!(newNode is QueryExpression)) {
        //				return null;
        //			}
        //
        //			return new CodeAction(context.TranslateString("Convert to query syntax"), script => {
        //				List<string> newNames = new List<string>();
        //				var identifiers = newNode.Descendants.OfType<Identifier>().ToList();
        //				foreach (var identifier in identifiers.Where(id => id.Name.StartsWith("<>")))
        //				{
        //					int nameId = int.Parse(identifier.Name.Substring(2)) - 1;
        //					while (newNames.Count <= nameId) {
        //						//Find new name
        //
        //						//This might skip some legitimate names, but that's not a real problem.
        //						var topMostBlock = node.AncestorsAndSelf.OfType<BlockStatement>().Last();
        //						var variableDeclarations = topMostBlock.Descendants.OfType<VariableDeclarationStatement>();
        //						var declaredNames = variableDeclarations.SelectMany(variableDeclaration => variableDeclaration.Variables).Select(variable => variable.Name).ToList();
        //
        //						int currentId = 1;
        //						while (identifiers.Any(id => id.Name == "_" + currentId) || declaredNames.Contains("_" + currentId)) {
        //							++currentId;
        //						}
        //
        //						newNames.Add("_" + currentId);
        //					}
        //
        //					identifier.Name = newNames[nameId];
        //				}
        //
        //				if (NeedsParenthesis(node)) {
        //					newNode = new ParenthesizedExpression(newNode);
        //				}
        //
        //				script.Replace(node, newNode);
        //			}, node);
        //		}
        //
        //		bool NeedsParenthesis(AstNode node)
        //		{
        //			AstNode parent = node.Parent;
        //			if (parent is BinaryOperatorExpression)
        //				return true;
        //
        //			UnaryOperatorExpression unaryExpression = parent as UnaryOperatorExpression;
        //			if (unaryExpression != null) {
        //				return unaryExpression.Operator == UnaryOperatorType.PostIncrement ||
        //					unaryExpression.Operator == UnaryOperatorType.PostDecrement;
        //			}
        //
        //			return parent is MemberReferenceExpression ||
        //				parent is InvocationExpression;
        //		}
        //
        //		bool IsLinqMethodInvocation(AstNode node)
        //		{
        //			var invocation = node as InvocationExpression;
        //			return invocation != null && IsLinqMethodInvocation(invocation);
        //		}
        //
        //		bool IsLinqMethodInvocation(InvocationExpression node)
        //		{
        //			var target = node.Target as MemberReferenceExpression;
        //			return target != null && IsLinqMethod(target);
        //		}
        //
        //		bool IsLinqMethod(MemberReferenceExpression node)
        //		{
        //			return LinqQueryMethods.Contains(node.MemberName);
        //		}
    }
}

