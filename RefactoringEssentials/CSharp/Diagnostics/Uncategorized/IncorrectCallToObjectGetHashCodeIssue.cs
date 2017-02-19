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
    [IssueDescription("Call resolves to Object.GetHashCode, which is reference based",
                       Description = "Finds calls to Object.GetHashCode inside overridden GetHashCode.",
                       Category = IssueCategories.CodeQualityIssues,
                       Severity = Severity.Warning,
                       AnalysisDisableKeyword = "BaseObjectGetHashCodeCallInGetHashCode")]
    public class IncorrectCallToObjectGetHashCodeDiagnosticAnalyzer : GatherVisitorCodeIssueProvider
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

        class GatherVisitor : GatherVisitorBase<IncorrectCallToObjectGetHashCodeIssue>
        {
            public GatherVisitor(SemanticModel semanticModel, Action<Diagnostic> addDiagnostic, CancellationToken cancellationToken)
                : base(semanticModel, addDiagnostic, cancellationToken)
            {
            }

            public override void VisitMethodDeclaration(MethodDeclaration methodDeclaration)
            {
                // Check that this declaration is a GetHashCode override, _then_ continue traversing

                if (methodDeclaration.Name != "GetHashCode")
                {
                    return;
                }
                if (!methodDeclaration.Modifiers.HasFlag(Modifiers.Override))
                {
                    return;
                }

                base.VisitMethodDeclaration(methodDeclaration);
            }

            public override void VisitInvocationExpression(InvocationExpression invocationExpression)
            {
                base.VisitInvocationExpression(invocationExpression);

                var resolveResult = ctx.Resolve(invocationExpression) as InvocationResolveResult;
                if (resolveResult == null || !(resolveResult.TargetResult is ThisResolveResult) || resolveResult.Member.Name != "GetHashCode" ||
                    !resolveResult.Member.DeclaringTypeDefinition.IsKnownType(KnownTypeCode.Object))
                {
                    return;
                }
                AddDiagnosticAnalyzer(new CodeIssue(invocationExpression, ctx.TranslateString("Call resolves to Object.GetHashCode, which is reference based")));
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