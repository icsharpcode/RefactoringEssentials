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
    [IssueDescription("Incorrect element type in foreach over generic collection",
                      Description = "Detects hidden explicit conversions in foreach loops.",
                      Category = IssueCategories.CodeQualityIssues,
                      Severity = Severity.Warning)]
    public class ExplicitConversionInForEachDiagnosticAnalyzer : GatherVisitorCodeIssueProvider
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

        class GatherVisitor : GatherVisitorBase<ExplicitConversionInForEachIssue>
        {
            CSharpConversions conversions;

            public GatherVisitor(SemanticModel semanticModel, Action<Diagnostic> addDiagnostic, CancellationToken cancellationToken)
                : base(semanticModel, addDiagnostic, cancellationToken)
            {
            }

            public override void VisitForeachStatement(ForeachStatement foreachStatement)
            {
                base.VisitForeachStatement(foreachStatement);
                var rr = ctx.Resolve(foreachStatement) as ForEachResolveResult;
                if (rr == null)
                    return;
                if (rr.ElementType.Kind == TypeKind.Unknown || rr.ElementVariable.Type.Kind == TypeKind.Unknown)
                    return;
                if (ReflectionHelper.GetTypeCode(rr.ElementType) == TypeCode.Object)
                    return;
                if (conversions == null)
                {
                    conversions = CSharpConversions.Get(ctx.Compilation);
                }
                Conversion c = conversions.ImplicitConversion(rr.ElementType, rr.ElementVariable.Type);
                if (c.IsValid)
                    return;
                var csResolver = ctx.GetResolverStateBefore(foreachStatement);
                var builder = new TypeSystemAstBuilder(csResolver);
                AstType elementType = builder.ConvertType(rr.ElementType);
                AstType variableType = foreachStatement.VariableType;
                string issueText = ctx.TranslateString("Collection element type '{0}' is not implicitly convertible to '{1}'");
                string fixText = ctx.TranslateString("Use type '{0}'");
                AddDiagnosticAnalyzer(new CodeIssue(variableType, string.Format(issueText, elementType.ToString(), variableType.ToString()),
                         new CodeAction(
                                string.Format(fixText, elementType.ToString()),
                                script => script.Replace(variableType, elementType),
                        foreachStatement)));
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