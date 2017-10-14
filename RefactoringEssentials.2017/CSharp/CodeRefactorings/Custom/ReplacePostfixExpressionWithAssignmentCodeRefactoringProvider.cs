using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Formatting;

namespace RefactoringEssentials.CSharp.CodeRefactorings
{
    [ExportCodeRefactoringProvider(LanguageNames.CSharp, Name = "Replace postfix expression with assignment")]
    public class ReplacePostfixExpressionWithAssignmentCodeRefactoringProvider : CodeRefactoringProvider
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

            var postfix = token.Parent.Parent as PostfixUnaryExpressionSyntax;
            if (postfix == null || !(postfix.IsKind(SyntaxKind.PostIncrementExpression) || postfix.IsKind(SyntaxKind.PostDecrementExpression)))
                return;

            context.RegisterRefactoring(
                CodeActionFactory.Create(
                    token.Span,
                    DiagnosticSeverity.Info,
                    string.Format(postfix.IsKind(SyntaxKind.PostIncrementExpression) ? GettextCatalog.GetString("Replace '{0}++' with '{0} += 1'") : GettextCatalog.GetString("Replace '{0}--' with '{0} -= 1'"), postfix.Operand.ToString()),
                    t2 =>
                    {
                        var op = postfix.OperatorToken.IsKind(SyntaxKind.PlusPlusToken) ? SyntaxKind.AddAssignmentExpression : SyntaxKind.SubtractAssignmentExpression;
                        var binexp = SyntaxFactory.AssignmentExpression(op, postfix.Operand, SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal(1)));
                        var newRoot = root.ReplaceNode((SyntaxNode)postfix, binexp.WithAdditionalAnnotations(Formatter.Annotation).WithLeadingTrivia(postfix.GetLeadingTrivia()));
                        return Task.FromResult(document.WithSyntaxRoot(newRoot));
                    }
                )
            );
        }
    }
}