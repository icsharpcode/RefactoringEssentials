using System;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;
using System.Text;

namespace RefactoringEssentials.CSharp.CodeRefactorings
{
    [ExportCodeRefactoringProvider(LanguageNames.CSharp, Name = "Convert 'string.Format' to string interpolation")]
    public class ConvertStringFormatToInterpolatedStringCodeRefactoringProvider : CodeRefactoringProvider
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
            var node = root.FindToken(span.Start).Parent;

            var invocation = node?.Parent?.Parent as InvocationExpressionSyntax;
            if (invocation == null)
                return;
            var target = model.GetSymbolInfo(invocation.Expression).Symbol;
            if (target == null || target.Name != "Format" || target.ContainingType.SpecialType != SpecialType.System_String)
                return;
            context.RegisterRefactoring(
                CodeActionFactory.Create(
                    node.Span,
                    DiagnosticSeverity.Info,
                    GettextCatalog.GetString("To interpolated string"),
                    t =>
                    {
                        var newRoot = root.ReplaceNode(invocation, CreateInterpolatedString(invocation));
                        return Task.FromResult(document.WithSyntaxRoot(newRoot));
                    }
                )
            );
        }

        SyntaxNode CreateInterpolatedString(InvocationExpressionSyntax invocation)
        {
            var expr = invocation.ArgumentList.Arguments[0].Expression as LiteralExpressionSyntax;

            var str = expr.Token.ToString();
            var stringFormatDigits = new StringBuilder();
            var sb = new StringBuilder();
            sb.Append("$\"");
            bool isVerbatim = str[0] == '@';

            bool inStringFormat = false;
            for (int i = isVerbatim ? 2 : 1; i < str.Length - 1; i++)
            {
                var ch = str[i];

                if (ch == '{')
                {
                    inStringFormat = true;
                    stringFormatDigits.Length = 0;
                }
                else if (ch == '}' || ch == ':')
                {
                    if (inStringFormat)
                    {
                        if (stringFormatDigits.Length > 0)
                        {
                            try
                            {
                                var argNum = int.Parse(stringFormatDigits.ToString());
                                if (argNum + 1 < invocation.ArgumentList.Arguments.Count)
                                {
                                    sb.Append(invocation.ArgumentList.Arguments[argNum + 1].Expression);
                                }
                                else
                                {
                                    sb.Append(stringFormatDigits.ToString());
                                }
                            }
                            catch (Exception)
                            {
                            }
                        }
                        inStringFormat = false;
                    }
                }
                else if (inStringFormat && char.IsDigit(ch))
                {
                    stringFormatDigits.Append(ch);
                    continue;
                }

                if (isVerbatim)
                {
                    switch (ch)
                    {
                        case '"':
                            sb.Append("\\\"");
                            i++; // end " is skipped so it's always a "" in verbatim strings
                            break;
                        case '\\':
                            sb.Append("\\\\");
                            break;
                        case '\0':
                            sb.Append("\\0");
                            break;

                        case '\a':
                            sb.Append("\\a");
                            break;

                        case '\b':
                            sb.Append("\\b");
                            break;

                        case '\f':
                            sb.Append("\\f");
                            break;

                        case '\n':
                            sb.Append("\\n");
                            break;

                        case '\r':
                            sb.Append("\\r");
                            break;

                        case '\t':
                            sb.Append("\\t");
                            break;

                        case '\v':
                            sb.Append("\\v");
                            break;

                        default:
                            if (char.IsControl(ch) || char.IsSurrogate(ch) ||
                                // print all uncommon white spaces as numbers
                                (char.IsWhiteSpace(ch) && ch != ' '))
                            {
                                sb.Append("\\u" + ((int)ch).ToString("x4"));
                            }
                            else {
                                sb.Append(ch);
                                break;
                            }
                            break;
                    }
                    continue;
                }

                sb.Append(ch);
            }

            sb.Append("\"");
            return SyntaxFactory.ParseExpression(sb.ToString());
        }
    }
}