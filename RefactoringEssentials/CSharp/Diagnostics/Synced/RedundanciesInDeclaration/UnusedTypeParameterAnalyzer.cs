using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;

namespace RefactoringEssentials.CSharp.Diagnostics
{
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class UnusedTypeParameterAnalyzer : DiagnosticAnalyzer
    {
        static readonly DiagnosticDescriptor descriptor = new DiagnosticDescriptor(
            CSharpDiagnosticIDs.UnusedTypeParameterAnalyzerID,
            GettextCatalog.GetString("Type parameter is never used"),
            GettextCatalog.GetString("Type parameter '{0}' is never used"),
            DiagnosticAnalyzerCategories.RedundanciesInDeclarations,
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            helpLinkUri: HelpLink.CreateFor(CSharpDiagnosticIDs.UnusedTypeParameterAnalyzerID),
            customTags: DiagnosticCustomTags.Unnecessary
        );

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(descriptor);

        //static FindReferences refFinder = new FindReferences();

        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.RegisterSyntaxNodeAction(
                AnalyzeParameterList,
                new SyntaxKind[] { SyntaxKind.TypeParameterList }
            );
        }

        static void AnalyzeParameterList(SyntaxNodeAnalysisContext nodeContext)
        {
            var node = nodeContext.Node as TypeParameterListSyntax;

            var member = node.Parent;
            if (member == null)
                return;

            var memberSymbol = nodeContext.SemanticModel.GetDeclaredSymbol(member);
            if (memberSymbol.IsAbstract || memberSymbol.IsVirtual || memberSymbol.IsOverride)
                return;
            if (memberSymbol.ExplicitInterfaceImplementations().Length > 0)
                return;
            
            var walker = new ReferenceFinder(nodeContext);
            walker.Visit(member);

            foreach (var param in node.Parameters)
            {
                var sym = nodeContext.SemanticModel.GetDeclaredSymbol(param);
                if (sym == null)
                    continue;
                if (!walker.UsedTypeParameters.Contains(sym)) {
                    var diagnostic = Diagnostic.Create(descriptor, param.Identifier.GetLocation(), sym.Name);
                    nodeContext.ReportDiagnostic(diagnostic);
                }

            }
        }

        class ReferenceFinder :  CSharpSyntaxWalker
        {
            SyntaxNodeAnalysisContext nodeContext;
            public HashSet<ISymbol> UsedTypeParameters = new HashSet<ISymbol>();

            public ReferenceFinder(SyntaxNodeAnalysisContext nodeContext)
            {
                this.nodeContext = nodeContext;
            }

            public override void VisitIdentifierName(IdentifierNameSyntax node)
            {
                base.VisitIdentifierName(node);
                var symbol = nodeContext.SemanticModel.GetSymbolInfo(node).Symbol;
                if (symbol.IsKind(SymbolKind.TypeParameter))
                    UsedTypeParameters.Add(symbol);
            }

            public override void VisitTypeParameterList(TypeParameterListSyntax node)
            {
                // skip
            }

            public override void VisitTypeConstraint(TypeConstraintSyntax node)
            {
                // skip
            }
        }
    }
}
