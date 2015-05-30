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
    public class LinqQueryToFluentAction : SpecializedCodeRefactoringProvider<QueryExpressionSyntax>
    {
        protected override IEnumerable<CodeAction> GetActions(Document document, SemanticModel semanticModel, SyntaxNode root, TextSpan span, QueryExpressionSyntax node, CancellationToken cancellationToken)
        {
            yield break;
        }
        //		protected override CodeAction GetAction(SemanticModel context, QueryExpression node)
        //		{
        //			AstNode currentNode = node;
        //			for (;;) {
        //				QueryContinuationClause continuationParent = currentNode.Parent as QueryContinuationClause;
        //				if (continuationParent != null) {
        //					currentNode = continuationParent;
        //					continue;
        //				}
        //				QueryExpression exprParent = currentNode.Parent as QueryExpression;
        //				if (exprParent != null) {
        //					currentNode = exprParent;
        //					continue;
        //				}
        //
        //				break;
        //			}
        //
        //			node = (QueryExpression)currentNode;
        //
        //			return new CodeAction(context.TranslateString("Convert LINQ query to fluent syntax"),
        //			                      script => ConvertQueryToFluent(context, script, node),
        //			                      node);
        //		}
        //
        //		static void ConvertQueryToFluent(SemanticModel context, Script script, QueryExpression query) {
        //			IEnumerable<string> underscoreIdentifiers = GetNameProposals (context, query, "_");
        //			Expression newExpression = GetFluentFromQuery(query, underscoreIdentifiers);
        //			script.Replace (query, newExpression);
        //		}
        //
        //		static IEnumerable<string> GetNameProposals(SemanticModel context, QueryExpression query, string baseName)
        //		{
        //			var resolver = context.GetResolverStateBefore(query);
        //			int current = -1;
        //			string nameProposal;
        //			for (;;) {
        //				do {
        //					++current;
        //					nameProposal = baseName + (current == 0 ? string.Empty : current.ToString());
        //				} while (IsNameUsed (resolver, query, nameProposal));
        //
        //				yield return nameProposal;
        //			}
        //		}
        //
        //		static bool IsNameUsed(CSharpResolver resolver, QueryExpression query, string name)
        //		{
        //			if (resolver.ResolveSimpleName(name, new List<IType>()) is LocalResolveResult) {
        //				return true;
        //			}
        //
        //			if (query.Ancestors.OfType <VariableInitializer>().Any(variable => variable.Name == name)) {
        //				return true;
        //			}
        //
        //			if (query.Ancestors.OfType <BlockStatement>()
        //			    .Any(blockStatement => DeclaresLocalVariable(blockStatement, name))) {
        //
        //				return true;
        //			}
        //
        //			return query.Descendants.OfType<Identifier> ().Any (identifier => identifier.Name == name);
        //		}
        //
        //		static bool DeclaresLocalVariable(BlockStatement blockStatement, string name) {
        //			return blockStatement.Descendants.OfType <VariableInitializer>()
        //				.Any(variable => variable.Name == name &&
        //				     variable.Ancestors.OfType<BlockStatement>().First() == blockStatement);
        //		}
        //
        //		static Expression GetFluentFromQuery (QueryExpression query, IEnumerable<string> underscoreIdentifiers)
        //		{
        //			var queryExpander = new QueryExpressionExpander();
        //			var expandResult = queryExpander.ExpandQueryExpressions(query, underscoreIdentifiers);
        //
        //			return (Expression) expandResult.AstNode;
        //		}
    }
}

