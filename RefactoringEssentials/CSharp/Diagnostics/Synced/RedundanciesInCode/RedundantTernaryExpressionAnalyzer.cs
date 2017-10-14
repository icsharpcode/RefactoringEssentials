using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace RefactoringEssentials.CSharp.Diagnostics
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class RedundantTernaryExpressionAnalyzer : DiagnosticAnalyzer
    {
        static readonly DiagnosticDescriptor descriptor = new DiagnosticDescriptor(
            CSharpDiagnosticIDs.RedundantTernaryExpressionAnalyzerID,
            GettextCatalog.GetString("Redundant conditional expression"),
            GettextCatalog.GetString("Redundant conditional expression"),
            DiagnosticAnalyzerCategories.RedundanciesInCode,
            DiagnosticSeverity.Hidden,
            isEnabledByDefault: true,
            helpLinkUri: HelpLink.CreateFor(CSharpDiagnosticIDs.RedundantTernaryExpressionAnalyzerID),
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
                new SyntaxKind[] { SyntaxKind.ConditionalExpression }
            );
        }

        static bool TryGetDiagnostic(SyntaxNodeAnalysisContext nodeContext, out Diagnostic diagnostic)
        {
            diagnostic = default(Diagnostic);

            var node = nodeContext.Node as ConditionalExpressionSyntax;

            bool whenTrue;
            bool whenFalse; ;
            if (!IsBoolean(node.WhenTrue as LiteralExpressionSyntax, out whenTrue) || !IsBoolean(node.WhenFalse as LiteralExpressionSyntax, out whenFalse))
                return false;
            if (!whenTrue || whenFalse)
                return false;

            diagnostic = Diagnostic.Create(descriptor, Location.Create(node.SyntaxTree, new TextSpan(node.QuestionToken.SpanStart, (node.WhenFalse.Span.End - node.QuestionToken.SpanStart))));
            return true;
        }

        static bool IsBoolean(LiteralExpressionSyntax expr, out bool value)
        {
            value = false;
            if (expr != null && (expr.Token.Value is bool))
            {
                value = (bool)expr.Token.Value;
                return true;
            }
            return false;
        }
    }
}