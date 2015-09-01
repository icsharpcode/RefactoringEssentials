using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System.Linq;

namespace RefactoringEssentials.CSharp.Diagnostics
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class LocalVariableNotUsedAnalyzer : DiagnosticAnalyzer
    {
        private static readonly DiagnosticDescriptor descriptor = new DiagnosticDescriptor(
            CSharpDiagnosticIDs.LocalVariableNotUsedAnalyzerID,
            GettextCatalog.GetString("Local variable is never used"),
            GettextCatalog.GetString("Local variable is never used"),
            DiagnosticAnalyzerCategories.RedundanciesInDeclarations,
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            helpLinkUri: HelpLink.CreateFor(CSharpDiagnosticIDs.LocalVariableNotUsedAnalyzerID)
        );

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(descriptor);

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(
                (nodeContext) =>
                {
                    Diagnostic diagnostic;
                    if (TryGetUnusedLocalVariableDiagnostic(nodeContext, out diagnostic))
                    {
                        nodeContext.ReportDiagnostic(diagnostic);
                    }
                },
                SyntaxKind.MethodDeclaration
            );
        }

        private static bool TryGetUnusedLocalVariableDiagnostic(SyntaxNodeAnalysisContext nodeContext, out Diagnostic diagnostic)
        {
            diagnostic = default(Diagnostic);
            if (nodeContext.IsFromGeneratedCode())
                return false;

            var method = nodeContext.Node as MethodDeclarationSyntax;
            if ((method == null) || (method.Body == null))
                return false;

            var dataFlow = nodeContext.SemanticModel.AnalyzeDataFlow(method.Body);
            
            var variablesDeclared = dataFlow.VariablesDeclared;
            var variablesRead = dataFlow.ReadInside.Union(dataFlow.ReadOutside);
            var unused = variablesDeclared.Except(variablesRead).Except(dataFlow.WrittenInside).ToArray();

            if (unused.Any())
            {
                foreach (var unusedVar in unused)
                {
                   
                    diagnostic = Diagnostic.Create(descriptor, unusedVar.Locations.First());
                    return true;
                }
            }
            return false;
        }

        private static bool TryGetDiagnostic(SyntaxNodeAnalysisContext nodeContext, out Diagnostic diagnostic)
        {
            diagnostic = default(Diagnostic);
            if (nodeContext.IsFromGeneratedCode())
                return false;

            var localDeclarationUnused = nodeContext.Node as LocalDeclarationStatementSyntax;
            var body = localDeclarationUnused?.Parent as BlockSyntax;
            if (body == null)
                return false;

            var dataFlow = nodeContext.SemanticModel.AnalyzeDataFlow(body);
            var variablesDeclared = dataFlow.VariablesDeclared;
            var variablesRead = dataFlow.ReadInside.Union(dataFlow.ReadOutside);
            var unused = variablesDeclared.Except(variablesRead).ToArray();
            if (unused == null)
                return false;

            if (localDeclarationUnused.Declaration == null || !localDeclarationUnused.Declaration.Variables.Any())
                return false; 

            var localDeclarationSymbol = nodeContext.SemanticModel.GetDeclaredSymbol(localDeclarationUnused.Declaration.Variables.FirstOrDefault());
            if (unused.Any())
            {
                if (unused.Contains(localDeclarationSymbol))
                {
                    diagnostic = Diagnostic.Create(descriptor, localDeclarationUnused.Declaration.GetLocation());
                    return true;
                }
            }
            return false;
        }
    }
}