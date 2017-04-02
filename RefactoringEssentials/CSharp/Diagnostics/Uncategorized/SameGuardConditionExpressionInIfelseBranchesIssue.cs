using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CodeFixes;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.Text;
using System.Threading;
using ICSharpCode.NRefactory6.CSharp.Refactoring;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.FindSymbols;

namespace ICSharpCode.NRefactory6.CSharp.Diagnostics
{
    [DiagnosticAnalyzer]
    [ExportDiagnosticAnalyzer("", LanguageNames.CSharp)]
    [NRefactoryCodeDiagnosticAnalyzer(Description = "", AnalysisDisableKeyword = "")]
    [IssueDescription("Same guard condition expression in different if else branch",
                      Description = "A warning should be given for the case: if (condition) {…} else if (condition) {…}.",
                      Category = IssueCategories.Notifications,
                      Severity = Severity.Warning)]
    public class SameGuardConditionExpressionInIfelseBranchesDiagnosticAnalyzer : GatherVisitorCodeIssueProvider
    {
        internal const string DiagnosticId = "";
        const string Description = "";
        const string MessageFormat = "";
        const string Category = IssueCategories.CodeQualityIssues;

        static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Description, MessageFormat, Category, DiagnosticSeverity.Warning);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                return ImmutableArray.Create(Rule);
            }
        }

        protected override CSharpSyntaxWalker CreateVisitor(SemanticModel semanticModel, Action<Diagnostic> addDiagnostic, CancellationToken cancellationToken)
        {
            return new GatherVisitor(semanticModel, addDiagnostic, cancellationToken);
        }

        class GatherVisitor : GatherVisitorBase<SameGuardConditionExpressionInIfelseBranchesIssue>
        {
            public GatherVisitor(SemanticModel semanticModel, Action<Diagnostic> addDiagnostic, CancellationToken cancellationToken)
                : base(semanticModel, addDiagnostic, cancellationToken)
            {
            }

            bool IsSafeExpression(Expression expression, BaseSemanticModel context)
            {
                var components = expression.DescendantsAndSelf;
                foreach (var c in components)
                {
                    if (c is AssignmentExpression)
                        return false;
                    else if (c is UnaryOperatorExpression)
                    {
                        var ope = ((UnaryOperatorExpression)c).Operator;
                        if (ope == UnaryOperatorType.Decrement || ope == UnaryOperatorType.Increment
                            || ope == UnaryOperatorType.PostDecrement || ope == UnaryOperatorType.PostIncrement)
                            return false;
                    }
                    else if (c is IdentifierExpression)
                    {
                        var result = context.Resolve(c);
                        if (result.IsError)
                            return false;
                        if (!(result is LocalResolveResult))
                            return false;
                        if ((((LocalResolveResult)result).IsParameter))
                            return false;
                    }
                }
                return true;
            }

            public override void VisitIfElseStatement(IfElseStatement ifElseStatement)
            {
                base.VisitIfElseStatement(ifElseStatement);

                HashSet<string> conditions = new HashSet<string>();
                conditions.Add(ifElseStatement.Condition.ToString());
                var temp = ifElseStatement.FalseStatement;
                AstNode redundantCondition = null;
                while (temp is IfElseStatement)
                {
                    var tempCondition = ((IfElseStatement)temp).Condition;
                    if (conditions.Contains(tempCondition.ToString()))
                    {
                        if (IsSafeExpression(tempCondition, ctx))
                        {
                            redundantCondition = tempCondition;
                            break;
                        }
                    }
                    conditions.Add(tempCondition.ToString());
                    temp = ((IfElseStatement)temp).FalseStatement;
                }

                if (redundantCondition == null)
                    return;

                AddDiagnosticAnalyzer(new CodeIssue(redundantCondition, ctx.TranslateString("Found duplicate condition")));
            }
        }
    }

    [ExportCodeFixProvider(.DiagnosticId, LanguageNames.CSharp)]
    public class FixProvider : ICodeFixProvider
    {
        public IEnumerable<string> GetFixableDiagnosticIds()
        {
            yield return .DiagnosticId;
        }

        public async Task<IEnumerable<CodeAction>> GetFixesAsync(Document document, TextSpan span, IEnumerable<Diagnostic> diagnostics, CancellationToken cancellationToken)
        {
            var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
            var result = new List<CodeAction>();
            foreach (var diagonstic in diagnostics)
            {
                var node = root.FindNode(diagonstic.Location.SourceSpan);
                //if (!node.IsKind(SyntaxKind.BaseList))
                //	continue;
                var newRoot = root.RemoveNode(node, SyntaxRemoveOptions.KeepNoTrivia);
                result.Add(CodeActionFactory.Create(node.Span, diagonstic.Severity, diagonstic.GetMessage(), document.WithSyntaxRoot(newRoot)));
            }
            return result;
        }
    }
}