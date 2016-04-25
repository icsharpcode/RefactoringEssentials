using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;

namespace RefactoringEssentials.CSharp.Diagnostics
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class EmptyDestructorAnalyzer : DiagnosticAnalyzer
    {
        static readonly DiagnosticDescriptor descriptor = new DiagnosticDescriptor(
            CSharpDiagnosticIDs.EmptyDestructorAnalyzerID,
            GettextCatalog.GetString("Empty destructor is redundant"),
            GettextCatalog.GetString("Empty destructor is redundant"),
            DiagnosticAnalyzerCategories.RedundanciesInDeclarations,
            DiagnosticSeverity.Info,
            isEnabledByDefault: true,
            helpLinkUri: HelpLink.CreateFor(CSharpDiagnosticIDs.EmptyDestructorAnalyzerID),
            customTags: DiagnosticCustomTags.Unnecessary
        );
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(descriptor);

        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.RegisterSyntaxNodeAction(
                nodeContext =>
                {
                    Diagnostic diagnostic;
                    if (TryGetDiagnostic(nodeContext, out diagnostic))
                    {
                        nodeContext.ReportDiagnostic(diagnostic);
                    }
                },
                new SyntaxKind[] { SyntaxKind.DestructorDeclaration }
            );
        }

        static bool TryGetDiagnostic(SyntaxNodeAnalysisContext nodeContext, out Diagnostic diagnostic)
        {
            var destructorDeclaration = nodeContext.Node as DestructorDeclarationSyntax;
            diagnostic = default(Diagnostic);

            if (!IsEmpty(destructorDeclaration.Body))
                return false;

            diagnostic = Diagnostic.Create(
                descriptor,
                destructorDeclaration.GetLocation()
            );
            return true;
        }

        internal static bool IsEmpty(SyntaxNode node)
        {
            if (node == null || node.IsKind(SyntaxKind.EmptyStatement))
                return true;

            var block = node as BlockSyntax;
            return block != null && block.Statements.All(IsEmpty);
        }
    }
}