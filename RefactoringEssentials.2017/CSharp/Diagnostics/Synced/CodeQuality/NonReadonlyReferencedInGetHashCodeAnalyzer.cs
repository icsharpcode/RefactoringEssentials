using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace RefactoringEssentials.CSharp.Diagnostics
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class NonReadonlyReferencedInGetHashCodeAnalyzer : DiagnosticAnalyzer
    {
        static readonly DiagnosticDescriptor descriptor = new DiagnosticDescriptor(
            CSharpDiagnosticIDs.NonReadonlyReferencedInGetHashCodeAnalyzerID,
            GettextCatalog.GetString("Non-readonly field referenced in 'GetHashCode()'"),
            GettextCatalog.GetString("Non-readonly field referenced in 'GetHashCode()'"),
            DiagnosticAnalyzerCategories.CodeQualityIssues,
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            helpLinkUri: HelpLink.CreateFor(CSharpDiagnosticIDs.NonReadonlyReferencedInGetHashCodeAnalyzerID)
        );

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(descriptor);

        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.RegisterSyntaxNodeAction(
                (nodeContext) =>
                {
                    IEnumerable<Diagnostic> diagnostics;
                    if (TryGetDiagnostic(nodeContext, out diagnostics))
                        foreach (var diagnostic in diagnostics)
                            nodeContext.ReportDiagnostic(diagnostic);
                },
                SyntaxKind.MethodDeclaration
            );
        }

        static bool TryGetDiagnostic(SyntaxNodeAnalysisContext nodeContext, out IEnumerable<Diagnostic> diagnostic)
        {
            diagnostic = default(IEnumerable<Diagnostic>);
            var node = nodeContext.Node as MethodDeclarationSyntax;
            IMethodSymbol method = nodeContext.SemanticModel.GetDeclaredSymbol(node);
            if (method == null || method.Name != "GetHashCode" || !method.IsOverride || method.Parameters.Count() > 0)
                return false;
            if (method.ReturnType.SpecialType != SpecialType.System_Int32)
                return false;

            diagnostic = node
                .DescendantNodes()
                .OfType<IdentifierNameSyntax>()
                .Where(n => IsNonReadonlyField(nodeContext.SemanticModel, n))
                .Select(n => Diagnostic.Create(descriptor, n.GetLocation()));
            return true;
        }

        static bool IsNonReadonlyField(SemanticModel semanticModel, SyntaxNode node)
        {
            var symbol = semanticModel.GetSymbolInfo(node).Symbol as IFieldSymbol;
            return symbol != null && !symbol.IsReadOnly && !symbol.IsConst;
        }
    }
}