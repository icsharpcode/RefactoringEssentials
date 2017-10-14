using System;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace RefactoringEssentials.CSharp.Diagnostics
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class BaseMethodParameterNameMismatchAnalyzer : DiagnosticAnalyzer
    {
        static readonly DiagnosticDescriptor descriptor = new DiagnosticDescriptor(
            CSharpDiagnosticIDs.BaseMethodParameterNameMismatchAnalyzerID,
            GettextCatalog.GetString("Parameter name differs in base declaration"),
            GettextCatalog.GetString("Parameter name differs in base declaration"),
            DiagnosticAnalyzerCategories.CodeQualityIssues,
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            helpLinkUri: HelpLink.CreateFor(CSharpDiagnosticIDs.BaseMethodParameterNameMismatchAnalyzerID)
        );

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(descriptor);

        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.RegisterSyntaxNodeAction(
                (nodeContext) =>
                {
                    ScanDiagnostic(nodeContext);
                },
                new SyntaxKind[] { SyntaxKind.IndexerDeclaration, SyntaxKind.MethodDeclaration }
            );
        }

        static void ScanDiagnostic(SyntaxNodeAnalysisContext nodeContext)
        {
            var node1 = nodeContext.Node as IndexerDeclarationSyntax;
            if (node1 != null)
            {
                var rr = nodeContext.SemanticModel.GetDeclaredSymbol(node1);
                var baseProperty = rr.OverriddenProperty;
                if (baseProperty == null)
                    return;
                Check(nodeContext, node1.ParameterList.Parameters, rr.Parameters, baseProperty.Parameters);
            }

            var node2 = nodeContext.Node as MethodDeclarationSyntax;
            if (node2 != null)
            {
                var rr = nodeContext.SemanticModel.GetDeclaredSymbol(node2);
                var baseMethod = rr.OverriddenMethod;
                if (baseMethod == null)
                    return;
                Check(nodeContext, node2.ParameterList.Parameters, rr.Parameters, baseMethod.Parameters);
            }
        }

        static void Check(SyntaxNodeAnalysisContext nodeContext, SeparatedSyntaxList<ParameterSyntax> syntaxParams, ImmutableArray<IParameterSymbol> list1, ImmutableArray<IParameterSymbol> list2)
        {
            var upper = Math.Min(list1.Length, list2.Length);
            for (int i = 0; i < upper; i++)
            {
                var arg = list1[i];
                var baseArg = list2[i];

                if (arg.Name != baseArg.Name)
                {
                    nodeContext.ReportDiagnostic(Diagnostic.Create(
                        descriptor.Id,
                        descriptor.Category,
                        descriptor.MessageFormat,
                        descriptor.DefaultSeverity,
                        descriptor.DefaultSeverity,
                        descriptor.IsEnabledByDefault,
                        4,
                        descriptor.Title,
                        descriptor.Description,
                        descriptor.HelpLinkUri,
                        Location.Create(nodeContext.SemanticModel.SyntaxTree, syntaxParams[i].Identifier.Span),
                        null,
                        new[] { baseArg.Name }
                    ));
                }
            }
        }
    }
}