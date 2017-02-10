using System.Linq;
using System.Collections.Generic;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Simplification;
using Microsoft.CodeAnalysis.Formatting;

namespace RefactoringEssentials.CSharp.CodeRefactorings
{
    [ExportCodeRefactoringProvider(LanguageNames.CSharp, Name = "Convert auto-property to computed propertyy")]
    public class ConvertAutoPropertyToPropertyCodeRefactoringProvider : CodeRefactoringProvider
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

            if (property.AccessorList.Accessors.Any(b => b.Body != null)) //ignore properties with >=1 accessor body
                return;

            TypeDeclarationSyntax enclosingTypeDeclaration = property.Ancestors().OfType<TypeDeclarationSyntax>().FirstOrDefault();
            if(enclosingTypeDeclaration == null || enclosingTypeDeclaration is InterfaceDeclarationSyntax)
                return;
            context.RegisterRefactoring(
                CodeActionFactory.Create(
                    property.Identifier.Span,
                    DiagnosticSeverity.Info,
                    GettextCatalog.GetString("To computed property"),
                    t2 =>
                    {
                        //string name = GetNameProposal(property.Identifier.ValueText, model, root);

                        //create our new property
                        //var fieldExpression = name == "value" ? 
                        //	(ExpressionSyntax)SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, SyntaxFactory.ThisExpression(), SyntaxFactory.IdentifierName("value")) : 
                        //	SyntaxFactory.IdentifierName(name);

                        var getBody = SyntaxFactory.Block(
                            SyntaxFactory.ThrowStatement(
                                SyntaxFactory.ObjectCreationExpression(
                                    SyntaxFactory.QualifiedName(
                                        SyntaxFactory.IdentifierName(@"System"),
                                        SyntaxFactory.IdentifierName(@"NotImplementedException")
                                    ).WithoutAnnotations(Simplifier.Annotation)
                                ).WithArgumentList(SyntaxFactory.ArgumentList())
                            )
                        );

                        var getter = SyntaxFactory.AccessorDeclaration(SyntaxKind.GetAccessorDeclaration, getBody);
                        var setter = SyntaxFactory.AccessorDeclaration(SyntaxKind.SetAccessorDeclaration, getBody);

                        //var newPropAnno = new SyntaxAnnotation();
                        var newProperty = property.WithAccessorList(SyntaxFactory.AccessorList(new SyntaxList<AccessorDeclarationSyntax>().Add(getter).Add(setter)));
                        newProperty = newProperty.WithAdditionalAnnotations(Formatter.Annotation);
                        var newRoot = root.ReplaceNode((SyntaxNode)property, newProperty);
                        return Task.FromResult(document.WithSyntaxRoot(newRoot));
                    })
            );
        }

        public static string GetNameProposal(string name, SemanticModel model, SyntaxNode node)
        {
            string baseName = char.ToLower(name[0]) + name.Substring(1);
            var enclosingClass = node.AncestorsAndSelf().OfType<TypeDeclarationSyntax>().FirstOrDefault();
            if (enclosingClass == null)
                return baseName;

            INamedTypeSymbol typeSymbol = model.GetDeclaredSymbol(enclosingClass);
            IEnumerable<string> members = typeSymbol.MemberNames;

            string proposedName = null;
            int number = 0;
            do
            {
                proposedName = baseName;
                if (number != 0)
                {
                    proposedName = baseName + number.ToString();
                }
                number++;
            } while (members.Contains(proposedName));
            return proposedName;
        }
    }
}

