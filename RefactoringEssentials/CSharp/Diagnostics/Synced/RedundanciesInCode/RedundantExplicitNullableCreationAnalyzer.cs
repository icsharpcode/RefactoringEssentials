using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace RefactoringEssentials.CSharp.Diagnostics
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    [NotPortedYet]
    public class RedundantExplicitNullableCreationAnalyzer : DiagnosticAnalyzer
    {
        static readonly DiagnosticDescriptor descriptor = new DiagnosticDescriptor(
            CSharpDiagnosticIDs.RedundantExplicitNullableCreationAnalyzerID,
            GettextCatalog.GetString("Value types are implicitly convertible to nullables"),
            GettextCatalog.GetString("Redundant explicit nullable type creation"),
            DiagnosticAnalyzerCategories.RedundanciesInCode,
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            helpLinkUri: HelpLink.CreateFor(CSharpDiagnosticIDs.RedundantExplicitNullableCreationAnalyzerID),
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

        //		class GatherVisitor : GatherVisitorBase<RedundantExplicitNullableCreationAnalyzer>
        //		{
        //			public GatherVisitor(SemanticModel semanticModel, Action<Diagnostic> addDiagnostic, CancellationToken cancellationToken)
        //				: base (semanticModel, addDiagnostic, cancellationToken)
        //			{
        //			}

        ////			public override void VisitObjectCreateExpression(ObjectCreateExpression objectCreateExpression)
        ////			{
        ////				base.VisitObjectCreateExpression(objectCreateExpression);
        ////
        ////				// test for var foo = new ... 
        ////				var parentVarDecl = objectCreateExpression.Parent.Parent as VariableDeclarationStatement;
        ////				if (parentVarDecl != null && parentVarDecl.Type.IsVar())
        ////					return;
        ////
        ////				var rr = ctx.Resolve(objectCreateExpression);
        ////				if (!NullableType.IsNullable(rr.Type))
        ////					return;
        ////				var arg = objectCreateExpression.Arguments.FirstOrDefault();
        ////				if (arg == null)
        ////					return;
        ////				AddDiagnosticAnalyzer(new CodeIssue(
        ////					objectCreateExpression.StartLocation,
        ////					objectCreateExpression.Type.EndLocation,
        ////					ctx.TranslateString(""),
        ////					ctx.TranslateString(""),
        ////					s => s.Replace(objectCreateExpression, arg.Clone())
        ////				) { IssueMarker = IssueMarker.GrayOut });
        ////			}
        //		}
    }
}