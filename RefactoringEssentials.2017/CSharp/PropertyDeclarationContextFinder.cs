using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;
using System.Threading.Tasks;

namespace RefactoringEssentials.CSharp
{
    public class PropertyDeclarationContextFinder
    {
        public async Task<PropertyDeclarationContext> Find(CodeRefactoringContext context)
        {
            var document = context.Document;
            if (document.Project.Solution.Workspace.Kind == WorkspaceKind.MiscellaneousFiles)
                return null;
            var span = context.Span;
            if (!span.IsEmpty)
                return null;
            var cancellationToken = context.CancellationToken;
            if (cancellationToken.IsCancellationRequested)
                return null;
            var model = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);
            if (model.IsFromGeneratedCode(cancellationToken))
                return null;
            var root = (CompilationUnitSyntax)await model.SyntaxTree.GetRootAsync(cancellationToken).ConfigureAwait(false);

            var property = root.FindNode(span) as PropertyDeclarationSyntax;
            if (property == null || !property.Identifier.Span.Contains(span))
                return null;
            var enclosingTypeDeclaration = property.Ancestors().OfType<TypeDeclarationSyntax>().FirstOrDefault();
            if (enclosingTypeDeclaration == null || enclosingTypeDeclaration is InterfaceDeclarationSyntax)
                return null;

            return new PropertyDeclarationContext(document, root, property);
        }
    }
}
