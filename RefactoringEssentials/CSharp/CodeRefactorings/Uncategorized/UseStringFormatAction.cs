using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;
using System.Text;
using System.Linq;

namespace RefactoringEssentials.CSharp.CodeRefactorings
{
    /// <summary>
    /// Refactors string and expression concatenation to use <see cref="string.Format(string, object[])"/>.
    /// </summary>
    [ExportCodeRefactoringProvider(LanguageNames.CSharp, Name = "Use string.Format()")]
    public class UseStringFormatAction : CodeRefactoringProvider
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
            var node = root.FindNode(span);
            node = node.SkipArgument();

            // Get ancestor binary expressions and ensure they are string concatenation type.
            var ancestors = node.AncestorsAndSelf().Where(n => n is BinaryExpressionSyntax);

            // If there is no binary expressions then why are we here?
            if (!ancestors.Any())
                return;

            if (!(ancestors.Last().IsKind(SyntaxKind.AddExpression)))
                return;

            // Get highest binary expression available.
            node = ancestors.LastOrDefault(n => n is BinaryExpressionSyntax);

            // If there is not a binary expression, then there is no concatenaton to act on.
            if (node == null)
                return;

            var parts = GetParts(node as BinaryExpressionSyntax);

            // Ensure there are some parts, not sure how this could possibly happen, but better safe than sorry.
            if (parts.Count() <= 0)
                return;

            // Get all string type parts.
            var stringParts = parts.Where(p => IsStringExpression(p));

            // Ensure there is at least one string type part.
            if (!stringParts.Any())
                return;

            // Ensure strings are all verbatim or all not verbatim. It just gets ugly otherwise. ;p
            var verbatimParts = stringParts.Where(p => p.GetFirstToken().Text.StartsWith("@", System.StringComparison.OrdinalIgnoreCase));
            if (verbatimParts.Count() != 0 && verbatimParts.Count() != stringParts.Count())
                return;

            context.RegisterRefactoring(
               CodeActionFactory.Create(
                   node.Span,
                   DiagnosticSeverity.Info,
                   GettextCatalog.GetString("Use 'string.Format()'"),
                   t2 =>
                   {
                       var newRoot = root.ReplaceNode(node, ReplaceNode(node as BinaryExpressionSyntax));
                       return Task.FromResult(document.WithSyntaxRoot(newRoot));
                   }
               )
           );

