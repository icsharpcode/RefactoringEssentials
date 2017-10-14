using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CodeFixes;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;
using Microsoft.CodeAnalysis.Formatting;

namespace RefactoringEssentials.CSharp.Diagnostics
{

    [ExportCodeFixProvider(LanguageNames.CSharp), System.Composition.Shared]
    public class OperatorIsCanBeUsedIssueCodeFixProvider : CodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds
        {
            get
            {
                return ImmutableArray.Create(CSharpDiagnosticIDs.OperatorIsCanBeUsedAnalyzerID);
            }
        }

        public override FixAllProvider GetFixAllProvider()
        {
            return WellKnownFixAllProviders.BatchFixer;
        }

        public async override Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var document = context.Document;
            var cancellationToken = context.CancellationToken;
            var span = context.Span;
            var diagnostics = context.Diagnostics;
            var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
            var diagnostic = diagnostics.First();
            var node = root.FindNode(context.Span) as BinaryExpressionSyntax;

            ExpressionSyntax a;
            TypeSyntax b;
            InvocationExpressionSyntax left = node.Left as InvocationExpressionSyntax;

            //we know it's one or the other
            if (left != null)
            {
                a = left.Expression;
                b = ((TypeOfExpressionSyntax)node.Right).Type;
            }
            else
            {
                a = ((InvocationExpressionSyntax)node.Right).Expression;
                b = ((TypeOfExpressionSyntax)node.Left).Type;
            }
            var isExpr = SyntaxFactory.BinaryExpression(SyntaxKind.IsExpression, ((MemberAccessExpressionSyntax)a).Expression, b);
            var newRoot = root.ReplaceNode((SyntaxNode)node, isExpr.WithAdditionalAnnotations(Formatter.Annotation));
            context.RegisterCodeFix(CodeActionFactory.Create(node.Span, diagnostic.Severity, "Replace with 'is' operator", document.WithSyntaxRoot(newRoot)), diagnostic);
        }
    }
}