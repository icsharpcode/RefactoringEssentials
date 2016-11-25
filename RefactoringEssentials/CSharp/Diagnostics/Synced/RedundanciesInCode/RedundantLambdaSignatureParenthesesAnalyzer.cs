using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace RefactoringEssentials.CSharp.Diagnostics
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    [NotPortedYet]
    public class RedundantLambdaSignatureParenthesesAnalyzer : DiagnosticAnalyzer
    {
        static readonly DiagnosticDescriptor descriptor = new DiagnosticDescriptor(
            CSharpDiagnosticIDs.RedundantLambdaSignatureParenthesesAnalyzerID,
            GettextCatalog.GetString("Redundant lambda signature parentheses"),
            GettextCatalog.GetString("Redundant lambda signature parentheses"),
            DiagnosticAnalyzerCategories.RedundanciesInCode,
            DiagnosticSeverity.Hidden,
            isEnabledByDefault: true,
            helpLinkUri: HelpLink.CreateFor(CSharpDiagnosticIDs.RedundantLambdaSignatureParenthesesAnalyzerID),
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

        //		class GatherVisitor : GatherVisitorBase<RedundantLambdaSignatureParenthesesAnalyzer>
        //		{
        //			public GatherVisitor(SemanticModel semanticModel, Action<Diagnostic> addDiagnostic, CancellationToken cancellationToken)
        //				: base (semanticModel, addDiagnostic, cancellationToken)
        //			{
        //			}

        ////			public override void VisitLambdaExpression(LambdaExpression lambdaExpression)
        ////			{
        ////				base.VisitLambdaExpression(lambdaExpression);
        ////				var p = lambdaExpression.Parameters.FirstOrDefault();
        ////				if (p == null || !p.Type.IsNull)
        ////					return;
        ////				var lParToken = lambdaExpression.LParToken;
        ////				if (lParToken.IsNull)
        ////					return;
        ////				if (p.GetNextSibling(n => n.Role == Roles.Parameter) != null)
        ////					return;
        ////				Action<Script> action = script =>  {
        ////					script.Remove(lParToken);
        ////					script.Remove(lambdaExpression.RParToken);
        ////					script.FormatText(lambdaExpression);
        ////				};
        ////
        ////				var issueText = ctx.TranslateString("");
        ////				var fixText = ctx.TranslateString("");
        ////				AddDiagnosticAnalyzer(new CodeIssue(lambdaExpression.LParToken, issueText, fixText, action) { IssueMarker = IssueMarker.GrayOut });
        ////				AddDiagnosticAnalyzer(new CodeIssue(lambdaExpression.RParToken, issueText, fixText, action) { IssueMarker = IssueMarker.GrayOut });
        ////			}
        //		}
    }
}