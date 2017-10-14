using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Formatting;

namespace RefactoringEssentials.Samples.CSharp
{
	/// <summary>
	/// Sample CodeRefactoringProvider for C#.
	/// </summary>
	// PLEASE UNCOMMENT THIS LINE TO REGISTER REFACTORING IN IDE.
	//[ExportCodeRefactoringProvider(LanguageNames.CSharp, Name = "Sample code refactoring")]
	public class SampleCodeRefactoringProvider : CodeRefactoringProvider
    {
        /*
            This sample code refactoring shows some basics of refactoring implementations
            and can serve as a simple template for own refactorings in RefactoringEssentials project.

            This example operates on interface declarations: It checks whether interface name begins with an "I"
            and if not, suggests to add an "I" prefix.
        */

        public override async Task ComputeRefactoringsAsync(CodeRefactoringContext context)
        {
            // Some general variables we need for further processing and some basic checks to exit as early as possible.
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

            // This is the initial code element on which the refactoring is to be offered.
            // Example: In code "public class SomeClass { ..." user presses Ctrl+. on the "class" keyword.
            // Then token will point to the "class" keyword token.
            var token = root.FindToken(span.Start);

            // Now we need to find out whether "token" is inside of a code element we need to refactor.
            // We want to refactor an interface name. Then we need to check if "token" is
            // a type name identifier and is inside of an interface declaration.
            var interfaceDeclaration = token.Parent as InterfaceDeclarationSyntax;
            if (interfaceDeclaration == null)
            {
                // Not cast-able to interface declaration -> exit, we are wrong here
                return;
            }

            // Is "token" itself the interface's name identifier? We want to offer the refactoring exactly on the name.
            // Note: Checking the syntax element's Kind is another way to detect a specific type, and often is even more detailed.
            if (!token.IsKind(SyntaxKind.IdentifierToken))
            {
                // Not the name identifier -> don't offer a refactoring here
                return;
            }

            // Get interface's name identifier and check its name
            var interfaceName = interfaceDeclaration.Identifier.ValueText;

            // Does interface name start with an "I"?
            // And if so: Is 2nd character a capital, too?
            // Then our refactoring is not needed anymore, name already has the desired format.
            if (interfaceName.StartsWith("I"))
            {
                if ((interfaceName.Length > 1) && char.IsUpper(interfaceName[1]))
                    return;
            }

            context.RegisterRefactoring(CodeActionFactory.Create(token.Span, DiagnosticSeverity.Info, GettextCatalog.GetString("Sample: Prepend with 'I'"), t2 =>
            {
                return Task.FromResult(PerformAction(document, model, root, interfaceDeclaration, interfaceName));
            }));
        }

        Document PerformAction(Document document, SemanticModel model, SyntaxNode root, InterfaceDeclarationSyntax interfaceDeclaration, string interfaceName)
        {
            // Applying a refactoring means replacing parts of syntax tree with our new elements.

            // Note: Every call to any .With...() method creates a completely new syntax tree object,
            // which is not part of current document anymore! Finally we create a completely new document
            // based on a new syntax tree containing the result of our refactoring.
            string newInterfaceName = "I" + interfaceName;
            var newRoot = root.ReplaceNode(
                interfaceDeclaration,
                interfaceDeclaration.WithIdentifier(SyntaxFactory.Identifier(newInterfaceName).WithAdditionalAnnotations(Formatter.Annotation)));
            return document.WithSyntaxRoot(newRoot);
        }
    }
}

