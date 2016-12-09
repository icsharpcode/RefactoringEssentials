using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace RefactoringEssentials.CSharp.Diagnostics
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    [NotPortedYet]
    public class RedundantBoolCompareAnalyzer : DiagnosticAnalyzer
    {
        static readonly DiagnosticDescriptor descriptor = new DiagnosticDescriptor(
            CSharpDiagnosticIDs.RedundantBoolCompareAnalyzerID,
            GettextCatalog.GetString("Comparison of a boolean value with 'true' or 'false' constant"),
            GettextCatalog.GetString("Comparison with '{0}' is redundant"),
            DiagnosticAnalyzerCategories.RedundanciesInCode,
            DiagnosticSeverity.Hidden,
            isEnabledByDefault: true,
            helpLinkUri: HelpLink.CreateFor(CSharpDiagnosticIDs.RedundantBoolCompareAnalyzerID),
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

        //		class GatherVisitor : GatherVisitorBase<RedundantBoolCompareAnalyzer>
        //		{
        ////			// note:this action should only check <bool> == true or <bool> != null - it needs excectly 
        ////			//      mimic the RedundantBoolCompare behavior otherwise it's no 1:1 mapping
        ////			static readonly Pattern pattern = new Choice {
        ////				PatternHelper.CommutativeOperatorWithOptionalParentheses(
        ////					new NamedNode ("const", new Choice { new PrimitiveExpression(true), new PrimitiveExpression(false) }),
        ////					BinaryOperatorType.Equality, new AnyNode("expr")),
        ////				PatternHelper.CommutativeOperatorWithOptionalParentheses(
        ////					new NamedNode ("const", new Choice { new PrimitiveExpression(true), new PrimitiveExpression(false) }),
        ////					BinaryOperatorType.InEquality, new AnyNode("expr")),
        ////			};
        ////
        ////			static readonly InsertParenthesesVisitor insertParenthesesVisitor = new InsertParenthesesVisitor ();

        //			public GatherVisitor(SemanticModel semanticModel, Action<Diagnostic> addDiagnostic, CancellationToken cancellationToken)
        //				: base (semanticModel, addDiagnostic, cancellationToken)
        //			{
        //			}
        ////
        ////			public override void VisitBinaryOperatorExpression (BinaryOperatorExpression binaryOperatorExpression)
        ////			{
        ////				base.VisitBinaryOperatorExpression (binaryOperatorExpression);
        ////
        ////				var match = pattern.Match (binaryOperatorExpression);
        ////				if (!match.Success)
        ////					return;
        ////				var expr = match.Get<Expression> ("expr").First ();
        ////				// check if expr is of boolean type
        ////				var exprType = ctx.Resolve (expr).Type.GetDefinition ();
        ////				if (exprType == null || exprType.KnownTypeCode != KnownTypeCode.Boolean)
        ////					return;
        ////
        ////				var boolExpr = match.Get<PrimitiveExpression>("const").First();
        ////				var boolConstant = (bool)boolExpr.Value;
        ////
        ////				TextLocation start, end;
        ////				if (boolExpr == binaryOperatorExpression.Left) {
        ////					start = binaryOperatorExpression.StartLocation;
        ////					end = binaryOperatorExpression.OperatorToken.EndLocation;
        ////				} else {
        ////					start = binaryOperatorExpression.OperatorToken.StartLocation;
        ////					end = binaryOperatorExpression.EndLocation;
        ////				}
        ////
        ////				AddDiagnosticAnalyzer (new CodeIssue(
        ////					start, end, 
        //			//					boolConstant ? ctx.TranslateString ("Comparison with 'true' is redundant") : ctx.TranslateString ("Comparison with 'false' is redundant"),
        //			//					ctx.TranslateString ("Remove redundant comparison"), 
        ////					script => {
        ////						if ((binaryOperatorExpression.Operator == BinaryOperatorType.InEquality && boolConstant) ||
        ////							(binaryOperatorExpression.Operator == BinaryOperatorType.Equality && !boolConstant)) {
        ////							expr = new UnaryOperatorExpression (UnaryOperatorType.Not, expr.Clone());
        ////							expr.AcceptVisitor (insertParenthesesVisitor);
        ////						}
        ////						script.Replace (binaryOperatorExpression, expr);
        ////					}
        ////				) { IssueMarker = IssueMarker.GrayOut });
        ////			}
        //		}
    }
}
