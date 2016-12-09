using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace RefactoringEssentials.CSharp.Diagnostics
{
    // OfType -> Underline (+suggest to compare to null)
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    [NotPortedYet]
    public class RedundantEnumerableCastCallAnalyzer : DiagnosticAnalyzer
    {
        static readonly DiagnosticDescriptor descriptor = new DiagnosticDescriptor(
            CSharpDiagnosticIDs.RedundantEnumerableCastCallAnalyzerID,
            GettextCatalog.GetString("Redundant 'IEnumerable.Cast<T>' or 'IEnumerable.OfType<T>' call"),
            GettextCatalog.GetString("Redundant '{0}' call"),
            DiagnosticAnalyzerCategories.RedundanciesInCode,
            DiagnosticSeverity.Hidden,
            isEnabledByDefault: true,
            helpLinkUri: HelpLink.CreateFor(CSharpDiagnosticIDs.RedundantEnumerableCastCallAnalyzerID),
            customTags: DiagnosticCustomTags.Unnecessary
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

        //		class GatherVisitor : GatherVisitorBase<RedundantEnumerableCastCallAnalyzer>
        //		{
        //			public GatherVisitor(SemanticModel semanticModel, Action<Diagnostic> addDiagnostic, CancellationToken cancellationToken)
        //				: base (semanticModel, addDiagnostic, cancellationToken)
        //			{
        //			}

        ////			public override void VisitInvocationExpression(InvocationExpression invocationExpression)
        ////			{
        ////				base.VisitInvocationExpression(invocationExpression);
        ////				var mt = invocationExpression.Target as MemberReferenceExpression;
        ////				if (mt == null)
        ////					return;
        ////				var rr = ctx.Resolve(invocationExpression) as CSharpInvocationResolveResult;
        ////				if (rr == null || rr.IsError)
        ////					return;
        ////				if (rr.Member.DeclaringType.Name != "Enumerable" || rr.Member.DeclaringType.Namespace != "System.Linq")
        ////					return;
        ////				bool isCast = rr.Member.Name == "Cast";
        ////				if (!isCast && rr.Member.Name != "OfType")
        ////					return;
        ////				var tr = ctx.Resolve(mt.Target);
        ////				if (tr.Type.Equals(rr.Type) || tr.Type.GetAllBaseTypes().Any (bt=> bt.Equals(rr.Type))) {
        ////					if (isCast) {
        ////						AddDiagnosticAnalyzer(new CodeIssue(
        ////							mt.DotToken.StartLocation,
        ////							mt.EndLocation,
        ////							ctx.TranslateString(""),
        ////							ctx.TranslateString(""),
        ////							s => s.Replace(invocationExpression, mt.Target.Clone())
        ////						) { IssueMarker = IssueMarker.GrayOut });
        ////					} else {
        ////						AddDiagnosticAnalyzer(new CodeIssue(
        ////							mt.DotToken.StartLocation,
        ////							mt.EndLocation,
        ////							ctx.TranslateString(""),
        ////							new [] {
        ////								new CodeAction(
        ////									ctx.TranslateString("Compare items with null"),
        ////									s => {
        ////										var name = ctx.GetNameProposal("i", mt.StartLocation);
        ////										s.Replace(invocationExpression, 
        ////											new InvocationExpression(
        ////												new MemberReferenceExpression(mt.Target.Clone(), "Where"), 
        ////												new LambdaExpression {
        ////													Parameters = { new ParameterDeclaration(name) },
        ////													Body = new BinaryOperatorExpression(new IdentifierExpression(name), BinaryOperatorType.InEquality, new NullReferenceExpression())
        ////												}
        ////											)
        ////										);
        ////									},
        ////									mt
        ////								),
        ////								new CodeAction(
        ////									ctx.TranslateString("Remove 'OfType<T>' call"),
        ////									s => s.Replace(invocationExpression, mt.Target.Clone()),
        ////									mt
        ////								),
        ////							}
        ////						));
        ////					}
        ////				}
        ////			}
        //		}
    }
}