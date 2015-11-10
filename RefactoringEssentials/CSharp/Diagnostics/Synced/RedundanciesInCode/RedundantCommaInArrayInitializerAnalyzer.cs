using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace RefactoringEssentials.CSharp.Diagnostics
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class RedundantCommaInArrayInitializerAnalyzer : DiagnosticAnalyzer
    {
        static readonly DiagnosticDescriptor descriptor = new DiagnosticDescriptor(
            CSharpDiagnosticIDs.RedundantCommaInArrayInitializerAnalyzerID,
            GettextCatalog.GetString("Redundant comma in array initializer"),
            GettextCatalog.GetString("Redundant comma in array initializer"),
            DiagnosticAnalyzerCategories.RedundanciesInCode,
            DiagnosticSeverity.Info,
            isEnabledByDefault: true,
            helpLinkUri: HelpLink.CreateFor(CSharpDiagnosticIDs.RedundantCommaInArrayInitializerAnalyzerID),
            customTags: DiagnosticCustomTags.Unnecessary
        );

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(descriptor);

        //			"Redundant comma in object initializer"
        //			"Redundant comma in collection initializer"
        //			"Redundant comma in array initializer"

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
                SyntaxKind.ArrayInitializerExpression, SyntaxKind.ObjectInitializerExpression, SyntaxKind.CollectionInitializerExpression
            );
        }

        static bool TryGetDiagnostic(SyntaxNodeAnalysisContext nodeContext, out Diagnostic diagnostic)
        {
            diagnostic = default(Diagnostic);
            if (nodeContext.IsFromGeneratedCode())
                return false;

            var node = nodeContext.Node as InitializerExpressionSyntax;
            if (node == null)
                return false;

            var elementCount = node.Expressions.Count;
            if (elementCount > node.Expressions.GetSeparators().Count())
                return false;

            var tokens = node.ChildTokens().ToArray();
            if (tokens.Length < 2)
                return false;
            var commaToken = tokens[tokens.Length - 2];
            if (!commaToken.IsKind(SyntaxKind.CommaToken))
                return false;

            diagnostic = Diagnostic.Create(descriptor, commaToken.GetLocation());
            return true;
        }
    }
}