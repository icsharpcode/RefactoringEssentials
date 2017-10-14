using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace RefactoringEssentials.CSharp.Diagnostics
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class PartialTypeWithSinglePartAnalyzer : DiagnosticAnalyzer
    {
        static readonly DiagnosticDescriptor descriptor = new DiagnosticDescriptor(
            CSharpDiagnosticIDs.PartialTypeWithSinglePartDiagnosticID,
            GettextCatalog.GetString("Class is declared partial but has only one part"),
            GettextCatalog.GetString("Partial class with single part"),
            DiagnosticAnalyzerCategories.RedundanciesInDeclarations,
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            helpLinkUri: HelpLink.CreateFor(CSharpDiagnosticIDs.PartialTypeWithSinglePartDiagnosticID),
            customTags: DiagnosticCustomTags.Unnecessary
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
                new SyntaxKind[] { SyntaxKind.ClassDeclaration }
            );
        }

        static bool TryGetDiagnostic(SyntaxNodeAnalysisContext nodeContext, out Diagnostic diagnostic)
        {
            var classDeclaration = nodeContext.Node as ClassDeclarationSyntax;
            diagnostic = default(Diagnostic);
            if (classDeclaration == null)
                return false;
            var modifier =
                classDeclaration
                    .Modifiers
                    .Cast<SyntaxToken?>()
                    .FirstOrDefault(m => m.Value.IsKind(SyntaxKind.PartialKeyword));
            if (!modifier.HasValue)
                return false;
            var symbol = nodeContext.SemanticModel.GetDeclaredSymbol(classDeclaration, nodeContext.CancellationToken);
            if (symbol == null || symbol.Locations.Length != 1)
                return false;

            diagnostic = Diagnostic.Create(descriptor, modifier.Value.GetLocation());
            return true;
        }
    }
}