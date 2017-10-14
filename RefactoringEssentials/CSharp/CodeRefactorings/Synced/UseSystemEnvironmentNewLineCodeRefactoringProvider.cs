using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Simplification;

namespace RefactoringEssentials.CSharp.CodeRefactorings
{
    [ExportCodeRefactoringProvider(LanguageNames.CSharp, Name = "Use System.Environment.NewLine")]
    public class UseSystemEnvironmentNewLineCodeRefactoringProvider : CodeRefactoringProvider
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
            if ((!token.IsKind(SyntaxKind.StringLiteralToken) || !token.IsKind(SyntaxKind.CharacterLiteralToken)))
            {
                var tokenValue = (token.Value ?? "").ToString();
                if (tokenValue != "\r" && tokenValue != "\n" && tokenValue != "\r\n")
                    return;
            }

            context.RegisterRefactoring(
                CodeActionFactory.Create(
                    token.Span,
                    DiagnosticSeverity.Info,
                    GettextCatalog.GetString("Use 'System.Environment.NewLine'"),
                    t2 =>
                    {
                        var newRoot = root.ReplaceNode((SyntaxNode)token.Parent, SyntaxFactory.ParseExpression("System.Environment.NewLine").WithAdditionalAnnotations(Simplifier.Annotation));
                        return Task.FromResult(document.WithSyntaxRoot(newRoot));
                    }
                )
            );
        }
    }
}
