using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace RefactoringEssentials.CSharp.Diagnostics
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class RedundantArgumentNameAnalyzer : DiagnosticAnalyzer
    {
        static readonly DiagnosticDescriptor descriptor = new DiagnosticDescriptor(
            CSharpDiagnosticIDs.RedundantArgumentNameAnalyzerID,
            GettextCatalog.GetString("Redundant explicit argument name specification"),
            GettextCatalog.GetString("Redundant argument name specification"),
            DiagnosticAnalyzerCategories.RedundanciesInCode,
            DiagnosticSeverity.Hidden,
            isEnabledByDefault: true,
            helpLinkUri: HelpLink.CreateFor(CSharpDiagnosticIDs.RedundantArgumentNameAnalyzerID),
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
                    GetDiagnostics(nodeContext, ((InvocationExpressionSyntax)nodeContext.Node).ArgumentList?.Arguments);
                },
                new SyntaxKind[] { SyntaxKind.InvocationExpression }
            );
            context.RegisterSyntaxNodeAction(
                nodeContext =>
                {
                    GetDiagnostics(nodeContext, ((ElementAccessExpressionSyntax)nodeContext.Node).ArgumentList?.Arguments);
                },
                new SyntaxKind[] { SyntaxKind.ElementAccessExpression }
            );
            context.RegisterSyntaxNodeAction(
                nodeContext =>
                {
                    GetDiagnostics(nodeContext, ((ObjectCreationExpressionSyntax)nodeContext.Node).ArgumentList?.Arguments);
                },
                new SyntaxKind[] { SyntaxKind.ObjectCreationExpression }
            );

            context.RegisterSyntaxNodeAction(
                nodeContext =>
                {
                    GetDiagnostics(nodeContext, ((AttributeSyntax)nodeContext.Node).ArgumentList?.Arguments);
                },
                new SyntaxKind[] { SyntaxKind.Attribute }
            );
        }

        static void GetDiagnostics(SyntaxNodeAnalysisContext nodeContext, SeparatedSyntaxList<ArgumentSyntax>? arguments)
        {
            if (!arguments.HasValue)
                return;

            var node = nodeContext.Node;
            CheckParameters(nodeContext, nodeContext.SemanticModel.GetSymbolInfo(node).Symbol, arguments.Value);
        }

        static void GetDiagnostics(SyntaxNodeAnalysisContext nodeContext, SeparatedSyntaxList<AttributeArgumentSyntax>? arguments)
        {
            if (!arguments.HasValue)
                return;

            var node = nodeContext.Node;
            CheckParameters(nodeContext, nodeContext.SemanticModel.GetSymbolInfo(node).Symbol, arguments.Value);
        }

        static void CheckParameters(SyntaxNodeAnalysisContext nodeContext, ISymbol ir, IEnumerable<ArgumentSyntax> arguments)
        {
            if (ir == null)
                return;
            var parameters = ir.GetParameters();
            int i = 0;

            foreach (var arg in arguments)
            {
                var na = arg.NameColon;
                if (na != null)
                {
                    if (i >= parameters.Length || na.Name.ToString() != parameters[i].Name)
                        break;
                    nodeContext.ReportDiagnostic(Diagnostic.Create(descriptor, na.GetLocation()));
                }
                i++;
            }
        }

        static void CheckParameters(SyntaxNodeAnalysisContext nodeContext, ISymbol ir, IEnumerable<AttributeArgumentSyntax> arguments)
        {
            if (ir == null)
                return;
            var parameters = ir.GetParameters();
            int i = 0;

            foreach (var arg in arguments)
            {
                var na = arg.NameColon;
                if (na != null)
                {
                    if (i >= parameters.Length || na.Name.ToString() != parameters[i].Name)
                        break;
                    nodeContext.ReportDiagnostic(Diagnostic.Create(descriptor, na.GetLocation()));
                }
                i++;
            }
        }
    }
}