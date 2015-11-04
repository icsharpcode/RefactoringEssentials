using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using RefactoringEssentials;
using RefactoringEssentials.Util;
using System;
using System.Collections.Immutable;
using System.Linq;
using System.Text.RegularExpressions;

namespace RefactoringEssentials.CSharp.Diagnostics
{
    /// <summary>
    /// Checks for str == null &amp;&amp; str == " "
    /// Converts to: string.IsNullOrEmpty (str)
    /// </summary>
    /// [NotPortedYet]
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class ReplaceWithStringIsNullOrEmptyAnalyzer : DiagnosticAnalyzer
    {
        //static readonly Pattern pattern = new Choice {
        //			// str == null || str == ""
        //			// str == null || str.Length == 0
        //			new BinaryOperatorExpression (
        //                PatternHelper.CommutativeOperatorWithOptionalParentheses (new AnyNode ("str"), BinaryOperatorType.Equality, new NullReferenceExpression ()),
        //                BinaryOperatorType.ConditionalOr,
        //                new Choice {
        //                    PatternHelper.CommutativeOperatorWithOptionalParentheses (new Backreference ("str"), BinaryOperatorType.Equality, new PrimitiveExpression ("")),
        //                    PatternHelper.CommutativeOperatorWithOptionalParentheses (new Backreference ("str"), BinaryOperatorType.Equality,
        //                                                       new PrimitiveType("string").Member("Empty")),
        //                    PatternHelper.CommutativeOperatorWithOptionalParentheses (
        //                        new MemberReferenceExpression (new Backreference ("str"), "Length"),
        //                        BinaryOperatorType.Equality,
        //                        new PrimitiveExpression (0)
        //                    )
        //                }
        //            ),
        //			// str == "" || str == null
        //			new BinaryOperatorExpression (
        //                new Choice {
        //                    PatternHelper.CommutativeOperatorWithOptionalParentheses (new AnyNode ("str"), BinaryOperatorType.Equality, new PrimitiveExpression ("")),
        //                    PatternHelper.CommutativeOperatorWithOptionalParentheses (new AnyNode ("str"), BinaryOperatorType.Equality,
        //                                                       new PrimitiveType("string").Member("Empty"))
        //                },
        //                BinaryOperatorType.ConditionalOr,
        //                PatternHelper.CommutativeOperator(new Backreference ("str"), BinaryOperatorType.Equality, new NullReferenceExpression ())
        //            )
        //        };
        //static readonly Pattern negPattern = new Choice {
        //			// str != null && str != ""
        //			// str != null && str.Length != 0
        //			// str != null && str.Length > 0
        //			new BinaryOperatorExpression (
        //                PatternHelper.CommutativeOperatorWithOptionalParentheses(new AnyNode ("str"), BinaryOperatorType.InEquality, new NullReferenceExpression ()),
        //                BinaryOperatorType.ConditionalAnd,
        //                new Choice {
        //                    PatternHelper.CommutativeOperatorWithOptionalParentheses (new Backreference ("str"), BinaryOperatorType.InEquality, new PrimitiveExpression ("")),
        //                    PatternHelper.CommutativeOperatorWithOptionalParentheses (new Backreference ("str"), BinaryOperatorType.InEquality,
        //                                                          new PrimitiveType("string").Member("Empty")),
        //                    PatternHelper.CommutativeOperatorWithOptionalParentheses (
        //                        new MemberReferenceExpression (new Backreference ("str"), "Length"),
        //                        BinaryOperatorType.InEquality,
        //                        new PrimitiveExpression (0)
        //                    ),
        //                    new BinaryOperatorExpression (
        //                        new MemberReferenceExpression (new Backreference ("str"), "Length"),
        //                        BinaryOperatorType.GreaterThan,
        //                        new PrimitiveExpression (0)
        //                    )
        //                }
        //            ),
        //			// str != "" && str != null
        //			new BinaryOperatorExpression (
        //                new Choice {
        //                    PatternHelper.CommutativeOperatorWithOptionalParentheses (new AnyNode ("str"), BinaryOperatorType.InEquality, new PrimitiveExpression ("")),
        //                    PatternHelper.CommutativeOperatorWithOptionalParentheses (new AnyNode ("str"), BinaryOperatorType.Equality,
        //                                                          new PrimitiveType("string").Member("Empty"))
        //                },
        //                BinaryOperatorType.ConditionalAnd,
        //                PatternHelper.CommutativeOperatorWithOptionalParentheses(new Backreference ("str"), BinaryOperatorType.InEquality, new NullReferenceExpression ())
        //            )
        //        };

        static readonly DiagnosticDescriptor descriptor = new DiagnosticDescriptor(
            CSharpDiagnosticIDs.ReplaceWithStringIsNullOrEmptyAnalyzerID,
            GettextCatalog.GetString("Uses shorter string.IsNullOrEmpty call instead of a longer condition"),
            GettextCatalog.GetString("Expression can be replaced with '{0}'"),
            DiagnosticAnalyzerCategories.PracticesAndImprovements,
            DiagnosticSeverity.Info,
            isEnabledByDefault: true,
            helpLinkUri: HelpLink.CreateFor(CSharpDiagnosticIDs.ReplaceWithStringIsNullOrEmptyAnalyzerID)
        );

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(descriptor);

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(
                (nodeContext) =>
                {
                    Diagnostic diagnostic;
                    if (TryGetDiagnostic(nodeContext, out diagnostic))
                    {
                        nodeContext.ReportDiagnostic(diagnostic);
                    }
                },
                SyntaxKind.LogicalAndExpression,
                SyntaxKind.LogicalOrExpression
            );
        }

