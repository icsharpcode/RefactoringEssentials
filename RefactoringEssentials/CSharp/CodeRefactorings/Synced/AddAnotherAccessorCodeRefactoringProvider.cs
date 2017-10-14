using System.Linq;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Formatting;

namespace RefactoringEssentials.CSharp.CodeRefactorings
{
    /// <summary>
    /// Add another accessor to a property declaration that has only one.
    /// </summary>

    [ExportCodeRefactoringProvider(LanguageNames.CSharp, Name = "Adds another accessor")]
    public class AddAnotherAccessorCodeRefactoringProvider : CodeRefactoringProvider
    {

        public static BlockSyntax GetNotImplementedBlock()
        {
            return SyntaxFactory.Block(SyntaxFactory.ThrowStatement(SyntaxFactory.ObjectCreationExpression(
                SyntaxFactory.QualifiedName(SyntaxFactory.IdentifierName(@"System"), SyntaxFactory.IdentifierName(@"NotImplementedException")))
                .WithArgumentList(SyntaxFactory.ArgumentList())));
        }

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
            var propertyDeclaration = token.Parent as PropertyDeclarationSyntax;

            if (propertyDeclaration == null || propertyDeclaration.AccessorList == null)
                return;
            var accessors = propertyDeclaration.AccessorList.Accessors;

            //ignore if it has both accessors
            if (accessors.Count == 2)
                return;
            //ignore interfaces
            if (propertyDeclaration.Parent is InterfaceDeclarationSyntax)
                return;
            //if it has a getter, then we need a setter (we've checked for 2 accessors)
            bool needsSetter = accessors.Any(m => m.IsKind(SyntaxKind.GetAccessorDeclaration));

            context.RegisterRefactoring(CodeActionFactory.Create(token.Span, DiagnosticSeverity.Info, GettextCatalog.GetString("Add another accessor"), t2 =>
            {
                return Task.FromResult(PerformAction(document, model, root, propertyDeclaration, needsSetter));
            })
            );
        }

        Document PerformAction(Document document, SemanticModel model, SyntaxNode root, PropertyDeclarationSyntax propertyDeclaration, bool needsSetter)
        {
            AccessorDeclarationSyntax accessor = null;
            PropertyDeclarationSyntax newProp = null;
            if (needsSetter)
            {
                accessor = SyntaxFactory.AccessorDeclaration(SyntaxKind.SetAccessorDeclaration);

                var getter = propertyDeclaration.AccessorList.Accessors.FirstOrDefault(m => m.IsKind(SyntaxKind.GetAccessorDeclaration));
                if (getter == null)
                {
                    //get;
                    accessor = accessor.WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken));
                }
                else
                {
                    var getField = ReplacePropertyWithBackingFieldWithAutoPropertyCodeRefactoringProvider.ScanGetter(model, getter);
                    if (getField == null && getter.Body == null)
                    {
                        //get;
                        accessor = accessor.WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken)).WithTrailingTrivia(getter.GetTrailingTrivia());
                    }
                    else if (getField == null || getField.IsReadOnly)
                    {
                        //readonly or no field can be found
                        accessor = accessor.WithBody(GetNotImplementedBlock());
                    }
                    else
                    {
                        //now we add a 'field = value'.
                        accessor = accessor.WithBody(SyntaxFactory.Block(
                            SyntaxFactory.ExpressionStatement(
                                SyntaxFactory.AssignmentExpression(SyntaxKind.SimpleAssignmentExpression, SyntaxFactory.IdentifierName(getField.Name), SyntaxFactory.IdentifierName("value")))));
                    }
                }
                newProp = propertyDeclaration.WithAccessorList(propertyDeclaration.AccessorList.AddAccessors(accessor));
            }
            else
            {
                accessor = SyntaxFactory.AccessorDeclaration(SyntaxKind.GetAccessorDeclaration);

                var setter = propertyDeclaration.AccessorList.Accessors.FirstOrDefault(m => m.IsKind(SyntaxKind.SetAccessorDeclaration));
                var accessorDeclList = new SyntaxList<AccessorDeclarationSyntax>();
                if (setter == null)
                {
                    //set;
                    accessor = accessor.WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken));
                }
                else
                {
                    var setField = ReplacePropertyWithBackingFieldWithAutoPropertyCodeRefactoringProvider.ScanSetter(model, setter);
                    if (setField == null && setter.Body == null)
                    {
                        //set;
                        accessor = accessor.WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken)).WithTrailingTrivia(setter.GetTrailingTrivia());
                    }
                    else if (setField == null)
                    {
                        //no field can be found
                        accessor = accessor.WithBody(GetNotImplementedBlock());
                    }
                    else
                    {
                        //now we add a 'return field;'.
                        accessor = accessor.WithBody(SyntaxFactory.Block(
                            SyntaxFactory.ReturnStatement(SyntaxFactory.IdentifierName(setField.Name))));
                    }
                    accessorDeclList = accessorDeclList.Add(propertyDeclaration.AccessorList.Accessors.First(m => m.IsKind(SyntaxKind.SetAccessorDeclaration)));
                }
                accessorDeclList = accessorDeclList.Insert(0, accessor);
                var accessorList = SyntaxFactory.AccessorList(accessorDeclList);
                newProp = propertyDeclaration.WithAccessorList(accessorList);
            }
            var newRoot = root.ReplaceNode((SyntaxNode)propertyDeclaration, newProp).WithAdditionalAnnotations(Formatter.Annotation);
            return document.WithSyntaxRoot(newRoot);
        }
    }
}

