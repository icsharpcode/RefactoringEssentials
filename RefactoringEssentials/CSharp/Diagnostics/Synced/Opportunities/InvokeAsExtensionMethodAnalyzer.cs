using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace RefactoringEssentials.CSharp.Diagnostics
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class InvokeAsExtensionMethodAnalyzer : DiagnosticAnalyzer
    {
        static readonly DiagnosticDescriptor descriptor = new DiagnosticDescriptor(
            CSharpDiagnosticIDs.InvokeAsExtensionMethodAnalyzerID,
            GettextCatalog.GetString("If an extension method is called as static method convert it to method syntax"),
            GettextCatalog.GetString("Convert static method call to extension method call"),
            DiagnosticAnalyzerCategories.Opportunities,
            DiagnosticSeverity.Info,
            isEnabledByDefault: true,
            helpLinkUri: HelpLink.CreateFor(CSharpDiagnosticIDs.InvokeAsExtensionMethodAnalyzerID)
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
                new SyntaxKind[] { SyntaxKind.InvocationExpression }
            );
        }

        static bool TryGetDiagnostic(SyntaxNodeAnalysisContext nodeContext, out Diagnostic diagnostic)
        {
            var node = nodeContext.Node as InvocationExpressionSyntax;
            var semanticModel = nodeContext.SemanticModel;
            var cancellationToken = nodeContext.CancellationToken;

            diagnostic = default(Diagnostic);
            if (nodeContext.IsFromGeneratedCode())
                return false;
            var memberReference = node.Expression as MemberAccessExpressionSyntax;
            if (memberReference == null)
                return false;
            var firstArgument = node.ArgumentList.Arguments.FirstOrDefault();
            if (firstArgument == null || firstArgument.Expression.IsKind(SyntaxKind.NullLiteralExpression))
                return false;
            var expressionSymbol = semanticModel.GetSymbolInfo(node.Expression).Symbol as IMethodSymbol;
            // Ignore non-extensions and reduced extensions (so a.Ext, as opposed to B.Ext(a))
            if (expressionSymbol == null || !expressionSymbol.IsExtensionMethod || expressionSymbol.MethodKind == MethodKind.ReducedExtension)
                return false;

            var extensionMethodDeclaringType = expressionSymbol.ContainingType;
            if (extensionMethodDeclaringType.Name != memberReference.Expression.ToString())
                return false;

            // Don't allow conversion if first parameter is a method name instead of variable (extension method on delegate type)
            var firstParameter = node.ArgumentList?.Arguments.FirstOrDefault();
            if ((firstParameter != null) && (firstParameter.Expression is IdentifierNameSyntax))
            {
                var extensionMethodTargetExpression = semanticModel.GetSymbolInfo(firstParameter.Expression).Symbol as IMethodSymbol;
                if (extensionMethodTargetExpression != null)
                    return false;
            }

            diagnostic = Diagnostic.Create(
                descriptor,
                memberReference.Name.GetLocation()
            );
            return true;
        }
    }
}