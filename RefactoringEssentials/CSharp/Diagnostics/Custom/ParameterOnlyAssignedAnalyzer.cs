using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System.Linq;

namespace RefactoringEssentials.CSharp.Diagnostics
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class ParameterOnlyAssignedAnalyzer : VariableOnlyAssignedAnalyzer
    {
        private static readonly DiagnosticDescriptor descriptor = new DiagnosticDescriptor(
            CSharpDiagnosticIDs.ParameterOnlyAssignedAnalyzerID,
            GettextCatalog.GetString("Parameter is assigned but its value is never used"),
            GettextCatalog.GetString("Parameter is assigned but its value is never used"),
            DiagnosticAnalyzerCategories.CodeQualityIssues,
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            helpLinkUri: HelpLink.CreateFor(CSharpDiagnosticIDs.ParameterOnlyAssignedAnalyzerID)
            );

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(descriptor);

        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.RegisterSyntaxNodeAction(
                AnalyzeParameterList,
                SyntaxKind.ParameterList
            );
        }

        private static void AnalyzeParameterList(SyntaxNodeAnalysisContext nodeContext)
        {
            var parameterList = nodeContext.Node as ParameterListSyntax;
            if (parameterList == null)
                return;

            var method = parameterList.Parent as MethodDeclarationSyntax;
            if (method == null || method.Body == null)
                return;

            var dataFlow = new System.Lazy<DataFlowAnalysis>(() => nodeContext.SemanticModel.AnalyzeDataFlow(method.Body));
            foreach (var parameter in parameterList.Parameters)
            {
                if (parameter.Modifiers.Any(SyntaxKind.OutKeyword) || parameter.Modifiers.Any(SyntaxKind.RefKeyword))
                    continue;

                var localParamSymbol = nodeContext.SemanticModel.GetDeclaredSymbol(parameter);
                if (localParamSymbol == null)
                    continue;
                
                if (dataFlow.Value.AlwaysAssigned.Except(dataFlow.Value.ReadInside).Contains(localParamSymbol))
                {
                    var statements = method.Body.Statements;
                    foreach (var statement in statements)
                    {
                        var expression = statement as ExpressionStatementSyntax;
                        var assignment = expression?.Expression as AssignmentExpressionSyntax;
                        if (assignment == null)
                            continue;
                        var symbol = nodeContext.SemanticModel.GetSymbolInfo(assignment.Left).Symbol as IParameterSymbol;
                        if (localParamSymbol.Equals(symbol))
                        {
                            nodeContext.ReportDiagnostic (Diagnostic.Create(descriptor, assignment.GetLocation()));
                        }
                    }
                }
            }
        }
    }
}