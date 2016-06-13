using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace RefactoringEssentials.CSharp.Diagnostics
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    [NotPortedYet]
    public class PolymorphicFieldLikeEventInvocationAnalyzer : DiagnosticAnalyzer
    {
        static readonly DiagnosticDescriptor descriptor = new DiagnosticDescriptor(
            CSharpDiagnosticIDs.PolymorphicFieldLikeEventInvocationAnalyzerID,
            GettextCatalog.GetString("Invocation of polymorphic field event leads to unpredictable result since invocation lists are not virtual"),
            GettextCatalog.GetString("The event `{0}' can only appear on the left hand side of `+=' or `-=' operator"),
            DiagnosticAnalyzerCategories.CodeQualityIssues,
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            helpLinkUri: HelpLink.CreateFor(CSharpDiagnosticIDs.PolymorphicFieldLikeEventInvocationAnalyzerID)
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

        //		class GatherVisitor : GatherVisitorBase<PolymorphicFieldLikeEventInvocationAnalyzer>
        //		{
        //			public GatherVisitor(SemanticModel semanticModel, Action<Diagnostic> addDiagnostic, CancellationToken cancellationToken)
        //				: base (semanticModel, addDiagnostic, cancellationToken)
        //			{
        //			}

        ////			public override void VisitInvocationExpression(InvocationExpression invocationExpression)
        ////			{
        ////				base.VisitInvocationExpression(invocationExpression);
        ////				var rr = ctx.Resolve(invocationExpression.Target) as MemberResolveResult;
        ////				if (rr == null || rr.IsError)
        ////					return;
        ////				var evt = rr.Member as IEvent;
        ////				if (evt == null || !evt.IsOverride)
        ////					return;
        ////				if (evt.AddAccessor.HasBody) {
        ////					AddDiagnosticAnalyzer(new CodeIssue(
        ////						invocationExpression.Target,
        //			//						string.Format(ctx.TranslateString("The event `{0}' can only appear on the left hand side of `+=' or `-=' operator"), evt.Name)
        ////					));
        ////					return;
        ////				}
        ////				AddDiagnosticAnalyzer(new CodeIssue(
        ////					invocationExpression,
        ////					ctx.TranslateString("Invocation of polymorphic field like event")));
        ////			}
        //		}
    }
}