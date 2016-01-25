using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using RefactoringEssentials.CSharp.Diagnostics;
using System.Linq;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CodeRefactorings;

namespace RefactoringEssentials.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class RoslynReflectionUsageAnalyzer : DiagnosticAnalyzer
    {
        static readonly DiagnosticDescriptor descriptor = new DiagnosticDescriptor(
            CSharpDiagnosticIDs.RoslynReflectionUsageAnalyzerID,
            GettextCatalog.GetString("Using internal Roslyn features through reflection in wrong context."),
            GettextCatalog.GetString("Using internal Roslyn features through reflection in wrong context."),
            DiagnosticAnalyzerCategories.CodeQualityIssues,
            DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            helpLinkUri: HelpLink.CreateFor(CSharpDiagnosticIDs.RoslynReflectionUsageAnalyzerID)
        );

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(descriptor);

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(
                (nodeContext) =>
                {
                    Diagnostic diagnostic;
                    if (TryGetDiagnostic(nodeContext, out diagnostic))
                        nodeContext.ReportDiagnostic(diagnostic);
                },
                SyntaxKind.SimpleMemberAccessExpression
            );
        }

        static bool TryGetDiagnostic(SyntaxNodeAnalysisContext nodeContext, out Diagnostic diagnostic)
        {
            diagnostic = default(Diagnostic);
            if (nodeContext.IsFromGeneratedCode())
                return false;
            var semanticModel = nodeContext.SemanticModel;

            var node = nodeContext.Node as MemberAccessExpressionSyntax;

            var symbol = semanticModel.GetSymbolInfo(node.Name).Symbol;
            if (symbol == null)
                return false;

            RoslynReflectionAllowedContext surroundingContext = RoslynReflectionAllowedContext.None;
            var surroundingClass = node.Ancestors().OfType<ClassDeclarationSyntax>().FirstOrDefault();
            if (surroundingClass == null)
                return false;
            var surroundingClassSymbol = semanticModel.GetDeclaredSymbol(surroundingClass);
            if (surroundingClassSymbol == null)
                return false;
            var surroundingClassAttributes = surroundingClassSymbol.GetAttributes();
            if (surroundingClassAttributes.Any(a => a.AttributeClass.GetFullName() == typeof(DiagnosticAnalyzerAttribute).FullName))
                surroundingContext = RoslynReflectionAllowedContext.Analyzers;
            else if (surroundingClassAttributes.Any(a => a.AttributeClass.GetFullName() == typeof(ExportCodeFixProviderAttribute).FullName))
                surroundingContext = RoslynReflectionAllowedContext.CodeFixes;
            else if (surroundingClassAttributes.Any(a => a.AttributeClass.GetFullName() == typeof(ExportCodeRefactoringProviderAttribute).FullName))
                surroundingContext = RoslynReflectionAllowedContext.CodeFixes;
            else
                return false;

            var reflectionUsageAttributeData =
                symbol.GetAttributes().FirstOrDefault(a => a.AttributeClass.GetFullName() == typeof(RoslynReflectionUsageAttribute).FullName);
            if (reflectionUsageAttributeData == null)
            {
                var containingTypeSymbol = symbol.ContainingType;
                if (containingTypeSymbol != null)
                {
                    reflectionUsageAttributeData =
                        containingTypeSymbol.GetAttributes().FirstOrDefault(a => a.AttributeClass.GetFullName() == typeof(RoslynReflectionUsageAttribute).FullName);
                }
            }

            if (reflectionUsageAttributeData == null)
                return false;

            RoslynReflectionAllowedContext allowedContext = RoslynReflectionAllowedContext.None;
            var attributeParam = reflectionUsageAttributeData.ConstructorArguments.FirstOrDefault();
            if (attributeParam.Value is int)
                allowedContext = (RoslynReflectionAllowedContext)attributeParam.Value;

            if (allowedContext.HasFlag(surroundingContext))
                return false;

            diagnostic = Diagnostic.Create(
                descriptor,
                node.Name.GetLocation()
            );
            return true;
        }
    }
}