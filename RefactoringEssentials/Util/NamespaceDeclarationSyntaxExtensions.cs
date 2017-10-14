using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace RefactoringEssentials
{
#if NR6
    public
#endif
    static class NamespaceDeclarationSyntaxExtensions
    {
        public static NamespaceDeclarationSyntax AddUsingDirectives(
            this NamespaceDeclarationSyntax namespaceDeclaration,
            IList<UsingDirectiveSyntax> usingDirectives,
            bool placeSystemNamespaceFirst,
            params SyntaxAnnotation[] annotations)
        {
            if (!usingDirectives.Any())
            {
                return namespaceDeclaration;
            }

            var specialCaseSystem = placeSystemNamespaceFirst;
            var comparer = specialCaseSystem
                ? UsingsAndExternAliasesDirectiveComparer.SystemFirstInstance
                : UsingsAndExternAliasesDirectiveComparer.NormalInstance;

            var usings = new List<UsingDirectiveSyntax>();
            usings.AddRange(namespaceDeclaration.Usings);
            usings.AddRange(usingDirectives);

            if (namespaceDeclaration.Usings.IsSorted(comparer))
            {
                usings.Sort(comparer);
            }

            usings = usings.Select(u => u.WithAdditionalAnnotations(annotations)).ToList();
            var newNamespace = namespaceDeclaration.WithUsings(SyntaxFactory.List(usings));

            return newNamespace;
        }
    }
}
