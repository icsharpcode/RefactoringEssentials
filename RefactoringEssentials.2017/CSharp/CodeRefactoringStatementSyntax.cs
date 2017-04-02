using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace RefactoringEssentials.CSharp
{
    public class CodeRefactoringStatementSyntax
    {
        StatementSyntax StatementSyntax { get; }

        public CodeRefactoringStatementSyntax(StatementSyntax statementSyntax)
        {
            StatementSyntax = statementSyntax;
        }

        public CodeRefactoringStatementSyntax WithAdditionalAnnotations(params SyntaxAnnotation[] annotations)
        {
            return new CodeRefactoringStatementSyntax(StatementSyntax.WithAdditionalAnnotations(annotations));
        }

        public BlockSyntax WrapInBlock()
        {
            return SyntaxFactory.Block(StatementSyntax);
        }
    }
}
