using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Formatting;

namespace RefactoringEssentials.CSharp.CodeFixes
{
    [ExportCodeFixProvider(LanguageNames.CSharp), System.Composition.Shared]
    public class CS0132StaticConstructorParameterCodeFixProvider : CodeFixProvider
    {
        public const string CS0132 = "CS0132"; // 'constructor' : a static constructor must be parameterless

        public override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(CS0132);

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
            var node = root.FindNode(context.Span) as ConstructorDeclarationSyntax;
            if (node == null)
                return;
            var newRoot = root.ReplaceNode(node.ParameterList, SyntaxFactory.ParameterList()).WithAdditionalAnnotations(Formatter.Annotation);
            context.RegisterCodeFix(CodeActionFactory.Create(node.Span, diagnostic.Severity, "Remove parameters from static constructor", document.WithSyntaxRoot(newRoot)), diagnostic);
        }
    }
}
