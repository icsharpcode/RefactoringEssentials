using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace RefactoringEssentials.CSharp.Diagnostics
{
    /// <summary>
    /// Checks for str == null &amp;&amp; str == " "
    /// Converts to: string.IsNullOrEmpty (str)
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    [NotPortedYet]
    public class ReplaceWithStringIsNullOrEmptyAnalyzer : DiagnosticAnalyzer
    {
        //		static readonly Pattern pattern = new Choice {
        //			// str == null || str == ""
        //			// str == null || str.Length == 0
        //			new BinaryOperatorExpression (
        //				PatternHelper.CommutativeOperatorWithOptionalParentheses (new AnyNode ("str"), BinaryOperatorType.Equality, new NullReferenceExpression ()),
        //				BinaryOperatorType.ConditionalOr,
        //				new Choice {
        //					PatternHelper.CommutativeOperatorWithOptionalParentheses (new Backreference ("str"), BinaryOperatorType.Equality, new PrimitiveExpression ("")),
        //					PatternHelper.CommutativeOperatorWithOptionalParentheses (new Backreference ("str"), BinaryOperatorType.Equality,
        //				                                       new PrimitiveType("string").Member("Empty")),
        //					PatternHelper.CommutativeOperatorWithOptionalParentheses (
        //						new MemberReferenceExpression (new Backreference ("str"), "Length"),
        //						BinaryOperatorType.Equality,
        //						new PrimitiveExpression (0)
        //					)
        //				}
        //			),
        //			// str == "" || str == null
        //			new BinaryOperatorExpression (
        //				new Choice {
        //					PatternHelper.CommutativeOperatorWithOptionalParentheses (new AnyNode ("str"), BinaryOperatorType.Equality, new PrimitiveExpression ("")),
        //					PatternHelper.CommutativeOperatorWithOptionalParentheses (new AnyNode ("str"), BinaryOperatorType.Equality,
        //				                                       new PrimitiveType("string").Member("Empty"))
        //				},
        //				BinaryOperatorType.ConditionalOr,
        //				PatternHelper.CommutativeOperator(new Backreference ("str"), BinaryOperatorType.Equality, new NullReferenceExpression ())
        //			)
        //		};
        //		static readonly Pattern negPattern = new Choice {
        //			// str != null && str != ""
        //			// str != null && str.Length != 0
        //			// str != null && str.Length > 0
        //			new BinaryOperatorExpression (
        //				PatternHelper.CommutativeOperatorWithOptionalParentheses(new AnyNode ("str"), BinaryOperatorType.InEquality, new NullReferenceExpression ()),
        //				BinaryOperatorType.ConditionalAnd,
        //				new Choice {
        //					PatternHelper.CommutativeOperatorWithOptionalParentheses (new Backreference ("str"), BinaryOperatorType.InEquality, new PrimitiveExpression ("")),
        //					PatternHelper.CommutativeOperatorWithOptionalParentheses (new Backreference ("str"), BinaryOperatorType.InEquality,
        //				                                   	   new PrimitiveType("string").Member("Empty")),
        //					PatternHelper.CommutativeOperatorWithOptionalParentheses (
        //						new MemberReferenceExpression (new Backreference ("str"), "Length"),
        //						BinaryOperatorType.InEquality,
        //						new PrimitiveExpression (0)
        //					),
        //					new BinaryOperatorExpression (
        //						new MemberReferenceExpression (new Backreference ("str"), "Length"),
        //						BinaryOperatorType.GreaterThan,
        //						new PrimitiveExpression (0)
        //					)
        //				}
        //			),
        //			// str != "" && str != null
        //			new BinaryOperatorExpression (
        //				new Choice {
        //					PatternHelper.CommutativeOperatorWithOptionalParentheses (new AnyNode ("str"), BinaryOperatorType.InEquality, new PrimitiveExpression ("")),
        //					PatternHelper.CommutativeOperatorWithOptionalParentheses (new AnyNode ("str"), BinaryOperatorType.Equality,
        //				                                   	   new PrimitiveType("string").Member("Empty"))
        //				},
        //				BinaryOperatorType.ConditionalAnd,
        //				PatternHelper.CommutativeOperatorWithOptionalParentheses(new Backreference ("str"), BinaryOperatorType.InEquality, new NullReferenceExpression ())
        //			)
        //		};

        static readonly DiagnosticDescriptor descriptor = new DiagnosticDescriptor(
            NRefactoryDiagnosticIDs.ReplaceWithStringIsNullOrEmptyAnalyzerID,
            GettextCatalog.GetString("Uses shorter string.IsNullOrEmpty call instead of a longer condition"),
            GettextCatalog.GetString("Expression can be replaced with '{0}'"),
            DiagnosticAnalyzerCategories.PracticesAndImprovements,
            DiagnosticSeverity.Info,
            isEnabledByDefault: true,
            helpLinkUri: HelpLink.CreateFor(NRefactoryDiagnosticIDs.ReplaceWithStringIsNullOrEmptyAnalyzerID)
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

        //		class GatherVisitor : GatherVisitorBase<ReplaceWithStringIsNullOrEmptyAnalyzer>
        //		{
        //			public GatherVisitor(SemanticModel semanticModel, Action<Diagnostic> addDiagnostic, CancellationToken cancellationToken)
        //				: base (semanticModel, addDiagnostic, cancellationToken)
        //			{
        //			}
        ////
        ////			public override void VisitBinaryOperatorExpression(BinaryOperatorExpression binaryOperatorExpression)
        ////			{
        ////				base.VisitBinaryOperatorExpression(binaryOperatorExpression);
        ////				Match m = pattern.Match(binaryOperatorExpression);
        ////				bool isNegated = false;
        ////				if (!m.Success) {
        ////					m = negPattern.Match(binaryOperatorExpression);
        ////					isNegated = true;
        ////				}
        ////				if (m.Success) {
        ////					var str = m.Get<Expression>("str").Single();
        ////					var def = ctx.Resolve(str).Type.GetDefinition();
        ////					if (def == null || def.KnownTypeCode != ICSharpCode.NRefactory.TypeSystem.KnownTypeCode.String)
        ////						return;
        ////					AddDiagnosticAnalyzer(new CodeIssue(
        ////						binaryOperatorExpression,
        //			//						isNegated ? ctx.TranslateString("Expression can be replaced with !string.IsNullOrEmpty") : ctx.TranslateString(""),
        ////						new CodeAction (
        //			//							isNegated ? ctx.TranslateString("Use !string.IsNullOrEmpty") : ctx.TranslateString("Use string.IsNullOrEmpty"),
        ////							script => {
        ////								Expression expr = new PrimitiveType("string").Invoke("IsNullOrEmpty", str.Clone());
        ////								if (isNegated)
        ////									expr = new UnaryOperatorExpression(UnaryOperatorType.Not, expr);
        ////								script.Replace(binaryOperatorExpression, expr);
        ////							},
        ////							binaryOperatorExpression
        ////						)
        ////					));
        ////					return;
        ////				}
        ////			}
        ////	
        //		}
    }


}