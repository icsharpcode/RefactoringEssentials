using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace RefactoringEssentials.CSharp.Diagnostics
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    [NotPortedYet]
    public class RedundantEmptyFinallyBlockAnalyzer : DiagnosticAnalyzer
    {
        static readonly DiagnosticDescriptor descriptor = new DiagnosticDescriptor(
            CSharpDiagnosticIDs.RedundantEmptyFinallyBlockAnalyzerID,
            GettextCatalog.GetString("Redundant empty finally block"),
            GettextCatalog.GetString("Redundant empty finally block"),
            DiagnosticAnalyzerCategories.RedundanciesInCode,
            DiagnosticSeverity.Info,
            isEnabledByDefault: true,
            helpLinkUri: HelpLink.CreateFor(CSharpDiagnosticIDs.RedundantEmptyFinallyBlockAnalyzerID),
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

        //		class GatherVisitor : GatherVisitorBase<RedundantEmptyFinallyBlockAnalyzer>
        //		{
        //			public GatherVisitor(SemanticModel semanticModel, Action<Diagnostic> addDiagnostic, CancellationToken cancellationToken)
        //				: base (semanticModel, addDiagnostic, cancellationToken)
        //			{
        //			}

        ////			static bool IsEmpty (BlockStatement blockStatement)
        ////			{
        ////				return !blockStatement.Descendants.Any(s => s is Statement && !(s is EmptyStatement || s is BlockStatement));
        ////			}
        ////
        ////			public override void VisitBlockStatement(BlockStatement blockStatement)
        ////			{
        ////				base.VisitBlockStatement(blockStatement);
        ////				if (blockStatement.Role != TryCatchStatement.FinallyBlockRole || !IsEmpty (blockStatement))
        ////					return;
        ////				var tryCatch = blockStatement.Parent as TryCatchStatement;
        ////				if (tryCatch == null)
        ////					return;
        ////				AddDiagnosticAnalyzer(new CodeIssue(
        ////					tryCatch.FinallyToken.StartLocation,
        ////					blockStatement.EndLocation,
        ////					ctx.TranslateString(""),
        ////					ctx.TranslateString(""),
        ////					s => {
        ////						if (tryCatch.CatchClauses.Any()) {
        ////							s.Remove(tryCatch.FinallyToken);
        ////							s.Remove(blockStatement); 
        ////							s.FormatText(tryCatch);
        ////							return;
        ////						}
        ////						s.Remove(tryCatch.TryToken);
        ////						s.Remove(tryCatch.TryBlock.LBraceToken);
        ////						s.Remove(tryCatch.TryBlock.RBraceToken);
        ////						s.Remove(tryCatch.FinallyToken);
        ////						s.Remove(tryCatch.FinallyBlock); 
        ////						s.FormatText(tryCatch.Parent);
        ////					}
        ////				) { IssueMarker = IssueMarker.GrayOut });
        ////			}
        //		}
    }
}