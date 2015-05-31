using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace RefactoringEssentials.CSharp.Diagnostics
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    [NotPortedYet]
    public class StaticFieldInGenericTypeAnalyzer : DiagnosticAnalyzer
    {
        static readonly DiagnosticDescriptor descriptor = new DiagnosticDescriptor(
            CSharpDiagnosticIDs.StaticFieldInGenericTypeAnalyzerID,
            GettextCatalog.GetString("Warns about static fields in generic types"),
            GettextCatalog.GetString("Static field in generic type"),
            DiagnosticAnalyzerCategories.CodeQualityIssues,
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            helpLinkUri: HelpLink.CreateFor(CSharpDiagnosticIDs.StaticFieldInGenericTypeAnalyzerID)
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

        //		class GatherVisitor : GatherVisitorBase<StaticFieldInGenericTypeAnalyzer>
        //		{
        //			public GatherVisitor(SemanticModel semanticModel, Action<Diagnostic> addDiagnostic, CancellationToken cancellationToken)
        //				: base (semanticModel, addDiagnostic, cancellationToken)
        //			{
        //			}

        ////			IList<ITypeParameter> availableTypeParameters = new List<ITypeParameter>();
        ////
        ////			public override void VisitTypeDeclaration(TypeDeclaration typeDeclaration)
        ////			{
        ////				var typeResolveResult = ctx.Resolve(typeDeclaration);
        ////				var typeDefinition = typeResolveResult.Type.GetDefinition();
        ////				if (typeDefinition == null)
        ////					return;
        ////				var newTypeParameters = typeDefinition.TypeParameters;
        ////
        ////				var oldTypeParameters = availableTypeParameters; 
        ////				availableTypeParameters = Concat(availableTypeParameters, newTypeParameters);
        ////
        ////				base.VisitTypeDeclaration(typeDeclaration);
        ////
        ////				availableTypeParameters = oldTypeParameters;
        ////			}
        ////
        ////			static IList<ITypeParameter> Concat(params IList<ITypeParameter>[] lists)
        ////			{
        ////				return lists.SelectMany(l => l).ToList();
        ////			}
        ////
        ////			bool UsesAllTypeParameters(FieldDeclaration fieldDeclaration)
        ////			{
        ////				if (availableTypeParameters.Count == 0)
        ////					return true;
        ////
        ////				var fieldType = ctx.Resolve(fieldDeclaration.ReturnType).Type as ParameterizedType;
        ////				if (fieldType == null)
        ////					return false;
        ////
        ////				// Check that all current type parameters are used in the field type
        ////				var fieldTypeParameters = fieldType.TypeArguments;
        ////				foreach (var typeParameter in availableTypeParameters) {
        ////					if (!fieldTypeParameters.Contains(typeParameter))
        ////						return false;
        ////				}
        ////				return true;
        ////			}
        ////
        ////			public override void VisitFieldDeclaration(FieldDeclaration fieldDeclaration)
        ////			{
        ////				base.VisitFieldDeclaration(fieldDeclaration);
        ////				if (fieldDeclaration.Modifiers.HasFlag(Modifiers.Static) && !UsesAllTypeParameters(fieldDeclaration)) {
        //			//					AddDiagnosticAnalyzer(new CodeIssue(fieldDeclaration, ctx.TranslateString("Static field in generic type")));
        ////				}
        ////			}
        //		}
    }
}