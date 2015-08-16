using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System.Linq;

namespace RefactoringEssentials.CSharp.Diagnostics
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class RedundantCheckBeforeAssignmentAnalyzer : DiagnosticAnalyzer
    {
        private static readonly DiagnosticDescriptor descriptor = new DiagnosticDescriptor(
            CSharpDiagnosticIDs.RedundantCheckBeforeAssignmentAnalyzerID,
            GettextCatalog.GetString("Check for inequality before assignment is redundant if (x != value) x = value;"),
            GettextCatalog.GetString("Redundant condition check before assignment"),
            DiagnosticAnalyzerCategories.RedundanciesInCode,
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            helpLinkUri: HelpLink.CreateFor(CSharpDiagnosticIDs.RedundantCheckBeforeAssignmentAnalyzerID),
            customTags: DiagnosticCustomTags.Unnecessary
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
                SyntaxKind.IfStatement
            );
        }

        private static bool TryGetDiagnostic(SyntaxNodeAnalysisContext nodeContext, out Diagnostic diagnostic)
        {
            diagnostic = default(Diagnostic);
            if (nodeContext.IsFromGeneratedCode())
                return false;
            int q = 1;
            if (q != 1)
                q = 1;
            
            var node = nodeContext.Node as IfStatementSyntax;
            var check = node?.Condition as BinaryExpressionSyntax;
            if (check == null)
                return false;
            var block = node.Statement as BlockSyntax;

            if (block?.Statements.Count > 0)
            {
                var statement = block.Statements.ElementAt(0) as ExpressionStatementSyntax;
                var assignmentExpression = statement?.Expression as AssignmentExpressionSyntax;
                if (assignmentExpression != null &&
                    check.Left.Equals(assignmentExpression.Left))
                {
                    diagnostic = Diagnostic.Create(descriptor, check.GetLocation());
                    return true;
                }
            }
            else
            {
                var statement = node.Statement as ExpressionStatementSyntax;
                if (statement != null)
                {
                    var expression = statement;
                    var syntax = expression.Expression as AssignmentExpressionSyntax;
                    if(syntax != null &&
                       check.Left.Equals(syntax.Left))
                    {
                        diagnostic = Diagnostic.Create(descriptor, check.GetLocation());
                        return true;
                    }
                }
            }
            return false;
        }

        //		class GatherVisitor : GatherVisitorBase<RedundantCheckBeforeAssignmentAnalyzer>
        //		{
        //			public GatherVisitor(SemanticModel semanticModel, Action<Diagnostic> addDiagnostic, CancellationToken cancellationToken)
        //				: base (semanticModel, addDiagnostic, cancellationToken)
        //			{
        //			}

        ////			static readonly AstNode pattern =
        ////				new IfElseStatement(
        ////					PatternHelper.CommutativeOperatorWithOptionalParentheses(new AnyNode("a"), BinaryOperatorType.InEquality, new AnyNode("b")),
        ////					PatternHelper.EmbeddedStatement(new AssignmentExpression(new Backreference("a"), PatternHelper.OptionalParentheses(new Backreference("b"))))
        ////				);
        ////
        ////			public override void VisitIfElseStatement(IfElseStatement ifElseStatement)
        ////			{
        ////				base.VisitIfElseStatement(ifElseStatement);
        ////				var m = pattern.Match(ifElseStatement);
        ////				if (!m.Success)
        ////					return;
        ////				AddDiagnosticAnalyzer(new CodeIssue(
        ////					ifElseStatement.Condition,
        ////					ctx.TranslateString(""),
        ////					ctx.TranslateString(""),
        ////					script => {
        ////						var stmt = ifElseStatement.TrueStatement;
        ////						var block = stmt as BlockStatement;
        ////						if (block != null)
        ////							stmt = block.Statements.First();
        ////						script.Replace(ifElseStatement, stmt.Clone());
        ////					}
        ////				));
        ////			}
        //		}
    }
}