using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Immutable;
using System.Collections.Generic;

namespace RefactoringEssentials.CSharp.Diagnostics
{
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class RedundantStringToCharArrayCallAnalyzer : DiagnosticAnalyzer
    {
        static readonly DiagnosticDescriptor descriptor = new DiagnosticDescriptor(
            CSharpDiagnosticIDs.RedundantStringToCharArrayCallAnalyzerID,
            GettextCatalog.GetString("Redundant 'string.ToCharArray()' call"),
            GettextCatalog.GetString("Redundant 'string.ToCharArray()' call"),
            DiagnosticAnalyzerCategories.RedundanciesInCode,
            DiagnosticSeverity.Hidden,
            isEnabledByDefault: true,
            helpLinkUri: HelpLink.CreateFor(CSharpDiagnosticIDs.RedundantStringToCharArrayCallAnalyzerID),
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
                SyntaxKind.InvocationExpression
            );
        }

        static bool TryGetDiagnostic(SyntaxNodeAnalysisContext nodeContext, out Diagnostic diagnostic)
        {
            diagnostic = default(Diagnostic);
            var node = nodeContext.Node as InvocationExpressionSyntax;
            if (node == null)
                return false;
            if (!(node.Parent is ForEachStatementSyntax) && !(node.Parent is ElementAccessExpressionSyntax))
                return false;
            if (!VerifyMethodCalled(node, nodeContext))
                return false;

            var accessExpression = (MemberAccessExpressionSyntax)node.Expression;
            if (accessExpression == null)
                return false;

            diagnostic = Diagnostic.Create(descriptor, accessExpression.Name.GetLocation(),
                (IEnumerable<Location>) new[] { node.ArgumentList.GetLocation() });
            return true;
        }

        static bool VerifyMethodCalled(InvocationExpressionSyntax methodCalled, SyntaxNodeAnalysisContext nodeContext)
        {
            if (methodCalled == null)
                throw new ArgumentNullException();
            if ((methodCalled.ArgumentList != null) && (methodCalled.ArgumentList.Arguments.Count > 0))
                return false;
            var expression = methodCalled.Expression as MemberAccessExpressionSyntax;
            if (expression == null)
                return false;
            var symbol = nodeContext.SemanticModel.GetSymbolInfo(expression).Symbol;
            if (symbol == null)
                return false;
            return symbol.ContainingType.SpecialType == SpecialType.System_String && symbol.Name == "ToCharArray";
        }
    }
}