using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace RefactoringEssentials.CSharp.Diagnostics
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    [NotPortedYet]
    public class RedundantParamsAnalyzer : DiagnosticAnalyzer
    {
        static readonly DiagnosticDescriptor descriptor = new DiagnosticDescriptor(
            CSharpDiagnosticIDs.RedundantParamsAnalyzerID,
            GettextCatalog.GetString("'params' is ignored on overrides"),
            GettextCatalog.GetString("'params' is always ignored in overrides"),
            DiagnosticAnalyzerCategories.RedundanciesInDeclarations,
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            helpLinkUri: HelpLink.CreateFor(CSharpDiagnosticIDs.RedundantParamsAnalyzerID),
            customTags: DiagnosticCustomTags.Unnecessary
        );

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(descriptor);

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(
                (nodeContext) =>
                {
                    Diagnostic diagnostic;
                    if (TryGetParamsDiagnostic(nodeContext, out diagnostic))
                    {
                        nodeContext.ReportDiagnostic(diagnostic);
                    }
                },
                SyntaxKind.ParameterList 
            );
        }
        //I think it's a better decision to head in this direction instead of MethodDeclaration.
        static bool TryGetParamsDiagnostic(SyntaxNodeAnalysisContext nodeContext, out Diagnostic diagnostic)
        {
            diagnostic = default(Diagnostic);
            if (nodeContext.IsFromGeneratedCode())
                return false;

            var paramList = nodeContext.Node as ParameterListSyntax;
            var declaration = paramList?.Parent as MethodDeclarationSyntax;

            if (declaration == null)
                return false;

            if (declaration.Modifiers.Count == 0 || !declaration.Modifiers.Any(SyntaxKind.OverrideKeyword))
                return false;

            var lastParam = declaration.ParameterList.Parameters.LastOrDefault();
            SyntaxToken? paramsModifierToken = null;
            if (lastParam == null)
                return false;

            foreach (var x in lastParam.Modifiers)
            {
                if (x.IsKind(SyntaxKind.ParamsKeyword))
                {
                    paramsModifierToken = x;
                    break;
                }
            }

            if (!paramsModifierToken.HasValue ||
                !paramsModifierToken.Value.IsKind(SyntaxKind.ParamsKeyword))
                return false;



            diagnostic = Diagnostic.Create(descriptor, lastParam.GetLocation());
            return true;
        }


        static bool TryGetDiagnostic(SyntaxNodeAnalysisContext nodeContext, out Diagnostic diagnostic)
        {
            diagnostic = default(Diagnostic);
            if (nodeContext.IsFromGeneratedCode())
                return false;
            var methodDeclaration = nodeContext.Node as MethodDeclarationSyntax;
            if (methodDeclaration == null)
                return false;

            if (  methodDeclaration.Modifiers.Count==0|| !methodDeclaration.Modifiers.Any(SyntaxKind.OverrideKeyword))
            {
                return false;
            }

            var lastParam = methodDeclaration.ParameterList.Parameters.LastOrDefault();
            if (lastParam == null || !lastParam.Modifiers.ElementAt(0).IsKind(SyntaxKind.ParamsKeyword))
                return false;

            var type = lastParam.Type; //What is a ComposedType?
            if (type == null)
                return false;
            ////				var type = lastParam.Type as ComposedType;
            ////				if (type == null || !type.ArraySpecifiers.Any())
            ////					return;

            var methodSymbol = nodeContext.SemanticModel.GetDeclaredSymbol(methodDeclaration);
            if (methodSymbol == null || methodSymbol.Parameters.Length == 0 || methodSymbol.Parameters.LastOrDefault().IsParams)
                return false;

            // Do I need to look for BlockSyntax ? If yes, What ? 

            diagnostic = Diagnostic.Create(descriptor, methodDeclaration.GetLocation());
            return true;
        }

        //		class GatherVisitor : GatherVisitorBase<RedundantParamsAnalyzer>
        //		{
        //			public GatherVisitor(SemanticModel semanticModel, Action<Diagnostic> addDiagnostic, CancellationToken cancellationToken)
        //				: base (semanticModel, addDiagnostic, cancellationToken)
        //			{
        //			}

        ////			public override void VisitMethodDeclaration(MethodDeclaration methodDeclaration)
        ////			{
        ////				if (!methodDeclaration.HasModifier(Modifiers.Override))
        ////					return;
        ////				var lastParam = methodDeclaration.Parameters.LastOrDefault();
        ////				if (lastParam == null || lastParam.ParameterModifier != ParameterModifier.Params)
        ////					return;
        ////				var type = lastParam.Type as ComposedType;
        ////				if (type == null || !type.ArraySpecifiers.Any())
        ////					return;
        ////				var rr = ctx.Resolve(methodDeclaration) as MemberResolveResult;
        ////				if (rr == null)
        ////					return;
        ////				var baseMember = InheritanceHelper.GetBaseMember(rr.Member) as IMethod;
        ////				if (baseMember == null || baseMember.Parameters.Count == 0 || baseMember.Parameters.Last().IsParams)
        ////					return;
        ////				AddDiagnosticAnalyzer(new CodeIssue(
        ////					lastParam.GetChildByRole(ParameterDeclaration.ParamsModifierRole),
        ////					ctx.TranslateString(""),
        ////					ctx.TranslateString(""),
        ////					script => {
        ////						var p = (ParameterDeclaration)lastParam.Clone();
        ////						p.ParameterModifier = ParameterModifier.None;
        ////						script.Replace(lastParam, p);
        ////					}
        ////				) { IssueMarker = IssueMarker.GrayOut });
        ////			}
        ////
        ////			public override void VisitBlockStatement(BlockStatement blockStatement)
        ////			{
        ////				// SKIP
        ////			}
        //		}
    }


}

