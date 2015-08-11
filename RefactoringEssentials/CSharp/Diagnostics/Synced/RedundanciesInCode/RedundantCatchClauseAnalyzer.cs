using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System.Linq;

namespace RefactoringEssentials.CSharp.Diagnostics
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class RedundantCatchClauseAnalyzer : DiagnosticAnalyzer
    {
        private static readonly DiagnosticDescriptor descriptor = new DiagnosticDescriptor(
            CSharpDiagnosticIDs.RedundantCatchClauseAnalyzerID,
            GettextCatalog.GetString("Catch clause with a single 'throw' statement is redundant"),
            "{0}",
            DiagnosticAnalyzerCategories.RedundanciesInCode,
            DiagnosticSeverity.Info,
            isEnabledByDefault: true,
            helpLinkUri: HelpLink.CreateFor(CSharpDiagnosticIDs.RedundantCatchClauseAnalyzerID),
            customTags: DiagnosticCustomTags.Unnecessary
        );

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(descriptor);

        // "Remove redundant catch clauses" / "Remove 'catch'" / "'try' statement is redundant" /
        //"Remove all '{0}' redundant 'catch' clauses" / "Remove 'try' statement"
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
                  SyntaxKind.CatchClause
            );
        }

        private static bool TryGetDiagnostic(SyntaxNodeAnalysisContext nodeContext, out Diagnostic diagnostic)
        {
            diagnostic = default(Diagnostic);
            if (nodeContext.IsFromGeneratedCode())
                return false;

            var catchClause = nodeContext.Node as CatchClauseSyntax;
            var tryStatement = catchClause?.Parent as TryStatementSyntax;
            if (tryStatement == null || tryStatement.Finally != null)
                return false;

            var catchBlock = catchClause.Block;

            if (catchBlock == null || !catchBlock.Statements.Any())
                return false;

            if (IsRedundant(catchClause, nodeContext))
            {
                diagnostic = Diagnostic.Create(descriptor, catchClause.GetLocation());
                return true;
            }
            return false;
        }

        private static bool IsThrowsClause(CatchClauseSyntax catchClause)
        {
            var firstStatement = catchClause.Block.Statements.FirstOrDefault();
            var throwStatement = firstStatement as ThrowStatementSyntax;
            return (throwStatement != null);
        }

        private static bool IsRedundant(CatchClauseSyntax catchClause, SyntaxNodeAnalysisContext nodeContext)
        {
            if (!IsThrowsClause(catchClause))
                return false;

            var type = nodeContext.SemanticModel.GetTypeInfo(catchClause).ConvertedType;
            var tryStatement = catchClause.Parent as TryStatementSyntax;
            if (tryStatement == null)
                return false;

            var catches = tryStatement.Catches;
            var index = catches.IndexOf(catchClause);
            var nextSibling = catches.ElementAtOrDefault(index + 1);
            var typeInfo = nodeContext.SemanticModel.GetTypeInfo(catchClause).Type;

            while (nextSibling != null)
            {
                var nextClause = nextSibling;
                if (!IsThrowsClause(nextClause)  || nextClause.Declaration?.Type == null ) 
                    return false;
                var nextTypeInfo = nodeContext.SemanticModel.GetTypeInfo(nextClause).Type;
                if (!IsThrowsClause(nextClause) && typeInfo.InheritsFromOrEqualsIgnoringConstruction(nextTypeInfo))
                    return false;
                //type.GetDefinition ().IsDerivedFrom (ctx.Resolve (nextClause.Type).Type.GetDefinition ()))
                nextSibling = catches.ElementAtOrDefault(catches.IndexOf(nextSibling) + 1);
            }
            return true;
        }


        private bool InheritsFrom<T>(INamedTypeSymbol symbol)
        {
            while (true)
            {
                if (symbol.ToString() == typeof(T).FullName)
                {
                    return true;
                }
                if (symbol.BaseType != null)
                {
                    symbol = symbol.BaseType;
                    continue;
                }
                break;
            }
            return false;
        }
    }

    //		class GatherVisitor : GatherVisitorBase<RedundantCatchClauseAnalyzer>
    //		{
    //			public GatherVisitor(SemanticModel semanticModel, Action<Diagnostic> addDiagnostic, CancellationToken cancellationToken)
    //				: base (semanticModel, addDiagnostic, cancellationToken)
    //			{
    //			}

    ////			public override void VisitTryCatchStatement(TryCatchStatement tryCatchStatement)
    ////			{
    ////				var redundantCatchClauses = new List<CatchClause>();
    ////				bool hasNonRedundantCatch = false;
    ////				foreach (var catchClause in tryCatchStatement.CatchClauses) {
    ////					if (IsRedundant(catchClause)) {
    ////						redundantCatchClauses.Add(catchClause);
    ////					} else {
    ////						hasNonRedundantCatch = true;
    ////					}
    ////				}
    ////
    ////				if (hasNonRedundantCatch || !tryCatchStatement.FinallyBlock.IsNull) {
    ////					AddDiagnosticAnalyzersForClauses(tryCatchStatement, redundantCatchClauses);
    ////				} else {
    ////					AddDiagnosticAnalyzerForTryCatchStatement(tryCatchStatement);
    ////				}
    ////			}
    ////
    ////			void AddDiagnosticAnalyzersForClauses(AstNode node, List<CatchClause> redundantCatchClauses)
    ////			{
    //			//				var allCatchClausesMessage = ctx.TranslateString("");
    ////				var removeAllRedundantClausesAction = new CodeAction(allCatchClausesMessage, script => {
    ////					foreach (var redundantCatchClause in redundantCatchClauses) {
    ////						script.Remove(redundantCatchClause);
    ////					}
    ////				}, node);
    ////				var singleCatchClauseMessage = ctx.TranslateString("");
    ////                var redundantCatchClauseMessage = ctx.TranslateString("");
    ////				foreach (var redundantCatchClause in redundantCatchClauses) {
    ////					var closureLocalCatchClause = redundantCatchClause;
    ////					var removeRedundantClauseAction = new CodeAction(singleCatchClauseMessage, script => {
    ////						script.Remove(closureLocalCatchClause);
    ////					}, node);
    ////					var actions = new List<CodeAction>();
    ////					actions.Add(removeRedundantClauseAction);
    ////					if (redundantCatchClauses.Count > 1) {
    ////						actions.Add(removeAllRedundantClausesAction);
    ////					}
    ////					AddDiagnosticAnalyzer(new CodeIssue(closureLocalCatchClause, redundantCatchClauseMessage, actions) { IssueMarker = IssueMarker.GrayOut });
    ////				}
    ////			}
    ////
    ////			void AddDiagnosticAnalyzerForTryCatchStatement(TryCatchStatement tryCatchStatement)
    ////			{
    ////				var lastCatch = tryCatchStatement.CatchClauses.LastOrNullObject();
    ////				if (lastCatch.IsNull)
    ////					return;
    ////
    ////				var removeTryCatchMessage = ctx.TranslateString("");
    ////
    ////				var removeTryStatementAction = new CodeAction(removeTryCatchMessage, script => {
    ////					var statements = tryCatchStatement.TryBlock.Statements;
    ////					if (statements.Count == 1 || tryCatchStatement.Parent is BlockStatement) {
    ////						foreach (var statement in statements) {
    ////							script.InsertAfter(tryCatchStatement.GetPrevSibling (s => !(s is NewLineNode)), statement.Clone());
    ////						}
    ////						script.Remove(tryCatchStatement);
    ////					} else {
    ////						var blockStatement = new BlockStatement();
    ////						foreach (var statement in statements) {
    ////							blockStatement.Statements.Add(statement.Clone());
    ////						}
    ////						script.Replace(tryCatchStatement, blockStatement);
    ////					}
    ////					// The replace and insert script functions does not format these things well on their own
    ////					script.FormatText(tryCatchStatement.Parent);
    ////				}, tryCatchStatement);
    ////
    ////				var fixes = new [] {
    ////					removeTryStatementAction
    ////				};
    ////				AddDiagnosticAnalyzer(new CodeIssue(tryCatchStatement.TryBlock.EndLocation, lastCatch.EndLocation, removeTryCatchMessage, fixes) { IssueMarker = IssueMarker.GrayOut });
    ////			}
    ////
    ////			static bool IsThrowsClause (CatchClause catchClause)
    ////			{
    ////				var firstStatement = catchClause.Body.Statements.FirstOrNullObject();
    ////				if (firstStatement.IsNull)
    ////					return false;
    ////				var throwStatement = firstStatement as ThrowStatement;
    ////				if (throwStatement == null || !throwStatement.Expression.IsNull)
    ////					return false;
    ////				return true;
    ////			}
    ////
    ////			bool IsRedundant(CatchClause catchClause)
    ////			{
    ////				if (!IsThrowsClause (catchClause))
    ////					return false;
    ////
    ////				var type = ctx.Resolve (catchClause.Type).Type;
    ////				var n = catchClause.NextSibling;
    ////				while (n != null) {
    ////					var nextClause = n as CatchClause;
    ////					if (nextClause != null) {
    ////						if (nextClause.Type.IsNull && !IsThrowsClause(nextClause))
    ////							return false;
    ////						if (!IsThrowsClause(nextClause) && type.GetDefinition ().IsDerivedFrom (ctx.Resolve (nextClause.Type).Type.GetDefinition ()))
    ////							return false;
    ////					}
    ////					n = n.NextSibling;
    ////				}
    ////				return true;
    ////			}
    //		}
}