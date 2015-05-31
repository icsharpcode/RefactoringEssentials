using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace RefactoringEssentials.CSharp.Diagnostics
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    [NotPortedYet]
    public class ForControlVariableIsNeverModifiedAnalyzer : DiagnosticAnalyzer
    {
        static readonly DiagnosticDescriptor descriptor = new DiagnosticDescriptor(
            CSharpDiagnosticIDs.ForControlVariableIsNeverModifiedAnalyzerID,
            GettextCatalog.GetString("'for' loop control variable is never modified"),
            GettextCatalog.GetString("'for' loop control variable is never modified"),
            DiagnosticAnalyzerCategories.CodeQualityIssues,
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            helpLinkUri: HelpLink.CreateFor(CSharpDiagnosticIDs.ForControlVariableIsNeverModifiedAnalyzerID)
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

        //		class GatherVisitor : GatherVisitorBase<ForControlVariableIsNeverModifiedAnalyzer>
        //		{
        //			public GatherVisitor(SemanticModel semanticModel, Action<Diagnostic> addDiagnostic, CancellationToken cancellationToken)
        //				: base (semanticModel, addDiagnostic, cancellationToken)
        //			{
        //			}

        ////			static VariableInitializer GetControlVariable(VariableDeclarationStatement variableDecl, 
        ////														  BinaryOperatorExpression condition)
        ////			{
        ////				var controlVariables = variableDecl.Variables.Where (
        ////					v =>
        ////					{
        ////						var identifier = new IdentifierExpression (v.Name);
        ////						return condition.Left.Match (identifier).Success ||
        ////							condition.Right.Match (identifier).Success;
        ////					}).ToList ();
        ////				return controlVariables.Count == 1 ? controlVariables [0] : null;
        ////			}
        ////
        ////			static VariableInitializer GetControlVariable(VariableDeclarationStatement variableDecl,
        ////														  UnaryOperatorExpression condition)
        ////			{
        ////				var controlVariables = variableDecl.Variables.Where (
        ////					v =>
        ////					{
        ////						var identifier = new IdentifierExpression (v.Name);
        ////						return condition.Expression.Match (identifier).Success;
        ////					}).ToList ();
        ////				return controlVariables.Count == 1 ? controlVariables [0] : null;
        ////			}
        ////
        ////			public override void VisitForStatement (ForStatement forStatement)
        ////			{
        ////				base.VisitForStatement (forStatement);
        ////
        ////				if (forStatement.Initializers.Count != 1)
        ////					return;
        ////				var variableDecl = forStatement.Initializers.First () as VariableDeclarationStatement;
        ////				if (variableDecl == null)
        ////					return;
        ////
        ////				VariableInitializer controlVariable = null;
        ////				if (forStatement.Condition is BinaryOperatorExpression) {
        ////					controlVariable = GetControlVariable (variableDecl, (BinaryOperatorExpression)forStatement.Condition);
        ////				} else if (forStatement.Condition is UnaryOperatorExpression) {
        ////					controlVariable = GetControlVariable (variableDecl, (UnaryOperatorExpression)forStatement.Condition);
        ////				} else if (forStatement.Condition is IdentifierExpression) {
        ////					controlVariable = variableDecl.Variables.FirstOrDefault (
        ////						v => v.Name == ((IdentifierExpression)forStatement.Condition).Identifier);
        ////				}
        ////
        ////				if (controlVariable == null)
        ////					return;
        ////
        ////				var localResolveResult = ctx.Resolve (controlVariable) as LocalResolveResult;
        ////				if (localResolveResult == null)
        ////					return;
        ////
        ////				var results = ctx.FindReferences (forStatement, localResolveResult.Variable);
        ////				var modified = false;
        ////				foreach (var result in results) {
        ////					if (modified)
        ////						break;
        ////					var node = result.Node;
        ////					var unary = node.Parent as UnaryOperatorExpression;
        ////					if (unary != null && unary.Expression == node) {
        ////						modified = unary.Operator == UnaryOperatorType.Decrement ||
        ////							unary.Operator == UnaryOperatorType.PostDecrement ||
        ////							unary.Operator == UnaryOperatorType.Increment ||
        ////							unary.Operator == UnaryOperatorType.PostIncrement;
        ////						continue;
        ////					}
        ////
        ////					var assignment = node.Parent as AssignmentExpression;
        ////					modified = assignment != null && assignment.Left == node;
        ////				}
        ////
        ////				if (!modified)
        ////					AddDiagnosticAnalyzer (new CodeIssue(controlVariable.NameToken,
        ////						ctx.TranslateString ("")));
        ////			}
        //		}
    }
}