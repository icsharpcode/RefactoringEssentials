using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CodeFixes;

namespace RefactoringEssentials.CSharp.Diagnostics
{
    [ExportCodeFixProvider(LanguageNames.CSharp), System.Composition.Shared]
    public class AccessToStaticMemberViaDerivedTypeCodeFixProvider : CodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds
        {
            get
            {
                return ImmutableArray.Create(CSharpDiagnosticIDs.AccessToStaticMemberViaDerivedTypeAnalyzerID);
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
            var semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);
            var root = semanticModel.SyntaxTree.GetRoot(cancellationToken);
            var diagnostic = diagnostics.First();
            var node = root.FindNode(context.Span);
            if (node == null)
                return;
            var typeInfo = semanticModel.GetSymbolInfo(node.Parent);
            var newType = typeInfo.Symbol.ContainingType.ToMinimalDisplayString(semanticModel, node.SpanStart);
            var newRoot = root.ReplaceNode((SyntaxNode)node, SyntaxFactory.ParseTypeName(newType).WithLeadingTrivia(node.GetLeadingTrivia()));
            context.RegisterCodeFix(CodeActionFactory.Create(node.Span, diagnostic.Severity, string.Format("Use base qualifier '{0}'", newType), document.WithSyntaxRoot(newRoot)), diagnostic);
        }
    }
}