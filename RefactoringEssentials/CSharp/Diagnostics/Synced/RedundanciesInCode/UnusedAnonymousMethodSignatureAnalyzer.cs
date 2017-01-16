using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace RefactoringEssentials.CSharp.Diagnostics
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    [NotPortedYet]
    public class UnusedAnonymousMethodSignatureAnalyzer : DiagnosticAnalyzer
    {
        static readonly DiagnosticDescriptor descriptor = new DiagnosticDescriptor(
            CSharpDiagnosticIDs.UnusedAnonymousMethodSignatureAnalyzerID,
            GettextCatalog.GetString("Detects when no delegate parameter is used in the anonymous method body"),
            GettextCatalog.GetString("Specifying signature is redundant because no parameter is used"),
            DiagnosticAnalyzerCategories.RedundanciesInCode,
            DiagnosticSeverity.Hidden,
            isEnabledByDefault: true,
            helpLinkUri: HelpLink.CreateFor(CSharpDiagnosticIDs.UnusedAnonymousMethodSignatureAnalyzerID)
        );

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(descriptor);

        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
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
            //var node = nodeContext.Node as ;
            //diagnostic = Diagnostic.Create (descriptor, node.GetLocation ());
            //return true;
            return false;
        }

        //		class GatherVisitor : GatherVisitorBase<UnusedAnonymousMethodSignatureAnalyzer>
        //		{
        //			public GatherVisitor(SemanticModel semanticModel, Action<Diagnostic> addDiagnostic, CancellationToken cancellationToken)
        //				: base (semanticModel, addDiagnostic, cancellationToken)
        //			{
        //			}

        ////			bool IsParameterListRedundant(Expression expression)
        ////			{
        ////				var validTypes = TypeGuessing.GetValidTypes(ctx.Resolver, expression);
        ////				return validTypes.Count(t => t.Kind == TypeKind.Delegate) == 1;
        ////			}
        ////
        ////			public override void VisitAnonymousMethodExpression(AnonymousMethodExpression anonymousMethodExpression)
        ////			{
        ////				base.VisitAnonymousMethodExpression(anonymousMethodExpression);
        ////				if (!anonymousMethodExpression.HasParameterList || !IsParameterListRedundant(anonymousMethodExpression))
        ////					return;
        ////
        ////				var parameters = anonymousMethodExpression.Parameters.ToList();
        ////				if (parameters.Count > 0) {
        ////					var usageAnalysis = new ConvertToConstantAnalyzer.VariableUsageAnalyzation(ctx);
        ////					anonymousMethodExpression.Body.AcceptVisitor(usageAnalysis); 
        ////					foreach (var parameter in parameters) {
        ////						var rr = ctx.Resolve(parameter) as LocalResolveResult;
        ////						if (rr == null)
        ////							continue;
        ////						if (usageAnalysis.GetStatus(rr.Variable) != RefactoringEssentials.Refactoring.ExtractMethod.VariableState.None)
        ////							return;
        ////					}
        ////				}
        ////
        ////				AddDiagnosticAnalyzer(new CodeIssue(anonymousMethodExpression.LParToken.StartLocation,
        ////					anonymousMethodExpression.RParToken.EndLocation,
        ////					ctx.TranslateString(""),
        ////					ctx.TranslateString(""),
        ////					script => {
        ////						int start = script.GetCurrentOffset(anonymousMethodExpression.DelegateToken.EndLocation);
        ////						int end = script.GetCurrentOffset(anonymousMethodExpression.Body.StartLocation);
        ////
        ////						script.Replace(start, end - start, " ");
        ////					}) { IssueMarker = IssueMarker.GrayOut });
        ////			}
        //		}
    }
}