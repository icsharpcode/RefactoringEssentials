using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace RefactoringEssentials.CSharp.Diagnostics
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    [NotPortedYet]
    public class PartialMethodParameterNameMismatchAnalyzer : DiagnosticAnalyzer
    {
        static readonly DiagnosticDescriptor descriptor = new DiagnosticDescriptor(
            CSharpDiagnosticIDs.PartialMethodParameterNameMismatchAnalyzerID,
            GettextCatalog.GetString("Parameter name differs in partial method declaration"),
            GettextCatalog.GetString("Parameter name differs in partial method declaration"),
            DiagnosticAnalyzerCategories.CodeQualityIssues,
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            helpLinkUri: HelpLink.CreateFor(CSharpDiagnosticIDs.PartialMethodParameterNameMismatchAnalyzerID)
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

        //		class GatherVisitor : GatherVisitorBase<PartialMethodParameterNameMismatchAnalyzer>
        //		{
        //			public GatherVisitor(SemanticModel semanticModel, Action<Diagnostic> addDiagnostic, CancellationToken cancellationToken)
        //				: base (semanticModel, addDiagnostic, cancellationToken)
        //			{
        //			}

        ////			public override void VisitConstructorDeclaration(ConstructorDeclaration constructorDeclaration)
        ////			{
        ////				// skip
        ////			}
        ////
        ////			public override void VisitDestructorDeclaration(DestructorDeclaration destructorDeclaration)
        ////			{
        ////				// skip
        ////			}
        ////
        ////			public override void VisitOperatorDeclaration(OperatorDeclaration operatorDeclaration)
        ////			{
        ////				// skip
        ////			}
        ////
        ////			public override void VisitPropertyDeclaration(PropertyDeclaration propertyDeclaration)
        ////			{
        ////				// skip
        ////			}
        ////
        ////			public override void VisitFieldDeclaration(FieldDeclaration fieldDeclaration)
        ////			{
        ////				// skip
        ////			}
        ////		
        ////			public override void VisitBlockStatement(BlockStatement blockStatement)
        ////			{
        ////				// SKIP
        ////			}
        ////
        ////			public override void VisitMethodDeclaration(MethodDeclaration methodDeclaration)
        ////			{
        ////				if (!methodDeclaration.HasModifier(Modifiers.Partial))
        ////					return;
        ////				var rr = ctx.Resolve(methodDeclaration) as MemberResolveResult;
        ////				if (rr == null || rr.IsError)
        ////					return;
        ////				var method = rr.Member as IMethod;
        ////				if (method == null)
        ////					return;
        ////
        ////				int arg = 0;
        ////				foreach (var param in methodDeclaration.Parameters) {
        ////					var pr = ctx.Resolve(param) as LocalResolveResult;
        ////					if (pr == null)
        ////						continue;
        ////					foreach (var part in method.Parts) {
        ////						if (param.Name != part.Parameters[arg].Name) {
        ////							List<CodeAction> fixes = new List<CodeAction>();
        ////							foreach (var p2 in method.Parts) {
        ////								if (param.Name != p2.Parameters[arg].Name) {
        ////									int _arg = arg;
        ////									fixes.Add(new CodeAction (
        ////										string.Format(ctx.TranslateString("Rename to '{0}'"), p2.Parameters[_arg].Name),
        ////										s => {
        ////											s.Rename(pr.Variable, p2.Parameters[_arg].Name);
        ////										},
        ////										param
        ////									)); 
        ////								}
        ////							}
        ////							// TODO: Atm I think it makes only sense to offer a fix if the issue disappears
        ////							// which might not be the case here.
        ////							if (fixes.Count > 1) {
        ////								fixes.Clear();
        ////							}
        ////							AddDiagnosticAnalyzer(new CodeIssue(
        ////								param.NameToken,
        ////								ctx.TranslateString("Parameter name differs in partial method declaration"),
        ////								fixes
        ////							));
        ////							break;
        ////						}
        ////					}
        ////					arg++;
        ////				}
        ////			}
        //		}
    }
}