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
    public class RewriteIfReturnToReturnCodeFixProvider : CodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds
        {
            get
            {
                return ImmutableArray.Create(CSharpDiagnosticIDs.RewriteIfReturnToReturnAnalyzerID);
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
            if (node == null)
                return;

            context.RegisterCodeFix(
                CodeActionFactory.Create(node.Span, diagnostic.Severity, "Convert to 'return' statement", token =>
                {
                    var statementCondition = (node as IfStatementSyntax).Condition;
                    var newReturn = SyntaxFactory.ReturnStatement(SyntaxFactory.Token(SyntaxKind.ReturnKeyword),
                        statementCondition, SyntaxFactory.Token(SyntaxKind.SemicolonToken));
                    var newRoot = root.ReplaceNode(node,
                        newReturn.WithLeadingTrivia(node.GetLeadingTrivia())
                            .WithAdditionalAnnotations(Formatter.Annotation));
                    return Task.FromResult(document.WithSyntaxRoot(newRoot));
                }), diagnostic);
        }
    }
}