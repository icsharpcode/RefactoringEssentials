using System.Linq;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Formatting;

namespace RefactoringEssentials.CSharp.CodeRefactorings
{
    [ExportCodeRefactoringProvider(LanguageNames.CSharp, Name = "Convert method group to lambda expression")]
    public class ConvertMethodGroupToLambdaCodeRefactoringProvider : CodeRefactoringProvider
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
            if (!node.IsKind(SyntaxKind.IdentifierName))
                return;

            var nodeGrandparent = node.Parent?.Parent;
            if ((nodeGrandparent is EventDeclarationSyntax) || (nodeGrandparent is EventFieldDeclarationSyntax))
                return;

            if (node.Parent.IsKind(SyntaxKind.SimpleMemberAccessExpression))
                node = node.Parent;

            var info = model.GetTypeInfo(node, cancellationToken);
            var type = info.ConvertedType ?? info.Type;
            if (type == null)
                return;

            var invocationMethod = type.GetDelegateInvokeMethod();
            if (invocationMethod == null)
                return;

            context.RegisterRefactoring(
                CodeActionFactory.Create(
                    node.Span,
                    DiagnosticSeverity.Info,
                    GettextCatalog.GetString("To lambda expression"),
                    t2 =>
                    {
                        var expr = SyntaxFactory.InvocationExpression(
                            (ExpressionSyntax)node,
                            SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList(invocationMethod.Parameters.Select(p => SyntaxFactory.Argument(SyntaxFactory.IdentifierName(p.Name)))))
                        );
                        var parameters = invocationMethod.Parameters.Select(p => CreateParameterSyntax(model, node, p)).ToList();
                        var ame = SyntaxFactory.ParenthesizedLambdaExpression(
                            SyntaxFactory.ParameterList(SyntaxFactory.SeparatedList(parameters)),
                            expr
                        );
                        var newRoot = root.ReplaceNode((SyntaxNode)node, ame.WithAdditionalAnnotations(Formatter.Annotation));
                        return Task.FromResult(document.WithSyntaxRoot(newRoot));
                    }
                )
            );
        }

        static ParameterSyntax CreateParameterSyntax(SemanticModel model, SyntaxNode node, IParameterSymbol p)
        {
            return SyntaxFactory.Parameter(
                SyntaxFactory.List<AttributeListSyntax>(),
                SyntaxFactory.TokenList(),
                null,
                SyntaxFactory.Identifier(p.Name),
                null
            );
        }
    }
}