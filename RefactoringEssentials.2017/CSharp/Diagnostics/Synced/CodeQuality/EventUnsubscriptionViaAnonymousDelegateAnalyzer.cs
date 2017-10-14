using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace RefactoringEssentials.CSharp.Diagnostics
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class EventUnsubscriptionViaAnonymousDelegateAnalyzer : DiagnosticAnalyzer
    {
        static readonly DiagnosticDescriptor descriptor = new DiagnosticDescriptor(
            CSharpDiagnosticIDs.EventUnsubscriptionViaAnonymousDelegateAnalyzerID,
            GettextCatalog.GetString("Event unsubscription via anonymous delegate is useless"),
            GettextCatalog.GetString("Event unsubscription via anonymous delegate is useless"),
            DiagnosticAnalyzerCategories.CodeQualityIssues,
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            helpLinkUri: HelpLink.CreateFor(CSharpDiagnosticIDs.EventUnsubscriptionViaAnonymousDelegateAnalyzerID)
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
                SyntaxKind.SubtractAssignmentExpression
            );
        }

        static bool TryGetDiagnostic(SyntaxNodeAnalysisContext nodeContext, out Diagnostic diagnostic)
        {
            diagnostic = default(Diagnostic);
            var node = nodeContext.Node as AssignmentExpressionSyntax;
            if (!(node.Right.IsKind(SyntaxKind.AnonymousMethodExpression)
                      || node.Right.IsKind(SyntaxKind.SimpleLambdaExpression)
                      || node.Right.IsKind(SyntaxKind.ParenthesizedLambdaExpression)))
                return false;
            var rr = nodeContext.SemanticModel.GetSymbolInfo(node.Left);
            if (rr.Symbol.Kind != SymbolKind.Event)
                return false;
            diagnostic = Diagnostic.Create(
                descriptor,
                node.OperatorToken.GetLocation()
            );
            return true;
        }
    }
}