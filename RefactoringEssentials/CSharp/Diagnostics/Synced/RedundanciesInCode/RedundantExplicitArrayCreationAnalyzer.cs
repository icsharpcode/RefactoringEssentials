using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace RefactoringEssentials.CSharp.Diagnostics
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    [NotPortedYet]
    public class RedundantExplicitArrayCreationAnalyzer : DiagnosticAnalyzer
    {
        static readonly DiagnosticDescriptor descriptor = new DiagnosticDescriptor(
            CSharpDiagnosticIDs.RedundantExplicitArrayCreationAnalyzerID,
            GettextCatalog.GetString("Redundant explicit type in array creation"),
            GettextCatalog.GetString("Redundant explicit array type specification"),
            DiagnosticAnalyzerCategories.RedundanciesInCode,
            DiagnosticSeverity.Hidden,
            isEnabledByDefault: true,
            helpLinkUri: HelpLink.CreateFor(CSharpDiagnosticIDs.RedundantExplicitArrayCreationAnalyzerID),
            customTags: DiagnosticCustomTags.Unnecessary
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

        //		class GatherVisitor : GatherVisitorBase<RedundantExplicitArrayCreationAnalyzer>
        //		{
        //			public GatherVisitor(SemanticModel semanticModel, Action<Diagnostic> addDiagnostic, CancellationToken cancellationToken)
        //				: base (semanticModel, addDiagnostic, cancellationToken)
        //			{
        //			}

        ////			public override void VisitSyntaxTree(SyntaxTree syntaxTree)
        ////			{
        ////				if (!ctx.Supports(new Version(3, 0)))
        ////					return;
        ////				base.VisitSyntaxTree(syntaxTree);
        ////			}
        ////
        ////			public override void VisitArrayCreateExpression(ArrayCreateExpression arrayCreateExpression)
        ////			{
        ////				base.VisitArrayCreateExpression(arrayCreateExpression);
        ////				if (arrayCreateExpression.Arguments.Count != 0)
        ////					return;
        ////				var arrayType = arrayCreateExpression.Type;
        ////				if (arrayType.IsNull)
        ////					return;
        ////				var arrayTypeRR = ctx.Resolve(arrayType);
        ////				if (arrayTypeRR.IsError)
        ////					return;
        ////
        ////				IType elementType = null;
        ////				foreach (var element in arrayCreateExpression.Initializer.Elements) {
        ////					var elementTypeRR = ctx.Resolve(element);
        ////					if (elementTypeRR.IsError)
        ////						return;
        ////					if (elementType == null) {
        ////						elementType = elementTypeRR.Type;
        ////						continue;
        ////					}
        ////					if (elementType != elementTypeRR.Type)
        ////						return;
        ////				}
        ////				if (elementType != arrayTypeRR.Type)
        ////					return;
        ////
        ////				AddDiagnosticAnalyzer(
        ////					new CodeIssue (
        ////						arrayType,
        ////						s => s.Remove(arrayType) 
        ////					) { IssueMarker = IssueMarker.GrayOut }
        ////				);
        ////			}
        //		}
    }
}