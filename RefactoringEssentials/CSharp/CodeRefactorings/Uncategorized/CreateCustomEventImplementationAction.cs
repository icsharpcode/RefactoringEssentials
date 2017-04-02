using System.Linq;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Simplification;
using Microsoft.CodeAnalysis.Formatting;

namespace RefactoringEssentials.CSharp.CodeRefactorings
{
    [ExportCodeRefactoringProvider(LanguageNames.CSharp, Name = "Create custom event implementation")]
    public class CreateCustomEventImplementationAction : CodeRefactoringProvider
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
            var variableDeclarator = root.FindNode(span) as VariableDeclaratorSyntax;
            var eventDecl = variableDeclarator?.Parent?.Parent as EventFieldDeclarationSyntax;

            if (variableDeclarator == null || eventDecl == null || !variableDeclarator.Identifier.Span.Contains(span))
                return;
            if (eventDecl.Parent is InterfaceDeclarationSyntax)
                return;

            context.RegisterRefactoring(
                CodeActionFactory.Create(
                    span,
                    DiagnosticSeverity.Info,
                    GettextCatalog.GetString("Create custom event implementation"),
                    t2 =>
                    {
                        var e = SyntaxFactory.EventDeclaration(
                            eventDecl.AttributeLists,
                            eventDecl.Modifiers,
                            eventDecl.Declaration.Type,
                            null,
                            variableDeclarator.Identifier,
                            SyntaxFactory.AccessorList(SyntaxFactory.List(new[] {
                                SyntaxFactory.AccessorDeclaration(SyntaxKind.AddAccessorDeclaration, ToAbstractVirtualNonVirtualConversionCodeRefactoringProvider.CreateNotImplementedBody()),
                                SyntaxFactory.AccessorDeclaration(SyntaxKind.RemoveAccessorDeclaration, ToAbstractVirtualNonVirtualConversionCodeRefactoringProvider.CreateNotImplementedBody())
                            }))
                        );

                        SyntaxNode newRoot;

                        if (eventDecl.Declaration.Variables.Count > 1)
                        {
                            newRoot = root.ReplaceNode(
                                eventDecl,
                                new SyntaxNode[] {
                                    eventDecl.WithDeclaration(
                                            eventDecl.Declaration.WithVariables(
                                                SyntaxFactory.SeparatedList(
                                                    eventDecl.Declaration.Variables.Where(decl => decl != variableDeclarator)
                                                )
                                            )
                                    ).WithAdditionalAnnotations(Formatter.Annotation),
                                    e.WithAdditionalAnnotations(Formatter.Annotation, Simplifier.Annotation)
                                }
                            );
                        }
                        else
                        {
                            newRoot = root.ReplaceNode(eventDecl, e.WithAdditionalAnnotations(Formatter.Annotation, Simplifier.Annotation));
                        }

                        return Task.FromResult(document.WithSyntaxRoot(newRoot));
                    })
            );
        }
    }
}
