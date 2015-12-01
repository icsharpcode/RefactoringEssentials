using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Formatting;

namespace RefactoringEssentials.CSharp.CodeRefactorings
{
    public abstract class CodeContractsCodeRefactoringProvider<T> : SpecializedCodeRefactoringProvider<T> where T : SyntaxNode
    {
        protected static bool UsingStatementNotPresent(CompilationUnitSyntax cu)
        {
            return !cu.Usings.Any((UsingDirectiveSyntax u) => u.Name.ToString() == "System.Diagnostics.Contracts");
        }

        protected static CompilationUnitSyntax AddUsingStatement(SyntaxNode node, CompilationUnitSyntax cu)
        {
            return cu.AddUsingDirective(
                SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("System.Diagnostics.Contracts")).WithAdditionalAnnotations(Formatter.Annotation)
                , node
                , true);
        }
    }
}
