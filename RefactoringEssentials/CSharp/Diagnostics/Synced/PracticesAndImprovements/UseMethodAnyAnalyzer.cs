using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace RefactoringEssentials.CSharp.Diagnostics
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class UseMethodAnyAnalyzer : DiagnosticAnalyzer
    {
        static readonly DiagnosticDescriptor descriptor = new DiagnosticDescriptor(
            CSharpDiagnosticIDs.UseMethodAnyAnalyzerID,
            GettextCatalog.GetString("Replace usages of 'Count()' with call to 'Any()'"),
            GettextCatalog.GetString("Use 'Any()' method for increased performance"),
            DiagnosticAnalyzerCategories.PracticesAndImprovements,
            DiagnosticSeverity.Info,
            isEnabledByDefault: true,
            helpLinkUri: HelpLink.CreateFor(CSharpDiagnosticIDs.UseMethodAnyAnalyzerID)
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
                 SyntaxKind.SimpleMemberAccessExpression
            );
        }

        static bool TryGetDiagnostic(SyntaxNodeAnalysisContext nodeContext, out Diagnostic diagnostic)
        {
            diagnostic = default(Diagnostic);
            if (nodeContext.IsFromGeneratedCode())
                return false;
            var memberAccessExpression = nodeContext.Node as MemberAccessExpressionSyntax;
            if (memberAccessExpression == null)
                return false;

            var accessExpressionSymbol = nodeContext.SemanticModel.GetSymbolInfo(memberAccessExpression).Symbol as IMethodSymbol;
            if (accessExpressionSymbol == null)
                return false;

            if (!accessExpressionSymbol.ContainingType.Equals(nodeContext.SemanticModel.Compilation.GetTypeByMetadataName("System.Linq.Enumerable")) ||
                !accessExpressionSymbol.IsReducedExtension() ||
                !accessExpressionSymbol.Name.Equals("Count"))
            {
                return false;
            }

            diagnostic = Diagnostic.Create(descriptor, memberAccessExpression.GetLocation());
            return true;
        }
    }
}