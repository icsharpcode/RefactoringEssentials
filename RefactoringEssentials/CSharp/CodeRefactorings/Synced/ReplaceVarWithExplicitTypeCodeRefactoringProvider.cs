using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;
using System;

namespace RefactoringEssentials.CSharp.CodeRefactorings
{
    [ExportCodeRefactoringProvider(LanguageNames.CSharp, Name = "Replaces 'var' with explicit type specification")]
    public class ReplaceVarWithExplicitTypeCodeRefactoringProvider : CodeRefactoringProvider
    {
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
            var token = root.FindToken(span.Start);
            var varDecl = ReplaceExplicitTypeWithVarCodeRefactoringProvider.GetVariableDeclarationStatement(token.Parent);
            ITypeSymbol type;
            TypeSyntax typeSyntax;
            if (varDecl != null)
            {
                var v = varDecl.Variables.FirstOrDefault();
                if (v == null || v.Initializer == null)
                    return;
                type = model.GetTypeInfo(v.Initializer.Value).Type;
                typeSyntax = varDecl.Type;
            }
            else
            {
                var foreachStatement = ReplaceExplicitTypeWithVarCodeRefactoringProvider.GetForeachStatement(token.Parent);
                if (foreachStatement == null)
                {
                    return;
                }
                type = model.GetTypeInfo(foreachStatement.Type).Type;
                typeSyntax = foreachStatement.Type;
            }
            if (type == null || !typeSyntax.IsVar || type.TypeKind == TypeKind.Error || type.TypeKind == TypeKind.Unknown)
                return;
            if (!(type.SpecialType != SpecialType.System_Nullable_T && type.TypeKind != TypeKind.Unknown && !ContainsAnonymousType(type)))
                return;
            context.RegisterRefactoring(
                CodeActionFactory.Create(
                    token.Span,
                    DiagnosticSeverity.Info,
                    GettextCatalog.GetString("To explicit type"),
                    t2 => Task.FromResult(PerformAction(document, root, typeSyntax, type.ToSyntax(model, typeSyntax)))
                )
            );
        }

        static Document PerformAction(Document document, SyntaxNode root, TypeSyntax typeSyntax, TypeSyntax replacementType)
        {
            return document.WithSyntaxRoot(root.ReplaceNode(typeSyntax, replacementType));
        }

        static bool ContainsAnonymousType(ITypeSymbol type)
        {
            if (type.TypeKind == TypeKind.Array && ContainsAnonymousType(((IArrayTypeSymbol)type).ElementType))
                return true;
            return type.TypeKind == TypeKind.Dynamic || type.IsAnonymousType;
        }
    }
}

