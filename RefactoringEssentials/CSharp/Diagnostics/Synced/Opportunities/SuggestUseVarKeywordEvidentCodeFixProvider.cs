using Microsoft.CodeAnalysis;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CodeFixes;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;
using RefactoringEssentials.CSharp.CodeRefactorings;

namespace RefactoringEssentials.CSharp.Diagnostics
{

    [ExportCodeFixProvider(LanguageNames.CSharp), System.Composition.Shared]
    public class SuggestUseVarKeywordEvidentCodeFixProvider : CodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds
        {
            get
            {
                return ImmutableArray.Create(CSharpDiagnosticIDs.SuggestUseVarKeywordEvidentAnalyzerID);
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
            if (node == null)
                return;
            TypeSyntax type = null;
            var varDecl = ReplaceExplicitTypeWithVarCodeRefactoringProvider.GetVariableDeclarationStatement(node);
            if (varDecl != null && varDecl.Parent is BaseFieldDeclarationSyntax)
                return;
            if (varDecl != null)
                type = varDecl.Type;
            var foreachStmt = ReplaceExplicitTypeWithVarCodeRefactoringProvider.GetForeachStatement(node);
            if (foreachStmt != null)
                type = foreachStmt.Type;
            if (type == null || type.IsVar)
                return;
            context.RegisterCodeFix(CodeActionFactory.Create(node.Span, diagnostic.Severity, "Use 'var' keyword", t2 => Task.FromResult(ReplaceExplicitTypeWithVarCodeRefactoringProvider.PerformAction(document, root, type))), diagnostic);
        }
    }
}