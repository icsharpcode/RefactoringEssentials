using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CSharp;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace RefactoringEssentials.CSharp.Diagnostics
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class ParameterHidesMemberAnalyzer : DiagnosticAnalyzer
    {
        static readonly DiagnosticDescriptor descriptor = new DiagnosticDescriptor(
            CSharpDiagnosticIDs.ParameterHidesMemberAnalyzerID,
            GettextCatalog.GetString("Parameter has the same name as a member and hides it"),
            "{0}",
            DiagnosticAnalyzerCategories.CodeQualityIssues,
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            helpLinkUri: HelpLink.CreateFor(CSharpDiagnosticIDs.ParameterHidesMemberAnalyzerID)
        );

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(descriptor);

        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.RegisterSyntaxNodeAction(AnalyzeParameterList, new SyntaxKind[] { SyntaxKind.ParameterList });
        }

        static void AnalyzeParameterList(SyntaxNodeAnalysisContext nodeContext)
        {
            var node = nodeContext.Node as ParameterListSyntax;

            var member = node.AncestorsAndSelf().FirstOrDefault(n => n is MemberDeclarationSyntax);
            if (member == null)
                return;

            var symbols = nodeContext.SemanticModel.LookupSymbols(node.SpanStart);
            var memberSymbol = nodeContext.SemanticModel.GetDeclaredSymbol(member);
            if (memberSymbol == null)
                return;
            if (memberSymbol.IsAbstract || memberSymbol.DeclaredAccessibility == Accessibility.Public || memberSymbol.DeclaredAccessibility == Accessibility.Protected || memberSymbol.IsOverride)
                return;
            if (memberSymbol is IMethodSymbol && ((IMethodSymbol)memberSymbol).MethodKind == MethodKind.Constructor || memberSymbol.ExplicitInterfaceImplementations().Length > 0)
                return;
            foreach (var param in node.Parameters)
            {
                var hidingMember = symbols.FirstOrDefault(v => v.Name == param.Identifier.ValueText && ((memberSymbol.IsStatic && v.IsStatic) || !memberSymbol.IsStatic) && !v.IsKind(SymbolKind.Local) && !v.IsKind(SymbolKind.Parameter));
                if (hidingMember == null)
                    continue;
                string msg;
                switch (hidingMember.Kind)
                {
                    case SymbolKind.Field:
                        msg = GettextCatalog.GetString("Parameter '{0}' hides field '{1}'");
                        break;
                    case SymbolKind.Method:
                        msg = GettextCatalog.GetString("Parameter '{0}' hides method '{1}'");
                        break;
                    case SymbolKind.Property:
                        msg = GettextCatalog.GetString("Parameter '{0}' hides property '{1}'");
                        break;
                    case SymbolKind.Event:
                        msg = GettextCatalog.GetString("Parameter '{0}' hides event '{1}'");
                        break;
                    default:
                        msg = GettextCatalog.GetString("Parameter '{0}' hides member '{1}'");
                        break;
                }

                var diagnostic = Diagnostic.Create(descriptor, param.Identifier.GetLocation(), string.Format(msg, param.Identifier.ValueText, hidingMember.Name));
                nodeContext.ReportDiagnostic(diagnostic);
            }
        }
    }
}