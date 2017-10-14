using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.VisualBasic;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;

namespace RefactoringEssentials.VB.Diagnostics
{
    [DiagnosticAnalyzer(LanguageNames.VisualBasic)]
    public class NonPublicMethodWithTestAttributeAnalyzer : DiagnosticAnalyzer
    {
        static readonly DiagnosticDescriptor descriptor = new DiagnosticDescriptor(
            VBDiagnosticIDs.NonPublicMethodWithTestAttributeAnalyzerID,
            GettextCatalog.GetString("Non public methods are not found by NUnit"),
            GettextCatalog.GetString("NUnit test methods should be public"),
            DiagnosticAnalyzerCategories.NUnit,
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            helpLinkUri: HelpLink.CreateFor(VBDiagnosticIDs.NonPublicMethodWithTestAttributeAnalyzerID)
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
                    if (TryAnalyzeMethod(nodeContext, out diagnostic))
                    {
                        nodeContext.ReportDiagnostic(diagnostic);
                    }
                },
                new SyntaxKind[] { SyntaxKind.SubStatement }
            );
        }

        static bool TryAnalyzeMethod(SyntaxNodeAnalysisContext nodeContext, out Diagnostic diagnostic)
        {
            var methodDeclaration = nodeContext.Node as MethodStatementSyntax;
            diagnostic = default(Diagnostic);

            var methodSymbol = nodeContext.SemanticModel.GetDeclaredSymbol(methodDeclaration);
            if (methodSymbol == null || methodSymbol.IsOverride || methodSymbol.IsStatic || methodDeclaration.Modifiers.Any(m => m.IsKind(SyntaxKind.PublicKeyword)))
                return false;

            if (!methodSymbol.GetAttributes().Any(a => a.AttributeClass.Name == "TestAttribute" && a.AttributeClass.ContainingNamespace.ToDisplayString() == "NUnit.Framework"))
                return false;

            diagnostic = Diagnostic.Create(
                descriptor,
                methodDeclaration.Identifier.GetLocation()
            );
            return true;
        }
    }
}