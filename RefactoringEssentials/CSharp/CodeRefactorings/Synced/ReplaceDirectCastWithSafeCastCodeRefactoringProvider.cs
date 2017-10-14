using System.Linq;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;

namespace RefactoringEssentials.CSharp.CodeRefactorings
{
    /// <summary>
    /// Converts a cast expression to an 'as' expression
    /// </summary>

    [ExportCodeRefactoringProvider(LanguageNames.CSharp, Name = "Convert cast to 'as'.")]
    public class ReplaceDirectCastWithSafeCastCodeRefactoringProvider : CodeRefactoringProvider
    {
        public override async Task ComputeRefactoringsAsync(CodeRefactoringContext context)
        {
            var document = context.Document;
            if (document.Project.Solution.Workspace.Kind == WorkspaceKind.MiscellaneousFiles)
                return;
            var span = context.Span;
            if (!span.IsEmpty)
                return;
            var cancellationToken = context.CancellationToken;
            if (cancellationToken.IsCancellationRequested)
                return;
            var model = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);
            if (model.IsFromGeneratedCode(cancellationToken))
                return;
            var root = await model.SyntaxTree.GetRootAsync(cancellationToken).ConfigureAwait(false);
            var token = root.FindToken(span.Start);
            var castExpression = token.Parent.AncestorsAndSelf().OfType<CastExpressionSyntax>().FirstOrDefault();
            if (castExpression == null || castExpression.Expression.Span.Contains(span))
                return;
            var type = model.GetTypeInfo(castExpression.Type).Type;
            if (type == null || type.IsValueType && !type.IsNullableType())
                return;
            context.RegisterRefactoring(
                CodeActionFactory.Create(
                    token.Span,
                    DiagnosticSeverity.Info,
                    GettextCatalog.GetString("Convert cast to 'as'"),
                    t2 => Task.FromResult(PerformAction(document, root, castExpression))
                )
            );

            //			// only works on reference and nullable types
            //			var type = context.ResolveType (node.Type);
            //			var typeDef = type.GetDefinition ();
            //			var isNullable = typeDef != null && typeDef.KnownTypeCode == KnownTypeCode.NullableOfT;
            //			if (type.IsReferenceType == true || isNullable) {
            //				return new CodeAction (context.TranslateString ("Convert cast to 'as'"), script => {
            //					var asExpr = new AsExpression (node.Expression.Clone (), node.Type.Clone ());
            //					// if parent is an expression, clone parent and replace the case expression with asExpr,
            //					// so that we can inset parentheses
            //					var parentExpr = node.Parent.Clone () as Expression;
            //					if (parentExpr != null) {
            //						var castExpr = parentExpr.GetNodeContaining (node.StartLocation, node.EndLocation);
            //						castExpr.ReplaceWith (asExpr);
            //						parentExpr.AcceptVisitor (insertParentheses);
            //						script.Replace (node.Parent, parentExpr);
            //					} else {
            //						script.Replace (node, asExpr);
            //					}
            //				}, node);
            //			}
        }


        static Document PerformAction(Document document, SyntaxNode root, CastExpressionSyntax castExpr)
        {
            ExpressionSyntax nodeToReplace = castExpr;
            while (nodeToReplace.Parent is ParenthesizedExpressionSyntax)
            {
                nodeToReplace = (ExpressionSyntax)nodeToReplace.Parent;
            }

            // The syntax factory doesn't automatically add spaces around the operator !
            var token = SyntaxFactory.ParseToken(" as ");
            var asExpr = (ExpressionSyntax)SyntaxFactory.BinaryExpression(SyntaxKind.AsExpression, castExpr.Expression, token, castExpr.Type);
            if (nodeToReplace.Parent is ExpressionSyntax)
                asExpr = SyntaxFactory.ParenthesizedExpression(asExpr);
            var newRoot = root.ReplaceNode((SyntaxNode)nodeToReplace, asExpr);
            return document.WithSyntaxRoot(newRoot);
        }

    }
}
