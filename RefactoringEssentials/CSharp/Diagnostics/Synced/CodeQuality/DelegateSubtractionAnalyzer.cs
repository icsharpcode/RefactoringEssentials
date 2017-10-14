using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace RefactoringEssentials.CSharp.Diagnostics
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class DelegateSubtractionAnalyzer : DiagnosticAnalyzer
    {
        static readonly DiagnosticDescriptor descriptor = new DiagnosticDescriptor(
            CSharpDiagnosticIDs.DelegateSubtractionAnalyzerID,
            GettextCatalog.GetString("Delegate subtraction has unpredictable result"),
            GettextCatalog.GetString("Delegate subtraction has unpredictable result"),
            DiagnosticAnalyzerCategories.CodeQualityIssues,
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            helpLinkUri: HelpLink.CreateFor(CSharpDiagnosticIDs.DelegateSubtractionAnalyzerID)
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
                SyntaxKind.SubtractAssignmentExpression,
                SyntaxKind.SubtractExpression
            );
        }

        static bool TryGetDiagnostic(SyntaxNodeAnalysisContext nodeContext, out Diagnostic diagnostic)
        {
            diagnostic = default(Diagnostic);
            var assignment = nodeContext.Node as AssignmentExpressionSyntax;
            if (assignment != null)
            {
                if (IsDelegate(nodeContext.SemanticModel, assignment.Left) && IsDelegate(nodeContext.SemanticModel, assignment.Right))
                {
                    diagnostic = Diagnostic.Create(
                        descriptor,
                        assignment.GetLocation()
                    );
                    return true;
                }
            }
            var binex = nodeContext.Node as BinaryExpressionSyntax;
            if (binex != null)
            {
                if (IsDelegate(nodeContext.SemanticModel, binex.Left) && IsDelegate(nodeContext.SemanticModel, binex.Right))
                {
                    diagnostic = Diagnostic.Create(
                        descriptor,
                        binex.GetLocation()
                    );
                    return true;
                }
            }
            return false;
        }

        static bool IsDelegate(SemanticModel semanticModel, SyntaxNode node)
        {
            var info = semanticModel.GetSymbolInfo(node);
            if (info.Symbol == null || info.Symbol.IsKind(SymbolKind.Event))
                return false;
            var type = info.Symbol.GetReturnType();
            return type != null && type.TypeKind == TypeKind.Delegate;
        }
    }
}