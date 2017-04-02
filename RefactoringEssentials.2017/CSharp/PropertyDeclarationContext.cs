using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Diagnostics.Contracts;

namespace RefactoringEssentials.CSharp
{
    public class PropertyDeclarationContext
    {
        public PropertyDeclarationSyntax Property { get; }
        public CompilationUnitSyntax Root { get; }
        public Document Document { get; }

        public PropertyDeclarationContext(Document document, CompilationUnitSyntax root, PropertyDeclarationSyntax property)
        {
            Contract.Requires(document != null);
            Contract.Requires(property != null);
            Contract.Requires(root != null);

            Document = document;
            Property = property;
            Root = root;
        }
    }
}
