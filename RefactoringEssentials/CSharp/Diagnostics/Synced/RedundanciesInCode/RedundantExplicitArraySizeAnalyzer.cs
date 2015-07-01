using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System.Linq;

namespace RefactoringEssentials.CSharp.Diagnostics
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class RedundantExplicitArraySizeAnalyzer : DiagnosticAnalyzer
    {
        private static readonly DiagnosticDescriptor descriptor = new DiagnosticDescriptor(
            CSharpDiagnosticIDs.RedundantExplicitArraySizeAnalyzerID,
            GettextCatalog.GetString("Redundant explicit size in array creation"),
            GettextCatalog.GetString("Redundant explicit size in array creation"),
            DiagnosticAnalyzerCategories.RedundanciesInCode,
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            helpLinkUri: HelpLink.CreateFor(CSharpDiagnosticIDs.RedundantExplicitArraySizeAnalyzerID),
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
                 SyntaxKind.ArrayCreationExpression 
            );
        }

        private static bool TryGetDiagnostic(SyntaxNodeAnalysisContext nodeContext, out Diagnostic diagnostic)
        {
            diagnostic = default(Diagnostic);
            if (nodeContext.IsFromGeneratedCode())
                return false;
            var node = nodeContext.Node as ArrayCreationExpressionSyntax;
            var arrayType = node?.ChildNodes().OfType<ArrayTypeSyntax>().FirstOrDefault();
            if (arrayType == null || !arrayType.RankSpecifiers.Any())
                return false;

            ////				var value = (int)arg.Value;
            //Looking for how to get int value inside the brackets.
            if (node.Initializer.Expressions.Count == arrayType.RankSpecifiers.FirstOrDefault().Sizes.Count)
            {
                diagnostic = Diagnostic.Create(descriptor, node.GetLocation());
                return true;
            }

            return false;

        }

        //		class GatherVisitor : GatherVisitorBase<RedundantExplicitArraySizeAnalyzer>
        //		{
        //			public GatherVisitor(SemanticModel semanticModel, Action<Diagnostic> addDiagnostic, CancellationToken cancellationToken)
        //				: base (semanticModel, addDiagnostic, cancellationToken)
        //			{
        //			}

        ////			public override void VisitArrayCreateExpression(ArrayCreateExpression arrayCreateExpression)
        ////			{
        ////				base.VisitArrayCreateExpression(arrayCreateExpression);
        ////				if (arrayCreateExpression.Arguments.Count != 1)
        ////					return;
        ////				var arg = arrayCreateExpression.Arguments.Single() as PrimitiveExpression;
        ////				if (arg == null || !(arg.Value is int))
        ////					return;
        ////				var value = (int)arg.Value;
        ////				if (value == 0)
        ////					return;
        ////				if (arrayCreateExpression.Initializer.Elements.Count() == value) {
        ////					AddDiagnosticAnalyzer(new CodeIssue(
        ////						arg,
        ////						ctx.TranslateString(""),
        ////						string.Format(ctx.TranslateString(""), arg),
        ////						s => { s.Remove(arg); }
        ////					) { IssueMarker = IssueMarker.GrayOut });
        ////				}
        ////			}
        //		}
    }
}