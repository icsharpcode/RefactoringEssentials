using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using RefactoringEssentials.Util.CompositieFormatStringParser;

namespace RefactoringEssentials.CSharp.Diagnostics
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class FormatStringProblemAnalyzer : DiagnosticAnalyzer
    {
        static readonly DiagnosticDescriptor descriptor = new DiagnosticDescriptor(
            CSharpDiagnosticIDs.FormatStringProblemAnalyzerID,
            GettextCatalog.GetString("Finds issues with format strings"),
            "{0}",
            DiagnosticAnalyzerCategories.CodeQualityIssues,
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            helpLinkUri: HelpLink.CreateFor(CSharpDiagnosticIDs.FormatStringProblemAnalyzerID)
        );

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(descriptor);

        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(
            	AnalyzeInvocation, 
                new SyntaxKind[] { SyntaxKind.InvocationExpression }
            );
        }

        void AnalyzeInvocation(SyntaxNodeAnalysisContext ctx)
        {
            var invocationExpression = (InvocationExpressionSyntax)ctx.Node;

            // Speed up the inspector by discarding some invocations early
            var hasEligibleArgument = invocationExpression.ArgumentList.Arguments.Any(argument => {
                var primitiveArg = argument.Expression as LiteralExpressionSyntax;
                return primitiveArg != null && primitiveArg.Token.IsKind(SyntaxKind.StringLiteralToken);
            });
            if (!hasEligibleArgument)
                return;

            var invocationResolveResult = ctx.SemanticModel.GetSymbolInfo(invocationExpression);
            if (invocationResolveResult.Symbol == null)
                return;
            ExpressionSyntax formatArgument;
            IList<ExpressionSyntax> formatArguments;
            if (!FormatStringHelper.TryGetFormattingParameters(ctx.SemanticModel, invocationExpression,
                                                               out formatArgument, out formatArguments, null, ctx.CancellationToken)) {
                return;
            }
            var primitiveArgument = formatArgument as LiteralExpressionSyntax;
            if (primitiveArgument == null || !primitiveArgument.Token.IsKind(SyntaxKind.StringLiteralToken))
                return;
            var format = primitiveArgument.Token.ValueText;
            var parsingResult = new CompositeFormatStringParser().Parse(format);
            CheckSegments(ctx, parsingResult.Segments, formatArgument.SpanStart + 1, formatArguments, invocationExpression);

            var argUsed = new HashSet<int> ();

            foreach (var item in parsingResult.Segments) {
                var fi = item as FormatItem;
                if (fi == null)
                    continue;
                argUsed.Add(fi.Index);
            }
            for (int i = 0; i < formatArguments.Count; i++) {
                if (!argUsed.Contains(i)) {
                    ctx.ReportDiagnostic(Diagnostic.Create(
                        descriptor, 
                        formatArguments[i].GetLocation(),
                        GettextCatalog.GetString("Argument is not used in format string")
                    ));
                }
            }
        }

        static void CheckSegments(SyntaxNodeAnalysisContext ctx, IList<IFormatStringSegment> segments, int formatStart, IList<ExpressionSyntax> formatArguments, SyntaxNode anchor)
        {
            int argumentCount = formatArguments.Count;
            foreach (var segment in segments) {
                var errors = segment.Errors.ToList();
                var formatItem = segment as FormatItem;
                if (formatItem != null) {
                    var location = Location.Create(ctx.SemanticModel.SyntaxTree, new Microsoft.CodeAnalysis.Text.TextSpan(formatStart + segment.StartLocation, segment.EndLocation - segment.StartLocation));

                    if (formatItem.Index >= argumentCount) {
                        ctx.ReportDiagnostic(Diagnostic.Create(
                            descriptor, 
                            location,
                            string.Format(GettextCatalog.GetString("The index '{0}' is out of bounds of the passed arguments"), formatItem.Index)
                        ));
                    }

                    if (formatItem.HasErrors) {
                        var errorMessage = string.Join(Environment.NewLine, errors.Select(error => error.Message).ToArray());
                        string messageFormat;
                        if (errors.Count > 1) {
                            messageFormat = GettextCatalog.GetString("Multiple:\n{0}");
                        } else {
                            messageFormat = "{0}";
                        }
                        ctx.ReportDiagnostic(Diagnostic.Create(
                            descriptor, 
                            location,
                            string.Format(messageFormat, errorMessage)
                        ));
                    }
                } else if (segment.HasErrors) {
                    foreach (var error in errors) {
                        ctx.ReportDiagnostic(Diagnostic.Create(
                            descriptor, 
                            Location.Create(ctx.SemanticModel.SyntaxTree, new Microsoft.CodeAnalysis.Text.TextSpan(formatStart + error.StartLocation, error.EndLocation - error.StartLocation)),
                            error.Message
                        ));
                    }
                }
            }
        }
    }
}