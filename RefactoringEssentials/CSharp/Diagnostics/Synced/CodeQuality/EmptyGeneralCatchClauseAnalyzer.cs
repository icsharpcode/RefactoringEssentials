using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace RefactoringEssentials.CSharp.Diagnostics
{
    /// <summary>
    /// A catch clause that catches System.Exception and has an empty body
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class EmptyGeneralCatchClauseAnalyzer : DiagnosticAnalyzer
    {
        static readonly DiagnosticDescriptor descriptor = new DiagnosticDescriptor(
            CSharpDiagnosticIDs.EmptyGeneralCatchClauseAnalyzerID,
            GettextCatalog.GetString("A catch clause that catches System.Exception and has an empty body"),
            GettextCatalog.GetString("Empty general catch clause suppresses any error"),
            DiagnosticAnalyzerCategories.CodeQualityIssues,
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            helpLinkUri: HelpLink.CreateFor(CSharpDiagnosticIDs.EmptyGeneralCatchClauseAnalyzerID)
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
                SyntaxKind.CatchClause
            );
        }

        static bool TryGetDiagnostic(SyntaxNodeAnalysisContext nodeContext, out Diagnostic diagnostic)
        {
            diagnostic = default(Diagnostic);
            var node = nodeContext.Node as CatchClauseSyntax;
            if (node.Declaration != null)
            {
                var type = node.Declaration.Type;
                if (type != null)
                {
                    var typeSymbol = nodeContext.SemanticModel.GetTypeInfo(type).Type;
                    if (typeSymbol == null || typeSymbol.TypeKind == TypeKind.Error || !typeSymbol.GetFullName().Equals("System.Exception"))
                        return false;
                }
            }

            // Don't consider a catch clause with "when (...)" as general
            if (node.Filter != null)
                return false;

            BlockSyntax body = node.Block;
            if (body.Statements.Any())
                return false;

            diagnostic = Diagnostic.Create(
                descriptor,
                node.CatchKeyword.GetLocation()
            );

            return true;
        }
    }
}