using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace RefactoringEssentials.CSharp.Diagnostics
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    [NotPortedYet]
    public class RedundantLambdaParameterTypeAnalyzer : DiagnosticAnalyzer
    {
        static readonly DiagnosticDescriptor descriptor = new DiagnosticDescriptor(
            CSharpDiagnosticIDs.RedundantLambdaParameterTypeAnalyzerID,
            GettextCatalog.GetString("Explicit type specification can be removed as it can be implicitly inferred"),
            GettextCatalog.GetString("Redundant lambda explicit type specification"),
            DiagnosticAnalyzerCategories.RedundanciesInCode,
            DiagnosticSeverity.Hidden,
            isEnabledByDefault: true,
            helpLinkUri: HelpLink.CreateFor(CSharpDiagnosticIDs.RedundantLambdaParameterTypeAnalyzerID),
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

        //		class GatherVisitor : GatherVisitorBase<RedundantLambdaParameterTypeAnalyzer>
        //		{
        //			public GatherVisitor(SemanticModel semanticModel, Action<Diagnostic> addDiagnostic, CancellationToken cancellationToken)
        //				: base (semanticModel, addDiagnostic, cancellationToken)
        //			{
        //			}

        ////			public override void VisitLambdaExpression(LambdaExpression lambdaExpression)
        ////			{
        ////				base.VisitLambdaExpression(lambdaExpression);
        ////
        ////				var arguments = lambdaExpression.Parameters.ToList();
        ////				if (arguments.Any(f => f.Type.IsNull))
        ////					return;
        ////				if (!LambdaTypeCanBeInferred(ctx, lambdaExpression, arguments))
        ////					return;
        ////
        ////				foreach (var argument in arguments) {
        ////					AddDiagnosticAnalyzer(new CodeIssue(
        ////						argument.Type,
        ////						ctx.TranslateString(""), 
        ////						ctx.TranslateString(""),
        ////						script => {
        ////							if (arguments.Count == 1) {
        ////								if (argument.NextSibling.ToString().Equals(")") && argument.PrevSibling.ToString().Equals("(")) {
        ////									script.Remove(argument.NextSibling);
        ////									script.Remove(argument.PrevSibling);
        ////								}
        ////							}
        ////							foreach (var arg in arguments)
        ////								script.Replace(arg, new ParameterDeclaration(arg.Name));
        ////						}) { IssueMarker = IssueMarker.GrayOut });
        ////				}
        ////			}
        //		}

        //		public static bool LambdaTypeCanBeInferred(BaseSemanticModel ctx, Expression expression, List<ParameterDeclaration> parameters)
        //		{
        //			var validTypes = TypeGuessing.GetValidTypes(ctx.Resolver, expression).ToList();
        //			foreach (var type in validTypes) {
        //				if (type.Kind != TypeKind.Delegate)
        //					continue;
        //				var invokeMethod = type.GetDelegateInvokeMethod();
        //				if (invokeMethod == null || invokeMethod.Parameters.Count != parameters.Count)
        //					continue;
        //				for (int p = 0; p < invokeMethod.Parameters.Count; p++) {
        //					var resolvedArgument = ctx.Resolve(parameters[p].Type);
        //					if (!invokeMethod.Parameters [p].Type.Equals(resolvedArgument.Type))
        //						return false;
        //				}
        //			}
        //			return true;
        //		}
    }
}