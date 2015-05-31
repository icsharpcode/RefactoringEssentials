using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace RefactoringEssentials.CSharp.Diagnostics
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    [NotPortedYet]
    public class MethodOverloadWithOptionalParameterAnalyzer : DiagnosticAnalyzer
    {
        static readonly DiagnosticDescriptor descriptor = new DiagnosticDescriptor(
            CSharpDiagnosticIDs.MethodOverloadWithOptionalParameterAnalyzerID,
            GettextCatalog.GetString("Method with optional parameter is hidden by overload"),
            GettextCatalog.GetString("{0} with optional parameter is hidden by overload"),  // Method/Indexer
            DiagnosticAnalyzerCategories.CodeQualityIssues,
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            helpLinkUri: HelpLink.CreateFor(CSharpDiagnosticIDs.MethodOverloadWithOptionalParameterAnalyzerID)
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

        //		class GatherVisitor : GatherVisitorBase<MethodOverloadWithOptionalParameterAnalyzer>
        //		{
        //			public GatherVisitor(SemanticModel semanticModel, Action<Diagnostic> addDiagnostic, CancellationToken cancellationToken)
        //				: base (semanticModel, addDiagnostic, cancellationToken)
        //			{
        //			}
        ////
        ////			void CheckParameters(IParameterizedMember member,  List<IParameterizedMember> overloads, List<ParameterDeclaration> parameterDeclarations)
        ////			{
        ////				for (int i = 0; i < member.Parameters.Count; i++) {
        ////					if (!member.Parameters[i].IsOptional)
        ////						continue;
        ////
        ////					foreach (var overload in overloads) {
        ////						if (overload.Parameters.Count != i)
        ////							continue;
        ////						bool equal = true;
        ////						for (int j = 0; j < i; j++)  {
        ////							if (overload.Parameters[j].Type != member.Parameters[j].Type) {
        ////								equal = false;
        ////								break;
        ////							}
        ////						}
        ////						if (equal) {
        ////							AddDiagnosticAnalyzer(new CodeIssue(
        ////								parameterDeclarations[i],
        ////								member.SymbolKind == SymbolKind.Method ?
        //			//								ctx.TranslateString("Method with optional parameter is hidden by overload") :
        //			//								ctx.TranslateString("Indexer with optional parameter is hidden by overload")));
        ////						}
        ////					}
        ////				}
        ////			}
        ////
        ////			public override void VisitMethodDeclaration(MethodDeclaration methodDeclaration)
        ////			{
        ////				var rr = ctx.Resolve(methodDeclaration) as MemberResolveResult;
        ////				if (rr == null || rr.IsError)
        ////					return;
        ////				var method = rr.Member as IMethod;
        ////				if (method == null)
        ////					return;
        ////				CheckParameters (method, 
        ////					method.DeclaringType.GetMethods(m =>
        ////						m.Name == method.Name && m.TypeParameters.Count == method.TypeParameters.Count).Cast<IParameterizedMember>().ToList(),
        ////					methodDeclaration.Parameters.ToList()
        ////				);
        ////
        ////			}
        ////
        ////			public override void VisitIndexerDeclaration(IndexerDeclaration indexerDeclaration)
        ////			{
        ////				var rr = ctx.Resolve(indexerDeclaration) as MemberResolveResult;
        ////				if (rr == null || rr.IsError)
        ////					return;
        ////				var method = rr.Member as IProperty;
        ////				if (method == null)
        ////					return;
        ////				CheckParameters (method, 
        ////					method.DeclaringType.GetProperties(m =>
        ////						m.IsIndexer &&
        ////						m != method.UnresolvedMember).Cast<IParameterizedMember>().ToList(),
        ////					indexerDeclaration.Parameters.ToList()
        ////				);
        ////			}
        ////
        ////
        ////			public override void VisitBlockStatement(BlockStatement blockStatement)
        ////			{
        ////				// SKIP
        ////			}
        //		}
    }
}