using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CodeFixes;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;

namespace RefactoringEssentials.CSharp.Diagnostics
{

    [ExportCodeFixProvider(LanguageNames.CSharp), System.Composition.Shared]
    public class BaseMethodParameterNameMismatchCodeFixProvider : CodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds
        {
            get
            {
                return ImmutableArray.Create(NRefactoryDiagnosticIDs.BaseMethodParameterNameMismatchAnalyzerID);
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
            if (!node.IsKind(SyntaxKind.Parameter))
                return;
            var renamedParameter = ((ParameterSyntax)node).WithIdentifier(SyntaxFactory.Identifier(diagnostic.Descriptor.CustomTags.First()));
            var newRoot = root.ReplaceNode((SyntaxNode)node, renamedParameter);
            context.RegisterCodeFix(CodeActionFactory.Create(node.Span, diagnostic.Severity, string.Format("Rename to '{0}'", diagnostic.Descriptor.CustomTags.First()), document.WithSyntaxRoot(newRoot)), diagnostic);
        }
    }
}