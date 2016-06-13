using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;
using System;

namespace RefactoringEssentials.CSharp.Diagnostics
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class RedundantDefaultFieldInitializerAnalyzer : DiagnosticAnalyzer
    {
        static readonly DiagnosticDescriptor descriptor = new DiagnosticDescriptor(
            CSharpDiagnosticIDs.RedundantDefaultFieldInitializerAnalyzerID,
            GettextCatalog.GetString("Initializing field with default value is redundant"),
            GettextCatalog.GetString("Initializing field by default value is redundant"),
            DiagnosticAnalyzerCategories.RedundanciesInDeclarations,
            DiagnosticSeverity.Info,
            isEnabledByDefault: true,
            helpLinkUri: HelpLink.CreateFor(CSharpDiagnosticIDs.RedundantDefaultFieldInitializerAnalyzerID),
            customTags: DiagnosticCustomTags.Unnecessary
        );

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(descriptor);

        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.RegisterSyntaxNodeAction(AnalyzeField,  new SyntaxKind[] { SyntaxKind.FieldDeclaration });
        }

        static void AnalyzeField(SyntaxNodeAnalysisContext nodeContext)
        {
            var node = nodeContext.Node as FieldDeclarationSyntax;
            if (node?.Declaration?.Variables == null)
                return;
            if (node.Modifiers.Any(m => m.IsKind(SyntaxKind.ConstKeyword)))
                return;
            var type = nodeContext.SemanticModel.GetTypeInfo(node.Declaration.Type).Type;
            if (type == null)
                return;
            foreach (var v in node.Declaration.Variables)
            {
                var initializer = v.Initializer?.Value;
                if (initializer == null)
                    continue;

                if (initializer.IsKind(SyntaxKind.DefaultExpression))
                {
                    //var defaultExpr = (DefaultExpressionSyntax)initializer;
                    //var defaultType = nodeContext.SemanticModel.GetTypeInfo(defaultExpr.Type).Type;
                    //if (defaultType == type) {
                    nodeContext.ReportDiagnostic(Diagnostic.Create(descriptor, v.Initializer.GetLocation()));
                    //}
                    continue;
                }

                var constValue = nodeContext.SemanticModel.GetConstantValue(initializer);
                if (!constValue.HasValue)
                    continue;
                if (IsDefaultValue(type, constValue.Value))
                {
                    nodeContext.ReportDiagnostic(Diagnostic.Create(descriptor, v.Initializer.GetLocation()));
                }
            }
        }

        static bool IsDefaultValue(ITypeSymbol type, object value)
        {
            if (type.IsReferenceType || type.IsNullableType())
                return null == value;
            try
            {
                switch (type.SpecialType)
                {
                    case SpecialType.System_Boolean:
                        return !(bool)value;
                    case SpecialType.System_Char:
                        return '\0' == (char)value;
                    case SpecialType.System_SByte:
                    case SpecialType.System_Byte:

                    case SpecialType.System_Int16:
                    case SpecialType.System_UInt16:
                    case SpecialType.System_Int32:
                    case SpecialType.System_UInt32:
                    case SpecialType.System_Int64:
                    case SpecialType.System_UInt64:
                        return 0 == Convert.ToInt32(value);
                    case SpecialType.System_Single:
                        return 0f == Convert.ToSingle(value);
                    case SpecialType.System_Double:

                        return 0d == Convert.ToDouble(value);
                    case SpecialType.System_Decimal:
                        return 0m == Convert.ToDecimal(value);
                    case SpecialType.System_Nullable_T:
                        return null == value;
                }
            }
            catch (Exception)
            {
                // ignore (no default value)
            }
            return false;
        }
    }
}