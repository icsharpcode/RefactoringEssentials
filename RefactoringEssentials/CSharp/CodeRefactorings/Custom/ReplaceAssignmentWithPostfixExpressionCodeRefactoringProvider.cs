using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Formatting;

namespace RefactoringEssentials.CSharp.CodeRefactorings
{
    [ExportCodeRefactoringProvider(LanguageNames.CSharp, Name = "Replace assignment with postfix expression")]
    public class ReplaceAssignmentWithPostfixExpressionCodeRefactoringProvider : CodeRefactoringProvider
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
            var token = root.FindToken(span.Start);

            var node = token.Parent as AssignmentExpressionSyntax;
            if (node == null || !node.OperatorToken.Span.Contains(span))
                return;

            var updatedNode = ReplaceWithOperatorAssignmentCodeRefactoringProvider.CreateAssignment(node) ?? node;

            if ((!updatedNode.IsKind(SyntaxKind.AddAssignmentExpression) && !updatedNode.IsKind(SyntaxKind.SubtractAssignmentExpression)))
                return;

            var rightLiteral = updatedNode.Right as LiteralExpressionSyntax;
            if (rightLiteral == null || ((int)rightLiteral.Token.Value) != 1)
                return;

            context.RegisterRefactoring(
                CodeActionFactory.Create(
                    token.Span,
                    DiagnosticSeverity.Info,
                    updatedNode.IsKind(SyntaxKind.AddAssignmentExpression) ? GettextCatalog.GetString("To '{0}++'") : GettextCatalog.GetString("To '{0}--'"),
                    t2 =>
                    {
                        var newNode = SyntaxFactory.PostfixUnaryExpression(updatedNode.IsKind(SyntaxKind.AddAssignmentExpression) ? SyntaxKind.PostIncrementExpression : SyntaxKind.PostDecrementExpression, updatedNode.Left);
                        var newRoot = root.ReplaceNode((SyntaxNode)node, newNode.WithAdditionalAnnotations(Formatter.Annotation).WithLeadingTrivia(node.GetLeadingTrivia()));
                        return Task.FromResult(document.WithSyntaxRoot(newRoot));
                    }
                )
            );
        }
    }
}

