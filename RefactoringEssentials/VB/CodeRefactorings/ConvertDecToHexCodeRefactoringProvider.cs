using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;
using Microsoft.CodeAnalysis.VisualBasic;

namespace RefactoringEssentials.VB.CodeRefactorings
{
    /// <summary>
    /// Convert a dec numer to hex. For example: 16 -> &H10
    /// </summary>

    [ExportCodeRefactoringProvider(LanguageNames.VisualBasic, Name = "Convert dec to hex.")]
    public class ConvertDecToHexCodeRefactoringProvider : CodeRefactoringProvider
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
            if (!token.IsKind(SyntaxKind.IntegerLiteralToken))
                return;
            var value = token.Value;
            if (!((value is int) || (value is long) || (value is short) || (value is sbyte) ||
                (value is uint) || (value is ulong) || (value is ushort) || (value is byte)))
            {
                return;
            }
            var literalValue = token.ToString();
            if (literalValue.StartsWith("&H", System.StringComparison.OrdinalIgnoreCase))
                return;
            context.RegisterRefactoring(CodeActionFactory.Create(token.Span, DiagnosticSeverity.Info, GettextCatalog.GetString("To hex"), t2 => Task.FromResult(PerformAction(document, root, token))));
        }

        static Document PerformAction(Document document, SyntaxNode root, SyntaxToken token)
        {
            var node = token.Parent as LiteralExpressionSyntax;

            var newRoot = root.ReplaceToken(
                token,
                SyntaxFactory.ParseToken(string.Format("&H{0:x}", token.Value))
                .WithLeadingTrivia(node.GetLeadingTrivia())
                .WithTrailingTrivia(node.GetTrailingTrivia())
            );
            return document.WithSyntaxRoot(newRoot);
        }
    }
}
