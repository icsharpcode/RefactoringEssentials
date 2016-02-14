using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.FindSymbols;
using Microsoft.CodeAnalysis.Formatting;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RefactoringEssentials.CSharp.CodeRefactorings.Uncategorized
{
    [ExportCodeRefactoringProvider(
        LanguageNames.CSharp,
        Name = @"Put the method's body into the body of its callers and remove the method.")]
    public class InlineMethodAction : CodeRefactoringProvider
    {
        public override async Task ComputeRefactoringsAsync(CodeRefactoringContext context)
        {
            var document = context.Document;
            var span = context.Span;
            var cancellationToken = context.CancellationToken;

            var model = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);
            if (model.IsFromGeneratedCode(cancellationToken))
                return;

            var root = await model.SyntaxTree.GetRootAsync(cancellationToken).ConfigureAwait(false);
            if (root == null)
                return;

            var node = root.FindNode(span);
            if (node == null)
                return;

            // MethodDeclaration, InvocationExpression
            var method = node as MethodDeclarationSyntax;

            if (method == null)
            {
                method = (node?.Parent ?? null) as MethodDeclarationSyntax;
            }

            // Ensure we are testing at a method signature.
            if (method == null)
                return;

            // Ensure the method does not have errors.
            if (method.ContainsDiagnostics)
                return;

            // Ensure the method body is simple.
            var isSimpleMethodBody = method.Body.Statements.Count() == 1;

            if (!isSimpleMethodBody)
                return;

            // Get method body.
            SyntaxNode methodBody = method.Body.Statements.First();

            // The statement will either be a return or expression, get the expression for either.
            // Too bad there is not a more generic way of doing this.
            if (methodBody.IsKind(SyntaxKind.ReturnStatement))
            {
                var returnStatement = methodBody as ReturnStatementSyntax;
                methodBody = returnStatement.Expression;
            }
            else if (methodBody.IsKind(SyntaxKind.ExpressionStatement))
            {
                var statement = methodBody as ExpressionStatementSyntax;
                methodBody = statement.Expression;
            }
            else
            {
                // If we do not understand what the method body is then do not refactor.
                return;
            }

            var methodSymbol = model.GetDeclaredSymbol(method);

            // If the method is external then it may be called externally.
            var isExternalAccessibility = true;
            ISymbol cs = methodSymbol;
            do
            {
                if (cs.DeclaredAccessibility == Accessibility.Public ||
                    cs.DeclaredAccessibility == Accessibility.NotApplicable)
                    cs = cs.ContainingSymbol;
                else
                    isExternalAccessibility = false;

            } while (isExternalAccessibility && cs != null);

            if (isExternalAccessibility)
                return;

            // Get callers of the method.
            var callers = await SymbolFinder.FindCallersAsync(methodSymbol, document.Project.Solution, cancellationToken).ConfigureAwait(false);

            // Ensure there are callers.
            if (callers == null || !callers.Any())
                return;

            // Ensure the reference has locations.
            if (!callers.First().Locations.Any())
                return;

            var methodContainer = methodSymbol.ContainingType;

            // Are any references made from external types?
            var hasExternalReferences = callers.Any((SymbolCallerInfo r) =>
           {
               return (r.CallingSymbol.ContainingType != methodContainer);
           });

            // If the method is called externally, ensure the method body does not reference internal members.
            if (hasExternalReferences)
            {
                var statement = method.Body.Statements.First();
                var descendents = statement.DescendantNodes();
                foreach (var descendent in descendents)
                {
                    var descendentSymbol = model.GetSymbolInfo(descendent);

                    if (descendentSymbol.Symbol != null)
                    {
                        // If symbol is internal and not a parameter.
                        if (descendentSymbol.Symbol.ContainingType == methodContainer
                            && descendentSymbol.Symbol.ContainingSymbol != methodSymbol)
                        {
                            return;
                        }
                    }
                }
            }

            // Passed all checks, suggest refactoring.
            context.RegisterRefactoring(
                CodeAction.Create(
                    "Inline Method",
                    ct => InlineMethodAsync(
                        document.Project.Solution,
                        methodSymbol,
                        methodBody,
                        method.ParameterList,
                        ct)));
        }

        /// <summary>
        /// Inline a method and return new solution.
        /// </summary>
        /// <param name="solution">The solution to modify.</param>
        /// <param name="methodSymbol">The symbol of the method.</param>
        /// <param name="methodBody">The body of the method.</param>
        /// <param name="methodParams">The parameters of the method.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        async Task<Solution> InlineMethodAsync(
            Solution solution,
            ISymbol methodSymbol,
            SyntaxNode methodBody,
            ParameterListSyntax methodParams,
            CancellationToken cancellationToken)
        {
            // Remove trivia, we will not copy comments everywhere.
            methodBody = methodBody.WithoutTrivia();

            // Find all method callers.
            var callers = SymbolFinder.FindCallersAsync(methodSymbol, solution).Result;

            var locationsByDocument = from caller in callers
                                      from location in caller.Locations
                                      group location by location.SourceTree into g
                                      select new
                                      {
                                          SourceTree = g.Key,
                                          Locations = g
                                      };


            foreach (var locationByDocument in locationsByDocument)
            {
                // Get source tree.
                var tree = locationByDocument.SourceTree;
                if (tree == null) continue;

                // Get document.
                var document = solution.GetDocument(tree);
                if (document == null) continue;

                // Get root syntax.
                var root = await document.GetSyntaxRootAsync(cancellationToken);
                if (root == null) continue;

                // Enumerate locations and replace in root.
                foreach (var location in locationByDocument.Locations)
                {
                    if (!location.IsInSource) continue;

                    // Get node and ensure it exists.
                    var node = root.FindNode(location.SourceSpan);
                    if (node == null) continue;

                    // If location is invocation, replace it.
                    if (node.Parent.IsKind(SyntaxKind.InvocationExpression))
                    {
                        var invocationNode = node.Parent as InvocationExpressionSyntax;
                        root = ReplaceInvocationExpression(root, invocationNode, methodBody, methodParams);
                    }

                    // If the location is member access. foo.Bar(1, 2);
                    if (node.Parent != null
                        && node.Parent.IsKind(SyntaxKind.SimpleMemberAccessExpression)
                        && node.Parent.Parent != null
                        && node.Parent.Parent.IsKind(SyntaxKind.InvocationExpression))
                    {
                        var invocationNode = node.Parent.Parent as InvocationExpressionSyntax;
                        root = ReplaceInvocationExpression(root, invocationNode, methodBody, methodParams);
                    }

                    // If location is delegate, convert to lambda expression.
                    if (node.IsKind(SyntaxKind.Argument))
                    {
                        var argument = node as ArgumentSyntax;

                        SyntaxNode newArgument;
                        ParameterListSyntax paramList = SyntaxFactory.ParameterList();

                        // If method has params, prep them for the lambda.
                        if (methodParams.Parameters.Any())
                        {
                            var lambdaParams = from methodParam in methodParams.Parameters
                                               select SyntaxFactory.Parameter(methodParam.AttributeLists, methodParam.Modifiers, methodParam.Type, methodParam.Identifier, null);
                            var lambdaParamList = SyntaxFactory.SeparatedList<ParameterSyntax>(lambdaParams);
                            paramList = SyntaxFactory.ParameterList(lambdaParamList);
                        }

                        var lambda = SyntaxFactory.ParenthesizedLambdaExpression(paramList, methodBody as CSharpSyntaxNode);
                        newArgument = SyntaxFactory.Argument(lambda);

                        // Replace location with final method body.
                        root = root.ReplaceNode(argument, newArgument.WithAdditionalAnnotations(Formatter.Annotation));
                    }
                }

                // Replace document syntax root.
                solution = solution.WithDocumentSyntaxRoot(document.Id, root);
            }

            // Because the method location may have changed after replacing references, find it again.
            methodSymbol = SymbolFinder.FindSourceDeclarationsAsync(
                    solution,
                    methodSymbol.Name,
                    false,
                    cancellationToken).Result.Single();

            // Remove inlined method.
            foreach (var methodLocation in methodSymbol.Locations)
            {
                if (!methodLocation.IsInSource) continue;

                var tree = methodLocation.SourceTree;
                var documentId = solution.GetDocumentId(tree);
                var root = await tree.GetRootAsync(cancellationToken);
                var node = root.FindNode(methodLocation.SourceSpan);
                root = root.RemoveNode(node, SyntaxRemoveOptions.KeepNoTrivia);
                solution = solution.WithDocumentSyntaxRoot(documentId, root);
            }

            // Return new solution.
            return await Task.FromResult(solution);
        }

        /// <summary>
        /// Replace invocation with method body.
        /// </summary>
        /// <param name="root">The root of the document.</param>
        /// <param name="invocationNode">The invocation node to replace.</param>
        /// <param name="methodBody">The body of the inlined method.</param>
        /// <param name="methodParams">The parameters of the inlined method.</param>
        /// <returns>
        /// The new root with the invocation node replaced by the inline method body.
        /// </returns>
        SyntaxNode ReplaceInvocationExpression(
            SyntaxNode root,
            InvocationExpressionSyntax invocationNode,
            SyntaxNode methodBody,
            ParameterListSyntax methodParams)
        {
            var replacementNode = methodBody;

            var args = invocationNode.ArgumentList.Arguments;
            for (var paramIdx = 0; paramIdx < methodParams.Parameters.Count(); paramIdx++)
            {
                var param = methodParams.Parameters[paramIdx];
                var arg = (paramIdx < args.Count) ? args[paramIdx].Expression : param.Default.Value;

                var ids = replacementNode.DescendantNodesAndSelf()
                    .Where(n => n.IsKind(SyntaxKind.IdentifierName))
                    .Where(n => (n as IdentifierNameSyntax).Identifier.ValueText == param.Identifier.ValueText);

                replacementNode = replacementNode.ReplaceNodes(ids, (n1, n2) => arg).WithAdditionalAnnotations(Formatter.Annotation);
            }

            // Replace location with final method body.
            return root.ReplaceNode(invocationNode, replacementNode.WithAdditionalAnnotations(Formatter.Annotation));
        }

    }
}
