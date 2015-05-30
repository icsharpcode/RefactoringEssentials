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
    [ExportCodeRefactoringProvider(LanguageNames.CSharp, Name = "Create changed event for property")]
    public class CreateChangedEventCodeRefactoringProvider : CodeRefactoringProvider
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
            var property = root.FindNode(span) as PropertyDeclarationSyntax;

            if (property == null || !property.Identifier.Span.Contains(span))
                return;

            var field = ReplacePropertyWithBackingFieldWithAutoPropertyCodeRefactoringProvider.GetBackingField(model, property);
            if (field == null)
                return;
            var type = property.Parent as TypeDeclarationSyntax;
            if (type == null)
                return;

            var resolvedType = model.Compilation.GetTypeSymbol("System", "EventHandler", 0, cancellationToken);
            if (resolvedType == null)
                return;

            context.RegisterRefactoring(
                CodeActionFactory.Create(
                    span,
                    DiagnosticSeverity.Info,
                    GettextCatalog.GetString("Create changed event"),
                    t2 =>
                    {
                        var eventDeclaration = CreateChangedEventDeclaration(property);
                        var methodDeclaration = CreateEventInvocatorCodeRefactoringProvider.CreateEventInvocator(
                            document,
                            type.Identifier.ToString(),
                            type.Modifiers.Any(m => m.IsKind(SyntaxKind.SealedKeyword)),
                            eventDeclaration.Modifiers.Any(m => m.IsKind(SyntaxKind.StaticKeyword)),
                            eventDeclaration.Declaration.Variables.First().Identifier.ToString(),
                            resolvedType.GetDelegateInvokeMethod(),
                            false
                        );
                        var invocation = SyntaxFactory.ExpressionStatement(SyntaxFactory.InvocationExpression(
                            SyntaxFactory.IdentifierName(methodDeclaration.Identifier.ToString()),
                            SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>(new[] { SyntaxFactory.Argument(SyntaxFactory.ParseExpression("System.EventArgs.Empty")) }))
                        ));

                        var marker = new SyntaxAnnotation();
                        var newRoot = root.ReplaceNode(property, property.WithAdditionalAnnotations(marker));

                        newRoot = newRoot.InsertNodesAfter(newRoot.GetAnnotatedNodes(marker).First(), new SyntaxNode[] {
                            methodDeclaration.WithAdditionalAnnotations(Formatter.Annotation),
                            eventDeclaration.WithAdditionalAnnotations(Formatter.Annotation)
                        });

                        newRoot = newRoot.InsertNodesAfter(newRoot.GetAnnotatedNodes(marker).OfType<PropertyDeclarationSyntax>().First().AccessorList.Accessors.First(a => a.IsKind(SyntaxKind.SetAccessorDeclaration)).Body.Statements.Last(),
                            new[] { invocation.WithAdditionalAnnotations(Formatter.Annotation, Simplifier.Annotation) }
                        );

                        return Task.FromResult(document.WithSyntaxRoot(newRoot));
                    })
            );
        }


        static EventFieldDeclarationSyntax CreateChangedEventDeclaration(PropertyDeclarationSyntax propertyDeclaration)
        {
            return SyntaxFactory.EventFieldDeclaration(
                SyntaxFactory.List<AttributeListSyntax>(),
                propertyDeclaration.Modifiers.Any(m => m.IsKind(SyntaxKind.StaticKeyword)) ?
                SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword), SyntaxFactory.Token(SyntaxKind.StaticKeyword)) :
                    SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword)),
                SyntaxFactory.VariableDeclaration(
                    SyntaxFactory.ParseTypeName("System.EventHandler").WithAdditionalAnnotations(Simplifier.Annotation),
                    SyntaxFactory.SeparatedList<VariableDeclaratorSyntax>(new[] {
                        SyntaxFactory.VariableDeclarator(propertyDeclaration.Identifier + "Changed")
                    }
                    )
                )
            );
        }
    }
}

