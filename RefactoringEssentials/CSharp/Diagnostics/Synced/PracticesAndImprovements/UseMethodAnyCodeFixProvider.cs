using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeAnalysis.Formatting;

namespace RefactoringEssentials.CSharp.Diagnostics.Synced.PracticesAndImprovements
{
    [ExportCodeFixProvider(LanguageNames.CSharp), System.Composition.Shared]
    public class UseMethodAnyCodeFixProvider : CodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(CSharpDiagnosticIDs.UseMethodAnyAnalyzerID);

        public override FixAllProvider GetFixAllProvider()
        {
            return WellKnownFixAllProviders.BatchFixer;
        }

        public async override Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var document = context.Document;
            var cancellationToken = context.CancellationToken;
            var diagnostics = context.Diagnostics;
            var root = await document.GetSyntaxRootAsync(cancellationToken);
            var diagnostic = diagnostics.First();
            var invocation = root.FindNode(context.Span) as InvocationExpressionSyntax;
            if (invocation == null)
                return;
        
            var newInvocation = CreateNewInvocation(invocation, root)
              .WithAdditionalAnnotations(Formatter.Annotation);
            if (newInvocation == null)
                return;

            context.RegisterCodeFix(CodeAction.Create("Replace with call to 'Any()'", async token =>
           {
               var editor = await DocumentEditor.CreateAsync(document, cancellationToken);
               editor.ReplaceNode(invocation.Parent as BinaryExpressionSyntax, newInvocation
                                   .WithLeadingTrivia(invocation.GetLeadingTrivia())
                                   .WithAdditionalAnnotations(Formatter.Annotation));
               var newDocument = editor.GetChangedDocument();
               return newDocument;
           }, string.Empty), diagnostic);
        }

        public static SyntaxNode ReplaceInvocation(InvocationExpressionSyntax invocation, ExpressionSyntax newInvocation, SyntaxNode root)
        {
            if (invocation == null)
                throw new ArgumentNullException(nameof(invocation));
            if (invocation.Parent.IsKind(SyntaxKind.LogicalNotExpression))
                return root.ReplaceNode(invocation.Parent, newInvocation);
            var negatedInvocation = SyntaxFactory.PrefixUnaryExpression(SyntaxKind.LogicalNotExpression, newInvocation);
            return root.ReplaceNode(invocation, negatedInvocation);
        }

        public static ExpressionSyntax CreateNewInvocation(InvocationExpressionSyntax invocation, SyntaxNode root)
        {
            if (invocation == null)
                throw new ArgumentNullException(nameof(invocation));
            var methodName = ((MemberAccessExpressionSyntax)invocation.Expression).Name.ToString();
            var nameToCheck = methodName == "Count" ? (SimpleNameSyntax)SyntaxFactory.ParseName("Any") : null;
            if (nameToCheck == null)
                return null;

            var newInvocation = invocation.WithExpression(((MemberAccessExpressionSyntax)invocation.Expression).WithName(nameToCheck));
            var comparisonExpression = (ExpressionSyntax)((LambdaExpressionSyntax)newInvocation.ArgumentList.Arguments.First().Expression).Body;
            var newComparisonExpression = CreateNewComparison(comparisonExpression);
            newComparisonExpression = RemoveParenthesis(newComparisonExpression);
            newInvocation = newInvocation.ReplaceNode(comparisonExpression, newComparisonExpression);
            return newInvocation;
        }

        public static ExpressionSyntax CreateNewComparison(ExpressionSyntax comparisonExpression)
        {
            if (comparisonExpression.IsKind(SyntaxKind.ConditionalExpression))
                return SyntaxFactory.BinaryExpression(SyntaxKind.EqualsExpression,
                    SyntaxFactory.ParenthesizedExpression(comparisonExpression),
                    SyntaxFactory.LiteralExpression(SyntaxKind.FalseLiteralExpression));
            if (comparisonExpression.IsKind(SyntaxKind.LogicalNotExpression))
                return ((PrefixUnaryExpressionSyntax)comparisonExpression).Operand;
            if (comparisonExpression.IsKind(SyntaxKind.EqualsExpression))
            {
                var comparisonBinary = (BinaryExpressionSyntax)comparisonExpression;
                if (comparisonBinary.Right.IsKind(SyntaxKind.TrueLiteralExpression))
                    return comparisonBinary.WithRight(SyntaxFactory.LiteralExpression(SyntaxKind.FalseLiteralExpression));
                if (comparisonBinary.Left.IsKind(SyntaxKind.TrueLiteralExpression))
                    return comparisonBinary.WithLeft(SyntaxFactory.LiteralExpression(SyntaxKind.FalseLiteralExpression));
                return (comparisonBinary.Right.IsKind(SyntaxKind.FalseLiteralExpression))? comparisonBinary.Left : comparisonBinary.Left.IsKind(SyntaxKind.FalseLiteralExpression) ? comparisonBinary.Right : CreateNewBinaryExpression(comparisonExpression, SyntaxKind.NotEqualsExpression);
            }
            if (comparisonExpression.IsKind(SyntaxKind.NotEqualsExpression))
            {
                var comparisonBinary = (BinaryExpressionSyntax)comparisonExpression;
                return (comparisonBinary.Right.IsKind(SyntaxKind.TrueLiteralExpression))? comparisonBinary.Left : comparisonBinary.Left.IsKind(SyntaxKind.TrueLiteralExpression) ? comparisonBinary.Right : CreateNewBinaryExpression(comparisonExpression, SyntaxKind.EqualsExpression);
            }
            if (comparisonExpression.IsKind(SyntaxKind.GreaterThanExpression))
                return CreateNewBinaryExpression(comparisonExpression, SyntaxKind.LessThanOrEqualExpression);
            if (comparisonExpression.IsKind(SyntaxKind.GreaterThanOrEqualExpression))
                return CreateNewBinaryExpression(comparisonExpression, SyntaxKind.LessThanExpression);
            if (comparisonExpression.IsKind(SyntaxKind.LessThanExpression))
                return CreateNewBinaryExpression(comparisonExpression, SyntaxKind.GreaterThanOrEqualExpression);
            if (comparisonExpression.IsKind(SyntaxKind.LessThanOrEqualExpression))
                return CreateNewBinaryExpression(comparisonExpression, SyntaxKind.GreaterThanExpression);
            if (comparisonExpression.IsKind(SyntaxKind.TrueLiteralExpression))
                return SyntaxFactory.LiteralExpression(SyntaxKind.FalseLiteralExpression);
            if (comparisonExpression.IsKind(SyntaxKind.FalseLiteralExpression))
                return SyntaxFactory.LiteralExpression(SyntaxKind.TrueLiteralExpression);
            return SyntaxFactory.BinaryExpression(SyntaxKind.EqualsExpression,
                comparisonExpression,
                SyntaxFactory.LiteralExpression(SyntaxKind.FalseLiteralExpression));
        }

        public static ExpressionSyntax RemoveParenthesis(ExpressionSyntax expression)
        {
            return expression != null && expression.IsKind(SyntaxKind.ParenthesizedExpression) ? ((ParenthesizedExpressionSyntax)expression).Expression : expression;
        }

        public static BinaryExpressionSyntax CreateNewBinaryExpression(ExpressionSyntax comparisonExpression, SyntaxKind kind)
        {
            var comparisonBinary = (BinaryExpressionSyntax)comparisonExpression;
            var left = comparisonBinary.Left;
            var newComparison = SyntaxFactory.BinaryExpression(kind, left.IsKind(SyntaxKind.ConditionalExpression) ? SyntaxFactory.ParenthesizedExpression(left) : left, comparisonBinary.Right);
            return newComparison;
        }
    }
}