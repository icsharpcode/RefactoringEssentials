using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;

namespace RefactoringEssentials.CSharp.Diagnostics
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class ValueParameterNotUsedAnalyzer : DiagnosticAnalyzer
    {
        static readonly DiagnosticDescriptor descriptor = new DiagnosticDescriptor(
            CSharpDiagnosticIDs.ValueParameterNotUsedAnalyzerID,
            GettextCatalog.GetString("Warns about property or indexer setters and event adders or removers that do not use the value parameter"),
            GettextCatalog.GetString("The {0} does not use the 'value' parameter"),
            DiagnosticAnalyzerCategories.CodeQualityIssues,
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            helpLinkUri: HelpLink.CreateFor(CSharpDiagnosticIDs.ValueParameterNotUsedAnalyzerID)
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
                SyntaxKind.SetAccessorDeclaration,
                SyntaxKind.AddAccessorDeclaration,
                SyntaxKind.RemoveAccessorDeclaration
            );
        }

        static bool TryGetDiagnostic(SyntaxNodeAnalysisContext nodeContext, out Diagnostic diagnostic)
        {
            diagnostic = default(Diagnostic);
            var node = nodeContext.Node as AccessorDeclarationSyntax;
            var evt = node.Parent.Parent as EventDeclarationSyntax;
            if (evt != null)
            {
                if (evt.AccessorList.Accessors.Any(a => a.IsKind(SyntaxKind.AddAccessorDeclaration) && a.Body.Statements.Count == 0) &&
                    (evt.AccessorList.Accessors.Any(a => a.IsKind(SyntaxKind.RemoveAccessorDeclaration) && a.Body.Statements.Count == 0)))
                    return false;
            }
            if (!FindIssuesInAccessor(nodeContext.SemanticModel, node))
                return false;
            diagnostic = Diagnostic.Create(
                descriptor,
                node.Keyword.GetLocation(),
                GetMessageArgument(node)
            );
            return true;
        }

        static string GetMessageArgument(AccessorDeclarationSyntax node)
        {
            switch (node.Kind())
            {
                case SyntaxKind.SetAccessorDeclaration:
                    return GettextCatalog.GetString("setter");
                case SyntaxKind.AddAccessorDeclaration:
                    return GettextCatalog.GetString("add accessor");
                case SyntaxKind.RemoveAccessorDeclaration:
                    return GettextCatalog.GetString("remove accessor");
            }
            return null;
        }

        static bool FindIssuesInAccessor(SemanticModel semanticModel, AccessorDeclarationSyntax accessor)
        {
            var body = accessor.Body;
            if (!IsEligible(body))
                return false;

            if (body.Statements.Any())
            {
                var foundValueSymbol = semanticModel.LookupSymbols(body.Statements.First().SpanStart, null, "value").FirstOrDefault();
                if (foundValueSymbol == null)
                    return false;

                foreach (var valueRef in body.DescendantNodes().OfType<IdentifierNameSyntax>().Where(ins => ins.Identifier.ValueText == "value"))
                {
                    var valueRefSymbol = semanticModel.GetSymbolInfo(valueRef).Symbol;
                    if (foundValueSymbol.Equals(valueRefSymbol))
                        return false;
                }
            }

            return true;
        }

        static bool IsEligible(BlockSyntax body)
        {
            if (body == null)
                return false;
            if (body.Statements.Any(s => s is ThrowStatementSyntax))
                return false;
            return true;
        }
    }
}