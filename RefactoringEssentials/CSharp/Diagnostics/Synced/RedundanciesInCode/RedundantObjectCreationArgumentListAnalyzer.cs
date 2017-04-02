using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace RefactoringEssentials.CSharp.Diagnostics
{
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class RedundantObjectCreationArgumentListAnalyzer : DiagnosticAnalyzer
    {
        private static readonly DiagnosticDescriptor descriptor = new DiagnosticDescriptor(
            CSharpDiagnosticIDs.RedundantObjectCreationArgumentListAnalyzerID,
            GettextCatalog.GetString("When object creation uses object or collection initializer, empty argument list is redundant"),
            GettextCatalog.GetString("Empty argument list is redundant"),
            DiagnosticAnalyzerCategories.RedundanciesInCode,
            DiagnosticSeverity.Hidden,
            isEnabledByDefault: true,
            helpLinkUri: HelpLink.CreateFor(CSharpDiagnosticIDs.RedundantObjectCreationArgumentListAnalyzerID),
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
                 SyntaxKind.ObjectCreationExpression
            );
        }

        private static bool TryGetDiagnostic(SyntaxNodeAnalysisContext nodeContext, out Diagnostic diagnostic)
        {
            diagnostic = default(Diagnostic);

            var objectCreation = nodeContext.Node as ObjectCreationExpressionSyntax;
            if (objectCreation?.Initializer == null ||
                objectCreation.ArgumentList == null ||
                objectCreation.ArgumentList.Arguments.Any()||
                objectCreation.Initializer.IsMissing)
                return false;

            diagnostic = Diagnostic.Create(descriptor, objectCreation.ArgumentList.GetLocation());
            return true;
        }
    }
}