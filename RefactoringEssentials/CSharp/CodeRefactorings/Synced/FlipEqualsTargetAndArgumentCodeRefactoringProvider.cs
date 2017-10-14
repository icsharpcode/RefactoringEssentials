using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Formatting;

namespace RefactoringEssentials.CSharp.CodeRefactorings
{
    [ExportCodeRefactoringProvider(LanguageNames.CSharp, Name = "Swap 'Equals' target and argument")]
    public class FlipEqualsTargetAndArgumentCodeRefactoringProvider : CodeRefactoringProvider
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

            var node = root.FindNode(span) as IdentifierNameSyntax;
            if (node == null || !node.Parent.IsKind(SyntaxKind.SimpleMemberAccessExpression))
                return;
            var memberAccess = node.Parent as MemberAccessExpressionSyntax;
            var invocation = node.Parent.Parent as InvocationExpressionSyntax;
            if (invocation == null || invocation.ArgumentList.Arguments.Count != 1 || invocation.ArgumentList.Arguments[0].Expression.IsKind(SyntaxKind.NullLiteralExpression))
                return;

            var invocationRR = model.GetSymbolInfo(invocation);
            if (invocationRR.Symbol == null)
                return;

            var method = invocationRR.Symbol as IMethodSymbol;
            if (method == null)
                return;

            if (method.Name != "Equals" || method.IsStatic || method.ReturnType.SpecialType != SpecialType.System_Boolean)
                return;

            context.RegisterRefactoring(
                CodeActionFactory.Create(
                    span,
                    DiagnosticSeverity.Info,
                    GettextCatalog.GetString("Flip 'Equals' target and argument"),
                    t2 =>
                    {
                        var newRoot = root.ReplaceNode((SyntaxNode)invocation,
                            invocation
                            .WithExpression(SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, CSharpUtil.AddParensIfRequired(invocation.ArgumentList.Arguments[0].Expression), memberAccess.Name))
                            .WithArgumentList(SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>(new[] { SyntaxFactory.Argument(memberAccess.Expression.SkipParens()) })))
                            .WithAdditionalAnnotations(Formatter.Annotation)
                        );
                        return Task.FromResult(document.WithSyntaxRoot(newRoot));
                    }
                )
            );
        }
    }
}