using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace RefactoringEssentials.CSharp.Diagnostics
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    [NotPortedYet]
    public class RedundantComparisonWithNullAnalyzer : DiagnosticAnalyzer
    {
        static readonly DiagnosticDescriptor descriptor = new DiagnosticDescriptor(
            CSharpDiagnosticIDs.RedundantComparisonWithNullAnalyzerID,
            GettextCatalog.GetString("When 'is' keyword is used, which implicitly check null"),
            GettextCatalog.GetString("Redundant comparison with 'null'"),
            DiagnosticAnalyzerCategories.RedundanciesInCode,
            DiagnosticSeverity.Hidden,
            isEnabledByDefault: true,
            helpLinkUri: HelpLink.CreateFor(CSharpDiagnosticIDs.RedundantComparisonWithNullAnalyzerID),
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

        //		class GatherVisitor : GatherVisitorBase<RedundantComparisonWithNullAnalyzer>
        //		{
        //			public GatherVisitor(SemanticModel semanticModel, Action<Diagnostic> addDiagnostic, CancellationToken cancellationToken)
        //				: base (semanticModel, addDiagnostic, cancellationToken)
        //			{
        //			}

        ////			public override void VisitBinaryOperatorExpression(BinaryOperatorExpression binaryOperatorExpression)
        ////			{
        ////				base.VisitBinaryOperatorExpression(binaryOperatorExpression);
        ////				Match m1 = pattern1.Match(binaryOperatorExpression);
        ////				if (m1.Success) {
        ////					AddDiagnosticAnalyzer(new CodeIssue(binaryOperatorExpression,
        ////					         ctx.TranslateString(""),
        ////					         ctx.TranslateString(""), 
        ////					         script => {
        ////					         	var isExpr = m1.Get<AstType>("t").Single().Parent;
        ////					         	script.Replace(binaryOperatorExpression, isExpr);
        ////					         }
        ////					) { IssueMarker = IssueMarker.GrayOut });
        ////					return;
        ////				}
        ////			}
        //		}

        //		private static readonly Pattern pattern1
        //		= new Choice {
        //			//  a is Record && a != null
        //			new BinaryOperatorExpression(
        //				PatternHelper.OptionalParentheses(
        //					new IsExpression {
        //						Expression = new AnyNode("a"),
        //						Type = new AnyNode("t")
        //					}),
        //				BinaryOperatorType.ConditionalAnd,
        //				PatternHelper.CommutativeOperatorWithOptionalParentheses(new Backreference("a"),
        //					BinaryOperatorType.InEquality,
        //					new NullReferenceExpression())
        //			),
        //			//  a != null && a is Record
        //			new BinaryOperatorExpression (
        //				PatternHelper.CommutativeOperatorWithOptionalParentheses(new AnyNode("a"),
        //					BinaryOperatorType.InEquality,
        //					new NullReferenceExpression()),
        //				BinaryOperatorType.ConditionalAnd,
        //				PatternHelper.OptionalParentheses(
        //					new IsExpression {
        //						Expression = new Backreference("a"),
        //						Type = new AnyNode("t")
        //					})
        //			)
        //		};

    }
}