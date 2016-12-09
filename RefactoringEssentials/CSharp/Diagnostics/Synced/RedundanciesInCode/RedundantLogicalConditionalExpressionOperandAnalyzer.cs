using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace RefactoringEssentials.CSharp.Diagnostics
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    [NotPortedYet]
    public class RedundantLogicalConditionalExpressionOperandAnalyzer : DiagnosticAnalyzer
    {
        static readonly DiagnosticDescriptor descriptor = new DiagnosticDescriptor(
            CSharpDiagnosticIDs.RedundantLogicalConditionalExpressionOperandAnalyzerID,
            GettextCatalog.GetString("Redundant operand in logical conditional expression"),
            GettextCatalog.GetString("Redundant operand in logical conditional expression"),
            DiagnosticAnalyzerCategories.RedundanciesInCode,
            DiagnosticSeverity.Hidden,
            isEnabledByDefault: true,
            helpLinkUri: HelpLink.CreateFor(CSharpDiagnosticIDs.RedundantLogicalConditionalExpressionOperandAnalyzerID),
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

        //		class GatherVisitor : GatherVisitorBase<RedundantLogicalConditionalExpressionOperandAnalyzer>
        //		{
        //			public GatherVisitor(SemanticModel semanticModel, Action<Diagnostic> addDiagnostic, CancellationToken cancellationToken)
        //				: base (semanticModel, addDiagnostic, cancellationToken)
        //			{
        //			}

        ////			static readonly AstNode pattern = new Choice {
        ////				PatternHelper.CommutativeOperator(new NamedNode ("redundant", PatternHelper.OptionalParentheses(new PrimitiveExpression(true))), BinaryOperatorType.ConditionalOr, new AnyNode("expr")),	
        ////				PatternHelper.CommutativeOperator(new NamedNode ("redundant", PatternHelper.OptionalParentheses(new PrimitiveExpression(false))), BinaryOperatorType.ConditionalAnd, new AnyNode("expr"))
        ////			};
        ////
        ////			public override void VisitBinaryOperatorExpression(BinaryOperatorExpression binaryOperatorExpression)
        ////			{
        ////				base.VisitBinaryOperatorExpression(binaryOperatorExpression);
        ////				var match = pattern.Match(binaryOperatorExpression);
        ////				if (!match.Success)
        ////					return;
        ////				var redundant = match.Get<Expression>("redundant").Single();
        ////				var expr = match.Get<Expression>("expr").Single();
        ////				AddDiagnosticAnalyzer(new CodeIssue(
        ////					redundant,
        ////					ctx.TranslateString(""),
        ////					ctx.TranslateString(""),
        ////					script => script.Replace(binaryOperatorExpression, expr.Clone())
        ////				) { IssueMarker = IssueMarker.GrayOut });
        ////			}
        //		}
    }


}