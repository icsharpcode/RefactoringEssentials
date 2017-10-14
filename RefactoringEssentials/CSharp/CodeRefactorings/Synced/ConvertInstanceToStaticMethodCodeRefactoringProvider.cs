using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.Text;
using Microsoft.CodeAnalysis.FindSymbols;

namespace RefactoringEssentials.CSharp
{
	/// <summary>
	/// Converts an instance method to a static method adding an additional parameter as "this" replacement.
	/// </summary>
	[ExportCodeRefactoringProvider(LanguageNames.CSharp, Name = "Convert instance to static method")]
    public class ConvertInstanceToStaticMethodCodeRefactoringProvider : SpecializedCodeRefactoringProvider<SyntaxNode>
    {
        protected override IEnumerable<CodeAction> GetActions(Document document, SemanticModel semanticModel, SyntaxNode root, TextSpan span, SyntaxNode node, CancellationToken cancellationToken)
        {
            MethodDeclarationSyntax methodDeclaration = node as MethodDeclarationSyntax;
            if (methodDeclaration == null)
                yield break;

            TypeDeclarationSyntax enclosingTypeDeclaration = methodDeclaration.Ancestors().OfType<TypeDeclarationSyntax>().FirstOrDefault();
            if (enclosingTypeDeclaration == null || enclosingTypeDeclaration is InterfaceDeclarationSyntax)
                yield break;
            if (methodDeclaration.Modifiers.Any(SyntaxKind.StaticKeyword))
                yield break;

            var declaringTypeSymbol = semanticModel.GetDeclaredSymbol(enclosingTypeDeclaration);
            var methodSymbol = semanticModel.GetDeclaredSymbol(methodDeclaration);

            yield return CodeActionFactory.Create(span, DiagnosticSeverity.Info, GettextCatalog.GetString("Convert to static method"), t2 =>
            {
                return PerformAction(document, semanticModel, root, enclosingTypeDeclaration, declaringTypeSymbol, methodDeclaration, methodSymbol, cancellationToken);
            });
        }

        class MethodReferencesInDocument
        {
            public readonly Document Document;
            public readonly IEnumerable<ReferencingInvocationExpression> References;

            public MethodReferencesInDocument(Document document, IEnumerable<ReferencingInvocationExpression> references)
            {
                this.Document = document;
                this.References = references;
            }
        }

        class ReferencingInvocationExpression
        {
            public readonly bool IsInChangedMethod;
            public readonly InvocationExpressionSyntax InvocationExpression;

            public ReferencingInvocationExpression(bool isInChangedMethod, InvocationExpressionSyntax invocationExpression)
            {
                this.IsInChangedMethod = isInChangedMethod;
                this.InvocationExpression = invocationExpression;
            }
        }

