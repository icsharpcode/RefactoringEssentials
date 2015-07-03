using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace RefactoringEssentials.CSharp.Diagnostics
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
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
            context.RegisterSyntaxNodeAction(
                (nodeContext) =>
                {
                    Diagnostic diagnostic;
                    if (TryGetDiagnostic(nodeContext, out diagnostic))
                    {
                        nodeContext.ReportDiagnostic(diagnostic);
                    }
                },
                SyntaxKind.ObjectCreationExpression
            );
        }

        static bool TryGetDiagnostic(SyntaxNodeAnalysisContext nodeContext, out Diagnostic diagnostic)
        {
            diagnostic = default(Diagnostic);
            if (nodeContext.IsFromGeneratedCode())
                return false;

            var objectCreation = nodeContext.Node as ObjectCreationExpressionSyntax;
            if (objectCreation == null)
                return false;

            var objectCreationSymbol = nodeContext.SemanticModel.GetDeclaredSymbol(objectCreation);
            if (objectCreationSymbol == null)
                return false;

            int? i = new int?(5);
            int? a = new System.Nullable<int>(5);


            //Not so sure about this check but was there before
            var parentVarDeclaration = objectCreation.Parent.Parent as VariableDeclarationSyntax;
            if (parentVarDeclaration != null && parentVarDeclaration.Type.IsVar)
                return false;

            if (objectCreationSymbol.OriginalDefinition.ToString().Equals("System.Nullable<T>.Nullable(T)"))
            {
                var qualifiedName = objectCreation.ChildNodes().OfType<QualifiedNameSyntax>().FirstOrDefault();
                if (qualifiedName != null)
                {
                    diagnostic = Diagnostic.Create(descriptor, qualifiedName.GetLocation());
                    return true;
                }

                var nullableType = objectCreation.Type as NullableTypeSyntax;
                if (nullableType != null)
                {
                    diagnostic = Diagnostic.Create(descriptor, nullableType.GetLocation());
                    return true;
                }
                return false;
            }
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