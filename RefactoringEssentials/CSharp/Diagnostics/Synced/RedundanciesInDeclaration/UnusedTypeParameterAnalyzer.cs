using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace RefactoringEssentials.CSharp.Diagnostics
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    [NotPortedYet]
    public class UnusedTypeParameterAnalyzer : DiagnosticAnalyzer
    {
        static readonly DiagnosticDescriptor descriptor = new DiagnosticDescriptor(
            CSharpDiagnosticIDs.UnusedTypeParameterAnalyzerID,
            GettextCatalog.GetString("Type parameter is never used"),
            GettextCatalog.GetString("Type parameter '{0}' is never used"),
            DiagnosticAnalyzerCategories.RedundanciesInDeclarations,
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            helpLinkUri: HelpLink.CreateFor(CSharpDiagnosticIDs.UnusedTypeParameterAnalyzerID),
            customTags: DiagnosticCustomTags.Unnecessary
        );

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(descriptor);

        //static FindReferences refFinder = new FindReferences();

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

        //		protected static bool FindUsage(BaseSemanticModel context, SyntaxTree unit,
        //		                                 ITypeParameter typeParameter, AstNode declaration)
        //		{
        //			var found = false;
        //			var searchScopes = refFinder.GetSearchScopes(typeParameter);
        //			refFinder.FindReferencesInFile(searchScopes, context.Resolver,
        //				(node, resolveResult) => {
        //					if (node != declaration)
        //						found = true;
        //				}, context.CancellationToken);
        //			return found;
        //		}
        //
        //		class GatherVisitor : GatherVisitorBase<UnusedTypeParameterAnalyzer>
        //		{
        //			//SyntaxTree unit;

        //			public GatherVisitor(SemanticModel semanticModel, Action<Diagnostic> addDiagnostic, CancellationToken cancellationToken)
        //				: base (semanticModel, addDiagnostic, cancellationToken)
        //			{
        //			}

        ////			public override void VisitTypeParameterDeclaration(TypeParameterDeclaration decl)
        ////			{
        ////				base.VisitTypeParameterDeclaration(decl);
        ////
        ////				var resolveResult = ctx.Resolve(decl) as TypeResolveResult;
        ////				if (resolveResult == null)
        ////					return;
        ////				var typeParameter = resolveResult.Type as ITypeParameter;
        ////				if (typeParameter == null)
        ////					return;
        ////				var methodDecl = decl.Parent as MethodDeclaration;
        ////				if (methodDecl == null)
        ////					return;
        ////
        ////				if (FindUsage(ctx, unit, typeParameter, decl))
        ////					return;
        ////
        //			//				AddDiagnosticAnalyzer(new CodeIssue(decl.NameToken, ctx.TranslateString("Type parameter is never used")) { IssueMarker = IssueMarker.GrayOut });
        ////			}
        //		}
    }

}
