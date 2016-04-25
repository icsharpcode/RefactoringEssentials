using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace RefactoringEssentials.CSharp.Diagnostics
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class ReplaceWithFirstOrDefaultAnalyzer : DiagnosticAnalyzer
    {
        static readonly DiagnosticDescriptor descriptor = new DiagnosticDescriptor(
            CSharpDiagnosticIDs.ReplaceWithFirstOrDefaultAnalyzerID,
            GettextCatalog.GetString("Replace with call to FirstOrDefault<T>()"),
            GettextCatalog.GetString("Expression can be simlified to 'FirstOrDefault<T>()'"),
            DiagnosticAnalyzerCategories.PracticesAndImprovements,
            DiagnosticSeverity.Info,
            isEnabledByDefault: true,
            helpLinkUri: HelpLink.CreateFor(CSharpDiagnosticIDs.ReplaceWithFirstOrDefaultAnalyzerID)
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

            //pattern is Any(param) ? First(param) : null/default
            var anyInvocation = node.Condition as InvocationExpressionSyntax;
            var firstInvocation = node.WhenTrue as InvocationExpressionSyntax;
            var nullDefaultWhenFalse = node.WhenFalse;

            if (anyInvocation == null || firstInvocation == null || nullDefaultWhenFalse == null)
                return false;
            var anyExpression = anyInvocation.Expression as MemberAccessExpressionSyntax;
            if (anyExpression == null || anyExpression.Name.Identifier.ValueText != "Any")
                return false;
            var anyParam = anyInvocation.ArgumentList;

            var firstExpression = firstInvocation.Expression as MemberAccessExpressionSyntax;
            if (firstExpression == null || firstExpression.Name.Identifier.ValueText != "First" || !firstInvocation.ArgumentList.IsEquivalentTo(anyParam))
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