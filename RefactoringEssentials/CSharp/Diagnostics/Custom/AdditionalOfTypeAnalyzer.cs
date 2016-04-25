using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace RefactoringEssentials.CSharp.Diagnostics
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    [NotPortedYet]
    public class AdditionalOfTypeAnalyzer : DiagnosticAnalyzer
    {
        //		static readonly AstNode whereSimpleCase =
        //			new InvocationExpression(
        //				new MemberReferenceExpression(new AnyNode("target"), "Where"),
        //				new NamedNode("lambda", 
        //					new LambdaExpression {
        //						Parameters = { PatternHelper.NamedParameter("param1", PatternHelper.AnyType("paramType", true), Pattern.AnyString) },
        //						Body = PatternHelper.OptionalParentheses(
        //								new IsExpression(PatternHelper.OptionalParentheses(new NamedNode("expr1", new IdentifierExpression(Pattern.AnyString))), new AnyNode("type"))
        //						)
        //					}
        //				)
        //			);

        static readonly DiagnosticDescriptor descriptor = new DiagnosticDescriptor(
            CSharpDiagnosticIDs.AdditionalOfTypeAnalyzerID,
            GettextCatalog.GetString("Replace with call to OfType<T> (extended cases)"),
            GettextCatalog.GetString("Replace with 'OfType<T>'"),
            DiagnosticAnalyzerCategories.PracticesAndImprovements,
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            helpLinkUri: HelpLink.CreateFor(CSharpDiagnosticIDs.AdditionalOfTypeAnalyzerID)
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

        //		class GatherVisitor : GatherVisitorBase<AdditionalOfTypeAnalyzer>
        //		{
        //			public GatherVisitor(SemanticModel semanticModel, Action<Diagnostic> addDiagnostic, CancellationToken cancellationToken)
        //				: base(semanticModel, addDiagnostic, cancellationToken)
        //			{
        //			}

        ////			public override void VisitInvocationExpression (InvocationExpression anyInvoke)
        ////			{
        ////				var match = ReplaceWithOfTypeAnalyzer.selectNotNullPattern.Match (anyInvoke);
        ////				if (match.Success)
        ////					return;
        ////
        ////				match = ReplaceWithOfTypeAnalyzer.wherePatternCase1.Match (anyInvoke);
        ////				if (match.Success)
        ////					return;
        ////
        ////				match = ReplaceWithOfTypeAnalyzer.wherePatternCase2.Match (anyInvoke); 
        ////				if (match.Success)
        ////					return;
        ////
        ////				// Warning: The simple case is not 100% equal in semantic, but it's one common code smell
        ////				match = whereSimpleCase.Match (anyInvoke); 
        ////				if (!match.Success)
        ////					return;
        ////				var lambda = match.Get<LambdaExpression>("lambda").Single();
        ////				var expr = match.Get<IdentifierExpression>("expr1").Single();
        ////				if (lambda.Parameters.Count != 1)
        ////					return;
        ////				if (expr.Identifier != lambda.Parameters.Single().Name)
        ////					return;
        ////				AddDiagnosticAnalyzer (new CodeIssue(
        ////					anyInvoke,
        //			//					ctx.TranslateString("Replace with OfType<T>"),
        ////					ctx.TranslateString("Replace with call to OfType<T>"),
        ////					script => {
        ////						var target = match.Get<Expression>("target").Single().Clone ();
        ////						var type = match.Get<AstType>("type").Single().Clone();
        ////						script.Replace(anyInvoke, new InvocationExpression(new MemberReferenceExpression(target, "OfType", type)));
        ////					}
        ////				));
        ////			}
        //		}
    }

}