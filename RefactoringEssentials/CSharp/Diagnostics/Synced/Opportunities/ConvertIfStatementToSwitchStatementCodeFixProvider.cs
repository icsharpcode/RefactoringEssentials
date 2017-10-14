using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Formatting;

namespace RefactoringEssentials.CSharp.Diagnostics
{
	[ExportCodeFixProvider(LanguageNames.CSharp), System.Composition.Shared]
    public class ConvertIfStatementToSwitchStatementCodeFixProvider : CodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds
        {
            get
            {
                return ImmutableArray.Create(CSharpDiagnosticIDs.ConvertIfStatementToSwitchStatementAnalyzerID);
            }
        }

        public override FixAllProvider GetFixAllProvider()
        {
            return WellKnownFixAllProviders.BatchFixer;
        }

        public async override Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var document = context.Document;
            var cancellationToken = context.CancellationToken;
            var span = context.Span;
            var diagnostics = context.Diagnostics;
            var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
            var semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);
            var diagnostic = diagnostics.First();
            var node = root.FindNode(context.Span) as IfStatementSyntax;

            var switchExpr = ConvertIfStatementToSwitchStatementAnalyzer.GetSwitchExpression(semanticModel, node.Condition);
            if (switchExpr == null)
                return;
            var switchSections = new List<SwitchSectionSyntax>();
            if (!ConvertIfStatementToSwitchStatementAnalyzer.CollectSwitchSections(switchSections, semanticModel, node, switchExpr))
                return;
            if (switchSections.Count(s => !s.Labels.OfType<DefaultSwitchLabelSyntax>().Any()) <= 2)
                return;

            var switchStatement = SyntaxFactory.SwitchStatement(switchExpr, new SyntaxList<SwitchSectionSyntax>().AddRange(switchSections));
            var newRoot = root.ReplaceNode((SyntaxNode)node, switchStatement.WithAdditionalAnnotations(Formatter.Annotation));
            context.RegisterCodeFix(CodeActionFactory.Create(node.Span, diagnostic.Severity, "Convert to 'switch' statement", document.WithSyntaxRoot(newRoot)), diagnostic);
        }
    }
}