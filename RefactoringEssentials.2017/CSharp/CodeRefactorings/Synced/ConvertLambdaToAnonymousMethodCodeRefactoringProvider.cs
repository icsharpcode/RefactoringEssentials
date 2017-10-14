using System.Collections.Generic;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Formatting;

namespace RefactoringEssentials.CSharp.CodeRefactorings
{
    [ExportCodeRefactoringProvider(LanguageNames.CSharp, Name = "Convert lambda to anonymous method")]
    public class ConvertLambdaToAnonymousMethodCodeRefactoringProvider : CodeRefactoringProvider
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

            var token = root.FindToken(span.Start);
            if (!token.IsKind(SyntaxKind.EqualsGreaterThanToken))
                return;
            var node = token.Parent;
            if (!node.IsKind(SyntaxKind.ParenthesizedLambdaExpression) && !node.IsKind(SyntaxKind.SimpleLambdaExpression))
                return;

            context.RegisterRefactoring(
                CodeActionFactory.Create(
                    token.Span,
                    DiagnosticSeverity.Info,
                    GettextCatalog.GetString("To anonymous method"),
                    t2 =>
                    {
                        var parameters = new List<ParameterSyntax>();

                        CSharpSyntaxNode bodyExpr;
                        if (node.IsKind(SyntaxKind.ParenthesizedLambdaExpression))
                        {
                            var ple = (ParenthesizedLambdaExpressionSyntax)node;
                            parameters.AddRange(ConvertParameters(model, node, ple.ParameterList.Parameters));
                            bodyExpr = ple.Body;
                        }
                        else
                        {
                            var sle = ((SimpleLambdaExpressionSyntax)node);
                            parameters.AddRange(ConvertParameters(model, node, new[] { sle.Parameter }));
                            bodyExpr = sle.Body;
                        }

                        if (ConvertLambdaBodyExpressionToStatementCodeRefactoringProvider.RequireReturnStatement(model, node))
                        {
                            bodyExpr = SyntaxFactory.Block(SyntaxFactory.ReturnStatement(bodyExpr as ExpressionSyntax));
                        }
                        var ame = SyntaxFactory.AnonymousMethodExpression(
                            parameters.Count == 0 ? null : SyntaxFactory.ParameterList(SyntaxFactory.SeparatedList(parameters)),
                            bodyExpr as BlockSyntax ?? SyntaxFactory.Block(SyntaxFactory.ExpressionStatement(bodyExpr as ExpressionSyntax))
                        );

                        var newRoot = root.ReplaceNode((SyntaxNode)node, ame.WithAdditionalAnnotations(Formatter.Annotation));
                        return Task.FromResult(document.WithSyntaxRoot(newRoot));
                    }
                )
            );
        }

        static IEnumerable<ParameterSyntax> ConvertParameters(SemanticModel model, SyntaxNode lambda, IEnumerable<ParameterSyntax> list)
        {
            ITypeSymbol type = null;
            int i = 0;
            foreach (var param in list)
            {
                if (param.Type != null)
                {
                    yield return param;
                }
                else
                {
                    if (type == null)
                    {
                        var typeInfo = model.GetTypeInfo(lambda);
                        type = typeInfo.ConvertedType ?? typeInfo.Type;
                        if (type == null || !type.IsDelegateType())
                            yield break;
                    }

                    yield return SyntaxFactory.Parameter(
                        param.AttributeLists,
                        param.Modifiers,
                        SyntaxFactory.ParseTypeName(type.GetDelegateInvokeMethod().Parameters[i].Type.ToMinimalDisplayString(model, lambda.SpanStart)),
                        param.Identifier,
                        null
                    );
                }
                i++;
            }
        }
    }
}