        static readonly SyntaxKind[] EqualitySyntaxKinds = {
            SyntaxKind.EqualsExpression,
            SyntaxKind.NotEqualsExpression };

        static bool TryGetDiagnostic(SyntaxNodeAnalysisContext nodeContext, out Diagnostic diagnostic)
        {
            diagnostic = default(Diagnostic);
            if (nodeContext.IsFromGeneratedCode())
                return false;

            var node = nodeContext.Node as BinaryExpressionSyntax;

            if (!node.IsKind(SyntaxKind.LogicalAndExpression, SyntaxKind.LogicalOrExpression))
                return false;

            if (!EqualitySyntaxKinds.Any(sk => node.Left.IsKind(sk)) || !EqualitySyntaxKinds.Any(sk => node.Left.IsKind(sk)))
                return false;

            var leftReplace = ShouldReplace(node.Left as BinaryExpressionSyntax);
            var rightReplace = ShouldReplace(node.Right as BinaryExpressionSyntax);



            //var node = nodeContext.Node as ;
            //diagnostic = Diagnostic.Create (descriptor, node.GetLocation ());
            //return true;
            return false;
        }

        private static readonly SyntaxKind[] LiteralKinds = { SyntaxKind.NullLiteralExpression, SyntaxKind.StringLiteralExpression };

        static ShouldReplaceResult ShouldReplace(BinaryExpressionSyntax node)
        {
            // string is null || string is empty
            // str == null || str == ""
            // str == null || str == string.Empty
            // str == null || str.Length == 0

            // str != null && str != ""
            // str != null && str.Length != 0
            // str != null && str.Length > 0
            // str == null || str == ""
            // str == null || str.Length == 0
            // str != "" && str != null

            //SyntaxKind.EqualsExpression,
            //SyntaxKind.NotEqualsExpression,
            //SyntaxKind.GreaterThanExpression,
            //SyntaxKind.GreaterThanOrEqualExpression

            var result = new ShouldReplaceResult();

            IdentifierNameSyntax identifierNode;

            if (node.Left.IsKind(SyntaxKind.IdentifierName))
            {
                identifierNode = (IdentifierNameSyntax)node.Left;

                if (node.Right.IsKind(SyntaxKind.NullLiteralExpression))
                {
                    result.ShouldReplace = true;
                    result.IsNullTest = true;
                }
                else if (node.Right.IsKind(SyntaxKind.StringLiteralExpression))
                {
                    if (!string.Equals("\"\"", node.Right.ToString()))
                        return result;

                    result.ShouldReplace = true;
                    result.IsEmptyTest = true;
                }
                else if (node.Right.IsKind(SyntaxKind.SimpleMemberAccessExpression))
                {
                    var sma = node.Right as MemberAccessExpressionSyntax;

                    if (!string.Equals("string", sma.Expression.ToString(), StringComparison.OrdinalIgnoreCase))
                        return result;

                    if (!string.Equals("Empty", sma.Name.ToString(), StringComparison.OrdinalIgnoreCase))
                        return result;

                    result.ShouldReplace = true;
                    result.IsEmptyTest = true;
                }

                if (result.ShouldReplace)
                    result.IdentifierName = ((IdentifierNameSyntax)node.Left).Identifier.Text;
            }
            else if (node.Right.IsKind(SyntaxKind.IdentifierName))
            {
                identifierNode = (IdentifierNameSyntax)node.Right;
            }

            if (node.IsKind(SyntaxKind.EqualsExpression))
            {
                
                //var eNode = node as Microsoft.CodeAnalysis.CSharp.Syntax.
            }
            return result;
        }

        private class ShouldReplaceResult
        {
            public bool ShouldReplace { get; set; } = false;

            public bool IsNullTest { get; set; } = false;

            public bool IsEmptyTest { get; set; } = false;

            public bool IsNegative { get; set; } = true;

            public string IdentifierName { get; set; } = string.Empty;

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