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
    [IssueDescription("A throw statement throws the caught exception by passing it explicitly",
                      Description = "Finds throws that throws the caught exception and therefore should be empty.",
                      Category = IssueCategories.CodeQualityIssues,
                      Severity = Severity.Warning,
                      AnalysisDisableKeyword = "PossibleIntendedRethrow")]
    public class ExceptionRethrowDiagnosticAnalyzer : GatherVisitorCodeIssueProvider
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

        class GatherVisitor : GatherVisitorBase<ExceptionRethrowIssue>
        {
            public GatherVisitor(SemanticModel semanticModel, Action<Diagnostic> addDiagnostic, CancellationToken cancellationToken)
                : base(semanticModel, addDiagnostic, cancellationToken)
            {
            }

            public override void VisitCatchClause(CatchClause catchClause)
            {
                base.VisitCatchClause(catchClause);
                var exceptionResolveResult = ctx.Resolve(catchClause.VariableNameToken) as LocalResolveResult;
                if (exceptionResolveResult == null)
                    return;

                var catchVisitor = new CatchClauseVisitor(ctx, exceptionResolveResult.Variable);
                catchClause.Body.AcceptVisitor(catchVisitor);

                foreach (var throwStatement in catchVisitor.OffendingThrows)
                {
                    var localThrowStatement = throwStatement;
                    var title = ctx.TranslateString("The exception is rethrown with explicit usage of the variable");
                    var action = new CodeAction(ctx.TranslateString("Change to 'throw;'"), script =>
                    {
                        script.Replace(localThrowStatement, new ThrowStatement());
                    }, catchClause);
                    AddDiagnosticAnalyzer(new CodeIssue(localThrowStatement, title, action));
                }
            }
        }

        class CatchClauseVisitor : DepthFirstAstVisitor
        {
            BaseSemanticModel ctx;

            IVariable parameter;

            bool variableWritten = false;

            public CatchClauseVisitor(BaseSemanticModel context, IVariable parameter)
            {
                ctx = context;
                this.parameter = parameter;
                OffendingThrows = new List<ThrowStatement>();
            }

            public IList<ThrowStatement> OffendingThrows { get; private set; }

            void HandlePotentialWrite(Expression expression)
            {
                var variableResolveResult = ctx.Resolve(expression) as LocalResolveResult;
                if (variableResolveResult == null)
                    return;
                variableWritten |= variableResolveResult.Equals(parameter);
            }

            public override void VisitAssignmentExpression(AssignmentExpression assignmentExpression)
            {
                base.VisitAssignmentExpression(assignmentExpression);

                var variableResolveResult = ctx.Resolve(assignmentExpression.Left) as LocalResolveResult;
                if (variableResolveResult == null)
                    return;
                variableWritten |= variableResolveResult.Variable.Equals(parameter);
            }

            public override void VisitDirectionExpression(DirectionExpression directionExpression)
            {
                base.VisitDirectionExpression(directionExpression);

                HandlePotentialWrite(directionExpression);
            }

            public override void VisitThrowStatement(ThrowStatement throwStatement)
            {
                base.VisitThrowStatement(throwStatement);

                if (variableWritten)
                    return;

                var argumentResolveResult = ctx.Resolve(throwStatement.Expression) as LocalResolveResult;
                if (argumentResolveResult == null)
                    return;
                if (parameter.Equals(argumentResolveResult.Variable))
                    OffendingThrows.Add(throwStatement);
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