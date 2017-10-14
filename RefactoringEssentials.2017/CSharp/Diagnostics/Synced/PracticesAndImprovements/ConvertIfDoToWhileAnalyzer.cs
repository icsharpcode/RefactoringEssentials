using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace RefactoringEssentials.CSharp.Diagnostics
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class ConvertIfDoToWhileAnalyzer : DiagnosticAnalyzer
    {
        static readonly DiagnosticDescriptor descriptor = new DiagnosticDescriptor(
            CSharpDiagnosticIDs.ConvertIfDoToWhileAnalyzerID,
            GettextCatalog.GetString("Convert 'if-do-while' to 'while' statement"),
            GettextCatalog.GetString("Statement can be simplified to 'while' statement"),
            DiagnosticAnalyzerCategories.PracticesAndImprovements,
            DiagnosticSeverity.Info,
            isEnabledByDefault: true,
            helpLinkUri: HelpLink.CreateFor(CSharpDiagnosticIDs.ConvertIfDoToWhileAnalyzerID)
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
                SyntaxKind.IfStatement
            );
        }

        static bool TryGetDiagnostic(SyntaxNodeAnalysisContext nodeContext, out Diagnostic diagnostic)
        {
            diagnostic = default(Diagnostic);
            var node = nodeContext.Node as IfStatementSyntax;

            if (node.Else != null)
                return false;
            var embeddedDo = GetEmbeddedDoStatement(node.Statement);
            if (embeddedDo == null)
                return false;
            if (!CSharpUtil.AreConditionsEqual(node.Condition, embeddedDo.Condition))
                return false;

            diagnostic = Diagnostic.Create(
                descriptor,
                node.IfKeyword.GetLocation()
            );
            return true;
        }

        internal static DoStatementSyntax GetEmbeddedDoStatement(SyntaxNode block)
        {
            var blockSyntax = block as BlockSyntax;
            if (blockSyntax != null)
            {
                if (blockSyntax.Statements.Count == 1)
                    return blockSyntax.Statements[0] as DoStatementSyntax;
                return null;
            }
            return block as DoStatementSyntax;
        }
    }
}