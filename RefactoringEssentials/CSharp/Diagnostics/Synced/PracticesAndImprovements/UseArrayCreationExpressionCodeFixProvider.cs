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
    public class UseArrayCreationExpressionCodeFixProvider : CodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds
        {
            get
            {
                return ImmutableArray.Create(CSharpDiagnosticIDs.UseArrayCreationExpressionAnalyzerID);
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
            var node = root.FindNode(context.Span) as InvocationExpressionSyntax;
            if (node == null)
                return;
            
            var newArgumentList = node.ArgumentList.Arguments.RemoveAt(0);

            context.RegisterCodeFix(
    CodeActionFactory.Create(node.Span, diagnostic.Severity, "Convert to 'return' statement", token =>
    {
        var newArrayInstantiation = SyntaxFactory.ArrayCreationExpression(SyntaxFactory.Token(SyntaxKind.NewKeyword),
            null, null, null);
        var newRoot = root.ReplaceNode(node,
newArrayInstantiation.WithLeadingTrivia(node.GetLeadingTrivia())
         .WithAdditionalAnnotations(Formatter.Annotation));
        return Task.FromResult(document.WithSyntaxRoot(newRoot));
    }), diagnostic);

            //if (!node.IsKind(SyntaxKind.BaseList))
            //	continue;
            context.RegisterCodeFix(CodeActionFactory.Create(node.Span, diagnostic.Severity, "Replace with 'new'", document.WithSyntaxRoot(newRoot)), diagnostic);
        }
    }
}