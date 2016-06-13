using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace RefactoringEssentials.CSharp.Diagnostics
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    [NotPortedYet]
    public class ConvertToAutoPropertyAnalyzer : DiagnosticAnalyzer
    {
        static readonly DiagnosticDescriptor descriptor = new DiagnosticDescriptor(
            CSharpDiagnosticIDs.ConvertToAutoPropertyAnalyzerID,
            GettextCatalog.GetString("Convert property to auto property"),
            GettextCatalog.GetString("Convert to auto property"),
            DiagnosticAnalyzerCategories.Opportunities,
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            helpLinkUri: HelpLink.CreateFor(CSharpDiagnosticIDs.ConvertToAutoPropertyAnalyzerID)
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

        //		class GatherVisitor : GatherVisitorBase<ConvertToAutoPropertyAnalyzer>
        //		{
        //			//readonly Stack<TypeDeclaration> typeStack = new Stack<TypeDeclaration>();

        //			public GatherVisitor(SemanticModel semanticModel, Action<Diagnostic> addDiagnostic, CancellationToken cancellationToken)
        //				: base (semanticModel, addDiagnostic, cancellationToken)
        //			{
        //			}

        ////			public override void VisitBlockStatement(BlockStatement blockStatement)
        ////			{
        ////				// SKIP
        ////			}
        ////
        ////			bool IsValidField(IField field)
        ////			{
        ////				if (field == null || field.Attributes.Count > 0 || field.IsVolatile)
        ////					return false;
        ////				foreach (var m in typeStack.Peek().Members.OfType<FieldDeclaration>()) {
        ////					foreach (var i in m.Variables) {
        ////						if (i.StartLocation == field.BodyRegion.Begin) {
        ////							if (!i.Initializer.IsNull)
        ////								return false;
        ////							break;
        ////						}
        ////					}
        ////				}
        ////				return true;
        ////			}
        ////
        ////			public override void VisitPropertyDeclaration(PropertyDeclaration propertyDeclaration)
        ////			{
        ////				var field = RemoveBackingStoreAction.GetBackingField(ctx, propertyDeclaration);
        ////				if (!IsValidField(field))
        ////					return;
        ////				AddDiagnosticAnalyzer(new CodeIssue(
        ////					propertyDeclaration.NameToken,
        ////					ctx.TranslateString("Convert to auto property")
        ////				) {
        ////					ActionProvider = { typeof (RemoveBackingStoreAction) }
        ////				}
        ////				);
        ////			}
        ////
        ////			public override void VisitTypeDeclaration(TypeDeclaration typeDeclaration)
        ////			{
        ////				typeStack.Push(typeDeclaration); 
        ////				base.VisitTypeDeclaration(typeDeclaration);
        ////				typeStack.Pop();
        ////			}
        //		}
    }


}