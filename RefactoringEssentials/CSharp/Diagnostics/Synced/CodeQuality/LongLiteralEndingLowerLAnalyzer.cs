using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace RefactoringEssentials.CSharp.Diagnostics
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class LongLiteralEndingLowerLAnalyzer : DiagnosticAnalyzer
    {
        static readonly DiagnosticDescriptor descriptor = new DiagnosticDescriptor(
            CSharpDiagnosticIDs.LongLiteralEndingLowerLAnalyzerID,
            GettextCatalog.GetString("Lowercase 'l' is often confused with '1'"),
            GettextCatalog.GetString("Long literal ends with 'l' instead of 'L'"),
            DiagnosticAnalyzerCategories.CodeQualityIssues,
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            helpLinkUri: HelpLink.CreateFor(CSharpDiagnosticIDs.LongLiteralEndingLowerLAnalyzerID)
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
                        nodeContext.ReportDiagnostic(diagnostic);
                },
                SyntaxKind.NumericLiteralExpression
            );
        }

        static bool TryGetDiagnostic(SyntaxNodeAnalysisContext nodeContext, out Diagnostic diagnostic)
        {
            diagnostic = default(Diagnostic);
            var node = nodeContext.Node as LiteralExpressionSyntax;
            if (!(node.Token.Value is long || node.Token.Value is ulong))
                return false;

            var literal = node.Token.Text;
            if (literal.Length < 2)
                return false;

            char prevChar = literal[literal.Length - 2];
            char lastChar = literal[literal.Length - 1];

            if (prevChar == 'u' || prevChar == 'U') //ul/Ul is not confusing
                return false;

            if (lastChar == 'l' || prevChar == 'l')
            {
                diagnostic = Diagnostic.Create(
                    descriptor,
                    node.GetLocation()
                );
                return true;
            }
            return false;
        }
    }
}