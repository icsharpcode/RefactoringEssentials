using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;
using System.Collections.Immutable;
using System.Linq.Expressions;

namespace RefactoringEssentials.CSharp.Diagnostics
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class RedundantStringToCharArrayCallAnalyzer : DiagnosticAnalyzer
    {
        private static readonly DiagnosticDescriptor descriptor = new DiagnosticDescriptor(
            CSharpDiagnosticIDs.RedundantStringToCharArrayCallAnalyzerID,
            GettextCatalog.GetString("Redundant 'string.ToCharArray()' call"),
            GettextCatalog.GetString("Redundant 'string.ToCharArray()' call"),
            DiagnosticAnalyzerCategories.RedundanciesInCode,
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            helpLinkUri: HelpLink.CreateFor(CSharpDiagnosticIDs.RedundantStringToCharArrayCallAnalyzerID),
            customTags: DiagnosticCustomTags.Unnecessary
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

        private static bool TryGetDiagnostic(SyntaxNodeAnalysisContext nodeContext, out Diagnostic diagnostic)
        {
            diagnostic = default(Diagnostic);
            if (nodeContext.IsFromGeneratedCode())
                return false;
            var node = nodeContext.Node as MemberAccessExpressionSyntax;

            if (!VerifyMethodCalled(node, nodeContext))
                return false;

            diagnostic = Diagnostic.Create(descriptor, node.GetLocation());
            return true;
        }

        static bool VerifyMethodCalled(MemberAccessExpressionSyntax methodCalled,SyntaxNodeAnalysisContext nodeContext)
        {
            if(methodCalled == null)
                throw new ArgumentNullException();

            var symbol = nodeContext.SemanticModel.GetSymbolInfo(methodCalled).Symbol;
            return (symbol.Name == "ToCharArray");
        }
    }
}