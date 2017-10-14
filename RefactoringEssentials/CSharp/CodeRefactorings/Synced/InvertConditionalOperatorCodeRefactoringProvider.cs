using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Formatting;

namespace RefactoringEssentials.CSharp.CodeRefactorings
{
    [ExportCodeRefactoringProvider(LanguageNames.CSharp, Name = "Invert conditional operator")]
    public class InvertConditionalOperatorCodeRefactoringProvider : CodeRefactoringProvider
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

            var node = token.Parent;
            if (node.IsKind(SyntaxKind.IdentifierName) && node.SpanStart == span.Start)
            {
                node = node.Parent;
                if (node.Parent is ConditionalExpressionSyntax && node.Parent.SpanStart == span.Start && ((ConditionalExpressionSyntax)node.Parent).Condition == node)
                    node = node.Parent;
            }

            var condExpr = node as ConditionalExpressionSyntax;
            if (condExpr == null)
                return;
            context.RegisterRefactoring(
                CodeActionFactory.Create(
                    token.Span,
                    DiagnosticSeverity.Info,
                    GettextCatalog.GetString("Invert '?:'"),
                    t2 =>
                    {
                        var newRoot = root.ReplaceNode((SyntaxNode)
                            condExpr,
                            condExpr
                            .WithCondition(CSharpUtil.InvertCondition(condExpr.Condition))
                            .WithWhenTrue(condExpr.WhenFalse)
                            .WithWhenFalse(condExpr.WhenTrue)
                            .WithAdditionalAnnotations(Formatter.Annotation)
                        );
                        return Task.FromResult(document.WithSyntaxRoot(newRoot));
                    }
                )
            );
        }
    }
}