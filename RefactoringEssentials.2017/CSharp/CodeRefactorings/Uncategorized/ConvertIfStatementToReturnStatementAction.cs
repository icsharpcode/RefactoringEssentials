using System.Linq;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Formatting;

namespace RefactoringEssentials.CSharp.CodeRefactorings
{
	[ExportCodeRefactoringProvider(LanguageNames.CSharp, Name = "Convert 'if' to 'return'")]
    public class ConvertIfStatementToReturnStatementAction : CodeRefactoringProvider
    {
        public override async Task ComputeRefactoringsAsync(CodeRefactoringContext context)
        {
            var document = context.Document;
            var span = context.Span;
            var cancellationToken = context.CancellationToken;
            var model = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);
            if (model.IsFromGeneratedCode(cancellationToken))
                return;
            var root = await model.SyntaxTree.GetRootAsync(cancellationToken).ConfigureAwait(false);

            var node = root.FindNode(span) as IfStatementSyntax;
            if (node == null)
                return;

            ExpressionSyntax condition;
            ReturnStatementSyntax return1, return2, rs;
            if (!ConvertIfStatementToReturnStatementAction.GetMatch(node, out condition, out return1, out return2, out rs))
                return;

            context.RegisterRefactoring(
                CodeActionFactory.Create(
                    span,
                    DiagnosticSeverity.Info,
                    GettextCatalog.GetString("Replace with 'return'"),
                    t2 =>
                    {
                        var newRoot = root.ReplaceNode((SyntaxNode)node, SyntaxFactory.ReturnStatement(CreateCondition(condition, return1, return2)).WithAdditionalAnnotations(Formatter.Annotation).WithLeadingTrivia(node.GetLeadingTrivia()));
                        if (rs != null)
                        {
                            var retToRemove = newRoot.DescendantNodes().OfType<ReturnStatementSyntax>().FirstOrDefault(r => r.IsEquivalentTo(rs));
                            newRoot = newRoot.RemoveNode(retToRemove, SyntaxRemoveOptions.KeepNoTrivia);
                        }

                        return Task.FromResult(document.WithSyntaxRoot(newRoot));
                    })
            );
        }

        static bool GetMatch(IfStatementSyntax node, out ExpressionSyntax c, out ReturnStatementSyntax e1, out ReturnStatementSyntax e2, out ReturnStatementSyntax rs)
        {
            rs = e1 = e2 = null;
            c = node.Condition;
            //attempt to match if(condition) return else return
            e1 = ConvertIfStatementToNullCoalescingExpressionAction.GetSimpleStatement(node.Statement) as ReturnStatementSyntax;
            if (e1 == null)
                return false;
            e2 = node.Else != null ? ConvertIfStatementToNullCoalescingExpressionAction.GetSimpleStatement(node.Else.Statement) as ReturnStatementSyntax : null;
            //match
            if (e1 != null && e2 != null)
            {
                return true;
            }

            //attempt to match if(condition) return; return
            if (e1 != null)
            {
                var parentBlock = node.Parent as BlockSyntax;
                if (parentBlock == null)
                    return false;
                var index = parentBlock.Statements.IndexOf(node);
                if (index + 1 < parentBlock.Statements.Count)
                {
                    rs = parentBlock.Statements[index + 1] as ReturnStatementSyntax;
                }

                if (rs != null)
                {
                    e2 = rs;
                    return true;
                }
            }
            return false;
        }

        static ExpressionSyntax CreateCondition(ExpressionSyntax c, ReturnStatementSyntax e1, ReturnStatementSyntax e2)
        {
            return e1.Expression.IsKind(SyntaxKind.TrueLiteralExpression) && e2.Expression.IsKind(SyntaxKind.FalseLiteralExpression) ?
                    c : SyntaxFactory.ConditionalExpression(c, e1.Expression, e2.Expression);
        }
    }
}

