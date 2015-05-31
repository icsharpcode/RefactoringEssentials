using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace RefactoringEssentials.CSharp.Diagnostics
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    [NotPortedYet]
    public class RedundantCheckBeforeAssignmentAnalyzer : DiagnosticAnalyzer
    {
        static readonly DiagnosticDescriptor descriptor = new DiagnosticDescriptor(
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