        async Task<Solution> PerformAction(Document document, SemanticModel model, SyntaxNode root, TypeDeclarationSyntax enclosingTypeDeclaration, INamedTypeSymbol declaringTypeSymbol, MethodDeclarationSyntax methodDeclaration, IMethodSymbol methodSymbol, CancellationToken cancellationToken)
        {
            // Collect all invocations of changed method
            var methodReferencesVisitor = new MethodReferencesVisitor(document.Project.Solution, methodSymbol, methodDeclaration, cancellationToken);
            await methodReferencesVisitor.Collect().ConfigureAwait(false);

            // Collect all references to type members and "this" expressions inside of changed method
            var memberReferencesVisitor = new MemberReferencesVisitor(model, declaringTypeSymbol.GetMembers().Where(m => m != methodSymbol), cancellationToken);
            memberReferencesVisitor.Collect(methodDeclaration.Body);

            Solution solution = document.Project.Solution;

            List<SyntaxNode> trackedNodesInMainDoc = new List<SyntaxNode>();
            trackedNodesInMainDoc.Add(methodDeclaration);
            var methodReferencesInMainDocument = methodReferencesVisitor.NodesToChange.FirstOrDefault(n => n.Document.Id == document.Id);
            if (methodReferencesInMainDocument != null)
            {
                trackedNodesInMainDoc.AddRange(methodReferencesInMainDocument.References.Select(r => r.InvocationExpression));
            }
            trackedNodesInMainDoc.AddRange(memberReferencesVisitor.NodesToChange);

            var newMainRoot = root.TrackNodes(trackedNodesInMainDoc);

            foreach (var invocationsInDocument in methodReferencesVisitor.NodesToChange)
            {
                SyntaxNode thisDocRoot = null;
                var thisDocumentId = invocationsInDocument.Document.Id;
                if (document.Id == thisDocumentId)
                {
                    // We are in same document as changed method declaration, reuse new root from outside
                    thisDocRoot = newMainRoot;
                }
                else
                {
                    thisDocRoot = await invocationsInDocument.Document.GetSyntaxRootAsync().ConfigureAwait(false);
                    if (thisDocRoot == null)
                        continue;
                    thisDocRoot = thisDocRoot.TrackNodes(invocationsInDocument.References.Select(r => r.InvocationExpression));
                }

                foreach (var referencingInvocation in invocationsInDocument.References)
                {
                    // Change this method invocation to invocation of a static method with instance parameter
                    var thisInvocation = thisDocRoot.GetCurrentNode(referencingInvocation.InvocationExpression);

                    ExpressionSyntax invocationExpressionPart = null;
                    SimpleNameSyntax methodName = null;
                    var memberAccessExpr = thisInvocation.Expression as MemberAccessExpressionSyntax;
                    if (memberAccessExpr != null)
                    {
                        invocationExpressionPart = memberAccessExpr.Expression;
                        methodName = memberAccessExpr.Name;
                    }

                    if (invocationExpressionPart == null)
                    {
                        var identifier = thisInvocation.Expression as IdentifierNameSyntax;
                        if (identifier != null)
                        {
                            // If changed method references itself, use "instance" as additional parameter! In other methods of affected class, use "this"!
                            if (referencingInvocation.IsInChangedMethod)
                                invocationExpressionPart = SyntaxFactory.IdentifierName("instance").WithLeadingTrivia(identifier.GetLeadingTrivia());
                            else
                                invocationExpressionPart = SyntaxFactory.ThisExpression().WithLeadingTrivia(identifier.GetLeadingTrivia());
                            methodName = identifier;
                        }
                    }

                    if (invocationExpressionPart == null)
                        continue;

                    List<ArgumentSyntax> invocationArguments = new List<ArgumentSyntax>();
                    invocationArguments.Add(SyntaxFactory.Argument(invocationExpressionPart.WithoutLeadingTrivia()));
                    invocationArguments.AddRange(referencingInvocation.InvocationExpression.ArgumentList.Arguments);

                    thisDocRoot = thisDocRoot.ReplaceNode(
                        thisInvocation,
                        SyntaxFactory.InvocationExpression(
                            SyntaxFactory.MemberAccessExpression(
                                SyntaxKind.SimpleMemberAccessExpression,
                                SyntaxFactory.IdentifierName(enclosingTypeDeclaration.Identifier.WithoutTrivia()).WithLeadingTrivia(invocationExpressionPart.GetLeadingTrivia()),
                                methodName.WithoutLeadingTrivia()
                            ),
                            SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList(invocationArguments)).WithAdditionalAnnotations(Formatter.Annotation)
                         ));
                }


                if (document.Id == thisDocumentId)
                {
                    // Write new root back to outside
                    newMainRoot = thisDocRoot;
                }
                else
                {
                    // Another document, replace it with modified version in solution
                    solution = solution.WithDocumentSyntaxRoot(thisDocumentId, thisDocRoot);
                }
            }

            foreach (var changedNode in memberReferencesVisitor.NodesToChange)
            {
                var trackedNode = newMainRoot.GetCurrentNode(changedNode);

                var thisExpression = trackedNode as ThisExpressionSyntax;
                if (thisExpression != null)
                {
                    // Replace "this" with instance parameter name
                    newMainRoot = newMainRoot.ReplaceNode(
                            thisExpression,
                            SyntaxFactory.IdentifierName("instance").WithLeadingTrivia(thisExpression.GetLeadingTrivia())
                        );
                }

                var memberIdentifier = trackedNode as IdentifierNameSyntax;
                if (memberIdentifier != null)
                {
                    newMainRoot = newMainRoot.ReplaceNode(
                            memberIdentifier,
                            SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                                SyntaxFactory.IdentifierName("instance").WithLeadingTrivia(memberIdentifier.GetLeadingTrivia()),
                                memberIdentifier.WithoutLeadingTrivia())
                        );
                }
            }

            List<ParameterSyntax> parameters = new List<ParameterSyntax>();
            parameters.Add(SyntaxFactory.Parameter(
                SyntaxFactory.List<AttributeListSyntax>(),
                SyntaxFactory.TokenList(),
                SyntaxFactory.ParseTypeName(enclosingTypeDeclaration.Identifier.ValueText),
                SyntaxFactory.Identifier("instance"), null)
                .WithAdditionalAnnotations(Formatter.Annotation));
            parameters.AddRange(methodDeclaration.ParameterList.Parameters);

