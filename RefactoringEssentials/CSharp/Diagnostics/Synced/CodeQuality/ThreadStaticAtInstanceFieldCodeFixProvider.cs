using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CodeFixes;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;
using Microsoft.CodeAnalysis.Formatting;

namespace RefactoringEssentials.CSharp.Diagnostics
{

    [ExportCodeFixProvider(LanguageNames.CSharp), System.Composition.Shared]
    public class ThreadStaticAtInstanceFieldCodeFixProvider : CodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds
        {
            get
            {
                return ImmutableArray.Create(CSharpDiagnosticIDs.ThreadStaticAtInstanceFieldAnalyzerID);
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
            var diagnostic = diagnostics.First();
            var node = root.FindToken(context.Span.Start).Parent.AncestorsAndSelf().OfType<AttributeSyntax>().FirstOrDefault();
            if (node == null)
                return;
            context.RegisterCodeFix(
                CodeActionFactory.Create(
                    node.Span,
                    diagnostic.Severity,
                    GettextCatalog.GetString("Remove attribute"),
                    (arg) =>
                    {
                        var list = node.Parent as AttributeListSyntax;
                        if (list.Attributes.Count == 1)
                        {
                            var newRoot = root.RemoveNode(list, SyntaxRemoveOptions.KeepNoTrivia);
                            return Task.FromResult(document.WithSyntaxRoot(newRoot));
                        }
                        var newRoot2 = root.RemoveNode(node, SyntaxRemoveOptions.KeepNoTrivia);
                        return Task.FromResult(document.WithSyntaxRoot(newRoot2));
                    }
                ),
                diagnostic
            );

            context.RegisterCodeFix(
                CodeActionFactory.Create(
                    node.Span,
                    diagnostic.Severity,
                    GettextCatalog.GetString("Make the field static"),
                    (arg) =>
                    {
                        var field = node.Parent.Parent as FieldDeclarationSyntax;
                        var newRoot = root.ReplaceNode(field, field.AddModifiers(SyntaxFactory.Token(SyntaxKind.StaticKeyword)).WithAdditionalAnnotations(Formatter.Annotation));
                        return Task.FromResult(document.WithSyntaxRoot(newRoot));
                    }
                ),
                diagnostic
            );
        }
    }
}