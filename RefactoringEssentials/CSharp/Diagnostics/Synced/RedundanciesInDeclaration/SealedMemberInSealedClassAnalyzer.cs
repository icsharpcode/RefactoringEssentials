using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace RefactoringEssentials.CSharp.Diagnostics
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class SealedMemberInSealedClassAnalyzer : DiagnosticAnalyzer
    {
        static readonly DiagnosticDescriptor descriptor = new DiagnosticDescriptor(
            CSharpDiagnosticIDs.SealedMemberInSealedClassAnalyzerID,
            GettextCatalog.GetString("'sealed' modifier is redundant in sealed classes"),
            GettextCatalog.GetString("'sealed' modifier is redundant in sealed classes"),
            DiagnosticAnalyzerCategories.RedundanciesInDeclarations,
            DiagnosticSeverity.Hidden,
            isEnabledByDefault: true,
            helpLinkUri: HelpLink.CreateFor(CSharpDiagnosticIDs.SealedMemberInSealedClassAnalyzerID),
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
                new SyntaxKind[] { SyntaxKind.MethodDeclaration, SyntaxKind.PropertyDeclaration, SyntaxKind.IndexerDeclaration, SyntaxKind.EventDeclaration }
            );
        }

        static bool TryGetDiagnostic(SyntaxNodeAnalysisContext nodeContext, out Diagnostic diagnostic)
        {
            diagnostic = default(Diagnostic);

            SyntaxToken sealedKeyword;
            if (!HasIssue(nodeContext, out sealedKeyword))
                return false;

            diagnostic = Diagnostic.Create(
                descriptor,
                sealedKeyword.GetLocation()
            );
            return true;
        }

        static bool HasIssue(SyntaxNodeAnalysisContext nodeContext, out SyntaxToken sealedKeyword)
        {
            SyntaxNode node = nodeContext.Node;
            var type = node.Parent as TypeDeclarationSyntax;
            if (type == null || !type.Modifiers.Any(m => m.IsKind(SyntaxKind.SealedKeyword)))
            {
                sealedKeyword = default(SyntaxToken);
                return false;
            }

            sealedKeyword = node.GetModifiers().FirstOrDefault(m => m.IsKind(SyntaxKind.SealedKeyword));
            if (sealedKeyword.IsKind(SyntaxKind.SealedKeyword))
                return true;
            return false;
        }
    }
}