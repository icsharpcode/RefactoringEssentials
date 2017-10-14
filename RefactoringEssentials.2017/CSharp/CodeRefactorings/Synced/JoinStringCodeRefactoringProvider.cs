using System;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;

namespace RefactoringEssentials.CSharp.CodeRefactorings
{
    [ExportCodeRefactoringProvider(LanguageNames.CSharp, Name = "Join string literal")]
    public class JoinStringCodeRefactoringProvider : CodeRefactoringProvider
    {
        public override async Task ComputeRefactoringsAsync(CodeRefactoringContext context)
        {
            var document = context.Document;
            if (document.Project.Solution.Workspace.Kind == WorkspaceKind.MiscellaneousFiles)
                return;
            var span = context.Span;
            if (!span.IsEmpty)
                return;
            var cancellationToken = context.CancellationToken;
            if (cancellationToken.IsCancellationRequested)
                return;
            var model = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);
            if (model.IsFromGeneratedCode(cancellationToken))
                return;
            var root = await model.SyntaxTree.GetRootAsync(cancellationToken).ConfigureAwait(false);

            var node = root.FindNode(span) as BinaryExpressionSyntax;
            //ignore nodes except string concat.
            if (node == null || !node.OperatorToken.IsKind(SyntaxKind.PlusToken))
                return;

            LiteralExpressionSyntax left;
            var leftBinaryExpr = node.Left as BinaryExpressionSyntax;
            //if there is something other than a string literal on the left, then just take the right node (e.g. a+b+c => a+(b+c))
            if (leftBinaryExpr != null && leftBinaryExpr.OperatorToken.IsKind(SyntaxKind.PlusToken))
                left = leftBinaryExpr.Right as LiteralExpressionSyntax;
            else
                left = node.Left as LiteralExpressionSyntax;

            var right = node.Right as LiteralExpressionSyntax;

            //ignore non-string literals
            if (left == null || right == null || !left.IsKind(SyntaxKind.StringLiteralExpression) || !right.IsKind(SyntaxKind.StringLiteralExpression))
                return;

            bool isLeftVerbatim = left.Token.IsVerbatimStringLiteral();
            bool isRightVerbatim = right.Token.IsVerbatimStringLiteral();
            if (isLeftVerbatim != isRightVerbatim)
                return;

            String newString = left.Token.ValueText + right.Token.ValueText;
            LiteralExpressionSyntax stringLit;

            if (isLeftVerbatim)
                stringLit = SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal("@\"" + newString + "\"", newString));
            else
                stringLit = SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal(newString));

            ExpressionSyntax exprNode;

            if (leftBinaryExpr == null)
                exprNode = stringLit;
            else
                exprNode = leftBinaryExpr.WithRight(stringLit);
            context.RegisterRefactoring(
                CodeActionFactory.Create(span, DiagnosticSeverity.Info, GettextCatalog.GetString("Join strings"), document.WithSyntaxRoot(root.ReplaceNode((SyntaxNode)node, exprNode as ExpressionSyntax)))
            );
        }
    }
}
