using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CodeFixes;

namespace RefactoringEssentials.CSharp.CodeFixes
{
    [ExportCodeFixProvider(LanguageNames.CSharp), System.Composition.Shared]
    public class CS0759RedundantPartialMethodCodeFixProvider : CodeFixProvider
    {
        const string CS0759 = "CS0759"; // Error CS0108: No defining declaration found for implementing declaration of partial method 'method'.

        public sealed override ImmutableArray<string> FixableDiagnosticIds
        {
            get { return ImmutableArray.Create(CS0759); }
        }

        public override FixAllProvider GetFixAllProvider()
        {
            return WellKnownFixAllProviders.BatchFixer;
        }

        public async override Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var document = context.Document;
            if (document.Project.Solution.Workspace.Kind == WorkspaceKind.MiscellaneousFiles)
                return;
            var span = context.Span;
            var cancellationToken = context.CancellationToken;
            if (cancellationToken.IsCancellationRequested)
                return;
            var diagnostic = context.Diagnostics.First();
            var model = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);
            if (model.IsFromGeneratedCode(cancellationToken))
                return;
            var root = await model.SyntaxTree.GetRootAsync(cancellationToken).ConfigureAwait(false);
            var node = root.FindNode(span) as MethodDeclarationSyntax;
            if (node == null || !node.Identifier.Span.Contains(span))
                return;
            context.RegisterCodeFix(
                CodeActionFactory.Create(
                    node.Span,
                    DiagnosticSeverity.Error,
                    GettextCatalog.GetString("Remove 'partial'"),
                    t => Task.FromResult(
                        document.WithSyntaxRoot(
                            root.ReplaceNode(
                                (SyntaxNode)node,
                                node.WithModifiers(SyntaxFactory.TokenList(node.Modifiers.Where(m => !m.IsKind(SyntaxKind.PartialKeyword))))
                                .WithLeadingTrivia(node.GetLeadingTrivia())
                            )
                        )
                    )
                ),
                diagnostic
            );
        }
    }
}