using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System.Linq;

namespace RefactoringEssentials.CSharp.Diagnostics
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class RedundantCatchClauseAnalyzer : DiagnosticAnalyzer
    {
        private static readonly DiagnosticDescriptor descriptor = new DiagnosticDescriptor(
            CSharpDiagnosticIDs.RedundantCatchClauseAnalyzerID,
            GettextCatalog.GetString("Catch clause with a single 'throw' statement is redundant"),
            GettextCatalog.GetString("Redundant catch clause will be removed"),
            DiagnosticAnalyzerCategories.RedundanciesInCode,
            DiagnosticSeverity.Info,
            isEnabledByDefault: true,
            helpLinkUri: HelpLink.CreateFor(CSharpDiagnosticIDs.RedundantCatchClauseAnalyzerID),
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
                  SyntaxKind.CatchClause
            );
        }

        private static bool TryGetDiagnostic(SyntaxNodeAnalysisContext nodeContext, out Diagnostic diagnostic)
        {
            diagnostic = default(Diagnostic);
            if (nodeContext.IsFromGeneratedCode())
                return false;

            var catchClause = nodeContext.Node as CatchClauseSyntax;
            var tryStatement = catchClause?.Parent as TryStatementSyntax;
            if (tryStatement == null || tryStatement.Finally != null)
                return false;


            var catchBlock = catchClause.Block;

            if (catchBlock == null || !catchBlock.Statements.Any())
                return false;

            if (IsRedundant(catchClause, nodeContext))
            {
                diagnostic = Diagnostic.Create(descriptor, catchClause.GetLocation());
                return true;
            }
            return false;
        }

        private static bool IsThrowsClause(CatchClauseSyntax catchClause)
        {
            var firstStatement = catchClause.Block.Statements.FirstOrDefault();
            var throwStatement = firstStatement as ThrowStatementSyntax;
            return (throwStatement != null);
        }

        private static bool IsRedundant(CatchClauseSyntax catchClause, SyntaxNodeAnalysisContext nodeContext)
        {
            if (!IsThrowsClause(catchClause))
                return false;

            var type = nodeContext.SemanticModel.GetTypeInfo(catchClause).ConvertedType;
            var tryStatement = catchClause.Parent as TryStatementSyntax;
            if (tryStatement == null)
                return false;

            var catches = tryStatement.Catches;
            var index = catches.IndexOf(catchClause);
            var nextSibling = catches.ElementAtOrDefault(index + 1);
            var typeInfo = nodeContext.SemanticModel.GetTypeInfo(catchClause).Type;

            while (nextSibling != null)
            {
                var nextClause = nextSibling;
                var nextTypeInfo = nodeContext.SemanticModel.GetTypeInfo(nextClause).Type;
                if (!IsThrowsClause(nextClause)  || nextClause.Declaration != null && nextClause.Declaration.Type == null)
                    return false;
                nextSibling = catches.ElementAtOrDefault(catches.IndexOf(nextSibling) + 1);
            }
            return true;
        }

        private bool InheritsFrom<T>(INamedTypeSymbol symbol)
        {
            while (true)
            {
                if (symbol.ToString() == typeof(T).FullName)
                {
                    return true;
                }
                if (symbol.BaseType != null)
                {
                    symbol = symbol.BaseType;
                    continue;
                }
                break;
            }
            return false;
        }
    }
}