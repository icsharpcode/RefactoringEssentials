using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace RefactoringEssentials.CSharp.Diagnostics
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    [NotPortedYet]
    public class RewriteIfReturnToReturnAnalyzer : DiagnosticAnalyzer
    {
        static readonly DiagnosticDescriptor descriptor = new DiagnosticDescriptor(
            NRefactoryDiagnosticIDs.RewriteIfReturnToReturnAnalyzerID,
            GettextCatalog.GetString("Convert 'if...return' to 'return'"),
            GettextCatalog.GetString("Convert to 'return' statement"),
            DiagnosticAnalyzerCategories.Opportunities,
            DiagnosticSeverity.Info,
            isEnabledByDefault: true,
            helpLinkUri: HelpLink.CreateFor(NRefactoryDiagnosticIDs.RewriteIfReturnToReturnAnalyzerID)
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

        //		class GatherVisitor : GatherVisitorBase<RewriteIfReturnToReturnAnalyzer>
        //		{
        //			public GatherVisitor(SemanticModel semanticModel, Action<Diagnostic> addDiagnostic, CancellationToken cancellationToken)
        //				: base (semanticModel, addDiagnostic, cancellationToken)
        //			{
        //			}
        ////
        ////			public override void VisitIfElseStatement(IfElseStatement ifElseStatement)
        ////			{
        ////				base.VisitIfElseStatement(ifElseStatement);
        ////
        ////				if (ifElseStatement.Parent is IfElseStatement)
        ////					return;
        ////				Expression c, e1, e2;
        ////				AstNode rs;
        ////				if (!ConvertIfStatementToReturnStatementAction.GetMatch(ifElseStatement, out c, out e1, out e2, out rs))
        ////					return;
        ////				if (ConvertIfStatementToConditionalTernaryExpressionAnalyzer.IsComplexCondition(c) || 
        ////				    ConvertIfStatementToConditionalTernaryExpressionAnalyzer.IsComplexExpression(e1) || 
        ////				    ConvertIfStatementToConditionalTernaryExpressionAnalyzer.IsComplexExpression(e2))
        ////					return;
        ////				AddDiagnosticAnalyzer(new CodeIssue(
        ////					ifElseStatement.IfToken,
        ////					ctx.TranslateString("")
        ////				) { IssueMarker = IssueMarker.DottedLine, ActionProvider = { typeof(ConvertIfStatementToReturnStatementAction) } });
        ////			}
        //		}
    }
}