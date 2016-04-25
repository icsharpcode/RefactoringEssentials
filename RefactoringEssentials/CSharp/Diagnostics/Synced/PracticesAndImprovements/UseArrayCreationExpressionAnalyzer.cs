using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace RefactoringEssentials.CSharp.Diagnostics
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    [NotPortedYet]
    public class UseArrayCreationExpressionAnalyzer : DiagnosticAnalyzer
    {
        static readonly DiagnosticDescriptor descriptor = new DiagnosticDescriptor(
            CSharpDiagnosticIDs.UseArrayCreationExpressionAnalyzerID,
            GettextCatalog.GetString("Use array creation expression"),
            GettextCatalog.GetString("Use array create expression"),
            DiagnosticAnalyzerCategories.PracticesAndImprovements,
            DiagnosticSeverity.Info,
            isEnabledByDefault: true,
            helpLinkUri: HelpLink.CreateFor(CSharpDiagnosticIDs.UseArrayCreationExpressionAnalyzerID)
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

        //		class GatherVisitor : GatherVisitorBase<UseArrayCreationExpressionAnalyzer>
        //		{
        //			public GatherVisitor(SemanticModel semanticModel, Action<Diagnostic> addDiagnostic, CancellationToken cancellationToken)
        //				: base (semanticModel, addDiagnostic, cancellationToken)
        //			{
        //			}


        ////			public override void VisitInvocationExpression(InvocationExpression invocationExpression)
        ////			{
        ////				base.VisitInvocationExpression(invocationExpression);
        ////
        ////				var rr = ctx.Resolve(invocationExpression) as CSharpInvocationResolveResult;
        ////				if (rr == null || rr.IsError)
        ////					return;
        ////
        ////				if (rr.Member.Name != "CreateInstance" ||
        ////				    !rr.Member.DeclaringType.IsKnownType(KnownTypeCode.Array))
        ////					return;
        ////				var firstArg = invocationExpression.Arguments.FirstOrDefault() as TypeOfExpression;
        ////				if (firstArg == null)
        ////					return;
        ////				var argRR = ctx.Resolve(invocationExpression.Arguments.ElementAt(1));
        ////				if (!argRR.Type.IsKnownType(KnownTypeCode.Int32))
        ////					return;
        ////
        ////				AddDiagnosticAnalyzer(new CodeIssue(
        ////					invocationExpression,
        ////					ctx.TranslateString(""), 
        ////					ctx.TranslateString(""), 
        ////					script => {
        ////						var ac = new ArrayCreateExpression {
        ////							Type = firstArg.Type.Clone()
        ////						};
        ////						foreach (var arg in invocationExpression.Arguments.Skip(1))
        ////							ac.Arguments.Add(arg.Clone()) ;
        ////						script.Replace(invocationExpression, ac);
        ////					}
        ////				));
        ////			}
        //		}
    }
}