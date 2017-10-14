using System;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Formatting;

namespace RefactoringEssentials.CSharp.CodeRefactorings
{
    [ExportCodeRefactoringProvider(LanguageNames.CSharp, Name = "Replace assignment with operator assignment")]
    public class ReplaceWithOperatorAssignmentCodeRefactoringProvider : CodeRefactoringProvider
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
            if (!token.IsKind(SyntaxKind.EqualsToken))
                return;
            var node = token.Parent as AssignmentExpressionSyntax;
            if (node == null)
                return;
            var assignment = CreateAssignment(node);
            if (assignment == null)
                return;
            assignment = assignment.WithAdditionalAnnotations(Formatter.Annotation);
            context.RegisterRefactoring(
                CodeActionFactory.Create(span, DiagnosticSeverity.Info, String.Format(GettextCatalog.GetString("To '{0}='"), node.Left.ToString()), document.WithSyntaxRoot(
                root.ReplaceNode((SyntaxNode)node, assignment)))
            );
        }

        internal static ExpressionSyntax GetOuterLeft(BinaryExpressionSyntax bop)
        {
            var leftBop = bop.Left as BinaryExpressionSyntax;
            if (leftBop != null && bop.OperatorToken.IsKind(leftBop.OperatorToken.Kind()))
                return GetOuterLeft(leftBop);
            return bop.Left;
        }

        internal static AssignmentExpressionSyntax CreateAssignment(AssignmentExpressionSyntax node)
        {
            var bop = node.Right as BinaryExpressionSyntax;
            if (bop == null)
                return null;
            var outerLeft = GetOuterLeft(bop);
			var outerLeftId = outerLeft as IdentifierNameSyntax;
			var leftId = node.Left as IdentifierNameSyntax;
			if (outerLeftId == null || leftId == null)
				return null;
			if (!outerLeftId.Identifier.Value.Equals (leftId.Identifier.Value))
                return null;
            var op = GetAssignmentOperator(bop.OperatorToken);
            if (op == SyntaxKind.None)
                return null;
            return SyntaxFactory.AssignmentExpression(op, node.Left, SplitIfWithAndConditionInTwoCodeRefactoringProvider.GetRightSide(outerLeft.Parent as BinaryExpressionSyntax));
        }

        internal static SyntaxKind GetAssignmentOperator(SyntaxToken token)
        {
            switch (token.Kind())
            {
                case SyntaxKind.AmpersandToken:
                    return SyntaxKind.AndAssignmentExpression;
                case SyntaxKind.BarToken:
                    return SyntaxKind.OrAssignmentExpression;
                case SyntaxKind.CaretToken:
                    return SyntaxKind.ExclusiveOrAssignmentExpression;
                case SyntaxKind.PlusToken:
                    return SyntaxKind.AddAssignmentExpression;
                case SyntaxKind.MinusToken:
                    return SyntaxKind.SubtractAssignmentExpression;
                case SyntaxKind.AsteriskToken:
                    return SyntaxKind.MultiplyAssignmentExpression;
                case SyntaxKind.SlashToken:
                    return SyntaxKind.DivideAssignmentExpression;
                case SyntaxKind.PercentToken:
                    return SyntaxKind.ModuloAssignmentExpression;
                case SyntaxKind.LessThanLessThanToken:
                    return SyntaxKind.LeftShiftAssignmentExpression;
                case SyntaxKind.GreaterThanGreaterThanToken:
                    return SyntaxKind.RightShiftAssignmentExpression;
                default:
                    return SyntaxKind.None;
            }
        }
    }
}
