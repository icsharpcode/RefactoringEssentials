using System.Linq;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.FindSymbols;

namespace RefactoringEssentials.CSharp.CodeRefactorings
{
    [ExportCodeRefactoringProvider(LanguageNames.CSharp, Name = "'catch (Exception)' to 'catch'")]
    public class RemoveCatchExceptionCodeRefactoringProvider : CodeRefactoringProvider
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

            var catchClause = root.FindNode(span) as CatchClauseSyntax;
            if (catchClause == null || catchClause.Declaration == null || catchClause.Declaration.IsMissing)
                return;

            if (!catchClause.Declaration.Identifier.IsMissing)
            {
                var sym = model.GetDeclaredSymbol(catchClause.Declaration);
                var refs = await SymbolFinder.FindReferencesAsync(sym, document.Project.Solution, cancellationToken).ConfigureAwait(false);
                foreach (var r in refs)
                {
                    if (r.Definition != sym)
                        continue;
                    if (r.Locations.Count() > 0)
                        return;
                }
            }

            context.RegisterRefactoring(
                CodeActionFactory.Create(
                    span,
                    DiagnosticSeverity.Info,
                    GettextCatalog.GetString("To 'catch'"),
                    t2 =>
                    {
                        var newRoot = root.ReplaceNode((SyntaxNode)catchClause,
                            catchClause
                                .WithDeclaration(null)
                                .WithLeadingTrivia(catchClause.GetLeadingTrivia())
                                .WithCatchKeyword(catchClause.CatchKeyword.WithTrailingTrivia(catchClause.Declaration.GetTrailingTrivia())));
                        return Task.FromResult(document.WithSyntaxRoot(newRoot));
                    }
                )
            );
        }
    }
}
