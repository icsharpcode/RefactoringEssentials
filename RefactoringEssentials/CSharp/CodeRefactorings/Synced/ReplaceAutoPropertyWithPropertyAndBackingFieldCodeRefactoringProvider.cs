using System.Linq;
using System.Collections.Generic;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Formatting;

namespace RefactoringEssentials.CSharp.CodeRefactorings
{
    [ExportCodeRefactoringProvider(LanguageNames.CSharp, Name = "Replace auto-property with property that uses a backing field")]
    public class ReplaceAutoPropertyWithPropertyAndBackingFieldCodeRefactoringProvider : CodeRefactoringProvider
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

            if (property.AccessorList?.Accessors.Any(b => !IsEmptyOrNotImplemented(b.Body)) != false) //ignore properties with >=1 accessor body
                return;
            
            TypeDeclarationSyntax enclosingTypeDeclaration = property.Ancestors().OfType<TypeDeclarationSyntax>().FirstOrDefault();
            if(enclosingTypeDeclaration == null || enclosingTypeDeclaration is InterfaceDeclarationSyntax)
                return;
            context.RegisterRefactoring(
                CodeActionFactory.Create(
                    property.Identifier.Span,
                    DiagnosticSeverity.Info,
                    GettextCatalog.GetString("To property with backing field"),
                    t2 =>
                    {
                        string name = GetNameProposal(property.Identifier.ValueText, model, root);

                        //create our backing store
                        var backingStore = SyntaxFactory.FieldDeclaration(
                            SyntaxFactory.VariableDeclaration(
                                property.Type,
                                SyntaxFactory.SingletonSeparatedList<VariableDeclaratorSyntax>(SyntaxFactory.VariableDeclarator(name)))
                        ).WithModifiers(!property.Modifiers.Any(m => m.IsKind(SyntaxKind.StaticKeyword)) ?
                                        SyntaxFactory.TokenList() :
                                        SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.StaticKeyword)))
                        .WithAdditionalAnnotations(Formatter.Annotation);

                        //create our new property
                        var fieldExpression = name == "value" ?
                            (ExpressionSyntax)SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, SyntaxFactory.ThisExpression(), SyntaxFactory.IdentifierName("value")) :
                            SyntaxFactory.IdentifierName(name);

                        var newPropAnno = new SyntaxAnnotation();
                        var syntaxList = new SyntaxList<AccessorDeclarationSyntax>();
                        var hasSetter = property.AccessorList.Accessors.Any(acc => acc.IsKind(SyntaxKind.SetAccessorDeclaration));
                        var hasGetter = property.AccessorList.Accessors.Any(acc => acc.IsKind (SyntaxKind.GetAccessorDeclaration));

                        if (hasGetter)
                        {
                            var getBody = SyntaxFactory.Block(SyntaxFactory.ReturnStatement(fieldExpression));
                            var getter = SyntaxFactory.AccessorDeclaration(SyntaxKind.GetAccessorDeclaration, getBody);
							getter = getter.WithModifiers (property.AccessorList.Accessors.First (acc => acc.IsKind (SyntaxKind.GetAccessorDeclaration)).Modifiers);
                            syntaxList = syntaxList.Add(getter);
                            if (!hasSetter)
                                backingStore = backingStore.WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.ReadOnlyKeyword)));
                        }

                        if (hasSetter)
                        {
                            var setBody = SyntaxFactory.Block(SyntaxFactory.ExpressionStatement(
                                SyntaxFactory.AssignmentExpression(SyntaxKind.SimpleAssignmentExpression, fieldExpression,
                                                                   SyntaxFactory.IdentifierName("value"))));
                            var setter = SyntaxFactory.AccessorDeclaration(SyntaxKind.SetAccessorDeclaration, setBody);
							setter = setter.WithModifiers (property.AccessorList.Accessors.First (acc => acc.IsKind (SyntaxKind.SetAccessorDeclaration)).Modifiers);
                            syntaxList = syntaxList.Add(setter);
                        }
                        var newProperty = property.WithAccessorList(SyntaxFactory.AccessorList(syntaxList))
                            .WithAdditionalAnnotations(newPropAnno, Formatter.Annotation);

                        var newRoot = root.ReplaceNode((SyntaxNode)property, newProperty);
                        return Task.FromResult(document.WithSyntaxRoot(newRoot.InsertNodesBefore(newRoot.GetAnnotatedNodes(newPropAnno).First(), new List<SyntaxNode>() {
                            backingStore
                        })));
                    })
            );
        }

        static bool IsEmptyOrNotImplemented(BlockSyntax body)
        {
            if (body == null)
                return true;
            if (body.Statements.Count != 1)
                return false;
            return body.Statements[0] is ThrowStatementSyntax;
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

