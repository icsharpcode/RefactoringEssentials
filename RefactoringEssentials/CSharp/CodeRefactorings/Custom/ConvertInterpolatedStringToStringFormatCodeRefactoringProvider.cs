using System.Collections.Generic;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Formatting;
using System.Text;

namespace RefactoringEssentials.CSharp.CodeRefactorings
{
    [ExportCodeRefactoringProvider(LanguageNames.CSharp, Name = "Convert string interpolation to 'string.Format'")]
    public class ConvertInterpolatedStringToStringFormatCodeRefactoringProvider : CodeRefactoringProvider
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
            var node = root.FindToken(span.Start).Parent as InterpolatedStringTextSyntax;
            if (node == null)
                return;
            context.RegisterRefactoring(
                CodeActionFactory.Create(
                    node.Span,
                    DiagnosticSeverity.Info,
                    GettextCatalog.GetString("To format string"),
                    t =>
                    {
                        var newRoot = root.ReplaceNode(node.Parent, CreateFormatString(node));
                        return Task.FromResult(document.WithSyntaxRoot(newRoot));
                    }
                )
            );
        }

        SyntaxNode CreateFormatString(InterpolatedStringTextSyntax node)
        {
            var sb = new StringBuilder();
            sb.Append("string.Format (\"");
            var stringExpressions = new List<ExpressionSyntax>();
            var parent = node.Parent as InterpolatedStringExpressionSyntax;
            foreach (var child in parent.Contents)
            {
                var kind = child.Kind();
                switch (kind)
                {
                    case SyntaxKind.InterpolatedStringText:
                        sb.Append(((InterpolatedStringTextSyntax)child).TextToken.ToString());
                        break;
                    case SyntaxKind.Interpolation:
                        var interpolation = child as InterpolationSyntax;
                        sb.Append("{");

                        int index = -1;
                        for (int i = 0; i < stringExpressions.Count; i++)
                        {
                            if (stringExpressions[i].IsEquivalentTo(interpolation.Expression))
                            {
                                index = i;
                                break;
                            }
                        }
                        if (index < 0)
                        {
                            index = stringExpressions.Count;
                            stringExpressions.Add(interpolation.Expression);
                        }

                        sb.Append(index);

                        if (interpolation.FormatClause != null)
                            sb.Append(interpolation.FormatClause);
                        sb.Append("}");
                        break;
                }
            }

            sb.Append("\"");

            for (int i = 0; i < stringExpressions.Count; i++)
            {
                sb.Append(", ");
                sb.Append(stringExpressions[i]);
            }
            sb.Append(")");
            return SyntaxFactory.ParseExpression(sb.ToString()).WithAdditionalAnnotations(Formatter.Annotation);
        }
    }
}

