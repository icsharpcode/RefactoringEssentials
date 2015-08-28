using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System.Linq;
using SyntaxKind = Microsoft.CodeAnalysis.CSharp.SyntaxKind;

namespace RefactoringEssentials.CSharp.Diagnostics
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class UseArrayCreationExpressionAnalyzer : DiagnosticAnalyzer
    {
        private static readonly DiagnosticDescriptor descriptor = new DiagnosticDescriptor(
            CSharpDiagnosticIDs.UseArrayCreationExpressionAnalyzerID,
            GettextCatalog.GetString("Use array creation expression"),
            GettextCatalog.GetString("Use array create expression"),
            DiagnosticAnalyzerCategories.PracticesAndImprovements,
            DiagnosticSeverity.Info,
            isEnabledByDefault: true,
            helpLinkUri: HelpLink.CreateFor(CSharpDiagnosticIDs.UseArrayCreationExpressionAnalyzerID)
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
                 SyntaxKind.InvocationExpression
            );
        }

        private static bool TryGetDiagnostic(SyntaxNodeAnalysisContext nodeContext, out Diagnostic diagnostic)
        {
            diagnostic = default(Diagnostic);
            if (nodeContext.IsFromGeneratedCode())
                return false;
            var node = nodeContext.Node as InvocationExpressionSyntax;
            if (node == null)
                return false;

            var invocationSymbol = nodeContext.SemanticModel.GetSymbolInfo(node).Symbol;
            if (invocationSymbol == null)
                return false;

            if (invocationSymbol.Name != "CreateInstance")
                return false;

            if (node.ArgumentList == null ||
               node.ArgumentList != null && node.ArgumentList.Arguments.Count <= 1)
                return false;

            if (node.ArgumentList.Arguments.FirstOrDefault().Expression == null)
                return false;

            var firstArgument = node.ArgumentList.Arguments.FirstOrDefault().Expression as TypeOfExpressionSyntax;
            if (firstArgument == null)
                return false;

            var argumentChildLiteralExpression = node.ArgumentList.Arguments[1].Expression as LiteralExpressionSyntax;
            if (argumentChildLiteralExpression == null)
                return false;

            if (!argumentChildLiteralExpression.IsKind(SyntaxKind.NumericLiteralExpression))
                return false;

            var typeOfFirstArgument = node.ArgumentList.Arguments[0].Expression as TypeOfExpressionSyntax;
            if (typeOfFirstArgument == null)
            {
                return false;
            }

            var typeSymbol = nodeContext.SemanticModel.GetSymbolInfo((typeOfFirstArgument).Type).Symbol;
            if (!typeSymbol.OriginalDefinition.Equals(nodeContext.SemanticModel.Compilation.GetTypeByMetadataName("System.Int32")))
                return false;

            diagnostic = Diagnostic.Create(descriptor, node.GetLocation());
            return true;
        }
    }
}