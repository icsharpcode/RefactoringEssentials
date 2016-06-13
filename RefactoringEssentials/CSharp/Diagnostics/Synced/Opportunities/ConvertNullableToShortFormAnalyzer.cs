using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace RefactoringEssentials.CSharp.Diagnostics
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class ConvertNullableToShortFormAnalyzer : DiagnosticAnalyzer
    {
        static readonly DiagnosticDescriptor descriptor = new DiagnosticDescriptor(
            CSharpDiagnosticIDs.ConvertNullableToShortFormAnalyzerID,
            GettextCatalog.GetString("Convert 'Nullable<T>' to the short form 'T?'"),
            GettextCatalog.GetString("Nullable type can be simplified"),
            DiagnosticAnalyzerCategories.Opportunities,
            DiagnosticSeverity.Info,
            isEnabledByDefault: true,
            helpLinkUri: HelpLink.CreateFor(CSharpDiagnosticIDs.ConvertNullableToShortFormAnalyzerID)
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
                new SyntaxKind[] { SyntaxKind.QualifiedName, SyntaxKind.GenericName }
            );
        }

        static bool TryGetDiagnostic(SyntaxNodeAnalysisContext nodeContext, out Diagnostic diagnostic)
        {
            var simpleType = nodeContext.Node;
            var semanticModel = nodeContext.SemanticModel;
            var cancellationToken = nodeContext.CancellationToken;

            diagnostic = default(Diagnostic);
            if (GetTypeArgument(simpleType) == null)
                return false;
            var rr = semanticModel.GetSymbolInfo(simpleType);
            var type = rr.Symbol as ITypeSymbol;
            if (type == null || type.Name != "Nullable" || type.ContainingNamespace.ToDisplayString() != "System")
                return false;

            diagnostic = Diagnostic.Create(
                descriptor,
                simpleType.GetLocation()
            );
            return true;
        }

        internal static TypeSyntax GetTypeArgument(SyntaxNode node)
        {
            var gns = node as GenericNameSyntax;
            if (gns == null)
            {
                var qns = node as QualifiedNameSyntax;
                if (qns != null)
                    gns = qns.Right as GenericNameSyntax;
            }
            else
            {
                var parent = gns.Parent as QualifiedNameSyntax;
                if (parent != null && parent.Right == node)
                    return null;
            }

            if (gns != null)
            {
                if (gns.TypeArgumentList.Arguments.Count == 1)
                {
                    var typeArgument = gns.TypeArgumentList.Arguments[0];
                    if (!typeArgument.IsKind(SyntaxKind.OmittedTypeArgument))
                        return typeArgument;
                }
            }
            return null;
        }
    }
}