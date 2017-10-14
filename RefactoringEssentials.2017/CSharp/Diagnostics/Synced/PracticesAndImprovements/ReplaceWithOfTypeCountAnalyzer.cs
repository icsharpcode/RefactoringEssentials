using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace RefactoringEssentials.CSharp.Diagnostics
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class ReplaceWithOfTypeCountAnalyzer : DiagnosticAnalyzer
    {
        static readonly DiagnosticDescriptor descriptor = new DiagnosticDescriptor(
            CSharpDiagnosticIDs.ReplaceWithOfTypeCountAnalyzerID,
            GettextCatalog.GetString("Replace with call to OfType<T>().Count()"),
            GettextCatalog.GetString("Replace with 'OfType<T>().Count()'"),
            DiagnosticAnalyzerCategories.PracticesAndImprovements,
            DiagnosticSeverity.Info,
            isEnabledByDefault: true,
            helpLinkUri: HelpLink.CreateFor(CSharpDiagnosticIDs.ReplaceWithOfTypeCountAnalyzerID)
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

            ExpressionSyntax target, followUp;
            TypeSyntax type;
            ParameterSyntax param;
            if (ReplaceWithOfTypeAnyAnalyzer.MatchSelect(anyInvoke, out target, out type, out param, out followUp))
            {
                // if (member == "Where" && followUp == null) return;
                diagnostic = Diagnostic.Create(
                    descriptor,
                    anyInvoke.GetLocation()
                );
                return true;
            }
            return false;
        }

        static bool HasPredicateVersion(IMethodSymbol member)
        {
            if (!ReplaceWithOfTypeAnyAnalyzer.IsQueryExtensionClass(member.ContainingType))
                return false;
            return member.Name == "Count";
        }
    }
}