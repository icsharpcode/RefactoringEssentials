using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace RefactoringEssentials.CSharp.Diagnostics
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    [NotPortedYet]
    public class ParameterOnlyAssignedAnalyzer : VariableOnlyAssignedAnalyzer
    {
        static readonly DiagnosticDescriptor descriptor = new DiagnosticDescriptor(
            NRefactoryDiagnosticIDs.ParameterOnlyAssignedAnalyzerID,
            GettextCatalog.GetString("Parameter is assigned but its value is never used"),
            GettextCatalog.GetString("Parameter is assigned but its value is never used"),
            DiagnosticAnalyzerCategories.CodeQualityIssues,
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            helpLinkUri: HelpLink.CreateFor(NRefactoryDiagnosticIDs.ParameterOnlyAssignedAnalyzerID)
        );

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(descriptor);

        public override void Initialize(AnalysisContext context)
        {
            //context.RegisterSyntaxNodeAction(
            //	(nodeContext) => {
            //		Diagnostic diagnostic;
            //		if (TryGetDiagnostic (nodeContext, out diagnostic)) {
            //			nodeContext.ReportDiagnostic(diagnostic);
            //		}
            //	}, 
            //	new SyntaxKind[] { SyntaxKind.None }
            //);
        }

        static bool TryGetDiagnostic(SyntaxNodeAnalysisContext nodeContext, out Diagnostic diagnostic)
        {
            diagnostic = default(Diagnostic);
            if (nodeContext.IsFromGeneratedCode())
                return false;
            //var node = nodeContext.Node as ;
            //diagnostic = Diagnostic.Create (descriptor, node.GetLocation ());
            //return true;
            return false;
        }

        //		private class GatherVisitor : GatherVisitorBase<ParameterOnlyAssignedAnalyzer>
        //		{
        //			public GatherVisitor(SemanticModel semanticModel, Action<Diagnostic> addDiagnostic, CancellationToken cancellationToken)
        //				: base(semanticModel, addDiagnostic, cancellationToken)
        //			{
        //			}

        ////			public override void VisitParameterDeclaration(ParameterDeclaration parameterDeclaration)
        ////			{
        ////				base.VisitParameterDeclaration(parameterDeclaration);
        ////
        ////				var resolveResult = ctx.Resolve(parameterDeclaration) as LocalResolveResult;
        ////				if (resolveResult == null)
        ////					return;
        ////
        ////				var parameterModifier = parameterDeclaration.ParameterModifier;
        ////				if (parameterModifier == ParameterModifier.Out || parameterModifier == ParameterModifier.Ref ||
        ////					!TestOnlyAssigned(ctx, parameterDeclaration.Parent, resolveResult.Variable)) {
        ////					return;
        ////				}
        ////				AddDiagnosticAnalyzer(new CodeIssue(parameterDeclaration.NameToken, 
        //			//					ctx.TranslateString("Parameter is assigned but its value is never used")));
        ////			}
        //		}
    }
}
