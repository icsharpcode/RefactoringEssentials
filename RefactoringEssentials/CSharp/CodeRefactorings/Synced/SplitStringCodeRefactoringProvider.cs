using System;
using System.Threading;
using System.Collections.Generic;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.Text;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Formatting;

namespace RefactoringEssentials.CSharp.CodeRefactorings
{
    [ExportCodeRefactoringProvider(LanguageNames.CSharp, Name = "Split string literal")]
    public class SplitStringCodeRefactoringProvider : SpecializedCodeRefactoringProvider<LiteralExpressionSyntax>
    {
        protected override IEnumerable<CodeAction> GetActions(Document document, SemanticModel semanticModel, SyntaxNode root, TextSpan span, LiteralExpressionSyntax node, CancellationToken cancellationToken)
        {
            if (!node.IsKind(SyntaxKind.StringLiteralExpression))
                yield break;

            yield return CodeActionFactory.Create(
                span,
                DiagnosticSeverity.Info,
                GettextCatalog.GetString("Split string literal"),
                t2 =>
                {
                    var text = node.ToString();
                    var left = SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.ParseToken(text.Substring(0, span.Start - node.Span.Start) + '"'));
                    var right = SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.ParseToken((text.StartsWith("@", StringComparison.Ordinal) ? "@\"" : "\"") + text.Substring(span.Start - node.Span.Start)));

                    var newRoot = root.ReplaceNode((SyntaxNode)node, SyntaxFactory.BinaryExpression(SyntaxKind.AddExpression, left, right).WithAdditionalAnnotations(Formatter.Annotation));
                    return Task.FromResult(document.WithSyntaxRoot(newRoot));
                }
            );
        }
    }
}
