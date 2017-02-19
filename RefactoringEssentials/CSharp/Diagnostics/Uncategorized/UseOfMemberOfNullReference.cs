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

    [IssueDescription("Use of (non-extension method) member of null value will cause a NullReferenceException",
                      Description = "Detects when a member of a null value is used",
                      Category = IssueCategories.CodeQualityIssues,
                      Severity = Severity.Warning)]
    public class UseOfMemberOfNullReference : GatherVisitorCodeIssueProvider
    {
        static readonly ISet<NullValueStatus> ProblematicNullStates = new HashSet<NullValueStatus> {
            NullValueStatus.DefinitelyNull,
            NullValueStatus.PotentiallyNull
        };

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

        class GatherVisitor : GatherVisitorBase<UseOfMemberOfNullReference>
        {
            Dictionary<AstNode, NullValueAnalysis> cachedNullAnalysis = new Dictionary<AstNode, NullValueAnalysis>();

            public GatherVisitor(SemanticModel semanticModel, Action<Diagnostic> addDiagnostic, CancellationToken cancellationToken)
                : base(semanticModel, addDiagnostic, cancellationToken)
            {
            }

            public override void VisitMemberReferenceExpression(MemberReferenceExpression memberReferenceExpression)
            {
                IMember member = GetMember(memberReferenceExpression);
                if (member == null || member.IsStatic || member.FullName == "System.Nullable.HasValue")
                {
                    base.VisitMemberReferenceExpression(memberReferenceExpression);
                    return;
                }

                var parentFunction = ConstantNullCoalescingConditionIssue.GetParentFunctionNode(memberReferenceExpression);
                var analysis = GetAnalysis(parentFunction);

                var nullStatus = analysis.GetExpressionResult(memberReferenceExpression.Target);
                if (ProblematicNullStates.Contains(nullStatus))
                {
                    AddDiagnosticAnalyzer(new CodeIssue(memberReferenceExpression,
                        ctx.TranslateString("Using member of null value will cause a NullReferenceException")));
                }
            }

            IMember GetMember(AstNode expression)
            {
                var resolveResult = ctx.Resolve(expression);
                MemberResolveResult memberResolveResult = resolveResult as MemberResolveResult;
                if (memberResolveResult != null)
                {
                    return memberResolveResult.Member;
                }

                var methodGroupResolveResult = resolveResult as MethodGroupResolveResult;
                if (methodGroupResolveResult != null && expression.Parent is InvocationExpression)
                {
                    return GetMember(expression.Parent);
                }

                return null;
            }

            NullValueAnalysis GetAnalysis(AstNode parentFunction)
            {
                NullValueAnalysis analysis;
                if (cachedNullAnalysis.TryGetValue(parentFunction, out analysis))
                {
                    return analysis;
                }

                analysis = new NullValueAnalysis(ctx, parentFunction.GetChildByRole(Roles.Body), parentFunction.GetChildrenByRole(Roles.Parameter), ctx.CancellationToken);
                analysis.Analyze();
                cachedNullAnalysis[parentFunction] = analysis;
                return analysis;
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