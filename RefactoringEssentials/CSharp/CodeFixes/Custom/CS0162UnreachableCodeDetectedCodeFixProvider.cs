using Microsoft.CodeAnalysis;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CodeFixes;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp;

namespace RefactoringEssentials.CSharp.CodeFixes
{
    [ExportCodeFixProvider(LanguageNames.CSharp), System.Composition.Shared]
    public class CS0162UnreachableCodeDetectedCodeFixProvider : CodeFixProvider
    {
        const string CS0162 = "CS0162"; // CS0162: The compiler detected code that will never be executed.

        public sealed override ImmutableArray<string> FixableDiagnosticIds
        {
            get { return ImmutableArray.Create(CS0162); }
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
            var model = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);
            if (model.IsFromGeneratedCode(cancellationToken))
                return;
            var root = await model.SyntaxTree.GetRootAsync(cancellationToken).ConfigureAwait(false);

            var diagnostic = diagnostics.First();
            var node = root.FindNode(context.Span);
            if (node == null)
                return;

            context.RegisterCodeFix(CodeActionFactory.Create(
                node.Span,
                diagnostic.Severity,
                GettextCatalog.GetString("Remove redundant code"),
                token =>
                {
                    var newRoot = GetNewRoot(root, node);

                    return Task.FromResult(document.WithSyntaxRoot(newRoot));
                }), diagnostic);
        }

        static SyntaxNode GetNewRoot(SyntaxNode root, SyntaxNode node)
        {
            var decl = node.AncestorsAndSelf().OfType<LocalDeclarationStatementSyntax>().FirstOrDefault();
            if (decl != null)
                return root.RemoveNode(decl, SyntaxRemoveOptions.KeepNoTrivia);
            if (node.Parent.IsKind(SyntaxKind.ElseClause))
                return root.RemoveNode(node.Parent, SyntaxRemoveOptions.KeepNoTrivia);

            var statement = node as StatementSyntax;
            if (statement != null)
                return root.RemoveNode(statement, SyntaxRemoveOptions.KeepNoTrivia);

            return root.RemoveNode(node.Parent, SyntaxRemoveOptions.KeepNoTrivia);
        }
    }
}