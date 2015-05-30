using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace RefactoringEssentials.CSharp.Diagnostics
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    [NotPortedYet]
    public class LocalVariableNotUsedAnalyzer : DiagnosticAnalyzer
    {
        static readonly DiagnosticDescriptor descriptor = new DiagnosticDescriptor(
            DiagnosticIDs.LocalVariableNotUsedAnalyzerID,
            GettextCatalog.GetString("Local variable is never used"),
            GettextCatalog.GetString("Local variable is never used"),
            DiagnosticAnalyzerCategories.RedundanciesInDeclarations,
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            helpLinkUri: HelpLink.CreateFor(DiagnosticIDs.LocalVariableNotUsedAnalyzerID)
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

        //		class GatherVisitor : GatherVisitorBase<LocalVariableNotUsedAnalyzer>
        //		{
        //			public GatherVisitor(SemanticModel semanticModel, Action<Diagnostic> addDiagnostic, CancellationToken cancellationToken)
        //				: base (semanticModel, addDiagnostic, cancellationToken)
        //			{
        //			}

        //			public override void VisitVariableDeclarator(VariableDeclaratorSyntax node)
        //			{
        //				base.VisitVariableDeclarator(node);

        ////				// check if variable is assigned
        ////				if (!variableInitializer.Initializer.IsNull)
        ////					return;
        ////				var decl = variableInitializer.Parent as VariableDeclarationStatement;
        ////				if (decl == null)
        ////					return;
        ////
        ////				var resolveResult = ctx.Resolve(variableInitializer) as LocalResolveResult;
        ////				if (resolveResult == null)
        ////					return;
        ////
        ////				if (IsUsed(decl.Parent, resolveResult.Variable, variableInitializer))
        ////					return;
        ////
        ////				AddDiagnosticAnalyzer(new CodeIssue(variableInitializer.NameToken, 
        ////					string.Format(ctx.TranslateString(""), resolveResult.Variable.Name), ctx.TranslateString(""),
        ////					script => {
        ////						if (decl.Variables.Count == 1) {
        ////							script.Remove(decl);
        ////						} else {
        ////							var newDeclaration = (VariableDeclarationStatement)decl.Clone();
        ////							newDeclaration.Variables.Remove(
        ////								newDeclaration.Variables.FirstOrNullObject(v => v.Name == variableInitializer.Name));
        ////							script.Replace(decl, newDeclaration);
        ////						}
        ////					}) { IssueMarker = IssueMarker.GrayOut });
        //			}


        //			public override void VisitForEachStatement(ForEachStatementSyntax node)
        //			{
        //				base.VisitForEachStatement(node);

        ////				var resolveResult = ctx.Resolve(foreachStatement.VariableNameToken) as LocalResolveResult;
        ////				if (resolveResult == null)
        ////					return;
        ////
        ////				if (IsUsed(foreachStatement, resolveResult.Variable, foreachStatement.VariableNameToken))
        ////					return;
        ////
        ////				AddDiagnosticAnalyzer(new CodeIssue(foreachStatement.VariableNameToken, ctx.TranslateString("Local variable is never used")));
        //			}

        ////			bool IsUsed(SyntaxNode rootNode, ILocalSymbol variable, SyntaxNode variableNode)
        ////			{
        ////				return ctx.FindReferences(rootNode, variable).Any(result => result.Node != variableNode);
        ////			}
        //		}
    }


}