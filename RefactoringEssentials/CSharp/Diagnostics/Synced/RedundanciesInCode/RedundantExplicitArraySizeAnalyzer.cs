using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace RefactoringEssentials.CSharp.Diagnostics
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class RedundantExplicitArraySizeAnalyzer : DiagnosticAnalyzer
    {
        static readonly DiagnosticDescriptor descriptor = new DiagnosticDescriptor(
            CSharpDiagnosticIDs.RedundantExplicitArraySizeAnalyzerID,
            GettextCatalog.GetString("Redundant explicit size in array creation"),
            GettextCatalog.GetString("Remove the redundant size indicator"),
            DiagnosticAnalyzerCategories.RedundanciesInCode,
            DiagnosticSeverity.Hidden,
            isEnabledByDefault: true,
            helpLinkUri: HelpLink.CreateFor(CSharpDiagnosticIDs.RedundantExplicitArraySizeAnalyzerID),
            customTags: DiagnosticCustomTags.Unnecessary
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
                 SyntaxKind.ArrayCreationExpression
            );
        }

        static bool TryGetDiagnostic(SyntaxNodeAnalysisContext nodeContext, out Diagnostic diagnostic)
        {
            diagnostic = default(Diagnostic);
            var node = nodeContext.Node as ArrayCreationExpressionSyntax;
            var arrayType = node?.Type;

            if (arrayType == null)
                return false;

            if (node.Initializer == null)
                return false;

            var rs = arrayType.RankSpecifiers;
            if (rs.Count != 1 || rs[0].Sizes.Count != 1)
                return false;

            var intSizeValue = nodeContext.SemanticModel.GetConstantValue(arrayType.RankSpecifiers[0].Sizes[0]);
            if (!intSizeValue.HasValue || !(intSizeValue.Value is int))
                return false;

            var size = (int)intSizeValue.Value;
            if (size <= -1)
                return false;
            
            if (node.Initializer.Expressions.Count == size)
            {
                diagnostic = Diagnostic.Create(descriptor, arrayType.RankSpecifiers[0].Sizes[0].GetLocation());
                return true;
            }

            return false;
        }
    }
}