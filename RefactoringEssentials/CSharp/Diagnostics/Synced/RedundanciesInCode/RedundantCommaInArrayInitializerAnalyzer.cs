using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace RefactoringEssentials.CSharp.Diagnostics
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    [NotPortedYet]
    public class RedundantCommaInArrayInitializerAnalyzer : DiagnosticAnalyzer
    {
        static readonly DiagnosticDescriptor descriptor = new DiagnosticDescriptor(
            CSharpDiagnosticIDs.RedundantCommaInArrayInitializerAnalyzerID,
            GettextCatalog.GetString("Redundant comma in array initializer"),
            GettextCatalog.GetString("Redundant comma in {0}"),
            DiagnosticAnalyzerCategories.RedundanciesInCode,
            DiagnosticSeverity.Info,
            isEnabledByDefault: true,
            helpLinkUri: HelpLink.CreateFor(CSharpDiagnosticIDs.RedundantCommaInArrayInitializerAnalyzerID),
            customTags: DiagnosticCustomTags.Unnecessary
        );

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(descriptor);

        //			"Redundant comma in object initializer"
        //			"Redundant comma in collection initializer"
        //			"Redundant comma in array initializer"

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

        //		class GatherVisitor : GatherVisitorBase<RedundantCommaInArrayInitializerAnalyzer>
        //		{
        //			public GatherVisitor(SemanticModel semanticModel, Action<Diagnostic> addDiagnostic, CancellationToken cancellationToken)
        //				: base (semanticModel, addDiagnostic, cancellationToken)
        //			{
        //			}

        ////			public override void VisitArrayInitializerExpression(ArrayInitializerExpression arrayInitializerExpression)
        ////			{
        ////				base.VisitArrayInitializerExpression(arrayInitializerExpression);
        ////
        ////				if (arrayInitializerExpression.IsSingleElement)
        ////					return;
        ////
        ////				var commaToken = arrayInitializerExpression.RBraceToken.PrevSibling as CSharpTokenNode;
        ////				if (commaToken == null || commaToken.ToString() != ",")
        ////					return;
        ////				string issueDescription;
        ////				if (arrayInitializerExpression.Parent is ObjectCreateExpression) {
        ////					if (arrayInitializerExpression.Elements.FirstOrNullObject() is NamedExpression) {
        ////						issueDescription = ctx.TranslateString("");
        ////					} else {
        ////						issueDescription = ctx.TranslateString("");
        ////					}
        ////				} else {
        ////					issueDescription = ctx.TranslateString("");
        ////				}
        ////				AddDiagnosticAnalyzer(new CodeIssue(commaToken,
        ////				         issueDescription,
        ////				         ctx.TranslateString(""),
        ////					script => script.Remove(commaToken)) { IssueMarker = IssueMarker.GrayOut });
        ////			}
        //		}
    }
}
