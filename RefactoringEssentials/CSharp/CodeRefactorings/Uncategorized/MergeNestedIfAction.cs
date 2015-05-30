using System.Threading;
using System.Collections.Generic;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.Text;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace RefactoringEssentials.CSharp.CodeRefactorings
{
    [ExportCodeRefactoringProvider(LanguageNames.CSharp, Name = "Merge nested 'if'")]
    [NotPortedYet]
    public class MergeNestedIfAction : SpecializedCodeRefactoringProvider<IfStatementSyntax>
    {
        protected override IEnumerable<CodeAction> GetActions(Document document, SemanticModel semanticModel, SyntaxNode root, TextSpan span, IfStatementSyntax node, CancellationToken cancellationToken)
        {
            yield break;
        }
        //		static readonly InsertParenthesesVisitor insertParenthesesVisitor = new InsertParenthesesVisitor ();
        //
        //		protected override CodeAction GetAction (SemanticModel context, IfElseStatement node)
        //		{
        //			if (!node.IfToken.Contains (context.Location))
        //				return null;
        //
        //			IfElseStatement outerIfStatement;
        //			IfElseStatement innerIfStatement = GetInnerIfStatement (node);
        //			if (innerIfStatement != null) {
        //				if (!innerIfStatement.FalseStatement.IsNull)
        //					return null;
        //				outerIfStatement = node;
        //			} else {
        //				outerIfStatement = GetOuterIfStatement (node);
        //				if (outerIfStatement == null || !outerIfStatement.FalseStatement.IsNull)
        //					return null;
        //				innerIfStatement = node;
        //			}
        //
        //			return new CodeAction (context.TranslateString ("Merge nested 'if's"),
        //				script =>
        //				{
        //					var mergedIfStatement = new IfElseStatement
        //					{
        //						Condition = new BinaryOperatorExpression (outerIfStatement.Condition.Clone (),
        //																  BinaryOperatorType.ConditionalAnd, 
        //																  innerIfStatement.Condition.Clone ()),
        //						TrueStatement = innerIfStatement.TrueStatement.Clone ()
        //					};
        //					mergedIfStatement.Condition.AcceptVisitor (insertParenthesesVisitor);
        //					script.Replace (outerIfStatement, mergedIfStatement);
        //				}, node);
        //		}
        //
        //		static IfElseStatement GetOuterIfStatement (IfElseStatement node)
        //		{
        //			var outerIf = node.Parent as IfElseStatement;
        //			if (outerIf != null)
        //				return outerIf;
        //
        //			var blockStatement = node.Parent as BlockStatement;
        //			while (blockStatement != null && blockStatement.Statements.Count == 1) {
        //				outerIf = blockStatement.Parent as IfElseStatement;
        //				if (outerIf != null)
        //					return outerIf;
        //				blockStatement = blockStatement.Parent as BlockStatement;
        //			}
        //
        //			return null;
        //		}
        //
        //		static IfElseStatement GetInnerIfStatement (IfElseStatement node)
        //		{
        //			if (!node.FalseStatement.IsNull)
        //				return null;
        //
        //			var innerIf = node.TrueStatement as IfElseStatement;
        //			if (innerIf != null)
        //				return innerIf;
        //
        //			var blockStatement = node.TrueStatement as BlockStatement;
        //			while (blockStatement != null && blockStatement.Statements.Count == 1) {
        //				innerIf = blockStatement.Statements.First () as IfElseStatement;
        //				if (innerIf != null)
        //					return innerIf;
        //				blockStatement = blockStatement.Statements.First () as BlockStatement;
        //			}
        //
        //			return null;
        //		}
    }
}
