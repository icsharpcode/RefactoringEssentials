using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.VisualBasic;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;

namespace RefactoringEssentials.Util
{
    static class VBSyntaxExtensions
    {
        public static IFieldSymbol ScanGetter(this AccessorBlockSyntax getter, SemanticModel model)
        {
            if (getter == null || getter.Statements.Count != 1)
                return null;
            var retStatement = getter.Statements.First() as ReturnStatementSyntax;
            if (retStatement == null)
                return null;
            if (!IsPossibleExpression(retStatement.Expression))
                return null;
            var retSymbol = model.GetSymbolInfo(retStatement.Expression).Symbol;
            return ((IFieldSymbol)retSymbol);
        }

        public static IFieldSymbol ScanSetter(this AccessorBlockSyntax setter, SemanticModel model)
        {
            if (setter == null || setter.Statements.Count != 1) //no getter/get;/get we can't easily work out
                return null;
            var setAssignment = setter.Statements.OfType<StatementSyntax>().FirstOrDefault();
            var assignment = setAssignment != null ? setAssignment as AssignmentStatementSyntax : null;
            if (assignment == null || !assignment.OperatorToken.IsKind(SyntaxKind.EqualsToken))
                return null;
            var id = assignment.Right as IdentifierNameSyntax;
            if (id == null || id.Identifier.ValueText != "value")
                return null;
            if (!IsPossibleExpression(assignment.Left))
                return null;
            var retSymbol = model.GetSymbolInfo(assignment.Left).Symbol;
            return ((IFieldSymbol)retSymbol);

        }

        static bool IsPossibleExpression(ExpressionSyntax left)
        {
            if (left.IsKind(SyntaxKind.IdentifierName))
                return true;
            var mr = left as MemberAccessExpressionSyntax;
            if (mr == null)
                return false;
            return mr.Expression is MyClassExpressionSyntax;
        }

        public static bool IsKind(this SyntaxNode node, SyntaxKind kind1, SyntaxKind kind2)
        {
            if (node == null)
            {
                return false;
            }

            var vbKind = node.Kind();
            return vbKind == kind1 || vbKind == kind2;
        }

        public static bool IsKind(this SyntaxNode node, SyntaxKind kind1, SyntaxKind kind2, SyntaxKind kind3)
        {
            if (node == null)
            {
                return false;
            }

            var vbKind = node.Kind();
            return vbKind == kind1 || vbKind == kind2 || vbKind == kind3;
        }

        public static bool IsKind(this SyntaxNode node, SyntaxKind kind1, SyntaxKind kind2, SyntaxKind kind3, SyntaxKind kind4)
        {
            if (node == null)
            {
                return false;
            }

            var vbKind = node.Kind();
            return vbKind == kind1 || vbKind == kind2 || vbKind == kind3 || vbKind == kind4;
        }

        public static bool IsKind(this SyntaxNode node, SyntaxKind kind1, SyntaxKind kind2, SyntaxKind kind3, SyntaxKind kind4, SyntaxKind kind5)
        {
            if (node == null)
            {
                return false;
            }

            var vbKind = node.Kind();
            return vbKind == kind1 || vbKind == kind2 || vbKind == kind3 || vbKind == kind4 || vbKind == kind5;
        }

        public static MethodBlockBaseSyntax WithStatements(this MethodBlockBaseSyntax syntax, SyntaxList<StatementSyntax> statements)
        {
            if (syntax == null)
                throw new System.ArgumentNullException(nameof(syntax));
            if (syntax is MethodBlockSyntax)
                return ((MethodBlockSyntax)syntax).WithStatements(statements);
            if (syntax is ConstructorBlockSyntax)
                return ((ConstructorBlockSyntax)syntax).WithStatements(statements);
            if (syntax is AccessorBlockSyntax)
                return ((AccessorBlockSyntax)syntax).WithStatements(statements);
            if (syntax is OperatorBlockSyntax)
                return ((OperatorBlockSyntax)syntax).WithStatements(statements);
            throw new System.NotSupportedException(syntax.GetType() + " is not supported!");
        }

        public static ExpressionSyntax SkipParens(this ExpressionSyntax expression)
        {
            if (expression == null)
                return null;
            while (expression != null && expression.IsKind(SyntaxKind.ParenthesizedExpression))
            {
                expression = ((ParenthesizedExpressionSyntax)expression).Expression;
            }
            return expression;
        }
    }
}
