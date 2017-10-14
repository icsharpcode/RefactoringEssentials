using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace RefactoringEssentials.CSharp.Diagnostics
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class ReplaceWithLastOrDefaultAnalyzer : DiagnosticAnalyzer
    {
        static readonly DiagnosticDescriptor descriptor = new DiagnosticDescriptor(
            CSharpDiagnosticIDs.ReplaceWithLastOrDefaultAnalyzerID,
            GettextCatalog.GetString("Replace with call to LastOrDefault<T>()"),
            GettextCatalog.GetString("Expression can be simlified to 'LastOrDefault<T>()'"),
            DiagnosticAnalyzerCategories.PracticesAndImprovements,
            DiagnosticSeverity.Info,
            isEnabledByDefault: true,
            helpLinkUri: HelpLink.CreateFor(CSharpDiagnosticIDs.ReplaceWithLastOrDefaultAnalyzerID)
        );

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(descriptor);

        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.RegisterSyntaxNodeAction(
                (nodeContext) =>
                {
                    Diagnostic diagnostic;
                    if (TryGetDiagnostic(nodeContext, out diagnostic))
                        nodeContext.ReportDiagnostic(diagnostic);
                },
                SyntaxKind.ConditionalExpression
            );
        }

        static bool TryGetDiagnostic(SyntaxNodeAnalysisContext nodeContext, out Diagnostic diagnostic)
        {
            diagnostic = default(Diagnostic);
            var node = nodeContext.Node as ConditionalExpressionSyntax;

            //pattern is Any(param) ? Last(param) : null/default
            var anyInvocation = node.Condition as InvocationExpressionSyntax;
            var lastInvocation = node.WhenTrue as InvocationExpressionSyntax;
            var nullDefaultWhenFalse = node.WhenFalse;

            if (anyInvocation == null || lastInvocation == null || nullDefaultWhenFalse == null)
                return false;
            var anyExpression = anyInvocation.Expression as MemberAccessExpressionSyntax;
            if (anyExpression == null || anyExpression.Name.Identifier.ValueText != "Any")
                return false;
            var anyParam = anyInvocation.ArgumentList;

            var lastExpression = lastInvocation.Expression as MemberAccessExpressionSyntax;
            if (lastExpression == null || lastExpression.Name.Identifier.ValueText != "Last" || !lastInvocation.ArgumentList.IsEquivalentTo(anyParam))
                return false;

            if (!nullDefaultWhenFalse.IsKind(SyntaxKind.NullLiteralExpression) && !nullDefaultWhenFalse.IsKind(SyntaxKind.DefaultExpression))
                return false;

            diagnostic = Diagnostic.Create(
                descriptor,
                node.GetLocation()
            );
            return true;
        }
    }
}