using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace RefactoringEssentials.CSharp.Diagnostics
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    [NotPortedYet]
    public class FormatStringProblemAnalyzer : DiagnosticAnalyzer
    {
        static readonly DiagnosticDescriptor descriptor = new DiagnosticDescriptor(
            DiagnosticIDs.FormatStringProblemAnalyzerID,
            GettextCatalog.GetString("The string format index is out of bounds of the passed arguments"),
            GettextCatalog.GetString("The index '{0}' is out of bounds of the passed arguments"),
            DiagnosticAnalyzerCategories.CodeQualityIssues,
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            helpLinkUri: HelpLink.CreateFor(DiagnosticIDs.FormatStringProblemAnalyzerID)
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

        //		class GatherVisitor : GatherVisitorBase<FormatStringProblemAnalyzer>
        //		{
        //			//readonly BaseSemanticModel context;

        //			public GatherVisitor(SemanticModel semanticModel, Action<Diagnostic> addDiagnostic, CancellationToken cancellationToken)
        //				: base (semanticModel, addDiagnostic, cancellationToken)
        //			{
        //			}

        ////			public override void VisitInvocationExpression(InvocationExpression invocationExpression)
        ////			{
        ////				base.VisitInvocationExpression(invocationExpression);
        ////
        ////				// Speed up the inspector by discarding some invocations early
        ////				var hasEligibleArgument = invocationExpression.Arguments.Any(argument => {
        ////					var primitiveArg = argument as PrimitiveExpression;
        ////					return primitiveArg != null && primitiveArg.Value is string;
        ////				});
        ////				if (!hasEligibleArgument)
        ////					return;
        ////
        ////				var invocationResolveResult = context.Resolve(invocationExpression) as CSharpInvocationResolveResult;
        ////				if (invocationResolveResult == null)
        ////					return;
        ////				Expression formatArgument;
        ////				IList<Expression> formatArguments;
        ////				if (!FormatStringHelper.TryGetFormattingParameters(invocationResolveResult, invocationExpression,
        ////				                                                   out formatArgument, out formatArguments, null)) {
        ////					return;
        ////				}
        ////				var primitiveArgument = formatArgument as PrimitiveExpression;
        ////				if (primitiveArgument == null || !(primitiveArgument.Value is string))
        //			//					return;'{0}' i
        ////				var format = (string)primitiveArgument.Value;
        ////				var parsingResult = context.ParseFormatString(format);
        ////				CheckSegments(parsingResult.Segments, formatArgument.StartLocation, formatArguments, invocationExpression);
        ////
        ////				var argUsed = new HashSet<int> ();
        ////
        ////				foreach (var item in parsingResult.Segments) {
        ////					var fi = item as FormatItem;
        ////					if (fi == null)
        ////						continue;
        ////					argUsed.Add(fi.Index);
        ////				}
        ////				for (int i = 0; i < formatArguments.Count; i++) {
        ////					if (!argUsed.Contains(i)) {
        ////						AddDiagnosticAnalyzer(new CodeIssue(formatArguments[i], ctx.TranslateString("Argument is not used in format string")) { IssueMarker = IssueMarker.GrayOut });
        ////					}
        ////				}
        ////			}
        ////
        ////			void CheckSegments(IList<IFormatStringSegment> segments, TextLocation formatStart, IList<Expression> formatArguments, AstNode anchor)
        ////			{
        ////				int argumentCount = formatArguments.Count;
        ////				foreach (var segment in segments) {
        ////					var errors = segment.Errors.ToList();
        ////					var formatItem = segment as FormatItem;
        ////					if (formatItem != null) {
        ////						var segmentEnd = new TextLocation(formatStart.Line, formatStart.Column + segment.EndLocation + 1);
        ////						var segmentStart = new TextLocation(formatStart.Line, formatStart.Column + segment.StartLocation + 1);
        ////						if (formatItem.Index >= argumentCount) {
        //			//							var outOfBounds = context.TranslateString("The index '{0}' is out of bounds of the passed arguments");
        ////							AddDiagnosticAnalyzer(new CodeIssue(segmentStart, segmentEnd, string.Format(outOfBounds, formatItem.Index)));
        ////						}
        ////						if (formatItem.HasErrors) {
        ////							var errorMessage = string.Join(Environment.NewLine, errors.Select(error => error.Message).ToArray());
        ////							string messageFormat;
        ////							if (errors.Count > 1) {
        ////								messageFormat = context.TranslateString("Multiple:\n{0}");
        ////							} else {
        ////								messageFormat = context.TranslateString("{0}");
        ////							}
        ////							AddDiagnosticAnalyzer(new CodeIssue(segmentStart, segmentEnd, string.Format(messageFormat, errorMessage)));
        ////						}
        ////					} else if (segment.HasErrors) {
        ////						foreach (var error in errors) {
        ////							var errorStart = new TextLocation(formatStart.Line, formatStart.Column + error.StartLocation + 1);
        ////							var errorEnd = new TextLocation(formatStart.Line, formatStart.Column + error.EndLocation + 1);
        ////							AddDiagnosticAnalyzer(new CodeIssue(errorStart, errorEnd, error.Message));
        ////						}
        ////					}
        ////				}
        ////			}
        //		}
    }
}