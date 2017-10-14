using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Nullaby;

namespace RefactoringEssentials.CSharp.Diagnostics
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class ConstantNullCoalescingConditionAnalyzer : DiagnosticAnalyzer
    {
        static readonly DiagnosticDescriptor descriptor = new DiagnosticDescriptor(
            CSharpDiagnosticIDs.ConstantNullCoalescingConditionAnalyzerID,
            GettextCatalog.GetString("Finds redundant null coalescing expressions such as expr ?? expr"),
            "{0}",
            DiagnosticAnalyzerCategories.PracticesAndImprovements,
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            helpLinkUri: HelpLink.CreateFor(CSharpDiagnosticIDs.ConstantNullCoalescingConditionAnalyzerID)
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
                    {
                        nodeContext.ReportDiagnostic(diagnostic);
                    }
                },
                new SyntaxKind[] { SyntaxKind.CoalesceExpression }
            );
        }

        static bool TryGetDiagnostic(SyntaxNodeAnalysisContext nodeContext, out Diagnostic diagnostic)
        {
            diagnostic = default(Diagnostic);

            var node = nodeContext.Node as BinaryExpressionSyntax;

            var flowAnalzyer = new FlowAnalyzer<NullFlowState>(nodeContext.SemanticModel, new NullFlowState(nodeContext.SemanticModel));

            var analyzer = flowAnalzyer.Analyze(nodeContext.Node);
            var state = analyzer.GetFlowState(node.Left);

            var leftState = state.GetReferenceState(node.Left);
            if (leftState == NullState.NotNull) {
                diagnostic = Diagnostic.Create (descriptor, node.Right.GetLocation (), "Remove redundant right side");
                return true;
            }
            if (leftState == NullState.Null) {
                diagnostic = Diagnostic.Create (descriptor, node.Left.GetLocation (), "Remove redundant left side");
                return true;
            }
            if (state.GetReferenceState(node.Right) == NullState.Null) {
                diagnostic = Diagnostic.Create (descriptor, node.Right.GetLocation (), "Remove redundant left side");
            }
            return false;
        }
    }
}