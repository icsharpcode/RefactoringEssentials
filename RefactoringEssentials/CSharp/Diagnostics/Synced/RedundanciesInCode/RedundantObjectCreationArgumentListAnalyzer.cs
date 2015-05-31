using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace RefactoringEssentials.CSharp.Diagnostics
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    [NotPortedYet]
    public class RedundantObjectCreationArgumentListAnalyzer : DiagnosticAnalyzer
    {
        static readonly DiagnosticDescriptor descriptor = new DiagnosticDescriptor(
            CSharpDiagnosticIDs.RedundantObjectCreationArgumentListAnalyzerID,
            GettextCatalog.GetString("When object creation uses object or collection initializer, empty argument list is redundant"),
            GettextCatalog.GetString("Empty argument list is redundant"),
            DiagnosticAnalyzerCategories.RedundanciesInCode,
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            helpLinkUri: HelpLink.CreateFor(CSharpDiagnosticIDs.RedundantObjectCreationArgumentListAnalyzerID),
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

        //		class GatherVisitor : GatherVisitorBase<RedundantObjectCreationArgumentListAnalyzer>
        //		{
        //			public GatherVisitor(SemanticModel semanticModel, Action<Diagnostic> addDiagnostic, CancellationToken cancellationToken)
        //				: base (semanticModel, addDiagnostic, cancellationToken)
        //			{
        //			}

        ////			public override void VisitObjectCreateExpression(ObjectCreateExpression objectCreateExpression)
        ////			{
        ////				base.VisitObjectCreateExpression(objectCreateExpression);
        ////
        ////				if (objectCreateExpression.Initializer.IsNull ||
        ////					objectCreateExpression.Arguments.Count > 0 ||
        ////					objectCreateExpression.LParToken.IsNull)
        ////					return;
        ////
        ////				AddDiagnosticAnalyzer(new CodeIssue(objectCreateExpression.LParToken.StartLocation, objectCreateExpression.RParToken.EndLocation,
        ////				         ctx.TranslateString(""),
        ////				         ctx.TranslateString(""), script => {
        ////					var l1 = objectCreateExpression.LParToken.GetPrevNode().EndLocation;
        ////					var l2 = objectCreateExpression.RParToken.GetNextNode().StartLocation;
        ////					var o1 = script.GetCurrentOffset(l1);
        ////					var o2 = script.GetCurrentOffset(l2);
        ////
        ////					script.Replace(o1, o2 - o1, " ");
        ////					}) { IssueMarker = IssueMarker.GrayOut });
        ////			}
        //		}
    }
}
