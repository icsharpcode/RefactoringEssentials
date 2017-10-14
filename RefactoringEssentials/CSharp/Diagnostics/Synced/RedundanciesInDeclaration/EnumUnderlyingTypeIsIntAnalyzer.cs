using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace RefactoringEssentials.CSharp.Diagnostics
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class EnumUnderlyingTypeIsIntAnalyzer : DiagnosticAnalyzer
    {
        static readonly DiagnosticDescriptor descriptor = new DiagnosticDescriptor(
            CSharpDiagnosticIDs.EnumUnderlyingTypeIsIntAnalyzerID,
            GettextCatalog.GetString("The default underlying type of enums is int, so defining it explicitly is redundant."),
            GettextCatalog.GetString("Default underlying type of enums is already int"),
            DiagnosticAnalyzerCategories.RedundanciesInDeclarations,
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            helpLinkUri: HelpLink.CreateFor(CSharpDiagnosticIDs.EnumUnderlyingTypeIsIntAnalyzerID),
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
                new SyntaxKind[] { SyntaxKind.EnumDeclaration }
            );
        }

        static bool TryGetDiagnostic(SyntaxNodeAnalysisContext nodeContext, out Diagnostic diagnostic)
        {
            var enumDeclaration = nodeContext.Node as EnumDeclarationSyntax;
            diagnostic = default(Diagnostic);
            if (enumDeclaration.BaseList == null || enumDeclaration.BaseList.Types.Count == 0)
                return false;
            var underlyingType = enumDeclaration.BaseList.Types.First();
            var info = nodeContext.SemanticModel.GetSymbolInfo(underlyingType.Type);
            var type = info.Symbol as ITypeSymbol;
            if (type == null || type.SpecialType != SpecialType.System_Int32)
                return false;

            diagnostic = Diagnostic.Create(
                descriptor,
                enumDeclaration.BaseList.GetLocation()
            );
            return true;
        }
    }
}