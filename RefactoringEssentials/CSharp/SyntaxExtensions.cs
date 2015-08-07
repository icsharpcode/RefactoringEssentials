using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
    }
}
