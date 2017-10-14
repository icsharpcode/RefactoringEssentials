using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;

namespace RefactoringEssentials.CSharp.CodeRefactorings
{
    /// <summary>
    /// Converts an 'as' expression to a cast expression
    /// </summary>

    [ExportCodeRefactoringProvider(LanguageNames.CSharp, Name = "Convert 'as' to cast.")]
    public class ReplaceSafeCastWithDirectCastCodeRefactoringProvider : CodeRefactoringProvider
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

            if (!token.IsKind(SyntaxKind.AsKeyword))
                return;
            var node = token.Parent as BinaryExpressionSyntax;

            context.RegisterRefactoring(
                CodeActionFactory.Create(token.Span, DiagnosticSeverity.Info, GettextCatalog.GetString("Convert 'as' to cast"), t2 => Task.FromResult(PerformAction(document, root, node)))
            );
        }

        static Document PerformAction(Document document, SyntaxNode root, BinaryExpressionSyntax bop)
        {
            var nodeToReplace = bop.IsParentKind(SyntaxKind.ParenthesizedExpression) ? bop.Parent : bop;
            var castExpr = (ExpressionSyntax)SyntaxFactory.CastExpression(bop.Right as TypeSyntax, CSharpUtil.AddParensIfRequired(bop.Left.WithoutLeadingTrivia().WithoutTrailingTrivia())).WithLeadingTrivia(bop.GetLeadingTrivia()).WithTrailingTrivia(bop.GetTrailingTrivia());

            var newRoot = root.ReplaceNode((SyntaxNode)nodeToReplace, castExpr);
            return document.WithSyntaxRoot(newRoot);
        }
    }
}
