using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CodeFixes;
using System.Threading.Tasks;
using System.Threading;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;
using Microsoft.CodeAnalysis.Formatting;

namespace RefactoringEssentials.CSharp.Diagnostics
{
    [ExportCodeFixProvider(LanguageNames.CSharp), System.Composition.Shared]
    public class StringCompareToIsCultureSpecificCodeFixProvider : CodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds
        {
            get
            {
                return ImmutableArray.Create(CSharpDiagnosticIDs.StringCompareToIsCultureSpecificAnalyzerID);
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
            var node = root.FindNode(context.Span).SkipArgument() as InvocationExpressionSyntax;
            if (node == null)
                return;
            RegisterFix(context, root, diagnostic, node, "Ordinal", GettextCatalog.GetString("Use ordinal comparison"), cancellationToken);
            RegisterFix(context, root, diagnostic, node, "CurrentCulture", GettextCatalog.GetString("Use culture-aware comparison"), cancellationToken);
        }

        internal static void RegisterFix(CodeFixContext context, SyntaxNode root, Diagnostic diagnostic, InvocationExpressionSyntax invocationExpression, string stringComparison, string message, CancellationToken cancellationToken = default(CancellationToken))
        {
            var stringComparisonType = SyntaxFactory.ParseTypeName("System.StringComparison").WithAdditionalAnnotations(Microsoft.CodeAnalysis.Simplification.Simplifier.Annotation);
            var stringComparisonArgument = SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, stringComparisonType, (SimpleNameSyntax)SyntaxFactory.ParseName(stringComparison));
            var ma = invocationExpression.Expression as MemberAccessExpressionSyntax;
            var newArguments = SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList(new[] {
                SyntaxFactory.Argument(ma.Expression),
                invocationExpression.ArgumentList.Arguments[0],
                SyntaxFactory.Argument(stringComparisonArgument)
            }));

            var newInvocation = SyntaxFactory.InvocationExpression(SyntaxFactory.ParseExpression("string.Compare"), newArguments);
            var newRoot = root.ReplaceNode(invocationExpression, newInvocation.WithAdditionalAnnotations(Formatter.Annotation));

            context.RegisterCodeFix(CodeActionFactory.Create(invocationExpression.Span, diagnostic.Severity, message, context.Document.WithSyntaxRoot(newRoot)), diagnostic);
        }
    }
}