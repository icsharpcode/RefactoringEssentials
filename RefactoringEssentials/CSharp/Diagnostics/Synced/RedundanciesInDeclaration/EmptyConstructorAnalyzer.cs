using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;

namespace RefactoringEssentials.CSharp.Diagnostics
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class EmptyConstructorAnalyzer : DiagnosticAnalyzer
    {
        static readonly DiagnosticDescriptor descriptor = new DiagnosticDescriptor(
            CSharpDiagnosticIDs.EmptyConstructorAnalyzerID,
            GettextCatalog.GetString("An empty public constructor without parameters is redundant."),
            GettextCatalog.GetString("Empty constructor is redundant"),
            DiagnosticAnalyzerCategories.RedundanciesInDeclarations,
            DiagnosticSeverity.Info,
            isEnabledByDefault: true,
            helpLinkUri: HelpLink.CreateFor(CSharpDiagnosticIDs.EmptyConstructorAnalyzerID),
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
                new SyntaxKind[] { SyntaxKind.ConstructorDeclaration }
            );
        }

        static bool TryGetDiagnostic(SyntaxNodeAnalysisContext nodeContext, out Diagnostic diagnostic)
        {
            var constructorDeclaration = nodeContext.Node as ConstructorDeclarationSyntax;
            diagnostic = default(Diagnostic);

            if (!IsEmpty(constructorDeclaration) || !constructorDeclaration.Modifiers.Any(m => m.IsKind(SyntaxKind.PublicKeyword)))
                return false;

            if ((constructorDeclaration.Parent).GetMembers().OfType<ConstructorDeclarationSyntax>().Count(child => !child.Modifiers.Any(m => m.IsKind(SyntaxKind.StaticKeyword))) > 1)
                return false;

            diagnostic = Diagnostic.Create(
                descriptor,
                constructorDeclaration.GetLocation()
            );
            return true;
        }

        static bool IsEmpty(ConstructorDeclarationSyntax constructorDeclaration)
        {
            if (constructorDeclaration.Initializer != null && constructorDeclaration.Initializer.ArgumentList.Arguments.Count > 0)
                return false;

            return constructorDeclaration.ParameterList.Parameters.Count == 0 &&
                EmptyDestructorAnalyzer.IsEmpty(constructorDeclaration.Body);
        }
    }
}