using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace RefactoringEssentials.CSharp.Diagnostics
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    [NotPortedYet]
    public class UseMethodIsInstanceOfTypeAnalyzer : DiagnosticAnalyzer
    {
        static readonly DiagnosticDescriptor descriptor = new DiagnosticDescriptor(
            CSharpDiagnosticIDs.UseMethodIsInstanceOfTypeAnalyzerID,
            GettextCatalog.GetString("Use method IsInstanceOfType"),
            GettextCatalog.GetString("Use method IsInstanceOfType (...)"),
            DiagnosticAnalyzerCategories.PracticesAndImprovements,
            DiagnosticSeverity.Info,
            isEnabledByDefault: true,
            helpLinkUri: HelpLink.CreateFor(CSharpDiagnosticIDs.UseMethodIsInstanceOfTypeAnalyzerID)
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

        //		class GatherVisitor : GatherVisitorBase<UseMethodIsInstanceOfTypeAnalyzer>
        //		{
        //			public GatherVisitor(SemanticModel semanticModel, Action<Diagnostic> addDiagnostic, CancellationToken cancellationToken)
        //				: base (semanticModel, addDiagnostic, cancellationToken)
        //			{
        //			}

        ////			static readonly Expression pattern = new InvocationExpression(
        ////				new MemberReferenceExpression(new AnyNode("target"), "IsAssignableFrom"),
        ////				new InvocationExpression(
        ////					new MemberReferenceExpression(new AnyNode("object"), "GetType")
        ////				)
        ////			);
        ////
        ////			public override void VisitInvocationExpression(InvocationExpression invocationExpression)
        ////			{
        ////				base.VisitInvocationExpression(invocationExpression);
        ////				var match = pattern.Match(invocationExpression);
        ////				if (match.Success) {
        ////					AddDiagnosticAnalyzer(new CodeIssue(
        ////						invocationExpression,
        ////						ctx.TranslateString(""),
        ////						ctx.TranslateString(""),
        ////						s => {
        ////							s.Replace(invocationExpression, new InvocationExpression(
        ////								new MemberReferenceExpression(match.Get<Expression>("target").Single().Clone(), "IsInstanceOfType"),
        ////								match.Get<Expression>("object").Single().Clone()
        ////							));
        ////						}
        ////					));
        ////				}
        ////			}
        //		}
    }
}