using Microsoft.CodeAnalysis;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CodeFixes;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace RefactoringEssentials.CSharp.Diagnostics
{

    [ExportCodeFixProvider(LanguageNames.CSharp), System.Composition.Shared]
    public class RedundantExplicitArraySizeCodeFixProvider : CodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds
        {
            get
            {
                return ImmutableArray.Create(CSharpDiagnosticIDs.RedundantExplicitArraySizeAnalyzerID);
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
            var root = await document.GetSyntaxRootAsync(cancellationToken);
            var diagnostic = diagnostics.First();
            var node = root.FindNode(context.Span);
            var arrayType = ((ArrayCreationExpressionSyntax) node)?.Type;
            if (arrayType != null)
            {
                var rs = arrayType.RankSpecifiers;
                var newRoot = root.RemoveNode(rs[0], SyntaxRemoveOptions.KeepNoTrivia);
                context.RegisterCodeFix(CodeActionFactory.Create(node.Span, diagnostic.Severity, "Remove the redundant size indicator", document.WithSyntaxRoot(newRoot)), diagnostic);
            }
            //var arrayType = ((ArrayCreationExpressionSyntax) node).Type;
            //a RemoveNode of arrayType.RankSpecifiers[0] should do it.
        }
    }
}