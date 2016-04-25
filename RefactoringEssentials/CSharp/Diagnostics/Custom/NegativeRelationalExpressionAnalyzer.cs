using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace RefactoringEssentials.CSharp.Diagnostics
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    [NotPortedYet]
    public class NegativeRelationalExpressionAnalyzer : DiagnosticAnalyzer
    {
        static readonly DiagnosticDescriptor descriptor = new DiagnosticDescriptor(
            CSharpDiagnosticIDs.NegativeRelationalExpressionAnalyzerID,
            GettextCatalog.GetString("Simplify negative relational expression"),
            GettextCatalog.GetString("Simplify negative relational expression"),
            DiagnosticAnalyzerCategories.PracticesAndImprovements,
            DiagnosticSeverity.Info,
            isEnabledByDefault: true,
            helpLinkUri: HelpLink.CreateFor(CSharpDiagnosticIDs.NegativeRelationalExpressionAnalyzerID)
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

        //		class GatherVisitor : GatherVisitorBase<NegativeRelationalExpressionAnalyzer>
        //		{
        //			public GatherVisitor(SemanticModel semanticModel, Action<Diagnostic> addDiagnostic, CancellationToken cancellationToken)
        //				: base(semanticModel, addDiagnostic, cancellationToken)
        //			{
        //			}

        ////			bool IsFloatingPoint (AstNode node)
        ////			{
        ////				var typeDef = ctx.Resolve (node).Type.GetDefinition ();
        ////				return typeDef != null &&
        ////					(typeDef.KnownTypeCode == KnownTypeCode.Single || typeDef.KnownTypeCode == KnownTypeCode.Double);
        ////			}
        ////
        ////			public override void VisitUnaryOperatorExpression (UnaryOperatorExpression unaryOperatorExpression)
        ////			{
        ////				base.VisitUnaryOperatorExpression (unaryOperatorExpression);
        ////
        ////				if (unaryOperatorExpression.Operator != UnaryOperatorType.Not)
        ////					return;
        ////
        ////				var expr = unaryOperatorExpression.Expression;
        ////				while (expr != null && expr is ParenthesizedExpression)
        ////					expr = ((ParenthesizedExpression)expr).Expression;
        ////
        ////				var binaryOperatorExpr = expr as BinaryOperatorExpression;
        ////				if (binaryOperatorExpr == null)
        ////					return;
        ////				switch (binaryOperatorExpr.Operator) {
        ////					case BinaryOperatorType.BitwiseAnd:
        ////					case BinaryOperatorType.BitwiseOr:
        ////					case BinaryOperatorType.ConditionalAnd:
        ////					case BinaryOperatorType.ConditionalOr:
        ////					case BinaryOperatorType.ExclusiveOr:
        ////						return;
        ////				}
        ////
        ////				var negatedOp = CSharpUtil.NegateRelationalOperator(binaryOperatorExpr.Operator);
        ////				if (negatedOp == BinaryOperatorType.Any)
        ////					return;
        ////
        ////				if (IsFloatingPoint (binaryOperatorExpr.Left) || IsFloatingPoint (binaryOperatorExpr.Right)) {
        ////					if (negatedOp != BinaryOperatorType.Equality && negatedOp != BinaryOperatorType.InEquality)
        ////						return;
        ////				}
        ////
        //			//				AddDiagnosticAnalyzer (new CodeIssue(unaryOperatorExpression, ctx.TranslateString ("Simplify negative relational expression"), ctx.TranslateString ("Simplify negative relational expression"),
        ////					script => script.Replace (unaryOperatorExpression,
        ////						new BinaryOperatorExpression (binaryOperatorExpr.Left.Clone (), negatedOp,
        ////							binaryOperatorExpr.Right.Clone ()))));
        ////			}
        ////			
        ////			public override void VisitOperatorDeclaration(OperatorDeclaration operatorDeclaration)
        ////			{
        ////				if (operatorDeclaration.OperatorType.IsComparisonOperator()) {
        ////					// Ignore operator declaration; within them it's common to define one operator
        ////					// by negating another.
        ////					return;
        ////				}
        ////				base.VisitOperatorDeclaration(operatorDeclaration);
        ////			}
        //		}
    }
}