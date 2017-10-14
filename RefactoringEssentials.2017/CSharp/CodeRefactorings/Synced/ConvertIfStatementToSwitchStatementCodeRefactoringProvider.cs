using System.Collections.Generic;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Formatting;
using RefactoringEssentials.CSharp.Diagnostics;

namespace RefactoringEssentials.CSharp.CodeRefactorings
{
	[ExportCodeRefactoringProvider(LanguageNames.CSharp, Name = "Convert 'if' to 'switch'")]
    public class ConvertIfStatementToSwitchStatementCodeRefactoringProvider : CodeRefactoringProvider
    {
        public override async Task ComputeRefactoringsAsync(CodeRefactoringContext context)
        {
            var document = context.Document;
            if (document.Project.Solution.Workspace.Kind == WorkspaceKind.MiscellaneousFiles)
                return;
            var span = context.Span;
            if (!span.IsEmpty)
                return;
            var cancellationToken = context.CancellationToken;
            if (cancellationToken.IsCancellationRequested)
                return;
            var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
            var model = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);
            if (model.IsFromGeneratedCode(cancellationToken))
                return;
            var node = root.FindNode(span) as IfStatementSyntax;

            if (node == null)
                return;

            var switchExpr = ConvertIfStatementToSwitchStatementAnalyzer.GetSwitchExpression(model, node.Condition);
            if (switchExpr == null)
                return;

            var switchSections = new List<SwitchSectionSyntax>();
            if (!ConvertIfStatementToSwitchStatementAnalyzer.CollectSwitchSections(switchSections, model, node, switchExpr))
                return;

            context.RegisterRefactoring(
                CodeActionFactory.Create(
                    span,
                    DiagnosticSeverity.Info,
                    GettextCatalog.GetString("To 'switch'"),
                    ct =>
                    {
                        var switchStatement = SyntaxFactory.SwitchStatement(switchExpr, new SyntaxList<SwitchSectionSyntax>().AddRange(switchSections));
                        return Task.FromResult(document.WithSyntaxRoot(root.ReplaceNode(
                            (SyntaxNode)node, switchStatement
                            .WithLeadingTrivia(node.GetLeadingTrivia())
                            .WithAdditionalAnnotations(Formatter.Annotation))));
                    })
            );
        }
    }
}
