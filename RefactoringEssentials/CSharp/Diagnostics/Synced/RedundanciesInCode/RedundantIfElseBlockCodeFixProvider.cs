using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;

namespace RefactoringEssentials.CSharp.Diagnostics
{
	[ExportCodeFixProvider(LanguageNames.CSharp), System.Composition.Shared]
    public class RedundantIfElseBlockCodeFixProvider : CodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds
        {
            get
            {
                return ImmutableArray.Create(CSharpDiagnosticIDs.RedundantIfElseBlockAnalyzerID);
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
            var node = root.FindToken(context.Span.Start).Parent as ElseClauseSyntax;
            if (node == null)
                return;

            context.RegisterCodeFix(CodeAction.Create("Remove redundant 'else'", token =>
            {
                var replacementNodes = new List<SyntaxNode>();
                replacementNodes.Add(((IfStatementSyntax)node.Parent).WithElse(null).WithAdditionalAnnotations(Formatter.Annotation));

                if (node.Statement is BlockSyntax)
                {
                    var bs = (BlockSyntax)node.Statement;
                    replacementNodes.AddRange(bs.Statements.Select(s => s.WithAdditionalAnnotations(Formatter.Annotation)));
                }
                else
                {
                    replacementNodes.Add(node.Statement.WithAdditionalAnnotations(Formatter.Annotation));
                }

                var newRoot = root.ReplaceNode(node.Parent, replacementNodes);
                return Task.FromResult(document.WithSyntaxRoot(newRoot));
            }, string.Empty), diagnostic);
        }
    }
}