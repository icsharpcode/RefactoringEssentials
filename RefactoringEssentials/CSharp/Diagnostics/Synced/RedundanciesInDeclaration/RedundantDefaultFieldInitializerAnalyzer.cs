using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace RefactoringEssentials.CSharp.Diagnostics
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    [NotPortedYet]
    public class RedundantDefaultFieldInitializerAnalyzer : DiagnosticAnalyzer
    {
        static readonly DiagnosticDescriptor descriptor = new DiagnosticDescriptor(
            CSharpDiagnosticIDs.RedundantDefaultFieldInitializerAnalyzerID,
            GettextCatalog.GetString("Initializing field with default value is redundant"),
            GettextCatalog.GetString("Initializing field by default value is redundant"),
            DiagnosticAnalyzerCategories.RedundanciesInDeclarations,
            DiagnosticSeverity.Info,
            isEnabledByDefault: true,
            helpLinkUri: HelpLink.CreateFor(CSharpDiagnosticIDs.RedundantDefaultFieldInitializerAnalyzerID),
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

        //		class GatherVisitor : GatherVisitorBase<RedundantDefaultFieldInitializerAnalyzer>
        //		{
        //			public GatherVisitor(SemanticModel semanticModel, Action<Diagnostic> addDiagnostic, CancellationToken cancellationToken)
        //				: base (semanticModel, addDiagnostic, cancellationToken)
        //			{
        //			}

        ////			public override void VisitFieldDeclaration(FieldDeclaration fieldDeclaration)
        ////			{
        ////				base.VisitFieldDeclaration(fieldDeclaration);
        ////				if (fieldDeclaration.HasModifier(Modifiers.Const) || fieldDeclaration.HasModifier(Modifiers.Readonly))
        ////					return;
        ////				var defaultValueExpr = GetDefaultValueExpression(fieldDeclaration.ReturnType);
        ////				if (defaultValueExpr == null)
        ////					return;
        ////
        ////				foreach (var variable1 in fieldDeclaration.Variables) {
        ////					var variable = variable1;
        ////					if (!defaultValueExpr.Match(variable.Initializer).Success)
        ////						continue;
        ////
        ////					AddDiagnosticAnalyzer(new CodeIssue(variable.Initializer, ctx.TranslateString(""),
        ////					         new CodeAction(ctx.TranslateString(""),
        ////					                         script => script.Replace(variable, new VariableInitializer(variable.Name)),
        ////							variable.Initializer)) { IssueMarker = IssueMarker.GrayOut });
        ////				}
        ////			}
        ////
        ////			Expression GetDefaultValueExpression(AstType astType)
        ////			{
        ////				var type = ctx.ResolveType(astType);
        ////
        ////				if ((type.IsReferenceType ?? false) || type.Kind == TypeKind.Dynamic)
        ////					return new NullReferenceExpression();
        ////
        ////				var typeDefinition = type.GetDefinition();
        ////				if (typeDefinition != null) {
        ////					switch (typeDefinition.KnownTypeCode) {
        ////						case KnownTypeCode.Boolean:
        ////							return new PrimitiveExpression(false);
        ////
        ////						case KnownTypeCode.Char:
        ////							return new PrimitiveExpression('\0');
        ////
        ////						case KnownTypeCode.SByte:
        ////						case KnownTypeCode.Byte:
        ////						case KnownTypeCode.Int16:
        ////						case KnownTypeCode.UInt16:
        ////						case KnownTypeCode.Int32:
        ////							return new PrimitiveExpression(0);
        ////
        ////						case KnownTypeCode.Int64:
        ////							return new Choice { new PrimitiveExpression(0), new PrimitiveExpression(0L) };
        ////						case KnownTypeCode.UInt32:
        ////							return new Choice { new PrimitiveExpression(0), new PrimitiveExpression(0U) };
        ////						case KnownTypeCode.UInt64:
        ////							return new Choice {
        ////								new PrimitiveExpression(0), new PrimitiveExpression(0U), new PrimitiveExpression(0UL)
        ////							};
        ////						case KnownTypeCode.Single:
        ////							return new Choice { new PrimitiveExpression(0), new PrimitiveExpression(0F) };
        ////						case KnownTypeCode.Double:
        ////							return new Choice {
        ////								new PrimitiveExpression(0), new PrimitiveExpression(0F), new PrimitiveExpression(0D)
        ////							};
        ////						case KnownTypeCode.Decimal:
        ////							return new Choice { new PrimitiveExpression(0), new PrimitiveExpression(0M) };
        ////
        ////						case KnownTypeCode.NullableOfT:
        ////							return new NullReferenceExpression();
        ////					}
        ////					if (type.Kind == TypeKind.Struct)
        ////						return new ObjectCreateExpression(astType.Clone());
        ////				}
        ////				return new DefaultValueExpression(astType.Clone());
        ////			}
        //		}
    }


}