using System.Linq;
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
    /// <summary>
    ///  Add name for argument
    /// </summary>

    [ExportCodeRefactoringProvider(LanguageNames.CSharp, Name = "Add name for argument")]
    public class AddNameToArgumentCodeRefactoringProvider : SpecializedCodeRefactoringProvider<ExpressionSyntax>
    {
        static CodeAction CreateAttributeCodeAction(Document document, SyntaxNode root, ExpressionSyntax node, IMethodSymbol constructor, AttributeSyntax attribute)
        {
            var arguments = attribute.ArgumentList.Arguments;
            var idx = arguments.IndexOf(node.Parent as AttributeArgumentSyntax);

            if (idx >= constructor.Parameters.Length) // this can happen with "params" parameters
                return null;
            var name = constructor.Parameters[idx].Name;
            return CodeActionFactory.Create(
                node.Span,
                DiagnosticSeverity.Info,
                string.Format(GettextCatalog.GetString("Add argument name '{0}'"), name),
                t2 =>
                {
                    var newArguments = SyntaxFactory.SeparatedList<AttributeArgumentSyntax>(
                        attribute.ArgumentList.Arguments.Take(idx).Concat(
                            attribute.ArgumentList.Arguments.Skip(idx).Select((arg, i) =>
                            {
                                if (arg.NameEquals != null)
                                    return arg;
                                return SyntaxFactory.AttributeArgument(null, SyntaxFactory.NameColon(constructor.Parameters[i + idx].Name), arg.Expression);
                            })
                        )
                    );
                    var newAttribute = attribute.WithArgumentList(attribute.ArgumentList.WithArguments(newArguments));
                    var newRoot = root.ReplaceNode((SyntaxNode)attribute, newAttribute).WithAdditionalAnnotations(Formatter.Annotation);
                    return Task.FromResult(document.WithSyntaxRoot(newRoot));
                }
            );
        }

        static CodeAction CreateIndexerCodeAction(Document document, SyntaxNode root, ExpressionSyntax node, IPropertySymbol indexer, ElementAccessExpressionSyntax elementAccess)
        {
            var arguments = elementAccess.ArgumentList.Arguments;
            var idx = arguments.IndexOf(node.Parent as ArgumentSyntax);

            if (idx >= indexer.Parameters.Length) // this can happen with "params" parameters
                return null;
            var name = indexer.Parameters[idx].Name;
            return CodeActionFactory.Create(
                node.Span,
                DiagnosticSeverity.Info,
                string.Format("Add argument name '{0}'", name),
                t2 =>
                {
                    var newArguments = SyntaxFactory.SeparatedList<ArgumentSyntax>(
                        elementAccess.ArgumentList.Arguments.Take(idx).Concat(
                            elementAccess.ArgumentList.Arguments.Skip(idx).Select((arg, i) =>
                            {
                                if (arg.NameColon != null)
                                    return arg;
                                return arg.WithNameColon(SyntaxFactory.NameColon(indexer.Parameters[i + idx].Name));
                            })
                        )
                    );
                    var newAttribute = elementAccess.WithArgumentList(elementAccess.ArgumentList.WithArguments(newArguments));
                    var newRoot = root.ReplaceNode((SyntaxNode)elementAccess, newAttribute).WithAdditionalAnnotations(Formatter.Annotation);
                    return Task.FromResult(document.WithSyntaxRoot(newRoot));
                }
            );
        }

        static CodeAction CreateInvocationCodeAction(Document document, SyntaxNode root, ExpressionSyntax node, IMethodSymbol method, InvocationExpressionSyntax invocation)
        {
            var arguments = invocation.ArgumentList.Arguments;
            var idx = arguments.IndexOf(node.Parent as ArgumentSyntax);

            if (idx >= method.Parameters.Length) // this can happen with "params" parameters
                return null;
            var parameters = method.Parameters[idx];
            var name = parameters.Name;
            return CodeActionFactory.Create(
                node.Span,
                DiagnosticSeverity.Info,
                string.Format("Add argument name '{0}'", name),
                t2 =>
                {
                    var newArguments = SyntaxFactory.SeparatedList<ArgumentSyntax>(
                        invocation.ArgumentList.Arguments.Take(idx).Concat(
                            invocation.ArgumentList.Arguments.Skip(idx).Select((arg, i) =>
                            {
                                if (arg.NameColon != null)
                                    return arg;
                                return arg.WithNameColon(SyntaxFactory.NameColon(method.Parameters[i].Name));
                            })
                        )
                    );
                    var newAttribute = invocation.WithArgumentList(invocation.ArgumentList.WithArguments(newArguments));
                    var newRoot = root.ReplaceNode((SyntaxNode)invocation, newAttribute).WithAdditionalAnnotations(Formatter.Annotation);
                    return Task.FromResult(document.WithSyntaxRoot(newRoot));
                }
            );
        }

        protected override IEnumerable<CodeAction> GetActions(Document document, SemanticModel semanticModel, SyntaxNode root, TextSpan span, ExpressionSyntax node, CancellationToken cancellationToken)
        {
            if (!node.Parent.IsKind(SyntaxKind.Argument) && !node.Parent.IsKind(SyntaxKind.AttributeArgument))
                yield break;
            if (span.Start != node.SpanStart)
                yield break;
            var parent = node.Parent.Parent.Parent;
            var attribute = parent as AttributeSyntax;
            if (attribute != null)
            {
                var resolvedResult = semanticModel.GetSymbolInfo(attribute);
                var constructor = resolvedResult.Symbol as IMethodSymbol;
                if (constructor == null)
                    yield break;
                var codeAction = CreateAttributeCodeAction(document, root, node, constructor, attribute);
                if (codeAction != null)
                    yield return codeAction;
            }

            var indexerExpression = parent as ElementAccessExpressionSyntax;
            if (indexerExpression != null)
            {
                var resolvedResult = semanticModel.GetSymbolInfo(indexerExpression);
                var indexer = resolvedResult.Symbol as IPropertySymbol;
                if (indexer == null)
                    yield break;
                var codeAction = CreateIndexerCodeAction(document, root, node, indexer, indexerExpression);
                if (codeAction != null)
                    yield return codeAction;
            }

            var invocationExpression = parent as InvocationExpressionSyntax;
            if (invocationExpression != null)
            {
                var resolvedResult = semanticModel.GetSymbolInfo(invocationExpression);
                var method = resolvedResult.Symbol as IMethodSymbol;
                if (method == null)
                    yield break;
                var codeAction = CreateInvocationCodeAction(document, root, node, method, invocationExpression);
                if (codeAction != null)
                    yield return codeAction;
            }
        }
    }
}
