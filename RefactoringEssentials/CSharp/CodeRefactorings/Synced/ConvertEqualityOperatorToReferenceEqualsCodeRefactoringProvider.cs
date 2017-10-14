using System.Linq;
using System.Collections.Generic;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Formatting;

namespace RefactoringEssentials.CSharp.CodeRefactorings
{
    [ExportCodeRefactoringProvider(LanguageNames.CSharp, Name = "Convert '==' to 'object.ReferenceEquals()'")]
    public class ConvertEqualityOperatorToReferenceEqualsCodeRefactoringProvider : CodeRefactoringProvider
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
            var node = root.FindNode(span) as BinaryExpressionSyntax;
            if (node == null || !(node.IsKind(SyntaxKind.EqualsExpression) || node.IsKind(SyntaxKind.NotEqualsExpression)))
                return;

            var leftType = model.GetTypeInfo(node.Left).Type;
            var rightType = model.GetTypeInfo(node.Right).Type;
            if (leftType == null || rightType == null || leftType.IsValueType || rightType.IsValueType)
                return;

            context.RegisterRefactoring(
                CodeActionFactory.Create(
                    span,
                    DiagnosticSeverity.Info,
                    GettextCatalog.GetString("To 'ReferenceEquals' call"),
                    t2 => Task.FromResult(document.WithSyntaxRoot(root.ReplaceNode((SyntaxNode)node, CreateEquals(model, node))))
                )
            );
        }

        SyntaxNode CreateEquals(SemanticModel model, BinaryExpressionSyntax node)
        {
            var expr = SyntaxFactory.InvocationExpression(
                GenerateTarget(model, node),
                SyntaxFactory.ArgumentList(
                    new SeparatedSyntaxList<ArgumentSyntax>()
                    .Add(SyntaxFactory.Argument(node.Left))
                    .Add(SyntaxFactory.Argument(node.Right))
                )
            );
            if (node.IsKind(SyntaxKind.NotEqualsExpression))
                return SyntaxFactory.PrefixUnaryExpression(SyntaxKind.LogicalNotExpression, expr).WithAdditionalAnnotations(Formatter.Annotation);
            return expr.WithAdditionalAnnotations(Formatter.Annotation);
        }

        ExpressionSyntax GenerateTarget(SemanticModel model, BinaryExpressionSyntax node)
        {
            var symbols = model.LookupSymbols(node.SpanStart).OfType<IMethodSymbol>();
            if (!symbols.Any() || HasDifferentEqualsMethod(symbols))
                return SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, SyntaxFactory.ParseExpression("object"), SyntaxFactory.IdentifierName("ReferenceEquals"));
            else
                return SyntaxFactory.IdentifierName("ReferenceEquals");
        }

        static bool HasDifferentEqualsMethod(IEnumerable<IMethodSymbol> symbols)
        {
            foreach (IMethodSymbol method in symbols)
            {
                if (method.Name == "ReferenceEquals" && method.Parameters.Count() == 2 && method.ToDisplayString() != "object.ReferenceEquals(object, object)")
                    return true;
            }
            return false;
        }
    }
}