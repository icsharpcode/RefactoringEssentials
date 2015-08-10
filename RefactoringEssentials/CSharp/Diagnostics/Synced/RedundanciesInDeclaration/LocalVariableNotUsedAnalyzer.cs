using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System.Linq;

namespace RefactoringEssentials.CSharp.Diagnostics
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class LocalVariableNotUsedAnalyzer : DiagnosticAnalyzer
    {
        private static readonly DiagnosticDescriptor descriptor = new DiagnosticDescriptor(
            CSharpDiagnosticIDs.LocalVariableNotUsedAnalyzerID,
            GettextCatalog.GetString("Local variable is never used"),
            GettextCatalog.GetString("Local variable is never used"),
            DiagnosticAnalyzerCategories.RedundanciesInDeclarations,
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            helpLinkUri: HelpLink.CreateFor(CSharpDiagnosticIDs.LocalVariableNotUsedAnalyzerID)
        );

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(descriptor);

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(
                (nodeContext) =>
                {
                    Diagnostic diagnostic;
                    if (TryGetUnusedVariableDiagnostic(nodeContext, out diagnostic))
                    {
                        nodeContext.ReportDiagnostic(diagnostic);
                    }
                },
                SyntaxKind.LocalDeclarationStatement 
            );
        }

        static bool TryGetUnusedVariableDiagnostic(SyntaxNodeAnalysisContext nodeContext, out Diagnostic diagnostic)
        {
            diagnostic = default(Diagnostic);
            if (nodeContext.IsFromGeneratedCode())
                return false;
            var localDeclaration = nodeContext.Node as LocalDeclarationStatementSyntax;
            if (localDeclaration == null)
                return false;

            if (localDeclaration.Declaration.Variables.Any() &&
               localDeclaration.Declaration.Variables.First().Initializer != null)
                return false;

            if (localDeclaration.Declaration == null)
                return false;
            var initializer = localDeclaration.Declaration.Variables.FirstOrDefault().Initializer;
            var declaredSymbol = nodeContext.SemanticModel.GetDeclaredSymbol(initializer) as ILocalSymbol;
            if (declaredSymbol == null)
                return false;

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

            return true;
        }


        private static bool TryGetDiagnostic(SyntaxNodeAnalysisContext nodeContext, out Diagnostic diagnostic)
        {
            diagnostic = default(Diagnostic);
            if (nodeContext.IsFromGeneratedCode())
                return false;

            var localDeclaration = nodeContext.Node as LocalDeclarationStatementSyntax;
            var initializer = localDeclaration.Declaration.Variables;
            var variableInitializer = localDeclaration?.DescendantNodes().OfType<VariableDeclaratorSyntax>().First(); ;

            if (variableInitializer == null)
                return false;
            //// check if variable is assigned
            //if (!variableInitializer.ChildNodes().OfType<EqualsValueClauseSyntax>().Any()) //I thought that we should only look for var... If looking for any variables, this check from the ancient visitor should not be here.
            //    return false;
            var decl = variableInitializer.Parent as VariableDeclarationSyntax;
            if (decl == null)
                return false;

            if (IsLocalVariableBeingUsed(variableInitializer, nodeContext))
            {
                diagnostic = Diagnostic.Create(descriptor, localDeclaration.GetLocation());
                return true;
            }

            return false;
        }

        private static bool IsLocalVariableBeingUsed(VariableDeclaratorSyntax variableDeclarator, SyntaxNodeAnalysisContext syntaxNode)
        {
            var model = syntaxNode.SemanticModel.Compilation.GetSemanticModel(variableDeclarator.SyntaxTree);
            var methodBody = variableDeclarator.AncestorsAndSelf(false).OfType<MethodDeclarationSyntax>().First();
            var lastMethodNode = methodBody?.ChildNodes().LastOrDefault();
            if (lastMethodNode == null)
                return false;

            var readWrite = syntaxNode.SemanticModel.AnalyzeDataFlow(variableDeclarator, lastMethodNode);
            DataFlowAnalysis result = model.AnalyzeDataFlow(variableDeclarator);
            var variablesDeclared = result.VariablesDeclared;
            var variablesRead = result.ReadInside.Union(result.ReadOutside);
            var unused = variablesDeclared.Except(variablesRead);
            var localVariableSymbol = model.GetDeclaredSymbol(variableDeclarator);
            var isLocalVariableNotBeingUsed = false;
            if (localVariableSymbol == null)
                return false;

            foreach (var varNotUsed in unused)
            {
                if (varNotUsed.Name == localVariableSymbol.Name)
                    isLocalVariableNotBeingUsed = true;
            }

            return isLocalVariableNotBeingUsed;
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