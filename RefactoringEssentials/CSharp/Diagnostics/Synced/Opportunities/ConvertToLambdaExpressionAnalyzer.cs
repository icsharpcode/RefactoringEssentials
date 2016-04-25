using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace RefactoringEssentials.CSharp.Diagnostics
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    [NotPortedYet]
    public class ConvertToLambdaExpressionAnalyzer : DiagnosticAnalyzer
    {
        static readonly DiagnosticDescriptor descriptor = new DiagnosticDescriptor(
            CSharpDiagnosticIDs.ConvertToLambdaExpressionAnalyzerID,
            GettextCatalog.GetString("Convert to lambda with expression"),
            GettextCatalog.GetString("Can be converted to expression"),
            DiagnosticAnalyzerCategories.Opportunities,
            DiagnosticSeverity.Info,
            isEnabledByDefault: true,
            helpLinkUri: HelpLink.CreateFor(CSharpDiagnosticIDs.ConvertToLambdaExpressionAnalyzerID)
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

        //		class GatherVisitor : GatherVisitorBase<ConvertToLambdaExpressionAnalyzer>
        //		{
        //			public GatherVisitor(SemanticModel semanticModel, Action<Diagnostic> addDiagnostic, CancellationToken cancellationToken)
        //				: base (semanticModel, addDiagnostic, cancellationToken)
        //			{
        //			}
        ////
        ////			public override void VisitLambdaExpression(LambdaExpression lambdaExpression)
        ////			{
        ////				base.VisitLambdaExpression(lambdaExpression);
        ////				BlockStatement block;
        ////				Expression expr;
        ////				if (!ConvertLambdaBodyStatementToExpressionAction.TryGetConvertableExpression(lambdaExpression.Body, out block, out expr))
        ////					return;
        ////				var node = block.Statements.FirstOrDefault() ?? block;
        ////				var expressionStatement = node as ExpressionStatement;
        ////				if (expressionStatement != null) {
        ////					if (expressionStatement.Expression is AssignmentExpression)
        ////						return;
        ////				}
        ////				var returnTypes = new List<IType>();
        ////				foreach (var type in TypeGuessing.GetValidTypes(ctx.Resolver, lambdaExpression)) {
        ////					if (type.Kind != TypeKind.Delegate)
        ////						continue;
        ////					var invoke = type.GetDelegateInvokeMethod();
        ////					if (!returnTypes.Contains(invoke.ReturnType))
        ////						returnTypes.Add(invoke.ReturnType);
        ////				}
        ////				if (returnTypes.Count > 1)
        ////					return;
        ////
        ////				AddDiagnosticAnalyzer(new CodeIssue(
        ////					node,
        ////					ctx.TranslateString(""),
        ////					ConvertLambdaBodyStatementToExpressionAction.CreateAction(ctx, node, block, expr)
        ////				));
        ////			}
        ////
        ////			public override void VisitAnonymousMethodExpression(AnonymousMethodExpression anonymousMethodExpression)
        ////			{
        ////				base.VisitAnonymousMethodExpression(anonymousMethodExpression);
        ////				if (!anonymousMethodExpression.HasParameterList)
        ////					return;
        ////				BlockStatement block;
        ////				Expression expr;
        ////				if (!ConvertLambdaBodyStatementToExpressionAction.TryGetConvertableExpression(anonymousMethodExpression.Body, out block, out expr))
        ////					return;
        ////				var node = block.Statements.FirstOrDefault() ?? block;
        ////				var returnTypes = new List<IType>();
        ////				foreach (var type in TypeGuessing.GetValidTypes(ctx.Resolver, anonymousMethodExpression)) {
        ////					if (type.Kind != TypeKind.Delegate)
        ////						continue;
        ////					var invoke = type.GetDelegateInvokeMethod();
        ////					if (!returnTypes.Contains(invoke.ReturnType))
        ////						returnTypes.Add(invoke.ReturnType);
        ////				}
        ////				if (returnTypes.Count > 1)
        ////					return;
        ////
        ////				AddDiagnosticAnalyzer(new CodeIssue(
        ////					node,
        ////					ctx.TranslateString(""),
        ////					ctx.TranslateString("Convert to lambda expression"),
        ////					script => {
        ////						var lambdaExpression = new LambdaExpression();
        ////						foreach (var parameter in anonymousMethodExpression.Parameters)
        ////							lambdaExpression.Parameters.Add(parameter.Clone());
        ////						lambdaExpression.Body = expr.Clone();
        ////						script.Replace(anonymousMethodExpression, lambdaExpression);
        ////					}
        ////				));
        ////			}
        //		}
    }


}