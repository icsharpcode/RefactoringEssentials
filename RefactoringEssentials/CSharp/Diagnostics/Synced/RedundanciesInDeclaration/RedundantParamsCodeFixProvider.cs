using Microsoft.CodeAnalysis;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CodeFixes;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
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
            var node = root.FindNode(context.Span);
            //if (!node.IsKind(SyntaxKind.BaseList))
            //	continue;
            var redundantParameterWithModifier = FindParameterWithParamsModifier(((MethodDeclarationSyntax)node).ParameterList);
            if (redundantParameterWithModifier == null)
                return;

            var paramsKeyword = (redundantParameterWithModifier.Modifiers.FirstOrDefault(x => x.IsKind(SyntaxKind.ParamsKeyword)));
            redundantParameterWithModifier.Modifiers.Remove(paramsKeyword);

            //var newRoot = root.RemoveNode(node, SyntaxRemoveOptions.KeepNoTrivia);
            //context.RegisterCodeFix(CodeActionFactory.Create(node.Span, diagnostic.Severity, "Remove 'params' modifier", document.WithSyntaxRoot(newRoot)), diagnostic);
        }


        public ParameterSyntax FindParameterWithParamsModifier( ParameterListSyntax parameterList)
        {
            return parameterList.Parameters.FirstOrDefault(p => p.Modifiers.Any(x => x.IsKind(SyntaxKind.ParamsKeyword)));
        }
    }
}
