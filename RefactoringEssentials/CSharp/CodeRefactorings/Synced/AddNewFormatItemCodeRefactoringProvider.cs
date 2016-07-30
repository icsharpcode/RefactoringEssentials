using System.Linq;
using System.Collections.Generic;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Formatting;

namespace RefactoringEssentials.CSharp.CodeRefactorings
{
    /// <summary>
    /// Introduce format item. Works on strings that contain selections.
    /// "this is <some> string" => string.Format ("this is {0} string", <some>)
    /// </summary>

    [ExportCodeRefactoringProvider(LanguageNames.CSharp, Name = "Introduce format item")]
    public class AddNewFormatItemCodeRefactoringProvider : CodeRefactoringProvider
    {
        public override async Task ComputeRefactoringsAsync(CodeRefactoringContext context)
        {
            var document = context.Document;
            if (document.Project.Solution.Workspace.Kind == WorkspaceKind.MiscellaneousFiles)
                return;
            var span = context.Span;
            if (span.IsEmpty)
                return;
            var cancellationToken = context.CancellationToken;
            if (cancellationToken.IsCancellationRequested)
                return;
            var model = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);
            if (model.IsFromGeneratedCode(cancellationToken))
                return;
            var root = await model.SyntaxTree.GetRootAsync(cancellationToken).ConfigureAwait(false);

            var token = root.FindToken(span.Start);
            if (!token.IsKind(SyntaxKind.StringLiteralToken) || token.Span.End < span.End || ((span.Start - token.SpanStart - 1) < 0))
                return;
            //			if (pexpr.LiteralValue.StartsWith("@", StringComparison.Ordinal)) {
            //				if (!(pexpr.StartLocation < new TextLocation(context.Location.Line, context.Location.Column - 1) && new TextLocation(context.Location.Line, context.Location.Column + 1) < pexpr.EndLocation)) {
            //					yield break;
            //				}
            //			} else {
            //				if (!(pexpr.StartLocation < context.Location && context.Location < pexpr.EndLocation)) {
            //					yield break;
            //				}
            //			}

            context.RegisterRefactoring(
                CodeActionFactory.Create(
                    span,
                    DiagnosticSeverity.Info,
                    GettextCatalog.GetString("Insert format argument"),
                    t2 =>
                    {
                        var parent = token.Parent;
                        var tokenText = token.ToString();

                        int argumentNumber = 0;
                        InvocationExpressionSyntax invocationExpression = null;
                        if (parent.Parent.IsKind(SyntaxKind.Argument))
                        {
                            invocationExpression = (InvocationExpressionSyntax)parent.Parent.Parent.Parent;
                            var info = model.GetSymbolInfo(invocationExpression);
                            var method = info.Symbol as IMethodSymbol;
                            if (method.Name == "Format" && method.ContainingType.SpecialType == SpecialType.System_String)
                            {
                                argumentNumber = invocationExpression.ArgumentList.Arguments.Count - 1;
                            }
                            else
                            {
                                invocationExpression = null;
                            }
                        }

                        var endOffset = span.End - token.SpanStart - 2;
                        string formatText = tokenText.Substring(1, span.Start - token.SpanStart - 1) + "{" + argumentNumber + "}" + tokenText.Substring(endOffset, tokenText.Length - endOffset - 1);

                        string argumentText = tokenText.Substring(span.Start - token.SpanStart, span.Length - 2);

                        InvocationExpressionSyntax newInvocation;
                        if (invocationExpression != null)
                        {
                            parent = invocationExpression;
                            var argumentList = new List<ArgumentSyntax>();
                            argumentList.Add(SyntaxFactory.Argument(SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal(formatText))));
                            argumentList.AddRange(invocationExpression.ArgumentList.Arguments.Skip(1));
                            argumentList.Add(SyntaxFactory.Argument(SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal(argumentText))));
                            newInvocation = invocationExpression.WithArgumentList(SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>(argumentList)));
                        }
                        else
                        {
                            newInvocation = SyntaxFactory.InvocationExpression(SyntaxFactory.ParseExpression("string.Format"), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>(new[] {
                                SyntaxFactory.Argument(SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal(formatText))),
                                SyntaxFactory.Argument(SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal(argumentText)))
                            })));
                        }

                        return Task.FromResult(document.WithSyntaxRoot(root.ReplaceNode((SyntaxNode)parent, newInvocation.WithAdditionalAnnotations(Formatter.Annotation))));
                    }
                )
            );
        }
    }
}