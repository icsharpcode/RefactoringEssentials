using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Formatting;

namespace RefactoringEssentials.CSharp.CodeRefactorings
{
    [ExportCodeRefactoringProvider(LanguageNames.CSharp, Name = "Replace operator assignment with assignment")]
    public class ReplaceOperatorAssignmentWithAssignmentCodeRefactoringProvider : CodeRefactoringProvider
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
            if (node.IsKind(SyntaxKind.SimpleAssignmentExpression) || node.OperatorToken != token)
                return;
            var assignment = GetAssignmentOperator(node.Kind());
            if (assignment == SyntaxKind.None)
                return;

            context.RegisterRefactoring(
                CodeActionFactory.Create(
                    token.Span,
                    DiagnosticSeverity.Info,
                    GettextCatalog.GetString("Replace with '='"),
                    t2 =>
                    {
                        var newNode = SyntaxFactory.AssignmentExpression(SyntaxKind.SimpleAssignmentExpression, node.Left,
                            SyntaxFactory.BinaryExpression(assignment, node.Left.WithoutLeadingTrivia(), node.Right));
                        var newRoot = root.ReplaceNode((SyntaxNode)node, newNode.WithAdditionalAnnotations(Formatter.Annotation));
                        return Task.FromResult(document.WithSyntaxRoot(newRoot));
                    }
                )
            );
        }

        static SyntaxKind GetAssignmentOperator(SyntaxKind op)
        {
            switch (op)
            {
                case SyntaxKind.AndAssignmentExpression:
                    return SyntaxKind.BitwiseAndExpression;
                case SyntaxKind.OrAssignmentExpression:
                    return SyntaxKind.BitwiseOrExpression;
                case SyntaxKind.ExclusiveOrAssignmentExpression:
                    return SyntaxKind.ExclusiveOrExpression;
                case SyntaxKind.AddAssignmentExpression:
                    return SyntaxKind.AddExpression;
                case SyntaxKind.SubtractAssignmentExpression:
                    return SyntaxKind.SubtractExpression;
                case SyntaxKind.MultiplyAssignmentExpression:
                    return SyntaxKind.MultiplyExpression;
                case SyntaxKind.DivideAssignmentExpression:
                    return SyntaxKind.DivideExpression;
                case SyntaxKind.ModuloAssignmentExpression:
                    return SyntaxKind.ModuloExpression;
                case SyntaxKind.LeftShiftAssignmentExpression:
                    return SyntaxKind.LeftShiftExpression;
                case SyntaxKind.RightShiftAssignmentExpression:
                    return SyntaxKind.RightShiftExpression;
                default:
                    return SyntaxKind.None;
            }
        }
    }
}

