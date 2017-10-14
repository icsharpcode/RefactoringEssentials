using System.Linq;
using Microsoft.CodeAnalysis;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CodeFixes;

namespace RefactoringEssentials.CSharp.CodeFixes
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = "Extension methods must be declared static"), System.Composition.Shared]
    public class CS1105ExtensionMethodMustBeDeclaredStaticCodeFixProvider : CodeFixProvider
    {
        const string CS1105 = "CS1105"; // Error CS1105: Extension methods must be static.

        public sealed override ImmutableArray<string> FixableDiagnosticIds
        {
            get { return ImmutableArray.Create(CS1105); }
        }

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
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
            var methodSymbol = model.GetDeclaredSymbol(node);
            if (methodSymbol == null || methodSymbol.IsStatic || !methodSymbol.IsExtensionMethod)
                return;

            context.RegisterCodeFix(
                CodeActionFactory.Create(
                    node.Span,
                    DiagnosticSeverity.Error,
                    GettextCatalog.GetString("Extension methods must be declared static"),
                    t => Task.FromResult(
                        document.WithSyntaxRoot(
                            root.ReplaceNode(
                                (SyntaxNode)node,
                                node.WithModifiers(
                                    node.Modifiers.Add(
                                        SyntaxFactory
                                        .Token(SyntaxKind.StaticKeyword)
                                        .WithTrailingTrivia(SyntaxFactory.SyntaxTrivia(SyntaxKind.WhitespaceTrivia, " "))
                                    )
                                )
                            )
                        )
                    )
                ),
                diagnostic
            );
        }
    }
}