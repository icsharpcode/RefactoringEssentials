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
        static readonly DiagnosticDescriptor descriptor = new DiagnosticDescriptor(
            CSharpDiagnosticIDs.RedundantDelegateCreationAnalyzerID,
            GettextCatalog.GetString("Explicit delegate creation expression is redundant"),
            GettextCatalog.GetString("Redundant explicit delegate declaration"),
            DiagnosticAnalyzerCategories.RedundanciesInCode,
            DiagnosticSeverity.Hidden,
            isEnabledByDefault: true,
            helpLinkUri: HelpLink.CreateFor(CSharpDiagnosticIDs.RedundantDelegateCreationAnalyzerID),
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
                SyntaxKind.AddAssignmentExpression,
                SyntaxKind.SubtractAssignmentExpression
            );
        }

        static bool TryGetDiagnostic(SyntaxNodeAnalysisContext nodeContext, out Diagnostic diagnostic)
        {
            diagnostic = default(Diagnostic);

            var semanticModel = nodeContext.SemanticModel;
            var addOrSubstractExpression = nodeContext.Node as AssignmentExpressionSyntax;
            var rightMember = addOrSubstractExpression?.Right as ObjectCreationExpressionSyntax;

            if (rightMember == null || rightMember.ArgumentList.Arguments.Count != 1)
                return false;

            var leftTypeInfo = ModelExtensions.GetTypeInfo(semanticModel, addOrSubstractExpression.Left).ConvertedType;
            if (leftTypeInfo == null || leftTypeInfo.Kind.Equals(SyntaxKind.EventDeclaration))
                return false;

            diagnostic = Diagnostic.Create(descriptor, addOrSubstractExpression.Right.GetLocation());
            return true;
        }
    }
}