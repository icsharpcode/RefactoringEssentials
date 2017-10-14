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
    // Disabled for now to avoid exceptions in compiler-only case, because this analyzer requires types from Roslyn's Workspaces layer.
    //[DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class RoslynUsageAnalyzer : DiagnosticAnalyzer
    {
        static readonly DiagnosticDescriptor descriptor = new DiagnosticDescriptor(
            CSharpDiagnosticIDs.RoslynReflectionUsageAnalyzerID,
            GettextCatalog.GetString("Unallowed usage of Roslyn features in this context."),
            GettextCatalog.GetString("{0}"),
            DiagnosticAnalyzerCategories.CodeQualityIssues,
            DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            helpLinkUri: HelpLink.CreateFor(CSharpDiagnosticIDs.RoslynReflectionUsageAnalyzerID)
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
                        nodeContext.ReportDiagnostic(diagnostic);
                },
                SyntaxKind.SimpleMemberAccessExpression
            );
        }

        static bool TryGetDiagnostic(SyntaxNodeAnalysisContext nodeContext, out Diagnostic diagnostic)
        {
            diagnostic = default(Diagnostic);
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
            surroundingContext = GetContextFromClass(surroundingClassSymbol);

            if (surroundingContext == RoslynReflectionAllowedContext.None)
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

            string diagnosticMessage = "";

            RoslynReflectionAllowedContext allowedContext = RoslynReflectionAllowedContext.None;
            if (reflectionUsageAttributeData != null)
            {
                var attributeParam = reflectionUsageAttributeData.ConstructorArguments.FirstOrDefault();
                if (attributeParam.Value is int)
                    allowedContext = (RoslynReflectionAllowedContext)attributeParam.Value;

                diagnosticMessage = "This call accesses Roslyn features through reflection, although not allowed in {0}.";
            }
            else
            {
                if (surroundingContext == RoslynReflectionAllowedContext.Analyzers)
                {
                    var symbolContainingType = symbol.ContainingType;
                    if (symbolContainingType != null)
                    {
                        allowedContext = GetContextFromClass(symbolContainingType);
                    }
                }
                else
                {
                    allowedContext = RoslynReflectionAllowedContext.Analyzers | RoslynReflectionAllowedContext.CodeFixes;
                }

                diagnosticMessage = "Accessing members from code fixes/refactorings is not allowed in {0}.";
            }

            if ((allowedContext == RoslynReflectionAllowedContext.None) || (allowedContext.HasFlag(surroundingContext)))
                return false;

            diagnostic = Diagnostic.Create(
                descriptor,
                node.Name.GetLocation(),
                string.Format(diagnosticMessage, GetContextName(surroundingContext))
            );
            return true;
        }

        static RoslynReflectionAllowedContext GetContextFromClass(INamedTypeSymbol symbol)
        {
            RoslynReflectionAllowedContext surroundingContext = RoslynReflectionAllowedContext.None;
            var surroundingClassAttributes = symbol.GetAttributes();
            if (surroundingClassAttributes.Any(a => a.AttributeClass.GetFullName() == typeof(DiagnosticAnalyzerAttribute).FullName))
                surroundingContext = RoslynReflectionAllowedContext.Analyzers;
            else if (surroundingClassAttributes.Any(a => a.AttributeClass.GetFullName() == typeof(ExportCodeFixProviderAttribute).FullName))
                surroundingContext = RoslynReflectionAllowedContext.CodeFixes;
            else if (surroundingClassAttributes.Any(a => a.AttributeClass.GetFullName() == typeof(ExportCodeRefactoringProviderAttribute).FullName))
                surroundingContext = RoslynReflectionAllowedContext.CodeFixes;

            return surroundingContext;
        }

        static string GetContextName(RoslynReflectionAllowedContext context)
        {
            switch (context)
            {
                case RoslynReflectionAllowedContext.Analyzers:
                    return "analyzers";
                case RoslynReflectionAllowedContext.CodeFixes:
                    return "code fixes/refactorings";
            }

            return "";
        }
    }
}