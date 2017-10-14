using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace RefactoringEssentials.CSharp.Diagnostics
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class EmptyEmbeddedStatementAnalyzer : DiagnosticAnalyzer
    {
        static readonly DiagnosticDescriptor descriptor = new DiagnosticDescriptor(
            CSharpDiagnosticIDs.EmptyEmbeddedStatementAnalyzerID,
            GettextCatalog.GetString("Empty control statement body"),
            GettextCatalog.GetString("';' should be avoided. Use '{}' instead"),
            DiagnosticAnalyzerCategories.PracticesAndImprovements,
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            helpLinkUri: HelpLink.CreateFor(CSharpDiagnosticIDs.EmptyEmbeddedStatementAnalyzerID)
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
                    if (TryGetDiagnosticForWhile(nodeContext, out diagnostic))
                        nodeContext.ReportDiagnostic(diagnostic);
                },
                SyntaxKind.WhileStatement
            );

            context.RegisterSyntaxNodeAction(
                (nodeContext) =>
                {
                    Diagnostic diagnostic;
                    if (TryGetDiagnosticForForeach(nodeContext, out diagnostic))
                        nodeContext.ReportDiagnostic(diagnostic);
                },
                SyntaxKind.ForEachStatement
            );

            context.RegisterSyntaxNodeAction(
                (nodeContext) =>
                {
                    Diagnostic diagnostic;
                    if (TryGetDiagnosticForIf(nodeContext, out diagnostic))
                        nodeContext.ReportDiagnostic(diagnostic);
                },
                SyntaxKind.IfStatement
            );

            context.RegisterSyntaxNodeAction(
                (nodeContext) =>
                {
                    Diagnostic diagnostic;
                    if (TryGetDiagnosticForFor(nodeContext, out diagnostic))
                        nodeContext.ReportDiagnostic(diagnostic);
                },
                SyntaxKind.ForStatement
            );

        }

        static bool TryGetDiagnosticForWhile(SyntaxNodeAnalysisContext nodeContext, out Diagnostic diagnostic)
        {
            diagnostic = default(Diagnostic);
            var node = nodeContext.Node as WhileStatementSyntax;

            if (!Check(node.Statement))
                return false;

            diagnostic = Diagnostic.Create(
                descriptor,
                node.Statement.GetLocation()
            );
            return true;
        }

        static bool TryGetDiagnosticForForeach(SyntaxNodeAnalysisContext nodeContext, out Diagnostic diagnostic)
        {
            diagnostic = default(Diagnostic);
            var node = nodeContext.Node as ForEachStatementSyntax;

            if (!Check(node.Statement))
                return false;

            diagnostic = Diagnostic.Create(
                descriptor,
                node.Statement.GetLocation()
            );
            return true;
        }

        static bool TryGetDiagnosticForIf(SyntaxNodeAnalysisContext nodeContext, out Diagnostic diagnostic)
        {
            diagnostic = default(Diagnostic);
            var node = nodeContext.Node as IfStatementSyntax;

            if (Check(node.Statement))
            {
                diagnostic = Diagnostic.Create(
                    descriptor,
                    node.Statement.GetLocation()
                );
                return true;
            }

            if (node.Else != null && Check(node.Else.Statement))
            {
                diagnostic = Diagnostic.Create(
                    descriptor,
                    node.Else.Statement.GetLocation()
                );
                return true;
            }

            return false;
        }

        static bool TryGetDiagnosticForFor(SyntaxNodeAnalysisContext nodeContext, out Diagnostic diagnostic)
        {
            diagnostic = default(Diagnostic);
            var node = nodeContext.Node as ForStatementSyntax;

            if (!Check(node.Statement))
                return false;

            diagnostic = Diagnostic.Create(
                descriptor,
                node.Statement.GetLocation()
            );
            return true;
        }


        static bool Check(SyntaxNode body)
        {
            if (body == null || !body.IsKind(SyntaxKind.EmptyStatement))
                return false;
            return true;
        }
    }
}