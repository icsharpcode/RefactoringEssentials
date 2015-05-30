using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace RefactoringEssentials.CSharp.Diagnostics
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    [NotPortedYet]
    public class ConstantNullCoalescingConditionAnalyzer : DiagnosticAnalyzer
    {
        static readonly DiagnosticDescriptor descriptor = new DiagnosticDescriptor(
            NRefactoryDiagnosticIDs.ConstantNullCoalescingConditionAnalyzerID,
            GettextCatalog.GetString("Finds redundant null coalescing expressions such as expr ?? expr"),
            "{0}",
            DiagnosticAnalyzerCategories.PracticesAndImprovements,
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            helpLinkUri: HelpLink.CreateFor(NRefactoryDiagnosticIDs.ConstantNullCoalescingConditionAnalyzerID)
        );

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(descriptor);


        //static readonly DiagnosticDescriptor descriptor = new DiagnosticDescriptor (DiagnosticId, "Redundant ??. Right side is always null.", "Remove redundant right side", Category, DiagnosticSeverity.Warning, true, "'??' condition is known to be null or not null");
        //static readonly DiagnosticDescriptor descriptor = new DiagnosticDescriptor (DiagnosticId, "Redundant ??. Left side is always null.", "Remove redundant left side", Category, DiagnosticSeverity.Warning, true, "'??' condition is known to be null or not null");
        //static readonly DiagnosticDescriptor descriptor = new DiagnosticDescriptor (DiagnosticId, "Redundant ??. Left side is never null.", "Remove redundant right side", Category, DiagnosticSeverity.Warning, true, "'??' condition is known to be null or not null");


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

        //		class GatherVisitor : GatherVisitorBase<ConstantNullCoalescingConditionAnalyzer>
        //		{
        //			public GatherVisitor(SemanticModel semanticModel, Action<Diagnostic> addDiagnostic, CancellationToken cancellationToken)
        //				: base (semanticModel, addDiagnostic, cancellationToken)
        //			{
        //			}

        ////			Dictionary<AstNode, NullValueAnalysis> cachedNullAnalysis = new Dictionary<AstNode, NullValueAnalysis>();
        ////
        ////			public override void VisitBinaryOperatorExpression(BinaryOperatorExpression binaryOperatorExpression)
        ////			{
        ////				base.VisitBinaryOperatorExpression(binaryOperatorExpression);
        ////
        ////				if (binaryOperatorExpression.Operator != BinaryOperatorType.NullCoalescing) {
        ////					//The issue is not applicable
        ////					return;
        ////				}
        ////
        ////				var parentFunction = GetParentFunctionNode(binaryOperatorExpression);
        ////				var analysis = GetAnalysis(parentFunction);
        ////
        ////				NullValueStatus leftStatus = analysis.GetExpressionResult(binaryOperatorExpression.Left);
        ////				if (leftStatus == NullValueStatus.DefinitelyNotNull) {
        ////					AddDiagnosticAnalyzer(new CodeIssue(binaryOperatorExpression.OperatorToken.StartLocation,
        ////					         binaryOperatorExpression.Right.EndLocation,
        ////					         ctx.TranslateString(""),
        ////					         ctx.TranslateString(""),
        ////					         script => {
        ////
        ////						script.Replace(binaryOperatorExpression, binaryOperatorExpression.Left.Clone());
        ////
        ////						}) { IssueMarker = IssueMarker.GrayOut });
        ////					return;
        ////				}
        ////				if (leftStatus == NullValueStatus.DefinitelyNull) {
        ////					AddDiagnosticAnalyzer(new CodeIssue(binaryOperatorExpression.Left.StartLocation,
        ////					         binaryOperatorExpression.OperatorToken.EndLocation,
        ////					         ctx.TranslateString(""),
        ////					         ctx.TranslateString(""),
        ////					         script => {
        ////
        ////						script.Replace(binaryOperatorExpression, binaryOperatorExpression.Right.Clone());
        ////
        ////						}));
        ////					return;
        ////				}
        ////				NullValueStatus rightStatus = analysis.GetExpressionResult(binaryOperatorExpression.Right);
        ////				if (rightStatus == NullValueStatus.DefinitelyNull) {
        ////					AddDiagnosticAnalyzer(new CodeIssue(binaryOperatorExpression.OperatorToken.StartLocation,
        ////					         binaryOperatorExpression.Right.EndLocation,
        ////					         ctx.TranslateString(""),
        ////					         ctx.TranslateString(""),
        ////					         script => {
        ////
        ////						script.Replace(binaryOperatorExpression, binaryOperatorExpression.Left.Clone());
        ////
        ////						}));
        ////					return;
        ////				}
        //			}

        //			NullValueAnalysis GetAnalysis(AstNode parentFunction)
        //			{
        //				NullValueAnalysis analysis;
        //				if (cachedNullAnalysis.TryGetValue(parentFunction, out analysis)) {
        //					return analysis;
        //				}
        //
        //				analysis = new NullValueAnalysis(ctx, parentFunction.GetChildByRole(Roles.Body), parentFunction.GetChildrenByRole(Roles.Parameter), ctx.CancellationToken);
        //				analysis.IsParametersAreUninitialized = true;
        //				analysis.Analyze();
        //				cachedNullAnalysis [parentFunction] = analysis;
        //				return analysis;
        //			}
        //		}
        //
        //		public static AstNode GetParentFunctionNode(AstNode node)
        //		{
        //			do {
        //				node = node.Parent;
        //			} while (node != null && !IsFunctionNode(node));
        //
        //			return node;
        //		}
        //
        //		static bool IsFunctionNode(AstNode node)
        //		{
        //			return node is EntityDeclaration ||
        //				node is LambdaExpression ||
        //					node is AnonymousMethodExpression;
        //		}
    }


}