using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace RefactoringEssentials.CSharp.Diagnostics
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class RedundantDelegateCreationAnalyzer : DiagnosticAnalyzer
    {
        private static readonly DiagnosticDescriptor descriptor = new DiagnosticDescriptor(
            CSharpDiagnosticIDs.RedundantDelegateCreationAnalyzerID,
            GettextCatalog.GetString("Explicit delegate creation expression is redundant"),
            GettextCatalog.GetString("Redundant explicit delegate declaration"),
            DiagnosticAnalyzerCategories.RedundanciesInCode,
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            helpLinkUri: HelpLink.CreateFor(CSharpDiagnosticIDs.RedundantDelegateCreationAnalyzerID),
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
                 SyntaxKind.SimpleAssignmentExpression
            );
        }

        private static bool TryGetDiagnostic(SyntaxNodeAnalysisContext nodeContext, out Diagnostic diagnostic)
        {
            diagnostic = default(Diagnostic);
            if (nodeContext.IsFromGeneratedCode())
                return false;

            var semanticModel = nodeContext.SemanticModel;
            var assignmentExpression = nodeContext.Node as AssignmentExpressionSyntax;

            var oces = assignmentExpression?.Left as ObjectCreationExpressionSyntax;
            if (oces == null || oces.ArgumentList.Arguments.Count != 1)
                return false;

            var leftTypeInfo = ModelExtensions.GetTypeInfo(semanticModel, assignmentExpression.Left).ConvertedType;
            if (leftTypeInfo == null || leftTypeInfo.Kind.Equals(SyntaxKind.EventDeclaration))
                return false;

            var rightTypeInfo = ModelExtensions.GetTypeInfo(semanticModel, assignmentExpression.Right).ConvertedType;
            if (rightTypeInfo == null || rightTypeInfo.IsErrorType() || leftTypeInfo.Equals(rightTypeInfo))
                return false;

            diagnostic = Diagnostic.Create(descriptor, assignmentExpression.GetLocation());
            return true;
        }
    }
}