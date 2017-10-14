using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace RefactoringEssentials.CSharp.Diagnostics
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class ThreadStaticAtInstanceFieldAnalyzer : DiagnosticAnalyzer
    {
        static readonly DiagnosticDescriptor descriptor = new DiagnosticDescriptor(
            CSharpDiagnosticIDs.ThreadStaticAtInstanceFieldAnalyzerID,
            GettextCatalog.GetString("[ThreadStatic] doesn't work with instance fields"),
            GettextCatalog.GetString("ThreadStatic does nothing on instance fields"),
            DiagnosticAnalyzerCategories.CodeQualityIssues,
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            helpLinkUri: HelpLink.CreateFor(CSharpDiagnosticIDs.ThreadStaticAtInstanceFieldAnalyzerID)
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
                new SyntaxKind[] { SyntaxKind.FieldDeclaration }
            );
        }

        static bool TryGetDiagnostic(SyntaxNodeAnalysisContext nodeContext, out Diagnostic diagnostic)
        {
            diagnostic = default(Diagnostic);
            var node = nodeContext.Node as FieldDeclarationSyntax;
            if (node.Modifiers.Any(m => m.IsKind(SyntaxKind.StaticKeyword)))
                return false;

            foreach (var attributeLists in node.AttributeLists)
            {
                foreach (var attribute in attributeLists.Attributes)
                {
                    var attrSymbol = nodeContext.SemanticModel.GetTypeInfo(attribute).Type;
                    if (attrSymbol == null)
                        continue;
                    if (attrSymbol.Name == "ThreadStaticAttribute" && attrSymbol.ContainingNamespace.Name == "System")
                    {
                        diagnostic = Diagnostic.Create(
                            descriptor,
                            attribute.GetLocation()
                        );
                        return true;
                    }
                }
            }
            return false;
        }
    }
}