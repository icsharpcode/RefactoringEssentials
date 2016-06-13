using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace RefactoringEssentials.CSharp.Diagnostics
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    [NotPortedYet]
    public class SimplifyLinqExpressionAnalyzer : DiagnosticAnalyzer
    {
        static readonly DiagnosticDescriptor descriptor = new DiagnosticDescriptor(
            CSharpDiagnosticIDs.SimplifyLinqExpressionAnalyzerID,
            GettextCatalog.GetString("Simplify LINQ expression"),
            GettextCatalog.GetString("Simplify LINQ expression"),
            DiagnosticAnalyzerCategories.PracticesAndImprovements,
            DiagnosticSeverity.Info,
            isEnabledByDefault: true,
            helpLinkUri: HelpLink.CreateFor(CSharpDiagnosticIDs.SimplifyLinqExpressionAnalyzerID),
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

        //		class GatherVisitor : GatherVisitorBase<SimplifyLinqExpressionAnalyzer>
        //		{
        //			public GatherVisitor(SemanticModel semanticModel, Action<Diagnostic> addDiagnostic, CancellationToken cancellationToken)
        //				: base (semanticModel, addDiagnostic, cancellationToken)
        //			{
        //			}

        ////			static readonly Expression simpleExpression = new Choice {
        ////				new UnaryOperatorExpression (UnaryOperatorType.Not, new AnyNode()),
        ////				new BinaryOperatorExpression(new AnyNode(), BinaryOperatorType.Equality, new AnyNode()),
        ////				new BinaryOperatorExpression(new AnyNode(), BinaryOperatorType.InEquality, new AnyNode())
        ////			};
        ////
        ////			static readonly AstNode argumentPattern = new Choice {
        ////				new LambdaExpression  {
        ////					Parameters = { new ParameterDeclaration(PatternHelper.AnyType(true), Pattern.AnyString) },
        ////					Body = new Choice {
        ////						new NamedNode ("expr", simpleExpression),
        ////						new BlockStatement { new ReturnStatement(new NamedNode ("expr", simpleExpression))}
        ////					} 
        ////				},
        ////				new AnonymousMethodExpression {
        ////					Parameters = { new ParameterDeclaration(PatternHelper.AnyType(true), Pattern.AnyString) },
        ////					Body = new BlockStatement { new ReturnStatement(new NamedNode ("expr", simpleExpression))}
        ////				}
        ////			};
        ////
        ////			public override void VisitUnaryOperatorExpression(UnaryOperatorExpression unaryOperatorExpression)
        ////			{
        ////				base.VisitUnaryOperatorExpression(unaryOperatorExpression);
        ////				if (unaryOperatorExpression.Operator != UnaryOperatorType.Not)
        ////					return;
        ////				var invocation =  CSharpUtil.GetInnerMostExpression(unaryOperatorExpression.Expression) as InvocationExpression;
        ////				if (invocation == null)
        ////					return; 
        ////				var rr = ctx.Resolve(invocation) as CSharpInvocationResolveResult;
        ////				if (rr == null || rr.IsError)
        ////					return;
        ////
        ////				if (rr.Member.DeclaringType.Name != "Enumerable" || rr.Member.DeclaringType.Namespace != "System.Linq")
        ////					return;
        ////				if (!new[] { "All", "Any" }.Contains(rr.Member.Name))
        ////					return;
        ////				if (invocation.Arguments.Count != 1)
        ////					return;
        ////				var arg = invocation.Arguments.First();
        ////				var match = argumentPattern.Match(arg);
        ////				if (!match.Success)
        ////					return;
        ////				AddDiagnosticAnalyzer(new CodeIssue (
        ////					unaryOperatorExpression,
        ////					ctx.TranslateString(""),
        ////					ctx.TranslateString("Simplify LINQ expression"),
        ////					s => {
        ////						var target = invocation.Target.Clone() as MemberReferenceExpression;
        ////						target.MemberName = target.MemberName == "All" ? "Any" : "All";
        ////
        ////						var expr = arg.Clone();
        ////						var nmatch = argumentPattern.Match(expr);
        ////						var cond = nmatch.Get<Expression>("expr").Single();
        ////						cond.ReplaceWith(CSharpUtil.InvertCondition(cond));
        ////						var simplifiedInvocation = new InvocationExpression(
        ////							target,
        ////							expr
        ////						);
        ////						s.Replace(unaryOperatorExpression, simplifiedInvocation);
        ////					}
        ////				));
        ////			}
        //		}
    }
}