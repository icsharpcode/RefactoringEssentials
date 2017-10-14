using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.Formatting;
using System.Linq;
using System.Collections.Generic;
using Microsoft.CodeAnalysis.Text;
using Microsoft.CodeAnalysis.Simplification;

namespace RefactoringEssentials.CSharp.CodeRefactorings
{
	[ExportCodeRefactoringProvider(LanguageNames.CSharp, Name = "Extract anonymous method")]
    public class ExtractAnonymousMethodCodeRefactoringProvider : CodeRefactoringProvider
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
            var root = await model.SyntaxTree.GetRootAsync(cancellationToken).ConfigureAwait(false);

            foreach (var refactoring in GetActions(document, model, root, span))
            {
                context.RegisterRefactoring(refactoring);
            }
        }

        public IEnumerable<CodeAction> GetActions(Document document, SemanticModel model, SyntaxNode root, TextSpan span)
        {
            var token = root.FindToken(span.Start);
            var lambdaExpression = token.GetAncestor<LambdaExpressionSyntax>();
            if ((lambdaExpression != null) && (lambdaExpression.ArrowToken.FullSpan.Contains(span)))
            {
                if (ContainsLocalReferences(model, lambdaExpression, lambdaExpression.Body))
                    yield break;

                var lambdaSymbol = model.GetSymbolInfo(lambdaExpression).Symbol as IMethodSymbol;
                if (lambdaSymbol == null)
                    yield break;

                bool noReturn = false;
                BlockSyntax body;
                if (lambdaExpression.Body is BlockSyntax)
                {
                    body = SyntaxFactory.Block(((BlockSyntax)lambdaExpression.Body).Statements);
                }
                else
                {
                    if (!(lambdaExpression.Body is ExpressionSyntax))
                        yield break;
                    body = SyntaxFactory.Block();

                    if ((lambdaSymbol.ReturnType == null) || lambdaSymbol.ReturnsVoid)
                    {
                        body = body.AddStatements(SyntaxFactory.ExpressionStatement((ExpressionSyntax)lambdaExpression.Body));
                    }
                    else
                    {
                        body = body.AddStatements(SyntaxFactory.ReturnStatement((ExpressionSyntax)lambdaExpression.Body));
                    }
                }
                var method = GetMethod(model, span, lambdaSymbol, body);
                yield return GetAction(document, model, root, lambdaExpression, method);
            }

            // anonymous method
            var anonymousMethod = token.GetAncestor<AnonymousMethodExpressionSyntax>();
            if ((anonymousMethod != null) && anonymousMethod.DelegateKeyword.FullSpan.Contains(span))
            {
                if (ContainsLocalReferences(model, anonymousMethod, anonymousMethod.Body))
                    yield break;

                var lambdaSymbol = model.GetSymbolInfo(anonymousMethod).Symbol as IMethodSymbol;
                if (lambdaSymbol == null)
                    yield break;
                var method = GetMethod(model, span, lambdaSymbol, SyntaxFactory.Block(((BlockSyntax)anonymousMethod.Body).Statements));
                yield return GetAction(document, model, root, anonymousMethod, method);
            }
        }

        static bool ContainsLocalReferences(SemanticModel context, SyntaxNode expr, SyntaxNode body)
        {
            var dataFlowAnalysis = context.AnalyzeDataFlow(expr);
            return dataFlowAnalysis.Captured.Any();
        }

        CodeAction GetAction(Document document, SemanticModel model, SyntaxNode root, SyntaxNode node, MethodDeclarationSyntax method)
        {
            return CodeActionFactory.Create(node.Span, DiagnosticSeverity.Info, GettextCatalog.GetString("Extract anonymous method"), t2 =>
            {
                var identifier = SyntaxFactory.IdentifierName(NameGenerator.EnsureUniqueness("Method", model.LookupSymbols(node.SpanStart).Select(s => s.Name)));
                var surroundingMemberDeclaration = node.GetAncestor<MemberDeclarationSyntax>();
                var rootWithTrackedMember = root.TrackNodes(node, surroundingMemberDeclaration);
                var newRoot = rootWithTrackedMember.ReplaceNode(rootWithTrackedMember.GetCurrentNode(node), identifier);
                newRoot = newRoot
                    .InsertNodesBefore(newRoot.GetCurrentNode(surroundingMemberDeclaration),
                    new[] { method.WithTrailingTrivia(surroundingMemberDeclaration.GetTrailingTrivia()) });
                return Task.FromResult(document.WithSyntaxRoot(newRoot));
            });
        }

        static MethodDeclarationSyntax GetMethod(SemanticModel context, TextSpan span, IMethodSymbol lambdaSymbol, BlockSyntax body)
        {
            TypeSyntax returnType = null;
            if (lambdaSymbol.ReturnsVoid)
            {
                returnType = SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.VoidKeyword));
            }
            else
            {
                var type = lambdaSymbol.ReturnType;
                returnType = type.TypeKind == TypeKind.Unknown ?
                    SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.VoidKeyword)) :
                    SyntaxFactory.ParseTypeName(type.Name).WithAdditionalAnnotations(Simplifier.Annotation);
            }

            var methodParameters = SyntaxFactory.ParameterList(
                SyntaxFactory.SeparatedList(lambdaSymbol.Parameters.Select(p => SyntaxFactory.Parameter(
                    SyntaxFactory.List<AttributeListSyntax>(),
                    SyntaxFactory.TokenList(),
                    SyntaxFactory.ParseTypeName(p.Type.Name),
                    SyntaxFactory.Identifier(p.Name),
                    null
                ))
            ));

            var method = SyntaxFactory.MethodDeclaration(returnType, NameGenerator.EnsureUniqueness("Method", context.LookupSymbols(span.Start).Select(s => s.Name)))
                .WithParameterList(methodParameters)
                .WithBody(body);

            if (lambdaSymbol.IsAsync)
                method = method.WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.AsyncKeyword)));

            return method.WithAdditionalAnnotations(Formatter.Annotation, Simplifier.Annotation);
        }
    }
}
