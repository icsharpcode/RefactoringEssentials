using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using CS = Microsoft.CodeAnalysis.CSharp;

namespace RefactoringEssentials.VB.Converter
{
    static class ConversionExtensions
    {
        public static CS.SyntaxKind CSKind(this SyntaxToken token)
        {
            return CS.CSharpExtensions.Kind(token);
        }

        public static bool HasUsingDirective(this CS.CSharpSyntaxTree tree, string fullName)
        {
            if (tree == null)
                throw new ArgumentNullException(nameof(tree));
            if (string.IsNullOrWhiteSpace(fullName))
                throw new ArgumentException("given namespace cannot be null or empty.", nameof(fullName));
            fullName = fullName.Trim();
            return tree.GetRoot()
                .DescendantNodes(MatchesNamespaceOrRoot)
                .OfType<CS.Syntax.UsingDirectiveSyntax>()
                .Any(u => u.Name.ToString().Equals(fullName, StringComparison.OrdinalIgnoreCase));
        }

        private static bool MatchesNamespaceOrRoot(SyntaxNode arg)
        {
            return arg is CS.Syntax.NamespaceDeclarationSyntax || arg is CS.Syntax.CompilationUnitSyntax;
        }
    }
}
