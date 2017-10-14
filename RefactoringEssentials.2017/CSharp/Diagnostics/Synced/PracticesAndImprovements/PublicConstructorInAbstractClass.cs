using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace RefactoringEssentials.CSharp.Diagnostics
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class PublicConstructorInAbstractClassAnalyzer : DiagnosticAnalyzer
    {
        static readonly DiagnosticDescriptor descriptor = new DiagnosticDescriptor(
            CSharpDiagnosticIDs.PublicConstructorInAbstractClassAnalyzerID,
            GettextCatalog.GetString("Constructor in abstract class should not be public"),
            GettextCatalog.GetString("Constructor in abstract class should not be public"),
            DiagnosticAnalyzerCategories.PracticesAndImprovements,
            DiagnosticSeverity.Info,
            isEnabledByDefault: true,
            helpLinkUri: HelpLink.CreateFor(CSharpDiagnosticIDs.PublicConstructorInAbstractClassAnalyzerID)
        );

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(descriptor);

        public override void Initialize(AnalysisContext context)
        {
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
                new SyntaxKind[] { SyntaxKind.ConstructorDeclaration }
            );
        }

        static bool TryGetDiagnostic(SyntaxNodeAnalysisContext nodeContext, out Diagnostic diagnostic)
        {
            diagnostic = default(Diagnostic);
            var node = nodeContext.Node as ConstructorDeclarationSyntax;

            if (!node.Modifiers.Any(m => m.IsKind(SyntaxKind.PublicKeyword)))
                return false;

            var type = node.Parent as ClassDeclarationSyntax;
            if (type == null || !type.Modifiers.Any(m => m.IsKind(SyntaxKind.AbstractKeyword)))
                return false;

            diagnostic = Diagnostic.Create(descriptor, node.Identifier.GetLocation());
            return true;
        }
    }
}