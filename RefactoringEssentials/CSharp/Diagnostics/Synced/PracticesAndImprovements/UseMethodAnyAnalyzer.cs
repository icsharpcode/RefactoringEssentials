using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace RefactoringEssentials.CSharp.Diagnostics
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    [NotPortedYet]
    public class UseMethodAnyAnalyzer : DiagnosticAnalyzer
    {
        static readonly DiagnosticDescriptor descriptor = new DiagnosticDescriptor(
            CSharpDiagnosticIDs.UseMethodAnyAnalyzerID,
            GettextCatalog.GetString("Replace usages of 'Count()' with call to 'Any()'"),
            GettextCatalog.GetString("Use '{0}' for increased performance"),
            DiagnosticAnalyzerCategories.PracticesAndImprovements,
            DiagnosticSeverity.Info,
            isEnabledByDefault: true,
            helpLinkUri: HelpLink.CreateFor(CSharpDiagnosticIDs.UseMethodAnyAnalyzerID)
        );

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(descriptor);

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

        //		class GatherVisitor : GatherVisitorBase<UseMethodAnyAnalyzer>
        //		{
        //			public GatherVisitor(SemanticModel semanticModel, Action<Diagnostic> addDiagnostic, CancellationToken cancellationToken)
        //				: base (semanticModel, addDiagnostic, cancellationToken)
        //			{
        //			}
        ////
        ////			void AddDiagnosticAnalyzer2(BinaryOperatorExpression binaryOperatorExpression, Expression expr)
        ////			{
        ////			}
        ////
        ////			readonly AstNode anyPattern =
        ////				new Choice {
        ////					PatternHelper.CommutativeOperatorWithOptionalParentheses(
        ////						new NamedNode ("invocation", new InvocationExpression(new MemberReferenceExpression(new AnyNode("expr"), "Count"))),
        ////						BinaryOperatorType.InEquality,
        ////						new PrimitiveExpression(0)
        ////					),
        ////					new BinaryOperatorExpression (
        ////						PatternHelper.OptionalParentheses(new NamedNode ("invocation", new InvocationExpression(new MemberReferenceExpression(new AnyNode("expr"), "Count")))),
        ////						BinaryOperatorType.GreaterThan,
        ////						PatternHelper.OptionalParentheses(new PrimitiveExpression(0))
        ////					),
        ////					new BinaryOperatorExpression (
        ////						PatternHelper.OptionalParentheses(new PrimitiveExpression(0)),
        ////						BinaryOperatorType.LessThan,
        ////						PatternHelper.OptionalParentheses(new NamedNode ("invocation", new InvocationExpression(new MemberReferenceExpression(new AnyNode("expr"), "Count"))))
        ////					),
        ////					new BinaryOperatorExpression (
        ////						PatternHelper.OptionalParentheses(new NamedNode ("invocation", new InvocationExpression(new MemberReferenceExpression(new AnyNode("expr"), "Count")))),
        ////						BinaryOperatorType.GreaterThanOrEqual,
        ////						PatternHelper.OptionalParentheses(new PrimitiveExpression(1))
        ////					),
        ////					new BinaryOperatorExpression (
        ////						PatternHelper.OptionalParentheses(new PrimitiveExpression(1)),
        ////						BinaryOperatorType.LessThanOrEqual,
        ////						PatternHelper.OptionalParentheses(new NamedNode ("invocation", new InvocationExpression(new MemberReferenceExpression(new AnyNode("expr"), "Count"))))
        ////					)
        ////			};
        ////
        ////			readonly AstNode notAnyPattern =
        ////				new Choice {
        ////					PatternHelper.CommutativeOperatorWithOptionalParentheses(
        ////						new NamedNode ("invocation", new InvocationExpression(new MemberReferenceExpression(new AnyNode("expr"), "Count"))),
        ////						BinaryOperatorType.Equality,
        ////						new PrimitiveExpression(0)
        ////					),
        ////					new BinaryOperatorExpression (
        ////						PatternHelper.OptionalParentheses(new NamedNode ("invocation", new InvocationExpression(new MemberReferenceExpression(new AnyNode("expr"), "Count")))),
        ////						BinaryOperatorType.LessThan,
        ////						PatternHelper.OptionalParentheses(new PrimitiveExpression(1))
        ////					),
        ////					new BinaryOperatorExpression (
        ////						PatternHelper.OptionalParentheses(new PrimitiveExpression(1)),
        ////						BinaryOperatorType.GreaterThan,
        ////						PatternHelper.OptionalParentheses(new NamedNode ("invocation", new InvocationExpression(new MemberReferenceExpression(new AnyNode("expr"), "Count"))))
        ////					),
        ////					new BinaryOperatorExpression (
        ////						PatternHelper.OptionalParentheses(new NamedNode ("invocation", new InvocationExpression(new MemberReferenceExpression(new AnyNode("expr"), "Count")))),
        ////						BinaryOperatorType.LessThanOrEqual,
        ////						PatternHelper.OptionalParentheses(new PrimitiveExpression(0))
        ////					),
        ////					new BinaryOperatorExpression (
        ////						PatternHelper.OptionalParentheses(new PrimitiveExpression(0)),
        ////						BinaryOperatorType.GreaterThanOrEqual,
        ////						PatternHelper.OptionalParentheses(new NamedNode ("invocation", new InvocationExpression(new MemberReferenceExpression(new AnyNode("expr"), "Count"))))
        ////					)
        ////				};
        ////
        ////			void AddMatch(BinaryOperatorExpression binaryOperatorExpression, Match match, bool negateAny)
        ////			{
        ////				AddDiagnosticAnalyzer(new CodeIssue(
        ////					binaryOperatorExpression,
        ////					ctx.TranslateString(""), 
        ////					script =>  {
        ////						Expression expr = new InvocationExpression(new MemberReferenceExpression(match.Get<Expression>("expr").First().Clone(), "Any"));
        ////						if (negateAny)
        ////							expr = new UnaryOperatorExpression(UnaryOperatorType.Not, expr);
        ////						script.Replace(binaryOperatorExpression, expr);
        ////					}
        ////				));
        ////			}
        ////
        ////			bool CheckMethod(Match match)
        ////			{
        ////				var invocation = match.Get<Expression>("invocation").First();
        ////				var rr = ctx.Resolve(invocation) as CSharpInvocationResolveResult;
        ////				if (rr == null || rr.IsError)
        ////					return false;
        ////				var method = rr.Member as IMethod;
        ////				return 
        ////					method != null &&
        ////					method.IsExtensionMethod &&
        ////					method.DeclaringTypeDefinition.Namespace == "System.Linq" && 
        ////					method.DeclaringTypeDefinition.Name == "Enumerable";
        ////			}
        ////
        ////			public override void VisitBinaryOperatorExpression(BinaryOperatorExpression binaryOperatorExpression)
        ////			{
        ////				base.VisitBinaryOperatorExpression(binaryOperatorExpression);
        ////				var match = anyPattern.Match(binaryOperatorExpression);
        ////				if (match.Success && CheckMethod (match)) {
        ////					AddMatch(binaryOperatorExpression, match, false);
        ////					return;
        ////				}
        ////				match = notAnyPattern.Match(binaryOperatorExpression);
        ////				if (match.Success && CheckMethod (match)) {
        ////					AddMatch(binaryOperatorExpression, match, true);
        ////					return;
        ////				}
        ////			}
        //		}
    }
}