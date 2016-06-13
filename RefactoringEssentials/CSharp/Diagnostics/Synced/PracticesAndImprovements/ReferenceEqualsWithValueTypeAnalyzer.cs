using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace RefactoringEssentials.CSharp.Diagnostics
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    [NotPortedYet]
    public class ReferenceEqualsWithValueTypeAnalyzer : DiagnosticAnalyzer
    {
        static readonly DiagnosticDescriptor descriptor = new DiagnosticDescriptor(
            CSharpDiagnosticIDs.ReferenceEqualsWithValueTypeAnalyzerID,
            GettextCatalog.GetString("Check for reference equality instead"),
            GettextCatalog.GetString("'Object.ReferenceEquals' is always false because it is called with value type"),
            DiagnosticAnalyzerCategories.PracticesAndImprovements,
            DiagnosticSeverity.Info,
            isEnabledByDefault: true,
            helpLinkUri: HelpLink.CreateFor(CSharpDiagnosticIDs.ReferenceEqualsWithValueTypeAnalyzerID)
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

        //		class GatherVisitor : GatherVisitorBase<ReferenceEqualsWithValueTypeAnalyzer>
        //		{
        //			public GatherVisitor(SemanticModel semanticModel, Action<Diagnostic> addDiagnostic, CancellationToken cancellationToken)
        //				: base (semanticModel, addDiagnostic, cancellationToken)
        //			{
        //			}

        ////			public override void VisitInvocationExpression(InvocationExpression invocationExpression)
        ////			{
        ////				base.VisitInvocationExpression(invocationExpression);
        ////
        ////				// Quickly determine if this invocation is eligible to speed up the inspector
        ////				var nameToken = invocationExpression.Target.GetChildByRole(Roles.Identifier);
        ////				if (nameToken.Name != "ReferenceEquals")
        ////					return;
        ////
        ////				var resolveResult = ctx.Resolve(invocationExpression) as InvocationResolveResult;
        ////				if (resolveResult == null ||
        ////				    resolveResult.Member.DeclaringTypeDefinition == null ||
        ////				    resolveResult.Member.DeclaringTypeDefinition.KnownTypeCode != KnownTypeCode.Object ||
        ////				    resolveResult.Member.Name != "ReferenceEquals" ||
        ////				    invocationExpression.Arguments.All(arg => ctx.Resolve(arg).Type.IsReferenceType ?? true))
        ////					return;
        ////
        ////				var action1 = new CodeAction(ctx.TranslateString("Replace expression with 'false'"),
        ////					              script => script.Replace(invocationExpression, new PrimitiveExpression(false)), invocationExpression);
        ////
        ////				var action2 = new CodeAction(ctx.TranslateString("Use Equals()"),
        ////					              script => script.Replace(invocationExpression.Target,
        ////						              new PrimitiveType("object").Member("Equals")), invocationExpression);
        ////
        ////				AddDiagnosticAnalyzer(new CodeIssue(invocationExpression,
        ////					ctx.TranslateString("'Object.ReferenceEquals' is always false because it is called with value type"),
        ////					new [] { action1, action2 }));
        ////			}
        //		}
    }
}