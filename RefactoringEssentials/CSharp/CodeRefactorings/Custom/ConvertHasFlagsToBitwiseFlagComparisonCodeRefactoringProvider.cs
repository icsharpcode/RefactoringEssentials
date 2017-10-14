using System.Linq;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Formatting;

namespace RefactoringEssentials.CSharp.CodeRefactorings
{
    [ExportCodeRefactoringProvider(LanguageNames.CSharp, Name = "Replace 'Enum.HasFlag' call with bitwise flag comparison")]
    public class ConvertHasFlagsToBitwiseFlagComparisonCodeRefactoringProvider : CodeRefactoringProvider
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
            var node = root.FindToken(span.Start).Parent;
            if (node.Parent == null || node.Parent.Parent == null || !node.Parent.Parent.IsKind(SyntaxKind.InvocationExpression))
                return;
            var symbol = model.GetSymbolInfo(node.Parent).Symbol;

            if (symbol == null || symbol.Kind != SymbolKind.Method || symbol.ContainingType.SpecialType != SpecialType.System_Enum || symbol.Name != "HasFlag")
                return;
            var invocationNode = (InvocationExpressionSyntax)node.Parent.Parent;
            var arg = invocationNode.ArgumentList.Arguments.Select(a => a.Expression).First();
            if (!arg.DescendantNodesAndSelf().OfType<BinaryExpressionSyntax>().All(bop => bop.IsKind(SyntaxKind.BitwiseOrExpression)))
                return;

            context.RegisterRefactoring(
                CodeActionFactory.Create(node.Span, DiagnosticSeverity.Info, GettextCatalog.GetString("To bitwise flag comparison"), t2 => Task.FromResult(PerformAction(document, root, invocationNode)))
            );
        }

        static Document PerformAction(Document document, SyntaxNode root, InvocationExpressionSyntax invocationNode)
        {
            var arg = invocationNode.ArgumentList.Arguments.Select(a => a.Expression).First();
            if (!arg.DescendantNodesAndSelf().OfType<BinaryExpressionSyntax>().All(bop => bop.IsKind(SyntaxKind.BitwiseOrExpression)))
                return document;

            arg = ConvertBitwiseFlagComparisonToHasFlagsCodeRefactoringProvider.MakeFlatExpression(arg, SyntaxKind.BitwiseAndExpression);
            if (arg is BinaryExpressionSyntax)
                arg = SyntaxFactory.ParenthesizedExpression(arg);

            SyntaxNode nodeToReplace = invocationNode;
            while (nodeToReplace.Parent is ParenthesizedExpressionSyntax)
                nodeToReplace = nodeToReplace.Parent;

            bool negateHasFlags = nodeToReplace.Parent != null && nodeToReplace.Parent.IsKind(SyntaxKind.LogicalNotExpression);
            if (negateHasFlags)
                nodeToReplace = nodeToReplace.Parent;

            var expr = SyntaxFactory.BinaryExpression(
                negateHasFlags ? SyntaxKind.EqualsExpression : SyntaxKind.NotEqualsExpression,
                SyntaxFactory.ParenthesizedExpression(SyntaxFactory.BinaryExpression(SyntaxKind.BitwiseAndExpression, ((MemberAccessExpressionSyntax)invocationNode.Expression).Expression, arg))
                .WithAdditionalAnnotations(Formatter.Annotation),
                SyntaxFactory.ParseExpression("0")
            );

            var newRoot = root.ReplaceNode((SyntaxNode)
                nodeToReplace,
                expr.WithAdditionalAnnotations(Formatter.Annotation)
            );
            return document.WithSyntaxRoot(newRoot);
        }
    }
}