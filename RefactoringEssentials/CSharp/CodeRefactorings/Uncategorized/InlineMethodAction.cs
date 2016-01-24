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

            var methodSymbol = model.GetDeclaredSymbol(method);
            var methodContainer = methodSymbol.ContainingType;

            // If the method is public then it may be referenced externally.
            // This is subjective and some may disagree with this choice of behavior.
            var isPublic = true;
            ISymbol cs = methodSymbol;
            do
            {
                if (cs.DeclaredAccessibility == Accessibility.Public ||
                    cs.DeclaredAccessibility == Accessibility.NotApplicable)
                    cs = cs.ContainingSymbol;
                else
                    isPublic = false;

            } while (isPublic && cs != null);

            if (isPublic)
                return;

            // Get references to the method.
            var methodReferences = await SymbolFinder.FindReferencesAsync(methodSymbol, document.Project.Solution, cancellationToken).ConfigureAwait(false);

            // Ensure there are references.
            if (!methodReferences.Any())
                return;
            // Ensure the reference has locations.
            if (!methodReferences.First().Locations.Any())
                return;

            // Ensure the method body is simple.
            var isSimpleMethodBody = method.Body.Statements.Count() == 1;

            if (!isSimpleMethodBody)
                return;

            // Are any references made from external types?
            var hasExternalReferences = methodReferences.Any(r =>
           {
               return r.Locations.Any(l =>
               {
                   var refType = model.GetEnclosingNamedType(l.Location.SourceSpan.Start, cancellationToken);
                   return (refType != methodContainer);
               });
           });

            // Ensure the method body does not reference internal members.
            if (hasExternalReferences)
            {
                var statement = method.Body.Statements.First();
                var descendents = statement.DescendantNodes();
                foreach (var descendent in descendents)
                {
                    var descendentSymbol = model.GetSymbolInfo(descendent);

                    if (descendentSymbol.Symbol != null)
                    {
                        if (descendentSymbol.Symbol.ContainingType == methodContainer)
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
                        method,
                        ct)));
        }

        /// <summary>
        /// Inline a method and return new solution.
        /// </summary>
        /// <param name="solution">The solution to modify.</param>
        /// <param name="methodSymbol">The symbol for the method.</param>
        /// <param name="methodDeclarationSyntax">The syntax for the method</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        async Task<Solution> InlineMethodAsync(
            Solution solution,
            ISymbol methodSymbol,
            MethodDeclarationSyntax methodDeclarationSyntax,
            CancellationToken cancellationToken)
        {
            // Get the method's param, these will be replaced for each location.
            var methodParams = methodDeclarationSyntax.ParameterList.Parameters;

            // Get method body.
            SyntaxNode methodBody = methodDeclarationSyntax.Body.Statements.First();

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

            // Remove trivia, we will not copy comments everywhere.
            methodBody = methodBody.WithoutTrivia();

            // Find all method references.
            var references = SymbolFinder.FindReferencesAsync(methodSymbol, solution).Result;

            // There is usually only a single reference, the method reference with referring locations.
            // Enumerate incase there are more.
            foreach (var reference in references)
            {
                // Group all referring locations by document.
                var groupedByDocument = from location in reference.Locations
                                        group location by location.Document into g
                                        select new
                                        {
                                            Document = g.Key,
                                            Locations = g
                                        };

                // Enumerate all referring documents and replace locations with method body.
                foreach (var g in groupedByDocument)
                {
                    var document = solution.GetDocument(g.Document.Id);
                    var model = await document.GetSemanticModelAsync(cancellationToken);
                    var root = await model.SyntaxTree.GetRootAsync(cancellationToken);

                    // Replace each referenced location in document.
                    foreach (var location in g.Locations)
                    {
                        var node = root.FindNode(location.Location.SourceSpan);

                        var invocationNode = node.AncestorsAndSelf().FirstOrDefault(n => n.IsKind(SyntaxKind.InvocationExpression)) as InvocationExpressionSyntax;

                        var replacementNode = methodBody;

                        // Replace method body's params with actual values.
                        var args = invocationNode.ArgumentList.Arguments;
                        for (var paramIdx = 0; paramIdx < methodParams.Count(); paramIdx++)
                        {
                            var param = methodParams[paramIdx];
                            var arg = (paramIdx < args.Count) ? args[paramIdx].Expression : param.Default.Value;

                            var ids = replacementNode.DescendantNodesAndSelf()
                                .Where(n => n.IsKind(SyntaxKind.IdentifierName))
                                .Where(n => (n as IdentifierNameSyntax).Identifier.ValueText == param.Identifier.ValueText);

                            replacementNode = replacementNode.ReplaceNodes(ids, (n1, n2) => arg).WithAdditionalAnnotations(Formatter.Annotation);
                        }

                        // Replace location with final method body.
                        root = root.ReplaceNode(invocationNode, replacementNode.WithAdditionalAnnotations(Formatter.Annotation));
                    }

                    // Replace document root.
                    solution = solution.WithDocumentSyntaxRoot(document.Id, root);
                }
            }

            // Because the method location may have changed after replacing references, find it again.
            methodSymbol = SymbolFinder.FindSourceDeclarationsAsync(
                solution,
                methodSymbol.Name,
                false,
                cancellationToken).Result.Single();

            references = SymbolFinder.FindReferencesAsync(methodSymbol, solution).Result;

            // Remove the method.
            foreach (var reference in references)
            {
                foreach (var declaringSyntaxReference in reference.Definition.DeclaringSyntaxReferences)
                {
                    var document = solution.GetDocument(declaringSyntaxReference.SyntaxTree);
                    var model = document.GetSemanticModelAsync().Result;
                    var root = model.SyntaxTree.GetRootAsync().Result;
                    var node = root.FindNode(declaringSyntaxReference.Span);
                    root = root.RemoveNode(node, SyntaxRemoveOptions.KeepNoTrivia);
                    solution = solution.WithDocumentSyntaxRoot(document.Id, root);
                }
            }

            // Return new solution.
            return await Task.FromResult(solution);
        }

    }
}
