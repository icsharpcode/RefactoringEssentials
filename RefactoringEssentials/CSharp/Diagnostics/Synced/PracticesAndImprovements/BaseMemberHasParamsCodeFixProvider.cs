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
    public class BaseMemberHasParamsCodeFixProvider : CodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds
        {
            get
            {
                return ImmutableArray.Create(CSharpDiagnosticIDs.BaseMemberHasParamsAnalyzerID);
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
            var node = root.FindNode(context.Span);
            if (!node.IsKind(SyntaxKind.Parameter))
                return;
            var param = (ParameterSyntax)node;
            var newRoot = root.ReplaceNode(node, param.AddModifiers(SyntaxFactory.Token(SyntaxKind.ParamsKeyword)).WithAdditionalAnnotations(Formatter.Annotation));
            context.RegisterCodeFix(CodeActionFactory.Create(node.Span, diagnostic.Severity, "Add 'params' modifier", document.WithSyntaxRoot(newRoot)), diagnostic);
        }
    }
}