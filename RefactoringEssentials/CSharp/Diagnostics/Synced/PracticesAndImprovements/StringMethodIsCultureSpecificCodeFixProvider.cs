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
    public class StringMethodIsCultureSpecificCodeFixProvider : CodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds
        {
            get
            {
                return ImmutableArray.Create(
                    CSharpDiagnosticIDs.StringIndexOfIsCultureSpecificAnalyzerID,
                    CSharpDiagnosticIDs.StringEndsWithIsCultureSpecificAnalyzerID,
                    CSharpDiagnosticIDs.StringLastIndexOfIsCultureSpecificAnalyzerID,
                    CSharpDiagnosticIDs.StringStartsWithIsCultureSpecificAnalyzerID
                );
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
            var node = root.FindNode(context.Span).SkipArgument() as InvocationExpressionSyntax;
            RegisterFix(context, root, diagnostic, node, "Ordinal", cancellationToken);
            RegisterFix(context, root, diagnostic, node, "CurrentCulture", cancellationToken);
        }

        internal static void RegisterFix(CodeFixContext context, SyntaxNode root, Diagnostic diagnostic, InvocationExpressionSyntax invocationExpression, string stringComparison, CancellationToken cancellationToken = default(CancellationToken))
        {
            var stringComparisonType = SyntaxFactory.ParseTypeName("System.StringComparison").WithAdditionalAnnotations(Microsoft.CodeAnalysis.Simplification.Simplifier.Annotation);
            var stringComparisonArgument = SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, stringComparisonType, (SimpleNameSyntax)SyntaxFactory.ParseName(stringComparison));
            var newArguments = invocationExpression.ArgumentList.AddArguments(SyntaxFactory.Argument(stringComparisonArgument));
            var newInvocation = SyntaxFactory.InvocationExpression(invocationExpression.Expression, newArguments);
            var newRoot = root.ReplaceNode(invocationExpression, newInvocation.WithAdditionalAnnotations(Formatter.Annotation));

            context.RegisterCodeFix(CodeActionFactory.Create(invocationExpression.Span, diagnostic.Severity, string.Format("Add 'StringComparison.{0}'", stringComparison), context.Document.WithSyntaxRoot(newRoot)), diagnostic);
        }
    }
}