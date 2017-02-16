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

    /// <summary>
    /// When array initializer has the different number of elements as specified in size creation, it is an error.
    /// </summary>
    [IssueDescription("Correct size specification in array creation",
                      Description = "When array initializer has the different number of elements as specified in size creation, it is an error.",
                      Category = IssueCategories.CompilerErrors,
                      Severity = Severity.Error,
                      AnalysisDisableKeyword = "UnmatchedSizeSpecificationInArrayCreation")]
    public class UnmatchedSizeSpecificationInArrayCreationDiagnosticAnalyzer : GatherVisitorCodeIssueProvider
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

        class GatherVisitor : GatherVisitorBase<UnmatchedSizeSpecificationInArrayCreationIssue>
        {
            public GatherVisitor(SemanticModel semanticModel, Action<Diagnostic> addDiagnostic, CancellationToken cancellationToken)
                : base(semanticModel, addDiagnostic, cancellationToken)
            {
            }

            public override void VisitArrayCreateExpression(ArrayCreateExpression arrayCreateExpression)
            {
                base.VisitArrayCreateExpression(arrayCreateExpression);

                if (arrayCreateExpression == null)
                    return;
                if (arrayCreateExpression.Arguments == null || !arrayCreateExpression.Arguments.Any())
                {
                    return;
                }

                var argument = arrayCreateExpression.Arguments.FirstOrNullObject();
                if (argument == null || !(argument is PrimitiveExpression))
                    return;

                int arraySize = (Int32)((PrimitiveExpression)argument).Value;

                if (arraySize < 1)
                    return;

                var initializer = arrayCreateExpression.Initializer;
                if (initializer.IsNull)
                    return;

                if (arraySize != initializer.Elements.Count)
                {
                    AddDiagnosticAnalyzer(new CodeIssue(argument, ctx.TranslateString("Unmatched size specification with array initializer"), ctx.TranslateString("Correct array size specification"), script =>
                    {
                        var newArgument = new PrimitiveExpression(initializer.Elements.Count);
                        script.Replace(argument, newArgument);
                    }));
                }
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