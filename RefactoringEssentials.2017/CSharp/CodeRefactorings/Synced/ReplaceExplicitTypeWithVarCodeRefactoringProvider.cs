using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;

namespace RefactoringEssentials.CSharp.CodeRefactorings
{
    [ExportCodeRefactoringProvider(LanguageNames.CSharp, Name = "Replace type with 'var'")]
    public class ReplaceExplicitTypeWithVarCodeRefactoringProvider : CodeRefactoringProvider
    {
        internal static VariableDeclarationSyntax GetVariableDeclarationStatement(SyntaxNode token)
        {
            return token.Parent as VariableDeclarationSyntax;
        }

        internal static ForEachStatementSyntax GetForeachStatement(SyntaxNode token)
        {
            return token.Parent as ForEachStatementSyntax;
        }

        #region CodeRefactoringProvider implementation

        public override async Task ComputeRefactoringsAsync(CodeRefactoringContext context)
        {
            var document = context.Document;
            if (document.Project.Solution.Workspace.Kind == WorkspaceKind.MiscellaneousFiles)
                return;
            var span = context.Span;
            if (!span.IsEmpty)
                return;
            var cancellationToken = context.CancellationToken;
            if (cancellationToken.IsCancellationRequested)
                return;
            var model = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);
            if (model.IsFromGeneratedCode(cancellationToken))
                return;
            var root = await model.SyntaxTree.GetRootAsync(cancellationToken).ConfigureAwait(false);
            var parseOptions = root.SyntaxTree.Options as CSharpParseOptions;
            if (parseOptions != null && parseOptions.LanguageVersion < LanguageVersion.CSharp3)
                return;

            var token = root.FindToken(span.Start);

            TypeSyntax type = null;
            var varDecl = GetVariableDeclarationStatement(token.Parent);
            if (varDecl != null && varDecl.Parent is BaseFieldDeclarationSyntax)
                return;
            if ((varDecl != null) && (varDecl.Variables.Count > 1))
                return;
            if (varDecl != null)
                type = varDecl.Type;
            var foreachStmt = GetForeachStatement(token.Parent);
            if (foreachStmt != null)
                type = foreachStmt.Type;
            if (type == null || type.IsVar)
                return;
            context.RegisterRefactoring(
                CodeActionFactory.Create(
                    token.Span,
                    DiagnosticSeverity.Info,
                    GettextCatalog.GetString("To 'var'"),
                    t2 => Task.FromResult(PerformAction(document, root, type))
                )
            );
        }

        #endregion

        internal static Document PerformAction(Document document, SyntaxNode root, TypeSyntax type)
        {
            var newRoot = root.ReplaceNode((SyntaxNode)
                type,
                SyntaxFactory.IdentifierName("var")
                .WithLeadingTrivia(type.GetLeadingTrivia())
                .WithTrailingTrivia(type.GetTrailingTrivia())
            );
            return document.WithSyntaxRoot(newRoot);
        }
    }
}
