using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Formatting;

namespace RefactoringEssentials.CSharp.CodeRefactorings
{
    [ExportCodeRefactoringProvider(LanguageNames.CSharp, Name = "Convert '??' to '?:'")]
    public class ConvertCoalescingToConditionalExpressionCodeRefactoringProvider : CodeRefactoringProvider
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
            var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
            var model = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);
            if (model.IsFromGeneratedCode(cancellationToken))
                return;

            var node = root.FindNode(span) as BinaryExpressionSyntax;
            if (node == null || !node.OperatorToken.IsKind(SyntaxKind.QuestionQuestionToken))
                return;

            context.RegisterRefactoring(
                CodeActionFactory.Create(
                    span,
                    DiagnosticSeverity.Info,
                    GettextCatalog.GetString("Replace '??' operator with '?:' expression"), t2 =>
                    {
                        var left = node.Left;
                        var info = model.GetTypeInfo(left, t2);
                        if (info.ConvertedType.IsNullableType())
                            left = SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, CSharpUtil.AddParensIfRequired(left), SyntaxFactory.IdentifierName("Value"));
                        var ternary = SyntaxFactory.ConditionalExpression(
                            SyntaxFactory.BinaryExpression(
                                SyntaxKind.NotEqualsExpression,
                                node.Left,
                                SyntaxFactory.LiteralExpression(SyntaxKind.NullLiteralExpression)
                            ),
                            left,
                            node.Right
                        ).WithAdditionalAnnotations(Formatter.Annotation);
                        return Task.FromResult(document.WithSyntaxRoot(root.ReplaceNode(node, ternary)));
                    }
                )
            );
        }
    }
}