using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace RefactoringEssentials.CSharp.Diagnostics
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class ReplaceWithSingleCallToSingleAnalyzer : DiagnosticAnalyzer
    {
        static readonly DiagnosticDescriptor descriptor = new DiagnosticDescriptor(
            CSharpDiagnosticIDs.ReplaceWithSingleCallToSingleAnalyzerID,
            GettextCatalog.GetString("Redundant Where() call with predicate followed by Single()"),
            GettextCatalog.GetString("Replace with single call to 'Single()'"),
            DiagnosticAnalyzerCategories.PracticesAndImprovements,
            DiagnosticSeverity.Info,
            isEnabledByDefault: true,
            helpLinkUri: HelpLink.CreateFor(CSharpDiagnosticIDs.ReplaceWithSingleCallToSingleAnalyzerID)
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
                SyntaxKind.InvocationExpression
            );
        }

        static bool TryGetDiagnostic(SyntaxNodeAnalysisContext nodeContext, out Diagnostic diagnostic)
        {
            diagnostic = default(Diagnostic);
            var anyInvoke = nodeContext.Node as InvocationExpressionSyntax;
            var info = nodeContext.SemanticModel.GetSymbolInfo(anyInvoke);

            IMethodSymbol anyResolve = info.Symbol as IMethodSymbol;
            if (anyResolve == null)
            {
                anyResolve = info.CandidateSymbols.OfType<IMethodSymbol>().FirstOrDefault(candidate => HasPredicateVersion(candidate));
            }

            if (anyResolve == null || !HasPredicateVersion(anyResolve))
                return false;

            ExpressionSyntax target;
            InvocationExpressionSyntax whereInvoke;
            if (!ReplaceWithSingleCallToAnyAnalyzer.MatchWhere(anyInvoke, out target, out whereInvoke))
                return false;
            info = nodeContext.SemanticModel.GetSymbolInfo(whereInvoke);
            IMethodSymbol whereResolve = info.Symbol as IMethodSymbol;
            if (whereResolve == null)
            {
                whereResolve = info.CandidateSymbols.OfType<IMethodSymbol>().FirstOrDefault(candidate => candidate.Name == "Where" && ReplaceWithSingleCallToAnyAnalyzer.IsQueryExtensionClass(candidate.ContainingType));
            }

            if (whereResolve == null || whereResolve.Name != "Where" || !ReplaceWithSingleCallToAnyAnalyzer.IsQueryExtensionClass(whereResolve.ContainingType))
                return false;
            if (whereResolve.Parameters.Length != 1)
                return false;
            var predResolve = whereResolve.Parameters[0];
            if (predResolve.Type.GetTypeParameters().Length != 2)
                return false;
            diagnostic = Diagnostic.Create(
                descriptor,
                anyInvoke.GetLocation()
            );
            return true;
        }

        static bool HasPredicateVersion(IMethodSymbol member)
        {
            if (!ReplaceWithSingleCallToAnyAnalyzer.IsQueryExtensionClass(member.ContainingType))
                return false;
            return member.Name == "Single";
        }
    }
}