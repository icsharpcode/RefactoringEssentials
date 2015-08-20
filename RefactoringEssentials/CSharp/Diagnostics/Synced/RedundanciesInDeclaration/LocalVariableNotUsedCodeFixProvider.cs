using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;

namespace RefactoringEssentials.CSharp.Diagnostics
{
    [ExportCodeFixProvider(LanguageNames.CSharp), System.Composition.Shared]
    public class LocalVariableNotUsedCodeFixProvider : CodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds
        {
            get
            {
                return ImmutableArray.Create(CSharpDiagnosticIDs.LocalVariableNotUsedAnalyzerID);
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
            var node = root.FindNode(context.Span) as VariableDeclaratorSyntax;
            if (node == null)
                return;
            int i, j;
            j = 1;
            var variableDeclarationSyntax = node.Parent as VariableDeclarationSyntax;
            if (variableDeclarationSyntax == null)
                return;

            if (variableDeclarationSyntax.Variables.Count == 1)
            {
                var newRoot = root.RemoveNode(node.Parent.Parent, SyntaxRemoveOptions.KeepNoTrivia);
                context.RegisterCodeFix(CodeActionFactory.Create(node.Span, diagnostic.Severity, "Remove unused local variable", document.WithSyntaxRoot(newRoot)), diagnostic);
            }
            else
            {
                var methodDeclaration = variableDeclarationSyntax.Parent.Parent as MethodDeclarationSyntax;
                if (methodDeclaration == null)
                    return; 

                foreach (var variable in variableDeclarationSyntax.Variables)
                {
                    
                }
            }
        }
    }
}