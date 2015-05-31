using System.Collections.Generic;
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
    public class StringCompareIsCultureSpecificCodeFixProvider : CodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds
        {
            get
            {
                return ImmutableArray.Create(CSharpDiagnosticIDs.StringCompareIsCultureSpecificAnalyzerID);
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
            var model = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);
            var root = await model.SyntaxTree.GetRootAsync(cancellationToken).ConfigureAwait(false);

            var diagnostic = diagnostics.First();
            var node = root.FindNode(context.Span).SkipArgument() as InvocationExpressionSyntax;
            if (node == null)
                return;

            RegisterFix(context, model, root, diagnostic, node, "Ordinal", GettextCatalog.GetString("Use ordinal comparison"), cancellationToken);
            RegisterFix(context, model, root, diagnostic, node, "CurrentCulture", GettextCatalog.GetString("Use culture-aware comparison"), cancellationToken);
        }

        static void RegisterFix(CodeFixContext context, SemanticModel model, SyntaxNode root, Diagnostic diagnostic, InvocationExpressionSyntax invocationExpression, string stringComparison, string message, CancellationToken cancellationToken = default(CancellationToken))
        {
            bool? ignoreCase = null;
            ExpressionSyntax caseArg = null;

            if (invocationExpression.ArgumentList.Arguments.Count == 3)
            {
                var arg = model.GetConstantValue(invocationExpression.ArgumentList.Arguments[2].Expression, cancellationToken);
                if (arg.HasValue)
                {
                    ignoreCase = (bool)arg.Value;
                }
                else
                {
                    caseArg = invocationExpression.ArgumentList.Arguments[2].Expression;
                }
            }

            if (invocationExpression.ArgumentList.Arguments.Count == 6)
            {
                var arg = model.GetConstantValue(invocationExpression.ArgumentList.Arguments[5].Expression, cancellationToken);
                if (arg.HasValue)
                {
                    ignoreCase = (bool)arg.Value;
                }
                else
                {
                    caseArg = invocationExpression.ArgumentList.Arguments[5].Expression;
                }
            }
            var argumentList = new List<ArgumentSyntax>();
            if (invocationExpression.ArgumentList.Arguments.Count <= 3)
            {
                argumentList.AddRange(invocationExpression.ArgumentList.Arguments.Take(2));
            }
            else
            {
                argumentList.AddRange(invocationExpression.ArgumentList.Arguments.Take(5));
            }

            argumentList.Add(SyntaxFactory.Argument(CreateCompareArgument(invocationExpression, ignoreCase, caseArg, stringComparison)));
            var newArguments = SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList(argumentList));
            var newInvocation = SyntaxFactory.InvocationExpression(invocationExpression.Expression, newArguments);
            var newRoot = root.ReplaceNode(invocationExpression, newInvocation.WithAdditionalAnnotations(Formatter.Annotation));

            context.RegisterCodeFix(CodeActionFactory.Create(invocationExpression.Span, diagnostic.Severity, message, context.Document.WithSyntaxRoot(newRoot)), diagnostic);
        }

        static ExpressionSyntax CreateCompareArgument(InvocationExpressionSyntax invocationExpression, bool? ignoreCase, ExpressionSyntax caseArg, string stringComparison)
        {
            var stringComparisonType = SyntaxFactory.ParseTypeName("System.StringComparison").WithAdditionalAnnotations(Microsoft.CodeAnalysis.Simplification.Simplifier.Annotation);

            if (caseArg == null)
                return SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, stringComparisonType, (SimpleNameSyntax)SyntaxFactory.ParseName(ignoreCase == true ? stringComparison + "IgnoreCase" : stringComparison));

            return SyntaxFactory.ConditionalExpression(
                caseArg,
                SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, stringComparisonType, (SimpleNameSyntax)SyntaxFactory.ParseName(stringComparison + "IgnoreCase")),
                SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, stringComparisonType, (SimpleNameSyntax)SyntaxFactory.ParseName(stringComparison))
            );
        }
    }
}