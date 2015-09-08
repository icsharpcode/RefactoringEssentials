using System.Collections.Generic;
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
            var node = root.FindNode(context.Span) as InitializerExpressionSyntax;
            //if (!node.IsKind(SyntaxKind.BaseList))
            //	continue;
            if (node == null)
                return;

            var elementCount = node.Expressions.Count;
            if (elementCount > node.Expressions.GetSeparators().Count())
                return;

            var redundantComma = node.Expressions.GetSeparator(elementCount - 1);
            var newRoot = root.ReplaceNode(
                node,
                node
                .WithExpressions(SyntaxFactory.SeparatedList(node.Expressions.ToArray()))
                .WithAdditionalAnnotations(Formatter.Annotation));

            context.RegisterCodeFix(CodeActionFactory.Create(node.Span, diagnostic.Severity, "Remove ','", document.WithSyntaxRoot(newRoot)), diagnostic);
        }
    }
}
