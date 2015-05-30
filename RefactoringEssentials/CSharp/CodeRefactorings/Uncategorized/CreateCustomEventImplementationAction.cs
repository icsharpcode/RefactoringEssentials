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
            if (variableDeclarator == null ||
                variableDeclarator.Parent == null ||
                variableDeclarator.Parent.Parent == null ||
                !variableDeclarator.Parent.Parent.IsKind(SyntaxKind.EventFieldDeclaration) ||
                !variableDeclarator.Identifier.Span.Contains(span))
                return;
            var eventDecl = (EventFieldDeclarationSyntax)variableDeclarator.Parent.Parent;

            context.RegisterRefactoring(
                CodeActionFactory.Create(
                    span,
                    DiagnosticSeverity.Info,
                    GettextCatalog.GetString("Create custom event implementation"),
                    t2 =>
                    {
                        //					var accessor = new Accessor
                        //					{
                        //						Body = new BlockStatement
                        //						{
                        //							new ThrowStatement(
                        //								new ObjectCreateExpression(context.CreateShortType("System", "NotImplementedException")))	
                        //						}
                        //					};
                        //					var e = new CustomEventDeclaration
                        //					{
                        //						Name = node.Name,
                        //						Modifiers = eventDecl.Modifiers,
                        //						ReturnType = eventDecl.ReturnType.Clone (),
                        //						AddAccessor = accessor,
                        //						RemoveAccessor = (Accessor)accessor.Clone(),
                        //					};
                        //					if (eventDecl.Variables.Count > 1) {
                        //						var newEventDecl = (EventDeclaration)eventDecl.Clone ();
                        //						newEventDecl.Variables.Remove (
                        //							newEventDecl.Variables.FirstOrNullObject (v => v.Name == node.Name));
                        //						script.InsertBefore (eventDecl, newEventDecl);
                        //					}
                        //					script.Replace (eventDecl, e);

                        var e = SyntaxFactory.EventDeclaration(
                            eventDecl.AttributeLists,
                            eventDecl.Modifiers,
                            eventDecl.Declaration.Type,
                            null,
                            variableDeclarator.Identifier,
                            SyntaxFactory.AccessorList(SyntaxFactory.List<AccessorDeclarationSyntax>(new[] {
                                SyntaxFactory.AccessorDeclaration(SyntaxKind.AddAccessorDeclaration, ToAbstractVirtualNonVirtualConversionCodeRefactoringProvider.CreateNotImplementedBody()),
                                SyntaxFactory.AccessorDeclaration(SyntaxKind.RemoveAccessorDeclaration, ToAbstractVirtualNonVirtualConversionCodeRefactoringProvider.CreateNotImplementedBody())
                            }))
                        );

                        SyntaxNode newRoot;

                        if (eventDecl.Declaration.Variables.Count > 1)
                        {
                            newRoot = root.ReplaceNode((SyntaxNode)
                                eventDecl,
                                new SyntaxNode[] {
                                    eventDecl.WithDeclaration(
                                            eventDecl.Declaration.WithVariables(
                                                SyntaxFactory.SeparatedList<VariableDeclaratorSyntax>(
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
                            newRoot = root.ReplaceNode((SyntaxNode)eventDecl, e.WithAdditionalAnnotations(Formatter.Annotation, Simplifier.Annotation));
                        }

                        return Task.FromResult(document.WithSyntaxRoot(newRoot));
                    })
            );
        }
    }
}
