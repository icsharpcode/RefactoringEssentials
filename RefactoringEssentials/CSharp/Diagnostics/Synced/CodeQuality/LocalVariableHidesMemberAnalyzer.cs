using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;

namespace RefactoringEssentials.CSharp.Diagnostics
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class LocalVariableHidesMemberAnalyzer : DiagnosticAnalyzer
    {
        static readonly DiagnosticDescriptor descriptor = new DiagnosticDescriptor(
            CSharpDiagnosticIDs.LocalVariableHidesMemberAnalyzerID,
            GettextCatalog.GetString("Local variable has the same name as a member and hides it"),
            GettextCatalog.GetString("Local variable '{0}' hides {1} '{2}'"),
            DiagnosticAnalyzerCategories.CodeQualityIssues,
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            helpLinkUri: HelpLink.CreateFor(CSharpDiagnosticIDs.LocalVariableHidesMemberAnalyzerID)
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
                new SyntaxKind[] { SyntaxKind.LocalDeclarationStatement }
            );
            context.RegisterSyntaxNodeAction(
                (nodeContext) =>
                {
                    Diagnostic diagnostic;
                    if (TryGetDiagnosticFromForeach(nodeContext, out diagnostic))
                    {
                        nodeContext.ReportDiagnostic(diagnostic);
                    }
                },
                new SyntaxKind[] { SyntaxKind.ForEachStatement }
            );
        }

        static string GetMemberType(SymbolKind symbolKind)
        {
            switch (symbolKind)
            {
                case SymbolKind.Field:
                    return GettextCatalog.GetString("field");
                case SymbolKind.Method:
                    return GettextCatalog.GetString("method");
                case SymbolKind.Property:
                    return GettextCatalog.GetString("property");
                case SymbolKind.Event:
                    return GettextCatalog.GetString("event");
            }

            return GettextCatalog.GetString("member");
        }

        static bool TryGetDiagnostic(SyntaxNodeAnalysisContext nodeContext, out Diagnostic diagnostic)
        {
            diagnostic = default(Diagnostic);
            var node = nodeContext.Node as LocalDeclarationStatementSyntax;
            var member = node.AncestorsAndSelf().FirstOrDefault(n => n is MemberDeclarationSyntax);
            if (member == null)
                return false;
            var symbols = nodeContext.SemanticModel.LookupSymbols(member.SpanStart);
            var memberSymbol = nodeContext.SemanticModel.GetDeclaredSymbol(member);

            foreach (var variable in node.Declaration.Variables)
            {
                var hidingMember = symbols.FirstOrDefault(v =>
                    v.Name == variable.Identifier.ValueText 
                        && ((memberSymbol.IsStatic && v.IsStatic) || !memberSymbol.IsStatic) 
                        && !v.IsKind(SymbolKind.Local) 
                        && !v.IsKind(SymbolKind.Parameter)
                        && !v.IsType());
                if (hidingMember == null)
                    continue;

                var mre = variable.Initializer?.Value as MemberAccessExpressionSyntax;
                if (mre != null && mre.Name.Identifier.ValueText == hidingMember.Name && mre.Expression.IsKind(SyntaxKind.ThisExpression))
                {
                    // Special case: the variable is initialized from the member it is hiding
                    // In this case, the hiding is obviously intentional and we shouldn't show a warning.
                    continue;
                }
                string memberType = GetMemberType(hidingMember.Kind);
                diagnostic = Diagnostic.Create(descriptor, variable.Identifier.GetLocation(), variable.Identifier, memberType, hidingMember.Name);
                return true;
            }
            return false;
        }

        static bool TryGetDiagnosticFromForeach(SyntaxNodeAnalysisContext nodeContext, out Diagnostic diagnostic)
        {
            diagnostic = default(Diagnostic);
            var node = nodeContext.Node as ForEachStatementSyntax;
            var member = node.AncestorsAndSelf().FirstOrDefault(n => n is MemberDeclarationSyntax);
            if (member == null)
                return false;
            var symbols = nodeContext.SemanticModel.LookupSymbols(member.SpanStart);
            var memberSymbol = nodeContext.SemanticModel.GetDeclaredSymbol(member);

            var hidingMember = symbols.FirstOrDefault(v => v.Name == node.Identifier.ValueText && ((memberSymbol.IsStatic && v.IsStatic) || !memberSymbol.IsStatic) && !v.IsKind(SymbolKind.Local) && !v.IsKind(SymbolKind.Parameter));
            if (hidingMember == null)
                return false;

            string memberType = GetMemberType(hidingMember.Kind);
            diagnostic = Diagnostic.Create(descriptor, node.Identifier.GetLocation(), node.Identifier, memberType, hidingMember.Name);
            return true;
        }
    }
}