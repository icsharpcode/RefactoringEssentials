using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace RefactoringEssentials
{
    static class ClassDeclarationSyntaxExtensions
    {
        public static IEnumerable<MemberDeclarationSyntax> GetMembersFromAllParts(this ClassDeclarationSyntax type, SemanticModel model)
        {
            var typeSymbol = model.GetDeclaredSymbol(type);
            if (typeSymbol.IsErrorType())
                return null;
            var allTypeDeclarations = typeSymbol.DeclaringSyntaxReferences.Select(sr => sr.GetSyntax()).OfType<ClassDeclarationSyntax>();
            if (allTypeDeclarations.Any())
                return allTypeDeclarations.SelectMany(t => t.Members);
            return null;
        }
    }
}
