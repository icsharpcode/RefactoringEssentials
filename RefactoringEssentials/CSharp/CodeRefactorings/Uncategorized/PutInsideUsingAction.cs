using System.Threading;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.FindSymbols;
using Microsoft.CodeAnalysis.Formatting;

namespace RefactoringEssentials.CSharp.CodeRefactorings
{
    [ExportCodeRefactoringProvider(LanguageNames.CSharp, Name = "put inside 'using'")]
    public class PutInsideUsingAction : SpecializedCodeRefactoringProvider<VariableDeclaratorSyntax>
    {        
        protected override IEnumerable<CodeAction> GetActions(Document document, SemanticModel semanticModel, SyntaxNode root, TextSpan span, VariableDeclaratorSyntax node, CancellationToken cancellationToken)
        {
            var variableDeclaration = node.GetAncestor<VariableDeclarationSyntax>();
            var localDeclaration = node.GetAncestor<LocalDeclarationStatementSyntax>();

            if (node.Initializer == null)
                yield break;

            var symbol = (ILocalSymbol)semanticModel.GetDeclaredSymbol(node);

            if (!IsDisposable(semanticModel, symbol.Type))
                yield break;

            var containingBlock = node.GetAncestor<BlockSyntax>();

            yield return CodeActionFactory.Create(span, DiagnosticSeverity.Info, "Put inside 'using'", ct =>
            {                
                var insideUsing = containingBlock.Statements.SkipWhile(x => x != localDeclaration).Skip(1).ToList();

                for (int i = insideUsing.Count - 1; i >= 0; i--)
                {
                    var flow = semanticModel.AnalyzeDataFlow(insideUsing[i]);

                    if (flow.ReadInside.Contains(symbol) || flow.WrittenInside.Contains(symbol))
                    {
                        break;
                    }

                    insideUsing.RemoveAt(i);
                }

                var nodesToRemove = new List<SyntaxNode>(insideUsing);

                var beforeUsing = new List<StatementSyntax>();

                if (insideUsing.Any())
                {
                    var inUsingDataFlow = semanticModel.AnalyzeDataFlow(insideUsing.First(), insideUsing.Last());

                    var declaredVariablesUsedOutside = inUsingDataFlow.ReadOutside.Union(inUsingDataFlow.WrittenOutside)
                        .Distinct()
                        .Intersect(inUsingDataFlow.VariablesDeclared)
                        .Select(x => new {Symbol = x, Declarator = (VariableDeclaratorSyntax) x.DeclaringSyntaxReferences[0].GetSyntax(ct)})
                        .ToArray();
                   

                    for (var i = 0; i < insideUsing.Count; i++)
                    {
                        var stmt = insideUsing[i];

                        var localDeclarationStmt = stmt as LocalDeclarationStatementSyntax;

                        if (localDeclarationStmt == null)
                        {
                            continue;
                        }

                        nodesToRemove.Add(localDeclarationStmt);

                        var declaredVariables = localDeclarationStmt.Declaration
                            .Variables.Select(x => new {Symbol = semanticModel.GetDeclaredSymbol(x), Declarator = x})
                            .ToArray();

                        var variablesToMove = declaredVariables
                            .Intersect(declaredVariablesUsedOutside)
                            .ToArray();

                        if (!variablesToMove.Any())
                        {
                            continue;
                        }

                        var reducedLocalDeclaration = localDeclarationStmt.RemoveNodes(variablesToMove.Select(x => x.Declarator), SyntaxRemoveOptions.AddElasticMarker);

                        if (reducedLocalDeclaration.Declaration.Variables.Any())
                        {
                            insideUsing[i] = reducedLocalDeclaration;
                        }
                        else
                        {
                            insideUsing.RemoveAt(i);
                            i--;
                        }

                        foreach (var needAssignment in variablesToMove.Where(x => x.Declarator.Initializer != null))
                        {
                            insideUsing.Insert(i + 1, SyntaxFactory.ExpressionStatement(
                                SyntaxFactory.AssignmentExpression(
                                    SyntaxKind.SimpleAssignmentExpression,
                                    SyntaxFactory.IdentifierName(needAssignment.Declarator.Identifier),
                                    needAssignment.Declarator.Initializer.Value
                                    )
                                )
                                );
                        }

                        beforeUsing.Add(SyntaxFactory
                            .LocalDeclarationStatement(
                                SyntaxFactory.VariableDeclaration(
                                    localDeclarationStmt.Declaration.Type,
                                    SyntaxFactory.SeparatedList(variablesToMove.Select(x => x.Declarator.WithInitializer(null)))
                                    )

                            )
                            .WithAdditionalAnnotations(Formatter.Annotation)
                            );
                    }
                }

                var lastInUsingAsCall = (((insideUsing.LastOrDefault() as ExpressionStatementSyntax)?.Expression as InvocationExpressionSyntax)?.Expression as MemberAccessExpressionSyntax);

                if (lastInUsingAsCall != null && symbol.Equals(semanticModel.GetSymbolInfo(lastInUsingAsCall.Expression).Symbol))
                {
                    var dispose = semanticModel.Compilation.GetSpecialType(SpecialType.System_IDisposable).GetMembers("Dispose").Single();

                    if (dispose.Equals(semanticModel.GetSymbolInfo(lastInUsingAsCall).Symbol))
                    {
                        nodesToRemove.Add(insideUsing.Last());
                        insideUsing.RemoveAt(insideUsing.Count - 1);                                                
                    }
                }

                var usingVariableDeclaration = SyntaxFactory.VariableDeclaration(variableDeclaration.Type.WithoutTrivia(), SyntaxFactory.SingletonSeparatedList(node));

                var usingNode = SyntaxFactory.UsingStatement(usingVariableDeclaration, null, SyntaxFactory.Block(insideUsing))
                    .WithAdditionalAnnotations(Formatter.Annotation)
                    .WithPrependedLeadingTrivia(variableDeclaration.GetLeadingTrivia());

                var newRoot = root.TrackNodes(nodesToRemove.Concat(localDeclaration));

                newRoot = newRoot.RemoveNodes(newRoot.GetCurrentNodes<SyntaxNode>(nodesToRemove), SyntaxRemoveOptions.AddElasticMarker);

                newRoot = newRoot.InsertNodesAfter(newRoot.GetCurrentNode(localDeclaration), beforeUsing.Concat(usingNode));

                if (localDeclaration.Declaration.Variables.Count > 1)
                {
                    var remainingVariables = localDeclaration.Declaration.Variables.Except(new[] {node});
                    newRoot = newRoot.ReplaceNode(newRoot.GetCurrentNode(localDeclaration), 
                        localDeclaration.WithDeclaration(localDeclaration.Declaration.WithVariables(SyntaxFactory.SeparatedList(remainingVariables)))
                        .WithAdditionalAnnotations(Formatter.Annotation));
                }
                else
                {
                    newRoot = newRoot.RemoveNode(newRoot.GetCurrentNode(localDeclaration), SyntaxRemoveOptions.AddElasticMarker);
                }

                return Task.FromResult(document.WithSyntaxRoot(newRoot));
            });
        }

        private static bool IsDisposable(SemanticModel semanticModel, ITypeSymbol type)
        {
            var disposable = semanticModel.Compilation.GetSpecialType(SpecialType.System_IDisposable);

            if (type.GetAllInterfacesIncludingThis().Contains(disposable))
                return true;

            var parameterType = type as ITypeParameterSymbol;

            if (parameterType != null && parameterType.ConstraintTypes.Any(x => IsDisposable(semanticModel, x)))
                return true;

            return false;
        }
    }
}
