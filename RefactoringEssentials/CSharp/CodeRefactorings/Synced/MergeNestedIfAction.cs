using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.Text;

namespace RefactoringEssentials.CSharp.CodeRefactorings
{
	[ExportCodeRefactoringProvider(LanguageNames.CSharp, Name = "Merge nested 'if'")]    
    public class MergeNestedIfAction : SpecializedCodeRefactoringProvider<IfStatementSyntax>
    {
        protected override IEnumerable<CodeAction> GetActions(Document document, SemanticModel semanticModel, SyntaxNode root, TextSpan span, IfStatementSyntax node, CancellationToken cancellationToken)
        {
            var outerIf = GetOuterIf(node);

            if (outerIf != null)
            {
                return HandleInnerIf(span, document, root, node, outerIf);
            }

            var innerIf = GetInnerIf(node.Statement);

            if (innerIf != null)
            {
                return HandleInnerIf(span, document, root, innerIf, node);
            }

            return Enumerable.Empty<CodeAction>();
        }

        private IEnumerable<CodeAction> HandleInnerIf(TextSpan span, Document document, SyntaxNode root, IfStatementSyntax innerIf, IfStatementSyntax outerIf)
        {
            if (innerIf.Else != null || outerIf.Else != null)
            {
                yield break;
            }

            yield return CodeActionFactory.Create(span, DiagnosticSeverity.Info, "Merge nested 'if'", ct =>
            {
                var innerCondition = SyntaxFactory.ParenthesizedExpression(innerIf.Condition);
                var outerCondition = SyntaxFactory.ParenthesizedExpression(outerIf.Condition);

                var newCondition = SyntaxFactory.BinaryExpression(SyntaxKind.LogicalAndExpression, outerCondition, innerCondition);

                if (((ParenthesizedExpressionSyntax)newCondition.Right).CanRemoveParentheses())
                {
                    newCondition = newCondition.ReplaceNode(newCondition.Right, innerCondition.Expression);
                }

                if (((ParenthesizedExpressionSyntax)newCondition.Left).CanRemoveParentheses())
                {
                    newCondition = newCondition.ReplaceNode(newCondition.Left, outerCondition.Expression);
                }

                var newIf = SyntaxFactory.IfStatement(newCondition, innerIf.Statement)
                    .WithAdditionalAnnotations(Formatter.Annotation);

                var newRoot = root.ReplaceNode(outerIf, newIf);
                
                return Task.FromResult(document.WithSyntaxRoot(newRoot));
            });
        }

        private IfStatementSyntax GetInnerIf(SyntaxNode node)
        {
            var @if = node as IfStatementSyntax;

            if (@if != null)
            {
                return @if;
            }

            var block = node as BlockSyntax;

            if (block != null && block.Statements.Count == 1)
            {
                return GetInnerIf(block.Statements[0]);
            }

            return null;
        }

        private static IfStatementSyntax GetOuterIf(SyntaxNode node)
        {
            var parentIf = node.Parent as IfStatementSyntax;

            if (parentIf != null)
            {
                return parentIf;                
            }

            var parentBlock = node.Parent as BlockSyntax;

            if (parentBlock != null && parentBlock.Statements.Count == 1)
            {
                return GetOuterIf(parentBlock);
            }

            return null;
        }     
    }
}
