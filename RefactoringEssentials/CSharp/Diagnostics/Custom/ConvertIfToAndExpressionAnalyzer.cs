using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace RefactoringEssentials.CSharp.Diagnostics
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class ConvertIfToAndExpressionAnalyzer : DiagnosticAnalyzer
    {
        static readonly DiagnosticDescriptor descriptor = new DiagnosticDescriptor(
            CSharpDiagnosticIDs.ConvertIfToAndExpressionAnalyzerID,
            GettextCatalog.GetString("Convert 'if' to '&&' expression"),
            "{0}",
            DiagnosticAnalyzerCategories.PracticesAndImprovements,
            DiagnosticSeverity.Info,
            isEnabledByDefault: true,
            helpLinkUri: HelpLink.CreateFor(CSharpDiagnosticIDs.ConvertIfToAndExpressionAnalyzerID)
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
                new SyntaxKind[] { SyntaxKind.IfStatement }
            );
        }

        static bool TryGetDiagnostic(SyntaxNodeAnalysisContext nodeContext, out Diagnostic diagnostic)
        {
            diagnostic = default(Diagnostic);
            var node = nodeContext.Node as IfStatementSyntax;
            ExpressionSyntax target;
            SyntaxTriviaList assignmentTrailingTriviaList;
            if (ConvertIfToOrExpressionAnalyzer.MatchIfElseStatement(node, SyntaxKind.FalseLiteralExpression, out target, out assignmentTrailingTriviaList))
            {
                var varDeclaration = ConvertIfToOrExpressionAnalyzer.FindPreviousVarDeclaration(node);
                if (varDeclaration != null)
                {
                    var targetIdentifier = target as IdentifierNameSyntax;
                    if (targetIdentifier == null)
                        return false;
                    var declaredVarName = varDeclaration.Declaration.Variables.First().Identifier.Value;
                    var assignedVarName = targetIdentifier.Identifier.Value;
                    if (declaredVarName != assignedVarName)
                        return false;
                    if (!ConvertIfToOrExpressionAnalyzer.CheckTarget(targetIdentifier, node.Condition))
                        return false;
                    diagnostic = Diagnostic.Create(descriptor, node.IfKeyword.GetLocation(), GettextCatalog.GetString("Convert to '&&' expression"));
                    return true;
                }
                else
                {
                    if (!ConvertIfToOrExpressionAnalyzer.CheckTarget(target, node.Condition))
                        return false;
                    diagnostic = Diagnostic.Create(descriptor, node.IfKeyword.GetLocation(), GettextCatalog.GetString("Replace with '&='"));
                    return true;
                }
            }
            return false;
        }
    }

}