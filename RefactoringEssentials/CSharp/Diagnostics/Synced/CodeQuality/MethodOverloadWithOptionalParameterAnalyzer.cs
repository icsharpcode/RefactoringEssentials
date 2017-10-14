using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CSharp;
using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace RefactoringEssentials.CSharp.Diagnostics
{
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class MethodOverloadWithOptionalParameterAnalyzer : DiagnosticAnalyzer
    {
        static readonly DiagnosticDescriptor descriptor = new DiagnosticDescriptor(
            CSharpDiagnosticIDs.MethodOverloadWithOptionalParameterAnalyzerID,
            GettextCatalog.GetString("Method with optional parameter is hidden by overload"),
            GettextCatalog.GetString("{0} with optional parameter is hidden by overload"),  // Method/Indexer
            DiagnosticAnalyzerCategories.CodeQualityIssues,
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            helpLinkUri: HelpLink.CreateFor(CSharpDiagnosticIDs.MethodOverloadWithOptionalParameterAnalyzerID)
        );

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(descriptor);

        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.RegisterSyntaxNodeAction(
                AnalyzeMember, 
                new SyntaxKind[] { SyntaxKind.MethodDeclaration, SyntaxKind.IndexerDeclaration }
            );
        }

        static void AnalyzeMember(SyntaxNodeAnalysisContext ctx)
        {
            var symbol = ctx.SemanticModel.GetDeclaredSymbol(ctx.Node);
            if (symbol == null)
                return;

            var overloads = new List<ISymbol>();
            foreach (var member in symbol.ContainingType.GetMembers())
            {
                if (ctx.Node.IsKind(SyntaxKind.IndexerDeclaration))
                {
                    if (member.IsKind(SymbolKind.Property) && ((IPropertySymbol)member).IsIndexer)
                        overloads.Add(member);
                }
                else {
                    if (member.IsKind(SymbolKind.Method) && member.Name == symbol.Name) {
                        var tp1 = symbol.GetTypeParameters();
                        var tp2 = member.GetTypeParameters();
                        if (tp1.Length != tp2.Length)
                            continue;
                        bool shouldAdd = true;
                        for (int i = 0; i < tp1.Length; i++)
                        {
                            if (!tp1[i].Equals(tp2[i])) {
                                shouldAdd = false;
                                break;
                            }
                        }
                        if (shouldAdd)
                            overloads.Add(member);
                    }
                }
            }

            CheckParameters(ctx, symbol, overloads, ctx.Node.IsKind(SyntaxKind.IndexerDeclaration) ? ((IndexerDeclarationSyntax)ctx.Node).ParameterList.Parameters : ((MethodDeclarationSyntax)ctx.Node).ParameterList.Parameters);
        }

        static void CheckParameters(SyntaxNodeAnalysisContext ctx, ISymbol member, List<ISymbol> overloads, SeparatedSyntaxList<ParameterSyntax> parameterListNodes)
        {
            var memberParameters = member.GetParameters();
            for (int i = 0; i < memberParameters.Length; i++)
            {
                if (!memberParameters[i].IsOptional)
                    continue;

                foreach (var overload in overloads)
                {
                    if (overload.GetParameters().Length != i)
                        continue;
                    bool equal = true;
                    for (int j = 0; j < i; j++)
                    {
                        if (overload.GetParameters()[j].Type != memberParameters[j].Type)
                        {
                            equal = false;
                            break;
                        }
                    }
                    if (equal)
                    {
                        ctx.ReportDiagnostic( Diagnostic.Create(
                            descriptor,
                            parameterListNodes[i].GetLocation(),
                            member.IsKind(SymbolKind.Method) ? GettextCatalog.GetString("Method") : GettextCatalog.GetString("Indexer")
                        ));
                    }
                }
            }
        }
    }
}