using System.Linq;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Formatting;

namespace RefactoringEssentials.CSharp.CodeRefactorings
{
    [ExportCodeRefactoringProvider(LanguageNames.CSharp, Name = "Invoke as static method")]
    public class InvokeAsStaticMethodCodeRefactoringProvider : CodeRefactoringProvider
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
            if (invocation == null)
                return;

            var invocationRR = model.GetSymbolInfo(invocation);
            if (invocationRR.Symbol == null)
                return;

            var method = invocationRR.Symbol as IMethodSymbol;
            if (method == null || !method.IsExtensionMethod || method.MethodKind == MethodKind.Ordinary)
                return;

            context.RegisterRefactoring(
                CodeActionFactory.Create(
                    span,
                    DiagnosticSeverity.Info,
                    GettextCatalog.GetString("To static invocation"),
                    t2 =>
                    {
                        var newRoot = root.ReplaceNode((SyntaxNode)invocation, ToStaticMethodInvocation(model, invocation, memberAccess, invocationRR).WithAdditionalAnnotations(Formatter.Annotation).WithLeadingTrivia(invocation.GetLeadingTrivia()));
                        return Task.FromResult(document.WithSyntaxRoot(newRoot));
                    }
                )
            );
        }

        static SyntaxNode ToStaticMethodInvocation(SemanticModel model, InvocationExpressionSyntax invocation, MemberAccessExpressionSyntax memberAccess, SymbolInfo invocationRR)
        {
            var newArgumentList = invocation.ArgumentList.Arguments.ToList();
            newArgumentList.Insert(0, SyntaxFactory.Argument(memberAccess.Expression.WithoutLeadingTrivia()));

            var newTarget = memberAccess.WithExpression(SyntaxFactory.ParseTypeName(invocationRR.Symbol.ContainingType.ToMinimalDisplayString(model, memberAccess.SpanStart)));
            return SyntaxFactory.InvocationExpression(newTarget, SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>(newArgumentList)));
        }

    }
}

