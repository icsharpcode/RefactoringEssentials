using System.Collections.Generic;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;

namespace RefactoringEssentials.CSharp.CodeRefactorings
{
    [ExportCodeRefactoringProvider(LanguageNames.CSharp, Name = "Split declaration list")]
    public class SplitDeclarationListCodeRefactoringProvider : CodeRefactoringProvider
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
            var declarator = token.Parent as VariableDeclaratorSyntax;
            if (declarator == null)
                return;
            var declaration = declarator.Parent as VariableDeclarationSyntax;
            if (declaration == null || declaration.Variables.Count <= 1)
                return;
            if (declaration.Parent.IsKind(SyntaxKind.ForStatement))
                return;
            context.RegisterRefactoring(
                CodeActionFactory.Create(
                    token.Span,
                    DiagnosticSeverity.Info,
                    GettextCatalog.GetString("Split declaration list"),
                    t2 =>
                    {
                        var newDeclarations = new List<SyntaxNode>();
                        foreach (var decl in declaration.Variables)
                        {
                            newDeclarations.Add(
                                declaration.Parent.ReplaceNode(
                                    declaration,
                                    declaration.WithVariables(SyntaxFactory.SeparatedList(new[] { decl }))
                                )
                            );
                        }

                        var newRoot = root.ReplaceNode(declaration.Parent, newDeclarations);
                        return Task.FromResult(document.WithSyntaxRoot(newRoot));
                    }
                )
            );
        }
    }
}
