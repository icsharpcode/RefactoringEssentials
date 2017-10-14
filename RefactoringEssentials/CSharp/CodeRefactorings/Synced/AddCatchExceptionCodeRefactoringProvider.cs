using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Simplification;
using Microsoft.CodeAnalysis.Formatting;

namespace RefactoringEssentials.CSharp.CodeRefactorings
{
    [ExportCodeRefactoringProvider(LanguageNames.CSharp, Name = "'catch' to 'catch (Exception)'")]
    public class AddCatchExceptionCodeRefactoringProvider : CodeRefactoringProvider
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

            var exceptionType = (await document.Project.GetCompilationAsync().ConfigureAwait(false)).GetTypeByMetadataName("System.Exception");

            var catchClause = root.FindNode(span) as CatchClauseSyntax;
            if (catchClause == null || catchClause.Declaration != null)
                return;
            var newIdent = SyntaxFactory.IdentifierName(exceptionType.ToMinimalDisplayString(model, span.Start));
            context.RegisterRefactoring(
                CodeActionFactory.Create(
                    span,
                    DiagnosticSeverity.Info,
                    GettextCatalog.GetString("To 'catch (Exception)'"),
                    t2 =>
                    {
                        var newRoot = root.ReplaceNode((SyntaxNode)catchClause, catchClause
                            .WithCatchKeyword(catchClause.CatchKeyword.WithTrailingTrivia(SyntaxFactory.Whitespace(" ")))
                            .WithDeclaration(SyntaxFactory.CatchDeclaration(newIdent, SyntaxFactory.Identifier("e")).WithTrailingTrivia(catchClause.CatchKeyword.TrailingTrivia))
                            .WithAdditionalAnnotations(Formatter.Annotation, Simplifier.Annotation));
                        return Task.FromResult(document.WithSyntaxRoot(newRoot));
                    }
                )
            );
        }
    }
}
