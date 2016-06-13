using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace RefactoringEssentials.CSharp.Diagnostics
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    [NotPortedYet]
    public class ConvertIfStatementToNullCoalescingExpressionAnalyzer : DiagnosticAnalyzer
    {
        static readonly DiagnosticDescriptor descriptor = new DiagnosticDescriptor(
            CSharpDiagnosticIDs.ConvertIfStatementToNullCoalescingExpressionAnalyzerID,
            GettextCatalog.GetString("Convert 'if' to '??'"),
            GettextCatalog.GetString("Convert to '??' expresssion"),
            DiagnosticAnalyzerCategories.Opportunities,
            DiagnosticSeverity.Info,
            isEnabledByDefault: true,
            helpLinkUri: HelpLink.CreateFor(CSharpDiagnosticIDs.ConvertIfStatementToNullCoalescingExpressionAnalyzerID)
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

        //		class GatherVisitor : GatherVisitorBase<ConvertIfStatementToNullCoalescingExpressionAnalyzer>
        //		{
        //			public GatherVisitor(SemanticModel semanticModel, Action<Diagnostic> addDiagnostic, CancellationToken cancellationToken)
        //				: base (semanticModel, addDiagnostic, cancellationToken)
        //			{
        //			}

        ////			public override void VisitIfElseStatement(IfElseStatement ifElseStatement)
        ////			{
        ////				base.VisitIfElseStatement(ifElseStatement);
        ////				Expression rightSide;
        ////				var leftSide = ConvertIfStatementToNullCoalescingExpressionAction.CheckNode(ifElseStatement, out rightSide);
        ////				if (leftSide == null)
        ////					return;
        ////				if (ConvertIfStatementToConditionalTernaryExpressionAnalyzer.IsComplexExpression(leftSide) || 
        ////				    ConvertIfStatementToConditionalTernaryExpressionAnalyzer.IsComplexExpression(rightSide))
        ////					return;
        ////				var previousNode = ifElseStatement.GetPrevSibling(sibling => sibling is Statement) as VariableDeclarationStatement;
        ////				if (previousNode == null || ConvertIfStatementToConditionalTernaryExpressionAnalyzer.IsComplexExpression(previousNode))
        ////					return;
        ////
        ////				AddDiagnosticAnalyzer(new CodeIssue(
        ////					ifElseStatement.IfToken,
        ////					ctx.TranslateString("")
        ////				){ IssueMarker = IssueMarker.DottedLine, ActionProvider = { typeof(ConvertIfStatementToNullCoalescingExpressionAction) } });
        ////			}
        //		}
    }


}