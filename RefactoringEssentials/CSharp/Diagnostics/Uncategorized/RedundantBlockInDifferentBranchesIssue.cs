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
    [IssueDescription("RedundantBlockInDifferentBranches",
                      Description = "Blocks in if/else can be simplified to any of the branches if they have the same block.",
                      Category = IssueCategories.RedundanciesInCode,
                      Severity = Severity.Hint,
                      AnalysisDisableKeyword = "RedundantBlockInDifferentBranches")]
    public class RedundantBlockInDifferentBranchesDiagnosticAnalyzer : GatherVisitorCodeIssueProvider
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

        class GatherVisitor : GatherVisitorBase<RedundantBlockInDifferentBranchesIssue>
        {
            public GatherVisitor(SemanticModel semanticModel, Action<Diagnostic> addDiagnostic, CancellationToken cancellationToken)
                : base(semanticModel, addDiagnostic, cancellationToken)
            {
            }

            static readonly AstNode pattern = new Choice{
                new IfElseStatement(
                    new AnyNode("c"),
                    new AnyNode("s"),
                    new BlockStatement{new Backreference("s")}),
                new IfElseStatement(
                    new AnyNode("c"),
                    new AnyNode("s"),
                    new Backreference("s")),
                new IfElseStatement(
                    new AnyNode("c"),
                    new BlockStatement{new AnyNode("s")},
                    new Backreference("s"))
            };

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
                var m = pattern.Match(ifElseStatement);

                if (!m.Success)
                    return;

                if (!IsSafeExpression(ifElseStatement.Condition, ctx))
                    return;

                AddDiagnosticAnalyzer(new CodeIssue(ifElseStatement.ElseToken, ctx.TranslateString("Blocks in if/else or switch branches can be simplified to any of the branches if they have the same block."))
                { IssueMarker = IssueMarker.WavedLine });
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