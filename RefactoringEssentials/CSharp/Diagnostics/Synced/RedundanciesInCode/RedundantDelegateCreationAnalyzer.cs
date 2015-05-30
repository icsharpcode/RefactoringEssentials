using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace RefactoringEssentials.CSharp.Diagnostics
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    [NotPortedYet]
    public class RedundantDelegateCreationAnalyzer : DiagnosticAnalyzer
    {
        static readonly DiagnosticDescriptor descriptor = new DiagnosticDescriptor(
            NRefactoryDiagnosticIDs.RedundantDelegateCreationAnalyzerID,
            GettextCatalog.GetString("Explicit delegate creation expression is redundant"),
            GettextCatalog.GetString("Redundant explicit delegate declaration"),
            DiagnosticAnalyzerCategories.RedundanciesInCode,
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            helpLinkUri: HelpLink.CreateFor(NRefactoryDiagnosticIDs.RedundantDelegateCreationAnalyzerID),
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

        //		class GatherVisitor : GatherVisitorBase<RedundantDelegateCreationAnalyzer>
        //		{
        //			public GatherVisitor(SemanticModel semanticModel, Action<Diagnostic> addDiagnostic, CancellationToken cancellationToken)
        //				: base (semanticModel, addDiagnostic, cancellationToken)
        //			{
        //			}

        ////			public override void VisitAssignmentExpression(AssignmentExpression assignmentExpression)
        ////			{
        ////				base.VisitAssignmentExpression(assignmentExpression);
        ////				if (assignmentExpression.Operator != AssignmentOperatorType.Add && assignmentExpression.Operator != AssignmentOperatorType.Subtract)
        ////					return;
        ////				var oce = assignmentExpression.Right as ObjectCreateExpression;
        ////				if (oce == null || oce.Arguments.Count != 1)
        ////					return;
        ////				var left = ctx.Resolve(assignmentExpression.Left) as MemberResolveResult;
        ////				if (left == null || left.Member.SymbolKind != SymbolKind.Event)
        ////					return;
        ////				var right = ctx.Resolve(assignmentExpression.Right);
        ////				if (right.IsError || !Equals(left.Type, right.Type))
        ////					return;
        ////				AddDiagnosticAnalyzer(new CodeIssue(oce.StartLocation, oce.Type.EndLocation,
        ////					ctx.TranslateString(""),
        ////					ctx.TranslateString(""),
        ////					s => s.Replace(assignmentExpression.Right, oce.Arguments.First())
        ////				) { IssueMarker = IssueMarker.GrayOut });
        ////			}
        //		}
    }
}