            return;
        }

        /// <summary>
        /// Replace concatenation node with <see cref="string.Format(string, object[])"/>.
        /// </summary>
        /// <param name="node">A <see cref="BinaryExpressionSyntax"/> with string and expression concatenation.</param>
        /// <returns>
        /// The new node.
        /// </returns>
        static SyntaxNode ReplaceNode(BinaryExpressionSyntax node)
        {
            var parts = GetParts(node);

            var format = new StringBuilder();
            //var expressionIndex = 0;
            var expressions = new List<string>();
            var isVerbatim = false;

            foreach (var part in parts)
            {
                var value = string.Empty;

                if (IsStringExpression(part))
                {
                    // Get string.
                    value = part.GetFirstToken().Text;

                    if (value.StartsWith("@", System.StringComparison.OrdinalIgnoreCase))
                    {
                        isVerbatim = true;
                        value = value.Substring(2, value.Length - 3);
                    }
                    else
                    {
                        value = value.Substring(1, value.Length - 2);
                    }

                    // Escape braces if the exist.
                    if (value.Contains("{"))
                    {
                        value = value.Replace("{", "{{");
                    }

                    if (value.Contains("}"))
                    {
                        value = value.Replace("}", "}}");
                    }
                }
                else
                {
                    var p = TestForToStringAccess(part);

                    var expression = string.Empty;
                    var expressionArgs = string.Empty;

                    // If ToString reference, then remove reference and pass arguments to format string.
                    if (p.IsToString)
                    {
                        expression = p.Expression.ToString();
                        if (p.HasArgument)
                        {
                            var argumentString = p.Argument.ToString();
                            if (IsStringExpression(p.Argument.Expression))
                            {
                                argumentString = argumentString.TrimStart('"').TrimEnd('"');
                            }

                            expressionArgs = string.Format(@":{0}", argumentString);
                        }
                    }
                    else
                    {
                        expression = part.ToString();
                    }

                    var expressionIndex = expressions.IndexOf(expression);
                    if (expressionIndex <= -1)
                    {
                        expressions.Add(expression);
                        expressionIndex = expressions.IndexOf(expression);
                    }

                    // Get expression.
                    value = $"{{{expressionIndex}{expressionArgs}}}";
                }

                format.Append(value);
            }

            var startStringQuote = isVerbatim ? @"@""" : @"""";
            var formatString = startStringQuote + format.ToString() + "\"";

            var result = string.Empty;

            if (expressions.Any())
            {
                var expressionString = (expressions.Any()) ? ", " + string.Join(", ", expressions) : string.Empty;
                result = $@"string.Format({formatString}{expressionString})";
            }
            else
            {
                result = formatString;
            }

            return SyntaxFactory.ParseExpression(result);
        }


        static ToStringRef TestForToStringAccess(ExpressionSyntax myItem)
        {
            var result = new ToStringRef
            {
                IsToString = false,
                HasArgument = false
            };

            var invocation = myItem as InvocationExpressionSyntax;
            if (invocation != null)
            {
                var memberAccess = invocation.Expression as MemberAccessExpressionSyntax;

                if (memberAccess != null)
                {
                    if (string.Equals("ToString", memberAccess.Name.ToString(), System.StringComparison.OrdinalIgnoreCase))
                    {
                        // Only handle unecessary calls to ToString or calls with only a format string parameter.
                        // IE: var.ToString() or var.ToString(""), but not var.ToString(a).
                        if (!invocation.ArgumentList.Arguments.Any())
                        {
                            result.IsToString = true;
                            result.Expression = memberAccess.Expression;
                        }
                        else if (invocation.ArgumentList.Arguments.Count == 1)
                        {
                            var argument = invocation.ArgumentList.Arguments.First();
                            if (IsStringExpression(argument.Expression))
                            {
                                result.IsToString = true;
                                result.Expression = memberAccess.Expression;
                                result.HasArgument = true;
                                result.Argument = argument;
                            }
                        }
                    }
                }
            }

            return result;
        }

        private class ToStringRef
        {
            public bool IsToString { get; set; }

            public bool HasArgument { get; set; }

            public ExpressionSyntax Expression { get; set; }

            public ArgumentSyntax Argument { get; set; }
        }

        /// <summary>
        /// Get parts in order from a binary expression.
        /// </summary>
        /// <returns></returns>
        static IEnumerable<ExpressionSyntax> GetParts(BinaryExpressionSyntax node)
        {
            var result = new List<ExpressionSyntax>();

            var descendents = node.DescendantNodes();
            if (!descendents.Any(d => IsStringExpression(d)))
            {
                result.Add(node);
            }
            else
            {
                result.AddRange((node.Left is BinaryExpressionSyntax) ?
                GetParts(node.Left as BinaryExpressionSyntax) :
                new[] { node.Left });

                result.AddRange((node.Right is BinaryExpressionSyntax) ?
                    GetParts(node.Right as BinaryExpressionSyntax) :
                    new[] { node.Right });
            }

            return result;
        }

        /// <summary>
        /// Is the expression a string type?
        /// </summary>
        /// <param name="expr">An expression.</param>
        /// <returns>
        /// True if the expression is of kind <see cref="SyntaxKind.StringLiteralExpression"/>.
        /// </returns>
        static bool IsStringExpression(SyntaxNode expr)
        {
            return (expr is LiteralExpressionSyntax && expr.IsKind(SyntaxKind.StringLiteralExpression));
        }

    }
}
