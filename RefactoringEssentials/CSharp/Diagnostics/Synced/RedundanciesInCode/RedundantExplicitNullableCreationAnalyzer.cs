using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace RefactoringEssentials.CSharp.Diagnostics
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class RedundantExplicitNullableCreationAnalyzer : DiagnosticAnalyzer
    {
        private static readonly DiagnosticDescriptor descriptor = new DiagnosticDescriptor(
            CSharpDiagnosticIDs.RedundantExplicitNullableCreationAnalyzerID,
            GettextCatalog.GetString("Value types are implicitly convertible to nullables"),
            GettextCatalog.GetString("Redundant explicit nullable type creation"),
            DiagnosticAnalyzerCategories.RedundanciesInCode,
            DiagnosticSeverity.Hidden,
            isEnabledByDefault: true,
            helpLinkUri: HelpLink.CreateFor(CSharpDiagnosticIDs.RedundantExplicitNullableCreationAnalyzerID),
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
                    if (TryGetRedundantNullableDiagnostic(nodeContext, out diagnostic))
                    {
                        nodeContext.ReportDiagnostic(diagnostic);
                    }
                },
                SyntaxKind.ObjectCreationExpression
            );
        }

        private static bool TryGetRedundantNullableDiagnostic(SyntaxNodeAnalysisContext nodeContext, out Diagnostic diagnostic)
        {
            diagnostic = default(Diagnostic);

            var objectCreation = nodeContext.Node as ObjectCreationExpressionSyntax;
            if (objectCreation == null)
                return false;

            // No Nullable, but "var"
            var parentVarDeclaration = objectCreation?.Parent?.Parent?.Parent as VariableDeclarationSyntax;
            if (parentVarDeclaration != null && parentVarDeclaration.Type.IsVar)
                return false;
            
            // No Nullable, but "var"
            var conditionalExpression = objectCreation.WalkUpParentheses().Parent as ConditionalExpressionSyntax;
            if (conditionalExpression != null && conditionalExpression.WhenFalse.SkipParens() == objectCreation)
                return false;

            var objectCreationSymbol = nodeContext.SemanticModel.GetTypeInfo(objectCreation);
            if (objectCreationSymbol.Type != null && objectCreationSymbol.Type.IsNullableType())
            {
                var creationTypeLocation = objectCreation.Type.GetLocation();
                var newKeywordLocation = objectCreation.NewKeyword.GetLocation();
                diagnostic = Diagnostic.Create(descriptor, newKeywordLocation, (IEnumerable<Location>) (new[] { creationTypeLocation }));
                return true;
            }
            return false;
        }
    }
}