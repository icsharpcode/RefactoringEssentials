using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace RefactoringEssentials.CSharp.Diagnostics
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    [NotPortedYet]
    public class EqualExpressionComparisonAnalyzer : DiagnosticAnalyzer
    {
        static readonly DiagnosticDescriptor descriptor = new DiagnosticDescriptor(
            CSharpDiagnosticIDs.EqualExpressionComparisonAnalyzerID,
            GettextCatalog.GetString("Comparing equal expression for equality is usually useless"),
            GettextCatalog.GetString("Replace with '{0}'"),
            DiagnosticAnalyzerCategories.CodeQualityIssues,
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            helpLinkUri: HelpLink.CreateFor(CSharpDiagnosticIDs.EqualExpressionComparisonAnalyzerID)
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

        //		class GatherVisitor : GatherVisitorBase<EqualExpressionComparisonAnalyzer>
        //		{
        //			public GatherVisitor(SemanticModel semanticModel, Action<Diagnostic> addDiagnostic, CancellationToken cancellationToken)
        //				: base (semanticModel, addDiagnostic, cancellationToken)
        //			{
        //			}

        ////			void AddDiagnosticAnalyzer(AstNode nodeToReplace, AstNode highlightNode, bool replaceWithTrue)
        ////			{
        ////				AddDiagnosticAnalyzer(new CodeIssue(
        ////					highlightNode, 
        ////					ctx.TranslateString(""), 
        ////					replaceWithTrue ? ctx.TranslateString() : ctx.TranslateString(), 
        ////					script =>  {
        ////						script.Replace(nodeToReplace, new PrimitiveExpression(replaceWithTrue));
        ////					}
        ////				));
        ////			}
        ////
        ////
        ////			readonly BinaryOperatorExpression pattern = 
        ////				new BinaryOperatorExpression(
        ////					PatternHelper.OptionalParentheses(new AnyNode("expression")), 
        ////					BinaryOperatorType.Any, 
        ////					PatternHelper.OptionalParentheses(new Backreference("expression"))
        ////				);
        ////
        ////			public override void VisitBinaryOperatorExpression(BinaryOperatorExpression binaryOperatorExpression)
        ////			{
        ////				base.VisitBinaryOperatorExpression(binaryOperatorExpression);
        ////
        ////				if (binaryOperatorExpression.Operator != BinaryOperatorType.Equality &&
        ////				    binaryOperatorExpression.Operator != BinaryOperatorType.InEquality &&
        ////				    binaryOperatorExpression.Operator != BinaryOperatorType.GreaterThan &&
        ////				    binaryOperatorExpression.Operator != BinaryOperatorType.GreaterThanOrEqual &&
        ////				    binaryOperatorExpression.Operator != BinaryOperatorType.LessThan &&
        ////				    binaryOperatorExpression.Operator != BinaryOperatorType.LessThanOrEqual) {
        ////					return;
        ////				}
        ////
        ////				var match = pattern.Match(binaryOperatorExpression);
        ////				if (match.Success) {
        ////					AddDiagnosticAnalyzer(binaryOperatorExpression, binaryOperatorExpression.OperatorToken, binaryOperatorExpression.Operator == BinaryOperatorType.Equality);
        ////					return;
        ////				}
        ////			}
        ////
        ////			public override void VisitInvocationExpression(InvocationExpression invocationExpression)
        ////			{
        ////				base.VisitInvocationExpression(invocationExpression);
        ////				var rr = ctx.Resolve(invocationExpression) as InvocationResolveResult;
        ////				if (rr == null || rr.Member.Name != "Equals" || !rr.Member.ReturnType.IsKnownType(KnownTypeCode.Boolean))
        ////					return;
        ////
        ////				if (rr.Member.IsStatic) {
        ////					if (rr.Member.Parameters.Count != 2)
        ////						return;
        ////					if (CSharpUtil.AreConditionsEqual(invocationExpression.Arguments.FirstOrDefault(), invocationExpression.Arguments.Last())) {
        ////						if ((invocationExpression.Parent is UnaryOperatorExpression) && ((UnaryOperatorExpression)invocationExpression.Parent).Operator == UnaryOperatorType.Not) {
        ////							AddDiagnosticAnalyzer(invocationExpression.Parent, invocationExpression.Parent, false);
        ////						} else {
        ////							AddDiagnosticAnalyzer(invocationExpression, invocationExpression, true);
        ////						}
        ////					}
        ////				} else {
        ////					if (rr.Member.Parameters.Count != 1)
        ////						return;
        ////					var target = invocationExpression.Target as MemberReferenceExpression;
        ////					if (target == null)
        ////						return;
        ////					if (CSharpUtil.AreConditionsEqual(invocationExpression.Arguments.FirstOrDefault(), target.Target)) {
        ////						if ((invocationExpression.Parent is UnaryOperatorExpression) && ((UnaryOperatorExpression)invocationExpression.Parent).Operator == UnaryOperatorType.Not) {
        ////							AddDiagnosticAnalyzer(invocationExpression.Parent, invocationExpression.Parent, false);
        ////						} else {
        ////							AddDiagnosticAnalyzer(invocationExpression, invocationExpression, true);
        ////						}
        ////					}
        ////				}
        ////			}
        //		}
    }
}