using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace RefactoringEssentials.CSharp.Diagnostics
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    [NotPortedYet]
    public class ParameterHidesMemberAnalyzer : VariableHidesMemberAnalyzer
    {
        static readonly DiagnosticDescriptor descriptor = new DiagnosticDescriptor(
            CSharpDiagnosticIDs.ParameterHidesMemberAnalyzerID,
            GettextCatalog.GetString("Parameter has the same name as a member and hides it"),
            "{0}",
            DiagnosticAnalyzerCategories.CodeQualityIssues,
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            helpLinkUri: HelpLink.CreateFor(CSharpDiagnosticIDs.ParameterHidesMemberAnalyzerID)
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

        //		class GatherVisitor : GatherVisitorBase<ParameterHidesMemberAnalyzer>
        //		{
        //			public GatherVisitor(SemanticModel semanticModel, Action<Diagnostic> addDiagnostic, CancellationToken cancellationToken)
        //				: base(semanticModel, addDiagnostic, cancellationToken)
        //			{
        //			}

        ////			public override void VisitParameterDeclaration(ParameterDeclaration parameterDeclaration)
        ////			{
        ////				base.VisitParameterDeclaration(parameterDeclaration);
        ////
        ////				var rr = ctx.Resolve(parameterDeclaration.Parent) as MemberResolveResult;
        ////				if (rr == null || rr.IsError)
        ////					return;
        ////				var parent = rr.Member;
        ////				if (parent.SymbolKind == SymbolKind.Constructor || parent.ImplementedInterfaceMembers.Any())
        ////					return;
        ////				if (parent.IsOverride || parent.IsAbstract || parent.IsPublic || parent.IsProtected)
        ////					return;
        ////					
        ////				IMember member;
        ////				if (HidesMember(ctx, parameterDeclaration, parameterDeclaration.Name, out member)) {
        ////					string msg;
        ////					switch (member.SymbolKind) {
        ////						case SymbolKind.Field:
        ////							msg = ctx.TranslateString("Parameter '{0}' hides field '{1}'");
        ////							break;
        ////						case SymbolKind.Method:
        ////							msg = ctx.TranslateString("Parameter '{0}' hides method '{1}'");
        ////							break;
        ////						case SymbolKind.Property:
        ////							msg = ctx.TranslateString("Parameter '{0}' hides property '{1}'");
        ////							break;
        ////						case SymbolKind.Event:
        ////							msg = ctx.TranslateString("Parameter '{0}' hides event '{1}'");
        ////							break;
        ////						default:
        ////							msg = ctx.TranslateString("Parameter '{0}' hides member '{1}'");
        ////							break;
        ////					}
        ////					AddDiagnosticAnalyzer(new CodeIssue(parameterDeclaration.NameToken,
        ////						string.Format(msg, parameterDeclaration.Name, member.FullName)));
        ////				}
        ////			}
        //		}
    }
}