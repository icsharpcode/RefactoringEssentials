using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace RefactoringEssentials.CSharp.Diagnostics
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    [NotPortedYet]
    /// <summary>
    /// Finds redundant internal modifiers.
    /// </summary>
    public class RedundantInternalAnalyzer : DiagnosticAnalyzer
    {
        static readonly DiagnosticDescriptor descriptor = new DiagnosticDescriptor(
            CSharpDiagnosticIDs.RedundantInternalAnalyzerID,
            GettextCatalog.GetString("Removes 'internal' modifiers that are not required"),
            GettextCatalog.GetString("'internal' modifier is redundant"),
            DiagnosticAnalyzerCategories.RedundanciesInCode,
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            helpLinkUri: HelpLink.CreateFor(CSharpDiagnosticIDs.RedundantInternalAnalyzerID),
            customTags: DiagnosticCustomTags.Unnecessary
        );

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(descriptor);

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(
                (nodeContext) =>
                {
                    Diagnostic diagnostic;
                    if (TryGetDiagnostic(nodeContext, out diagnostic))
                    {
                        nodeContext.ReportDiagnostic(diagnostic);
                    }
                },
                new SyntaxKind[] { SyntaxKind.ClassDeclaration, SyntaxKind.InterfaceDeclaration, SyntaxKind.StructDeclaration, SyntaxKind.EnumDeclaration, SyntaxKind.DelegateDeclaration }
            );
        }

        static bool TryGetDiagnostic(SyntaxNodeAnalysisContext nodeContext, out Diagnostic diagnostic)
        {
            diagnostic = default(Diagnostic);
            if (nodeContext.IsFromGeneratedCode())
                return false;
            var node = nodeContext.Node as BaseTypeDeclarationSyntax;
            if (!node.Modifiers.Any(m => m.IsKind(SyntaxKind.InternalKeyword)))
                return false;
            if (node.Parent is BaseTypeDeclarationSyntax)
                return false;

            diagnostic = Diagnostic.Create(descriptor, node.Modifiers.First(m => m.IsKind(SyntaxKind.InternalKeyword)).GetLocation());
            return true;
        }
    }
}