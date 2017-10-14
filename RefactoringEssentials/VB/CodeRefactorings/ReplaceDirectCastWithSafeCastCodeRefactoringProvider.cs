using System.Linq;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;
using Microsoft.CodeAnalysis.VisualBasic;

namespace RefactoringEssentials.VB.CodeRefactorings
{
    /// <summary>
    /// Converts a cast expression to a 'TryCast' expression
    /// </summary>
    [ExportCodeRefactoringProvider(LanguageNames.VisualBasic, Name = "Convert cast to 'TryCast'.")]
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
            if (castExpression == null || castExpression.IsKind(SyntaxKind.TryCastExpression) || castExpression.Expression.Span.Contains(span))
                return;
            var type = model.GetTypeInfo(castExpression.Type).Type;
            if (type == null || type.IsValueType && !type.IsNullableType())
                return;
            context.RegisterRefactoring(
                CodeActionFactory.Create(
                    token.Span,
                    DiagnosticSeverity.Info,
                    GettextCatalog.GetString("Convert cast to 'TryCast'"),
                    t2 => Task.FromResult(PerformAction(document, root, castExpression))
                )
            );
        }


        static Document PerformAction(Document document, SyntaxNode root, CastExpressionSyntax castExpr)
        {
            ExpressionSyntax nodeToReplace = castExpr.ReplaceToken(castExpr.Keyword, SyntaxFactory.Token(SyntaxKind.TryCastKeyword));
            var newRoot = root.ReplaceNode(castExpr, nodeToReplace);
            return document.WithSyntaxRoot(newRoot);
        }
    }
}
