using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Formatting;

namespace RefactoringEssentials.CSharp.Diagnostics
{
    [ExportCodeFixProvider(LanguageNames.CSharp), System.Composition.Shared]
    public class RedundantParamsCodeFixProvider : CodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds
        {
            get
            {
                return ImmutableArray.Create(CSharpDiagnosticIDs.RedundantParamsAnalyzerID);
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
            var fullParameterNode = root.FindNode(diagnostic.Location.SourceSpan) as ParameterSyntax;
            if (fullParameterNode == null)
                return;

            // Keep all modifiers except the params
            var newModifiers = fullParameterNode.Modifiers.Where(m => !m.IsKind(SyntaxKind.ParamsKeyword));
            var syntaxModifiers = SyntaxTokenList.Create(new SyntaxToken());
            syntaxModifiers.AddRange(newModifiers);


            context.RegisterCodeFix(CodeActionFactory.Create(fullParameterNode.Span, diagnostic.Severity, "Remove 'params' modifier", token =>
            {
             var updatedParameterNode = fullParameterNode.WithModifiers(syntaxModifiers);
             var newRoot = root.ReplaceNode(fullParameterNode, updatedParameterNode);
             return Task.FromResult(document.WithSyntaxRoot(newRoot));
            }), diagnostic);
        }
    }
}