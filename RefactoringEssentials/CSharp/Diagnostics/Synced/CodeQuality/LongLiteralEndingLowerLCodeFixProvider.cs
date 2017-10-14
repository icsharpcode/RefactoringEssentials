using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CodeFixes;

namespace RefactoringEssentials.CSharp.Diagnostics
{
    [ExportCodeFixProvider(LanguageNames.CSharp), System.Composition.Shared]
    public class LongLiteralEndingLowerLCodeFixProvider : CodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds
        {
            get
            {
                return ImmutableArray.Create(CSharpDiagnosticIDs.LongLiteralEndingLowerLAnalyzerID);
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
            String newLiteral = ((LiteralExpressionSyntax)node).Token.Text.ToUpperInvariant();
            char prevChar = newLiteral[newLiteral.Length - 2];
            char lastChar = newLiteral[newLiteral.Length - 1];
            double newLong = 0;
            if (prevChar == 'U' || prevChar == 'L') //match ul, lu, or l. no need to match just u.
                newLong = long.Parse(newLiteral.Remove(newLiteral.Length - 2));
            else if (lastChar == 'L')
                newLong = long.Parse(newLiteral.Remove(newLiteral.Length - 1));
            else
                newLong = long.Parse(newLiteral); //just in case

            var newRoot = root.ReplaceNode((SyntaxNode)node, ((LiteralExpressionSyntax)node).WithToken(SyntaxFactory.Literal(newLiteral, newLong)));
            context.RegisterCodeFix(CodeActionFactory.Create(node.Span, diagnostic.Severity, "Make suffix upper case", document.WithSyntaxRoot(newRoot)), diagnostic);
        }
    }
}