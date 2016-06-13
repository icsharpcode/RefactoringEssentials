using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace RefactoringEssentials.CSharp.Diagnostics
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    [NotPortedYet]
    public class CheckNamespaceAnalyzer : DiagnosticAnalyzer
    {
        static readonly DiagnosticDescriptor descriptor = new DiagnosticDescriptor(
            CSharpDiagnosticIDs.CheckNamespaceAnalyzerID,
            GettextCatalog.GetString("Check if a namespace corresponds to a file location"),
            "{0}",
            DiagnosticAnalyzerCategories.CodeQualityIssues,
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            helpLinkUri: HelpLink.CreateFor(CSharpDiagnosticIDs.CheckNamespaceAnalyzerID)
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

        //		class GatherVisitor : GatherVisitorBase<CheckNamespaceAnalyzer>
        //		{
        //			//readonly string defaultNamespace;

        //			public GatherVisitor(SemanticModel semanticModel, Action<Diagnostic> addDiagnostic, CancellationToken cancellationToken)
        //				: base(semanticModel, addDiagnostic, cancellationToken)
        //			{
        //			}

        ////			public override void VisitSyntaxTree(SyntaxTree syntaxTree)
        ////			{
        ////				if (string.IsNullOrEmpty(defaultNamespace))
        ////					return;
        ////				base.VisitSyntaxTree(syntaxTree);
        ////			}
        ////
        ////			public override void VisitNamespaceDeclaration(NamespaceDeclaration namespaceDeclaration)
        ////			{
        ////				base.VisitNamespaceDeclaration(namespaceDeclaration);
        ////				// only check top level namespaces
        ////				if (namespaceDeclaration.Parent is NamespaceDeclaration ||
        ////				    namespaceDeclaration.FullName == defaultNamespace)
        ////					return;
        ////				AddDiagnosticAnalyzer(new CodeIssue(
        ////					namespaceDeclaration.NamespaceName,
        ////					string.Format(ctx.TranslateString("Namespace does not correspond to file location, should be: '{0}'"), ctx.DefaultNamespace)
        ////				));
        ////			}
        ////
        ////			public override void VisitTypeDeclaration(TypeDeclaration typeDeclaration)
        ////			{
        ////				var ns = typeDeclaration.Parent as NamespaceDeclaration;
        ////				if (ns == null) {
        ////					AddDiagnosticAnalyzer(new CodeIssue(
        ////						typeDeclaration.NameToken,
        ////						string.Format(ctx.TranslateString("Type should be declared inside the namespace '{0}'"), ctx.DefaultNamespace)
        ////					));
        ////				}
        ////				// skip children
        ////			}
        //		}
    }
}