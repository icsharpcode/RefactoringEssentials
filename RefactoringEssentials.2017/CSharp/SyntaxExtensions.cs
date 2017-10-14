using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace RefactoringEssentials.CSharp
{
	public static class SyntaxExtensions
    {
        public static ExpressionStatementSyntax InitializerAsAssignment(this VariableDeclaratorSyntax variableDeclarator)
        {
            return SyntaxFactory.ExpressionStatement(
                SyntaxFactory.AssignmentExpression(
                    SyntaxKind.SimpleAssignmentExpression,
                    SyntaxFactory.IdentifierName(variableDeclarator.Identifier),
                    variableDeclarator.Initializer.Value
                    )
                );
        }

        public static BlockSyntax EnsureBlock(this StatementSyntax statement)
        {
            var asBlock = statement as BlockSyntax;

            if (asBlock != null)
            {
                return asBlock;
            }

            var emptyStatement = statement as EmptyStatementSyntax;

            if (emptyStatement != null)
            {
                return SyntaxFactory.Block();
            }

            return SyntaxFactory.Block(statement);
        }
    }
}
