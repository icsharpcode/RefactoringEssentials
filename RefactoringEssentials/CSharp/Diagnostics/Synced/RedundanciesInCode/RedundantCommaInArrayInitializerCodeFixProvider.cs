using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CodeFixes;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace RefactoringEssentials.CSharp.Diagnostics
{

    [ExportCodeFixProvider(LanguageNames.CSharp), System.Composition.Shared]
    public class RedundantCommaInArrayInitializerCodeFixProvider : CodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds
        {
            get
            {
                return ImmutableArray.Create(CSharpDiagnosticIDs.RedundantCommaInArrayInitializerAnalyzerID);
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
            var commaCount = (node as InitializerExpressionSyntax)?.Expressions.Count;
            if (!commaCount.HasValue)
                return;

            var tokenList = (node as InitializerExpressionSyntax)?.ChildTokens();
            if (tokenList == null)
                return;

            var syntaxTokens = tokenList.ToList();
            var redundantCommaToken = syntaxTokens.ElementAt(commaCount.Value - 1);
            //syntaxTokens.RemoveAt(commaCount.Value-1);
            //var newTokenList = (IEnumerable<SyntaxToken>) syntaxTokens;
            var newlyRoot = root.ReplaceToken(redundantCommaToken, null);
            context.RegisterCodeFix(CodeActionFactory.Create(node.Span, diagnostic.Severity, "Remove ','", document.WithSyntaxRoot(newlyRoot)), diagnostic);
        }
    }
}
