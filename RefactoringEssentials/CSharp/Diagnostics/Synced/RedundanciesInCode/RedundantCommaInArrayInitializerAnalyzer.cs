using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace RefactoringEssentials.CSharp.Diagnostics
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class RedundantCommaInArrayInitializerAnalyzer : DiagnosticAnalyzer
    {
        private static readonly DiagnosticDescriptor descriptor = new DiagnosticDescriptor(
            CSharpDiagnosticIDs.RedundantCommaInArrayInitializerAnalyzerID,
            GettextCatalog.GetString("Redundant comma in array initializer"),
            GettextCatalog.GetString("Redundant comma in {0}"),
            DiagnosticAnalyzerCategories.RedundanciesInCode,
            DiagnosticSeverity.Info,
            isEnabledByDefault: true,
            helpLinkUri: HelpLink.CreateFor(CSharpDiagnosticIDs.RedundantCommaInArrayInitializerAnalyzerID),
            customTags: DiagnosticCustomTags.Unnecessary
        );

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(descriptor);

        //			"Redundant comma in object initializer"
        //			"Redundant comma in collection initializer"
        //			"Redundant comma in array initializer"

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
                 SyntaxKind.ArrayInitializerExpression
            );
        }

        private static bool TryGetDiagnostic(SyntaxNodeAnalysisContext nodeContext, out Diagnostic diagnostic)
        {
            diagnostic = default(Diagnostic);
            if (nodeContext.IsFromGeneratedCode())
                return false;

            var node = nodeContext.Node as InitializerExpressionSyntax;
            if (node == null || (node != null && node.Expressions.Count <= 1))
                return false;

            var commaCount = node.Expressions.Count;
            var commaToken = node.ChildTokens().ElementAt(commaCount-1);
            if (!commaToken.IsKind(SyntaxKind.CommaToken))
                return false;


            diagnostic = Diagnostic.Create(descriptor, node.GetLocation());
            return true;
        }

        //		class GatherVisitor : GatherVisitorBase<RedundantCommaInArrayInitializerAnalyzer>
        //		{
        //			public GatherVisitor(SemanticModel semanticModel, Action<Diagnostic> addDiagnostic, CancellationToken cancellationToken)
        //				: base (semanticModel, addDiagnostic, cancellationToken)
        //			{
        //			}

        ////			public override void VisitArrayInitializerExpression(ArrayInitializerExpression arrayInitializerExpression)
        ////			{
        ////				base.VisitArrayInitializerExpression(arrayInitializerExpression);
        ////
        ////				if (arrayInitializerExpression.IsSingleElement)
        ////					return;
        ////
        ////				var commaToken = arrayInitializerExpression.RBraceToken.PrevSibling as CSharpTokenNode;
        ////				if (commaToken == null || commaToken.ToString() != ",")
        ////					return;
        ////				string issueDescription;
        ////				if (arrayInitializerExpression.Parent is ObjectCreateExpression) {
        ////					if (arrayInitializerExpression.Elements.FirstOrNullObject() is NamedExpression) {
        ////						issueDescription = ctx.TranslateString("");
        ////					} else {
        ////						issueDescription = ctx.TranslateString("");
        ////					}
        ////				} else {
        ////					issueDescription = ctx.TranslateString("");
        ////				}
        ////				AddDiagnosticAnalyzer(new CodeIssue(commaToken,
        ////				         issueDescription,
        ////				         ctx.TranslateString(""),
        ////					script => script.Remove(commaToken)) { IssueMarker = IssueMarker.GrayOut });
        ////			}
        //		}
    }
}