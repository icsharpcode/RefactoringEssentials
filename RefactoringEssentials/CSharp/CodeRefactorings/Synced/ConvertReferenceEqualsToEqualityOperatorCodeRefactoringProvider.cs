using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Formatting;

namespace RefactoringEssentials.CSharp.CodeRefactorings
{
    [ExportCodeRefactoringProvider(LanguageNames.CSharp, Name = "Convert 'ReferenceEquals' call to '==' or '!='")]
    public class ConvertReferenceEqualsToEqualityOperatorCodeRefactoringProvider : CodeRefactoringProvider
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
            if (node == null)
                return;
            var invocation = node.Parent as InvocationExpressionSyntax ?? node.Parent.Parent as InvocationExpressionSyntax;
            if (invocation == null)
                return;

            var symbol = model.GetSymbolInfo(node).Symbol;
            if (symbol == null || symbol.Name != "ReferenceEquals" || symbol.ContainingType.SpecialType != SpecialType.System_Object)
                return;

            ExpressionSyntax expr = invocation;
            bool useEquality = true;

            if (invocation.ArgumentList.Arguments.Count != 2 && invocation.ArgumentList.Arguments.Count != 1)
                return;
            //node is identifier, parent is invocation, parent.parent (might) be unary negation
            var uOp = invocation.Parent as PrefixUnaryExpressionSyntax;
            if (uOp != null && uOp.IsKind(SyntaxKind.LogicalNotExpression))
            {
                expr = uOp;
                useEquality = false;
            }

            context.RegisterRefactoring(
                CodeActionFactory.Create(
                    span,
                    DiagnosticSeverity.Info,
                    useEquality ? GettextCatalog.GetString("To '=='") : GettextCatalog.GetString("To '!='"),
                    t2 =>
                    {
                        var newRoot = root.ReplaceNode((SyntaxNode)
                            expr,
                            SyntaxFactory.BinaryExpression(
                                useEquality ? SyntaxKind.EqualsExpression : SyntaxKind.NotEqualsExpression,
                                invocation.ArgumentList.Arguments.Count == 1 ? ((MemberAccessExpressionSyntax)invocation.Expression).Expression : invocation.ArgumentList.Arguments.First().Expression,
                                invocation.ArgumentList.Arguments.Last().Expression
                            ).WithAdditionalAnnotations(Formatter.Annotation)
                        );

                        return Task.FromResult(document.WithSyntaxRoot(newRoot));
                    }
                )
            );
        }
    }
}

