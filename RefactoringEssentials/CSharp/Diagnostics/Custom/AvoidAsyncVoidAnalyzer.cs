using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace RefactoringEssentials.CSharp.Diagnostics.Custom
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    /// <summary>
    ///
    /// </summary>
    public class AvoidAsyncVoidAnalyzer : DiagnosticAnalyzer
    {
        static readonly DiagnosticDescriptor descriptor = new DiagnosticDescriptor(
            CSharpDiagnosticIDs.AvoidAsyncVoidAnalyzerID,
            GettextCatalog.GetString("Asynchronous methods should return a Task instead of void"),
            GettextCatalog.GetString("Asynchronous method '{0}' should not return void"),
            DiagnosticAnalyzerCategories.CodeQualityIssues,
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            helpLinkUri: HelpLink.CreateFor(CSharpDiagnosticIDs.AvoidAsyncVoidAnalyzerID),
            customTags: DiagnosticCustomTags.Unnecessary
        );

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(descriptor);

        static readonly ImmutableArray<SyntaxKind> AnonymousFunctionExpressionKinds =
           ImmutableArray.Create(
               SyntaxKind.AnonymousMethodExpression,
               SyntaxKind.ParenthesizedLambdaExpression,
               SyntaxKind.SimpleLambdaExpression);

        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.RegisterSymbolAction(
                (symbol) =>
                {
                    Diagnostic diagnostic;
                    if (TryGetDiagnostic(symbol, out diagnostic))
                    {
                        symbol.ReportDiagnostic(diagnostic);
                    }
                },
                new SymbolKind[] {
                    SymbolKind.Method
                }
            );

            context.RegisterSyntaxNodeAction(
               (nodeContext) =>
               {
                   Diagnostic diagnostic;
                   if (TryGetDiagnostic(nodeContext, out diagnostic))
                   {
                       nodeContext.ReportDiagnostic(diagnostic);
                   }
               },
               new SyntaxKind[] {
                   SyntaxKind.AnonymousMethodExpression,
                    SyntaxKind.ParenthesizedLambdaExpression,
                    SyntaxKind.SimpleLambdaExpression
               }
           );
        }

        static bool TryGetDiagnostic(SymbolAnalysisContext symbolContext, out Diagnostic diagnostic)
        {
            diagnostic = default(Diagnostic);

            var symbol = (IMethodSymbol)symbolContext.Symbol;
            if (symbol.IsAsync && symbol.ReturnsVoid && !symbol.Parameters.Any(x=> x.Type.Name.Contains("EventArgs")))
            {
                diagnostic = Diagnostic.Create(descriptor, symbol.Locations.FirstOrDefault(), symbol.Name);
                return true;
            }

            return false;
        }

        static bool TryGetDiagnostic(SyntaxNodeAnalysisContext symbolContext, out Diagnostic diagnostic)
        {
            diagnostic = default(Diagnostic);
            var node = (AnonymousFunctionExpressionSyntax)symbolContext.Node;
            if (node.AsyncKeyword.IsKind(SyntaxKind.None))
                return false;

            var typeInfo = symbolContext.SemanticModel.GetTypeInfo(node);
            var convertedType = typeInfo.ConvertedType as INamedTypeSymbol;
            if (convertedType == null)
                return false;

            if (convertedType.DelegateInvokeMethod != null
                && !convertedType.DelegateInvokeMethod.ReturnsVoid)
                return false;

            diagnostic = Diagnostic.Create(descriptor, node.AsyncKeyword.GetLocation(), "<anonymous>");

            return true;
        }

    }
}
