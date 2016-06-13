using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace RefactoringEssentials.CSharp.Diagnostics
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    [NotPortedYet]
    public class UseIsOperatorAnalyzer : DiagnosticAnalyzer
    {
        static readonly DiagnosticDescriptor descriptor = new DiagnosticDescriptor(
            CSharpDiagnosticIDs.UseIsOperatorAnalyzerID,
            GettextCatalog.GetString("'is' operator can be used"),
            GettextCatalog.GetString("Use 'is' operator"),
            DiagnosticAnalyzerCategories.PracticesAndImprovements,
            DiagnosticSeverity.Info,
            isEnabledByDefault: true,
            helpLinkUri: HelpLink.CreateFor(CSharpDiagnosticIDs.UseIsOperatorAnalyzerID)
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

        //		class GatherVisitor : GatherVisitorBase<UseIsOperatorAnalyzer>
        //		{
        //			public GatherVisitor(SemanticModel semanticModel, Action<Diagnostic> addDiagnostic, CancellationToken cancellationToken)
        //				: base (semanticModel, addDiagnostic, cancellationToken)
        //			{
        //			}

        ////			static readonly Expression pattern1 = new InvocationExpression(
        ////				new MemberReferenceExpression(new TypeOfExpression(new AnyNode("Type")), "IsAssignableFrom"),
        ////				new InvocationExpression(
        ////				new MemberReferenceExpression(new AnyNode("object"), "GetType")
        ////			)
        ////			);
        ////			static readonly Expression pattern2 = new InvocationExpression(
        ////				new MemberReferenceExpression(new TypeOfExpression(new AnyNode("Type")), "IsInstanceOfType"),
        ////				new AnyNode("object")
        ////			);
        ////
        ////
        ////
        ////			void AddDiagnosticAnalyzer(AstNode invocationExpression, Match match, bool negate = false)
        ////			{
        ////				AddDiagnosticAnalyzer(new CodeIssue(
        ////					invocationExpression,
        ////					ctx.TranslateString(""),
        ////					ctx.TranslateString(""), 
        ////					s => {
        ////						Expression expression = new IsExpression(CSharpUtil.AddParensForUnaryExpressionIfRequired(match.Get<Expression>("object").Single().Clone()), match.Get<AstType>("Type").Single().Clone());
        ////						if (negate)
        ////							expression = new UnaryOperatorExpression (UnaryOperatorType.Not, new ParenthesizedExpression(expression));
        ////						s.Replace(invocationExpression, expression);
        ////					}
        ////				));
        ////			}
        ////
        ////			public override void VisitInvocationExpression(InvocationExpression invocationExpression)
        ////			{
        ////				base.VisitInvocationExpression(invocationExpression);
        ////				var match = pattern1.Match(invocationExpression);
        ////				if (!match.Success)
        ////					match = pattern2.Match(invocationExpression);
        ////				if (match.Success) {
        ////					AddDiagnosticAnalyzer(invocationExpression, match);
        ////				}
        ////			}

        //			/* Unfortunately not quite the same :/
        //			static readonly AstNode equalityComparePattern =
        //				PatternHelper.CommutativeOperator(
        //					PatternHelper.OptionalParentheses(new TypeOfExpression(new AnyNode("Type"))),
        //					BinaryOperatorType.Equality,
        //					PatternHelper.OptionalParentheses(new InvocationExpression(
        //						new MemberReferenceExpression(new AnyNode("object"), "GetType")
        //					))
        //				);
        //			static readonly AstNode inEqualityComparePattern =
        //				PatternHelper.CommutativeOperator(
        //					PatternHelper.OptionalParentheses(new TypeOfExpression(new AnyNode("Type"))),
        //					BinaryOperatorType.InEquality,
        //					PatternHelper.OptionalParentheses(new InvocationExpression(
        //					new MemberReferenceExpression(new AnyNode("object"), "GetType")
        //					))
        //					);
        //			public override void VisitBinaryOperatorExpression(BinaryOperatorExpression binaryOperatorExpression)
        //			{
        //				base.VisitBinaryOperatorExpression(binaryOperatorExpression);
        //				var match = equalityComparePattern.Match(binaryOperatorExpression);
        //				if (match.Success) {
        //					AddDiagnosticAnalyzer(new CodeIssue(binaryOperatorExpression, match);
        //					return;
        //				}

        //				match = inEqualityComparePattern.Match(binaryOperatorExpression);
        //				if (match.Success) {
        //					AddDiagnosticAnalyzer(new CodeIssue(binaryOperatorExpression, match, true);
        //					return;
        //				}
        //			}*/
        //		}
    }
}