            var staticModifierLeadingTrivia =
                methodDeclaration.Modifiers.Any() ? SyntaxFactory.TriviaList() : methodDeclaration.GetLeadingTrivia();
            var methodDeclarationLeadingTrivia =
                methodDeclaration.Modifiers.Any() ? methodDeclaration.GetLeadingTrivia() : SyntaxFactory.TriviaList();

            var trackedMethodDeclaration = newMainRoot.GetCurrentNode(methodDeclaration);
            newMainRoot = newMainRoot.ReplaceNode((SyntaxNode)trackedMethodDeclaration, trackedMethodDeclaration
                .WithLeadingTrivia(methodDeclarationLeadingTrivia)
                .WithModifiers(trackedMethodDeclaration.Modifiers.Add(SyntaxFactory.Token(SyntaxKind.StaticKeyword).WithLeadingTrivia(staticModifierLeadingTrivia).WithTrailingTrivia(SyntaxFactory.TriviaList(SyntaxFactory.Whitespace(" ")))))
                .WithParameterList(SyntaxFactory.ParameterList(SyntaxFactory.SeparatedList(parameters)).WithTrailingTrivia(trackedMethodDeclaration.ParameterList.GetTrailingTrivia())));
            return solution.WithDocumentSyntaxRoot(document.Id, newMainRoot);
        }

        class MethodReferencesVisitor
        {
            readonly Solution solution;
            readonly MethodDeclarationSyntax changedMethodDeclaration;
            readonly ISymbol methodSymbol;
            readonly CancellationToken cancellationToken;

            public readonly List<MethodReferencesInDocument> NodesToChange = new List<MethodReferencesInDocument>();

            public MethodReferencesVisitor(Solution solution, ISymbol methodSymbol, MethodDeclarationSyntax changedMethodDeclaration, CancellationToken cancellationToken)
            {
                this.solution = solution;
                this.methodSymbol = methodSymbol;
                this.changedMethodDeclaration = changedMethodDeclaration;
                this.cancellationToken = cancellationToken;
            }

            public async Task Collect()
            {
                var invocations = await SymbolFinder.FindCallersAsync(methodSymbol, solution).ConfigureAwait(false);
                var invocationsPerDocument = from invocation in invocations
                                             from location in invocation.Locations
                                             where location.SourceTree != null
                                             group location by location.SourceTree into locationGroup
                                             select locationGroup;

                foreach (var locationsInDocument in invocationsPerDocument)
                {
                    var document = solution.GetDocument(locationsInDocument.Key);
                    if (document == null)
                        continue;

                    var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
                    if (root == null)
                        continue;

                    NodesToChange.Add(new MethodReferencesInDocument(
                        document,
                        locationsInDocument.Select(loc =>
                        {
                            if (!loc.IsInSource)
                                return null;

                            var node = root.FindNode(loc.SourceSpan);
                            if (node == null)
                                return null;

                            var invocationExpression = node.AncestorsAndSelf().OfType<InvocationExpressionSyntax>().FirstOrDefault();
                            if (invocationExpression == null)
                                return null;

                            return new ReferencingInvocationExpression(invocationExpression.Ancestors().Contains(changedMethodDeclaration), invocationExpression);
                        })
                        .Where(r => r != null)));
                }
            }
        }

        class MemberReferencesVisitor : CSharpSyntaxWalker
        {
            readonly SemanticModel semanticModel;
            readonly IEnumerable<ISymbol> referenceSymbols;
            readonly CancellationToken cancellationToken;

            public readonly List<SyntaxNode> NodesToChange = new List<SyntaxNode>();

            public MemberReferencesVisitor(SemanticModel semanticModel, IEnumerable<ISymbol> referenceSymbols, CancellationToken cancellationToken)
            {
                this.semanticModel = semanticModel;
                this.referenceSymbols = referenceSymbols;
                this.cancellationToken = cancellationToken;
            }

            public void Collect(SyntaxNode root)
            {
                this.Visit(root);
            }

            public override void VisitIdentifierName(IdentifierNameSyntax node)
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (!(node.Parent is MemberAccessExpressionSyntax))
                {
                    var thisSymbolInfo = semanticModel.GetSymbolInfo(node);
                    if ((thisSymbolInfo.Symbol != null) && !(thisSymbolInfo.Symbol is ITypeSymbol))
                    {
                        if (referenceSymbols.Contains(thisSymbolInfo.Symbol))
                        {
                            NodesToChange.Add(node);
                        }
                    }
                }

                base.VisitIdentifierName(node);
            }

            public override void VisitThisExpression(ThisExpressionSyntax node)
            {
                cancellationToken.ThrowIfCancellationRequested();

                NodesToChange.Add(node);

                base.VisitThisExpression(node);
            }
        }
    }
}

