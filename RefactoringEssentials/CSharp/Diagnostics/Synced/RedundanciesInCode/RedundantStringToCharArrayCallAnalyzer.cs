using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace RefactoringEssentials.CSharp.Diagnostics
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    [NotPortedYet]
    public class RedundantStringToCharArrayCallAnalyzer : DiagnosticAnalyzer
    {
        static readonly DiagnosticDescriptor descriptor = new DiagnosticDescriptor(
            CSharpDiagnosticIDs.RedundantStringToCharArrayCallAnalyzerID,
            GettextCatalog.GetString("Redundant 'string.ToCharArray()' call"),
            GettextCatalog.GetString("Redundant 'string.ToCharArray()' call"),
            DiagnosticAnalyzerCategories.RedundanciesInCode,
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            helpLinkUri: HelpLink.CreateFor(CSharpDiagnosticIDs.RedundantStringToCharArrayCallAnalyzerID),
            customTags: DiagnosticCustomTags.Unnecessary
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

        //		class GatherVisitor : GatherVisitorBase<RedundantStringToCharArrayCallAnalyzer>
        //		{
        //			public GatherVisitor(SemanticModel semanticModel, Action<Diagnostic> addDiagnostic, CancellationToken cancellationToken)
        //				: base (semanticModel, addDiagnostic, cancellationToken)
        //			{
        //			}

        ////			void AddProblem(AstNode replaceExpression, InvocationExpression invocationExpression)
        ////			{
        ////				var t = invocationExpression.Target as MemberReferenceExpression;
        ////				AddDiagnosticAnalyzer(new CodeIssue (
        ////					t.DotToken.StartLocation,
        ////					t.MemberNameToken.EndLocation,
        ////					ctx.TranslateString(""),
        ////					ctx.TranslateString(""),
        ////					s => s.Replace(replaceExpression, t.Target.Clone())
        ////				) { IssueMarker = IssueMarker.GrayOut });
        ////			}
        ////
        ////			public override void VisitInvocationExpression(InvocationExpression invocationExpression)
        ////			{
        ////				base.VisitInvocationExpression(invocationExpression);
        ////
        ////				var t = invocationExpression.Target as MemberReferenceExpression;
        ////				if (t == null || t.MemberName != "ToCharArray")
        ////					return;
        ////				var rr = ctx.Resolve(t.Target);
        ////				if (!rr.Type.IsKnownType(KnownTypeCode.String))
        ////					return;
        ////				if (invocationExpression.Parent is ForeachStatement && invocationExpression.Role == Roles.Expression) {
        ////					AddProblem(invocationExpression, invocationExpression);
        ////					return;
        ////				}
        ////				var p = invocationExpression.Parent;
        ////				while (p is ParenthesizedExpression) {
        ////					p = p.Parent;
        ////				}
        ////				var idx = p as IndexerExpression;
        ////				if (idx != null) {
        ////					AddProblem(idx.Target, invocationExpression);
        ////					return;
        ////				}
        ////			}
        //		}
    }
}