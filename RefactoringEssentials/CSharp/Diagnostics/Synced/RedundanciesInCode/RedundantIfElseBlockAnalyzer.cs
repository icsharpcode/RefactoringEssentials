using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace RefactoringEssentials.CSharp.Diagnostics
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    [NotPortedYet]
    public class RedundantIfElseBlockAnalyzer : DiagnosticAnalyzer
    {
        static readonly DiagnosticDescriptor descriptor = new DiagnosticDescriptor(
            NRefactoryDiagnosticIDs.RedundantIfElseBlockAnalyzerID,
            GettextCatalog.GetString("Redundant 'else' keyword"),
            GettextCatalog.GetString("Redundant 'else' keyword"),
            DiagnosticAnalyzerCategories.RedundanciesInCode,
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            helpLinkUri: HelpLink.CreateFor(NRefactoryDiagnosticIDs.RedundantIfElseBlockAnalyzerID),
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

        //		class GatherVisitor : GatherVisitorBase<RedundantIfElseBlockAnalyzer>
        //		{
        //			//readonly LocalDeclarationSpaceVisitor declarationSpaceVisitor;

        //			public GatherVisitor(SemanticModel semanticModel, Action<Diagnostic> addDiagnostic, CancellationToken cancellationToken)
        //				: base (semanticModel, addDiagnostic, cancellationToken)
        //			{
        //				//this.declarationSpaceVisitor = new LocalDeclarationSpaceVisitor();
        //			}

        ////			public override void VisitSyntaxTree(SyntaxTree syntaxTree)
        ////			{
        ////				syntaxTree.AcceptVisitor(declarationSpaceVisitor);
        ////				base.VisitSyntaxTree(syntaxTree);
        ////			}
        ////
        ////			bool ElseIsRedundantControlFlow(IfElseStatement ifElseStatement)
        ////			{
        ////				if (ifElseStatement.FalseStatement.IsNull || ifElseStatement.Parent is IfElseStatement)
        ////					return false;
        ////				var blockStatement = ifElseStatement.FalseStatement as BlockStatement;
        ////				if (blockStatement != null && blockStatement.Statements.Count == 0)
        ////					return true;
        ////				var reachability = ctx.CreateReachabilityAnalysis(ifElseStatement.TrueStatement);
        ////				return !reachability.IsEndpointReachable(ifElseStatement.TrueStatement);
        ////			}
        ////
        ////			bool HasConflictingNames(AstNode targetContext, AstNode currentContext)
        ////			{
        ////				var targetSpace = declarationSpaceVisitor.GetDeclarationSpace(targetContext);
        ////				var currentSpace = declarationSpaceVisitor.GetDeclarationSpace(currentContext);
        ////				foreach (var name in currentSpace.DeclaredNames) {
        ////					var isUsed = targetSpace.GetNameDeclarations(name).Any(node => node.Ancestors.Any(n => n == currentContext));
        ////					if (isUsed)
        ////						return true;
        ////				}
        ////				return false;
        ////			}
        ////			public override void VisitIfElseStatement (IfElseStatement ifElseStatement)
        ////			{
        ////				base.VisitIfElseStatement(ifElseStatement);
        ////
        ////				if (!ElseIsRedundantControlFlow(ifElseStatement) || HasConflictingNames(ifElseStatement.Parent, ifElseStatement.FalseStatement))
        ////					return;
        ////
        ////				AddDiagnosticAnalyzer(new CodeIssue(ifElseStatement.ElseToken, ctx.TranslateString(""), ctx.TranslateString(""), script =>  {
        ////					int start = script.GetCurrentOffset(ifElseStatement.ElseToken.GetPrevNode(n => !(n is NewLineNode)).EndLocation);
        ////					int end;
        ////					var blockStatement = ifElseStatement.FalseStatement as BlockStatement;
        ////					if (blockStatement != null) {
        ////						if (blockStatement.Statements.Count == 0) {
        ////							// remove empty block
        ////							end = script.GetCurrentOffset(blockStatement.LBraceToken.StartLocation);
        ////							script.Remove(blockStatement);
        ////						}
        ////						else {
        ////							// remove block braces
        ////							end = script.GetCurrentOffset(blockStatement.LBraceToken.EndLocation);
        ////							script.Remove(blockStatement.RBraceToken);
        ////						}
        ////					}
        ////					else {
        ////						end = script.GetCurrentOffset(ifElseStatement.ElseToken.EndLocation);
        ////					}
        ////					if (end > start)
        ////						script.RemoveText(start, end - start);
        ////					script.FormatText(ifElseStatement.Parent);
        ////				}) { IssueMarker = IssueMarker.GrayOut });
        ////			}
        //		}
    }
}