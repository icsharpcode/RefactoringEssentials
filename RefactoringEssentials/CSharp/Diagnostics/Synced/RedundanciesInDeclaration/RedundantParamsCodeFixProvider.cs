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
            var node = root.FindNode(context.Span) as ParameterSyntax;
            if (node == null)
                return;

            if (!node.Modifiers.Any(x => x.IsKind(SyntaxKind.ParamsKeyword)))
                return;

            var oldParameterNode = node;
            var paramList = node.Parent as ParameterListSyntax;
            if (paramList == null)
                return;

            //var newRoot = root.ReplaceNode(
            //                            oldParameterNode.Parent as ParameterListSyntax,
            //                            paramList.WithParameters
            //                            (SyntaxFactory.SeparatedList(paramList.Parameters.ToArray()))
            //                            .WithLeadingTrivia(node.GetLeadingTrivia())
            //                            .WithTrailingTrivia(node.GetTrailingTrivia()))
            //                            .WithAdditionalAnnotations(Formatter.Annotation);

            //var paramsKeyword = (node.Modifiers.FirstOrDefault(x => x.IsKind(SyntaxKind.ParamsKeyword)));
            //var indexParams = node.Modifiers.IndexOf(paramsKeyword);
            //var syntaxListWithoutParams = node.Modifiers.RemoveAt(indexParams);
            //node.ReplaceToken(paramsKeyword, syntaxListWithoutParams.AsEnumerable());
            context.RegisterCodeFix(CodeActionFactory.Create(node.Span, diagnostic.Severity, "Remove 'params' modifier", token =>
            {
              var newNode = SyntaxFactory.Parameter(node.AttributeLists,node.Modifiers.Remove(SyntaxFactory.Token(SyntaxKind.ParamsKeyword)),node.Type,node.Identifier,node.Default);
             var newRoot = root.ReplaceNode(node, newNode);
             return Task.FromResult(document.WithSyntaxRoot(newRoot));
            }), diagnostic);
            //context.RegisterCodeFix(CodeActionFactory.Create(node.SKCpan, diagnostic.Severity, , document.WithSyntaxRoot(newRoot)), diagnostic);
        }
    }
}