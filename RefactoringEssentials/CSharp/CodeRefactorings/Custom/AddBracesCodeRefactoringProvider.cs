using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Formatting;

namespace RefactoringEssentials.CSharp.CodeRefactorings
{
    [ExportCodeRefactoringProvider(LanguageNames.CSharp, Name = "Add braces")]
    public class AddBracesCodeRefactoringProvider : CodeRefactoringProvider
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
            string keyword;
            StatementSyntax embeddedStatement;
            if (!RemoveBracesCodeRefactoringProvider.IsSpecialNode(token, out keyword, out embeddedStatement))
                return;
            if (embeddedStatement is BlockSyntax)
                return;
            context.RegisterRefactoring(
                CodeActionFactory.Create(
                    token.Span,
                    DiagnosticSeverity.Info,
                    string.Format(GettextCatalog.GetString("Add braces to '{0}'"), keyword),
                    t2 =>
                    {
                        var blockSyntax = SyntaxFactory.Block(embeddedStatement).WithAdditionalAnnotations(Formatter.Annotation);
                        var newRoot = root.ReplaceNode((SyntaxNode)embeddedStatement, blockSyntax);
                        return Task.FromResult(document.WithSyntaxRoot(newRoot));
                    }
                )
            );
        }


    }
}

