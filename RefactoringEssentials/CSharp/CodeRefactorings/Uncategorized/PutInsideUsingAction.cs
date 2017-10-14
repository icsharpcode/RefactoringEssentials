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

            if (node.Initializer == null || node.Parent.Parent != null && node.Parent.Parent.IsKind(SyntaxKind.UsingStatement))
                yield break;
            
            var symbol = semanticModel.GetDeclaredSymbol(node) as ILocalSymbol;
            if (symbol == null)
                yield break;

            if (!symbol.Type.ImplementsSpecialTypeInterface(SpecialType.System_IDisposable))
                yield break;

            var containingBlock = node.GetAncestor<BlockSyntax>();

            yield return CodeActionFactory.Create(span, DiagnosticSeverity.Info, "Put inside 'using'", ct =>
            {                
                var insideUsing = containingBlock.Statements.SkipWhile(x => x != localDeclaration).Skip(1).ToList();

                ReduceUsingBlock(semanticModel, insideUsing, symbol);

                var nodesToRemove = new List<SyntaxNode>(insideUsing);

                var beforeUsing = new List<StatementSyntax>();

                if (insideUsing.Any())
                {
                    ExtractVariableDeclarationsBeforeUsing(semanticModel, beforeUsing, insideUsing, nodesToRemove, ct);
                }

                if (IsEndingWithDispose(semanticModel, insideUsing, symbol))
                {
                    nodesToRemove.Add(insideUsing.Last());
                    insideUsing.RemoveAt(insideUsing.Count - 1);
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

        private static void ReduceUsingBlock(SemanticModel semanticModel, List<StatementSyntax> insideUsing, ILocalSymbol symbol)
        {
            for (int i = insideUsing.Count - 1; i >= 0; i--)
            {
                var flow = semanticModel.AnalyzeDataFlow(insideUsing[i]);

                if (flow.ReadInside.Contains(symbol) || flow.WrittenInside.Contains(symbol))
                {
                    break;
                }

                insideUsing.RemoveAt(i);
            }
        }

        private static void ExtractVariableDeclarationsBeforeUsing(SemanticModel semanticModel, List<StatementSyntax> beforeUsing, List<StatementSyntax> insideUsing, List<SyntaxNode> nodesToRemove, CancellationToken ct)
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
                    insideUsing.Insert(i + 1, needAssignment.Declarator.InitializerAsAssignment());
                }

                var localDeclarationTypeSyntax = localDeclarationStmt.Declaration.Type;

                if (localDeclarationTypeSyntax.IsVar)
                {
                    var localDeclarationType = (ITypeSymbol)semanticModel.GetSymbolInfo(localDeclarationTypeSyntax).Symbol;
                    localDeclarationTypeSyntax = localDeclarationType.ToSyntax(semanticModel, localDeclarationTypeSyntax);
                }

                beforeUsing.Add(SyntaxFactory.LocalDeclarationStatement(
                        SyntaxFactory.VariableDeclaration(
                            localDeclarationTypeSyntax,
                            SyntaxFactory.SeparatedList(variablesToMove.Select(x => x.Declarator.WithInitializer(null)))
                            )
                    )
                    .WithAdditionalAnnotations(Formatter.Annotation)
                    );
            }
        }

        private static bool IsEndingWithDispose(SemanticModel semanticModel, List<StatementSyntax> insideUsing, ILocalSymbol disposableLocal)
        {
            var lastInUsingAsCall = (((insideUsing.LastOrDefault() as ExpressionStatementSyntax)?.Expression as InvocationExpressionSyntax)?.Expression as MemberAccessExpressionSyntax);

            if (lastInUsingAsCall == null)
                return false;

            var targetSymbol = semanticModel.GetSymbolInfo(lastInUsingAsCall.Expression);

            if (!disposableLocal.Equals(targetSymbol.Symbol))
                return false;


            var dispose = semanticModel.Compilation.GetSpecialType(SpecialType.System_IDisposable).GetMembers("Dispose").Single();

            var calledMethod = semanticModel.GetSymbolInfo(lastInUsingAsCall).Symbol;

            if (!dispose.Equals(calledMethod))
                return false;


            return true;
        }
    }
}
