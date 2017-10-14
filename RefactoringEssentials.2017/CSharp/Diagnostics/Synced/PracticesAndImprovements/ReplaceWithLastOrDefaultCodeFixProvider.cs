using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Formatting;

namespace RefactoringEssentials.CSharp.Diagnostics
{
    [ExportCodeFixProvider(LanguageNames.CSharp), System.Composition.Shared]
    public class ReplaceWithLastOrDefaultCodeFixProvider : CodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds
        {
            get
            {
                return ImmutableArray.Create(CSharpDiagnosticIDs.ReplaceWithLastOrDefaultAnalyzerID);
            }
        }

        public async override Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var document = context.Document;
            var cancellationToken = context.CancellationToken;
            var span = context.Span;
            var diagnostics = context.Diagnostics;
            var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
            var diagnostic = diagnostics.First();
            var node = root.FindNode(context.Span) as ConditionalExpressionSyntax;
            //replace a conditional Any(x) ? Last(x) : null/default with LastOrDefault(x)
            var parameterExpr = ((InvocationExpressionSyntax)node.Condition).ArgumentList;
            var baseExpression = ((MemberAccessExpressionSyntax)((InvocationExpressionSyntax)node.Condition).Expression).Expression;
            var newNode = SyntaxFactory.InvocationExpression(SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, baseExpression,
                SyntaxFactory.IdentifierName("LastOrDefault")), parameterExpr);
            var newRoot = root.ReplaceNode((ExpressionSyntax)node, newNode.WithAdditionalAnnotations(Formatter.Annotation));
            context.RegisterCodeFix(CodeActionFactory.Create(node.Span, diagnostic.Severity, "Replace with 'LastOrDefault<T>()'", document.WithSyntaxRoot(newRoot)), diagnostic);
        }
    }
}