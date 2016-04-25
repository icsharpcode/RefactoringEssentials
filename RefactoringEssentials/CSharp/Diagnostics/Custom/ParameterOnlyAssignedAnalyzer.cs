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
                (nodeContext) =>
                {
                    Diagnostic diagnostic;
                    if (TryGetDiagnostic(nodeContext, out diagnostic))
                    {
                        nodeContext.ReportDiagnostic(diagnostic);
                    }
                },
                SyntaxKind.Parameter
                );
        }

        private static bool TryGetDiagnostic(SyntaxNodeAnalysisContext nodeContext, out Diagnostic diagnostic)
        {
            diagnostic = default(Diagnostic);

            var parameter = nodeContext.Node as ParameterSyntax;
            if (parameter == null)
                return false;

            if (parameter.Modifiers.Any(SyntaxKind.OutKeyword) || parameter.Modifiers.Any(SyntaxKind.RefKeyword))
                return false;

            var localParamSymbol = nodeContext.SemanticModel.GetDeclaredSymbol(parameter);
            if (localParamSymbol == null)
                return false;

            var method = parameter.Parent.Parent as MethodDeclarationSyntax;
            if (method == null || method.Body == null)
                return false;

            var dataFlow = nodeContext.SemanticModel.AnalyzeDataFlow(method.Body);
            if (dataFlow.AlwaysAssigned.Except(dataFlow.ReadInside).Contains(localParamSymbol))
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
                        diagnostic = Diagnostic.Create(descriptor, assignment.GetLocation());
                        return true;
                    }
                }
            }
            return false;
        }
    }
}