using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;

namespace RefactoringEssentials.CSharp.Diagnostics
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class ConvertToStaticTypeAnalyzer : DiagnosticAnalyzer
    {
        static readonly DiagnosticDescriptor descriptor = new DiagnosticDescriptor(
            CSharpDiagnosticIDs.ConvertToStaticTypeAnalyzerID,
            GettextCatalog.GetString("If all fields, properties and methods members are static, the class can be made static."),
            GettextCatalog.GetString("This class is recommended to be defined as static"),
            DiagnosticAnalyzerCategories.Opportunities,
            DiagnosticSeverity.Info,
            isEnabledByDefault: true,
            helpLinkUri: HelpLink.CreateFor(CSharpDiagnosticIDs.ConvertToStaticTypeAnalyzerID)
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
                new SyntaxKind[] { SyntaxKind.ClassDeclaration }
            );
        }

        static bool TryGetDiagnostic(SyntaxNodeAnalysisContext nodeContext, out Diagnostic diagnostic)
        {
            var node = nodeContext.Node as ClassDeclarationSyntax;
            var semanticModel = nodeContext.SemanticModel;
            var cancellationToken = nodeContext.CancellationToken;

            diagnostic = default(Diagnostic);
            ITypeSymbol classType = semanticModel.GetDeclaredSymbol(node);
            if (!node.Modifiers.Any() || node.Modifiers.Any(m => m.IsKind(SyntaxKind.PartialKeyword)) || classType.IsAbstract || classType.IsStatic)
                return false;
            if ((node.BaseList != null) && node.BaseList.Types.Any())
                return false;
            IEnumerable<ISymbol> members = classType.GetMembers().Where(m => !(m is ITypeSymbol));
            if (!members.Any(m => !m.IsImplicitlyDeclared)) // Ignore implicitly declared (e.g. default ctor)
                return false;
            if (Enumerable.Any(members, f => (!f.IsStatic && !f.IsImplicitlyDeclared) || (f is IMethodSymbol && IsMainMethod((IMethodSymbol)f))))
                return false;

            diagnostic = Diagnostic.Create(
                descriptor,
                node.Identifier.GetLocation()
            );
            return true;
        }

        internal static bool IsMainMethod(IMethodSymbol m)
        {
            return (m.ReturnType.SpecialType == SpecialType.System_Int32 || m.ReturnType.SpecialType == SpecialType.System_Void) && m.IsStatic && m.Name.Equals("Main");
        }
    }
}