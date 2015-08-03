using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace RefactoringEssentials.CSharp.Diagnostics
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class RedundantAnonymousTypePropertyNameAnalyzer : DiagnosticAnalyzer
    {
        static readonly DiagnosticDescriptor descriptor = new DiagnosticDescriptor(
            CSharpDiagnosticIDs.RedundantAnonymousTypePropertyNameAnalyzerID,
            GettextCatalog.GetString("Redundant explicit property name"),
            GettextCatalog.GetString("The name can be inferred from the initializer expression"),
            DiagnosticAnalyzerCategories.RedundanciesInCode,
            DiagnosticSeverity.Info,
            isEnabledByDefault: true,
            helpLinkUri: HelpLink.CreateFor(CSharpDiagnosticIDs.RedundantAnonymousTypePropertyNameAnalyzerID),
            customTags: DiagnosticCustomTags.Unnecessary
        );

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(descriptor);

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(
                nodeContext =>
                {
                    GetDiagnostics(nodeContext);
                },
                new SyntaxKind[] { SyntaxKind.AnonymousObjectCreationExpression }
            );
        }

        static void GetDiagnostics(SyntaxNodeAnalysisContext nodeContext)
        {
            if (nodeContext.IsFromGeneratedCode())
                return;
            var node = nodeContext.Node as AnonymousObjectCreationExpressionSyntax;

            foreach (var expr in node.Initializers)
            {
                if (expr.NameEquals == null || expr.NameEquals.Name == null)
                    continue;

                if (expr.NameEquals.Name.ToString() == GetAnonymousTypePropertyName(expr.Expression))
                {
                    nodeContext.ReportDiagnostic(Diagnostic.Create(descriptor, expr.NameEquals.GetLocation()));
                }
            }
        }

        static string GetAnonymousTypePropertyName(SyntaxNode expr)
        {
            var mAccess = expr as MemberAccessExpressionSyntax;
            return mAccess != null ? mAccess.Name.ToString() : expr.ToString();
        }
    }
}