using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;

namespace RefactoringEssentials.CSharp.CodeRefactorings
{
    [ExportCodeRefactoringProvider(LanguageNames.CSharp, Name = "Generate getter")]
    public class GenerateGetterAction : CodeRefactoringProvider
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
            if (!(token.Parent is VariableDeclaratorSyntax))
                return;
            var declarator = (VariableDeclaratorSyntax)token.Parent;
            var fieldDeclaration = declarator.Parent?.Parent as FieldDeclarationSyntax;
            if (fieldDeclaration == null)
                return;
            var enclosingType = fieldDeclaration.Parent as TypeDeclarationSyntax;
            if (enclosingType == null)
                return;
            foreach (var member in enclosingType.Members) {
                if (member is PropertyDeclarationSyntax && ContainsGetter(model, (PropertyDeclarationSyntax)member, declarator))
                    return;
            }
            context.RegisterRefactoring(
                CodeActionFactory.Create(
                    token.Span,
                    DiagnosticSeverity.Info,
                    GettextCatalog.GetString("Generate getter"),
                    t2 => {
                        var newRoot = root.InsertNodesAfter(fieldDeclaration, new[] { GeneratePropertyDeclaration(fieldDeclaration, declarator) });
                        return Task.FromResult(document.WithSyntaxRoot(newRoot));
                    }
                )
            );
        }
	
        static PropertyDeclarationSyntax GeneratePropertyDeclaration(FieldDeclarationSyntax field, VariableDeclaratorSyntax initializer)
        {
            var modifiers = SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword));
            if (field.Modifiers.Any(m => m.IsKind(SyntaxKind.StaticKeyword)))
                modifiers = modifiers.Add(SyntaxFactory.Token(SyntaxKind.StaticKeyword));

            string propertyName = NameProposalService.GetNameProposal(initializer.Identifier.ValueText, SyntaxKind.PropertyDeclaration);
            var block = SyntaxFactory.Block(SyntaxFactory.ReturnStatement(SyntaxFactory.IdentifierName(initializer.Identifier)));
            return SyntaxFactory.PropertyDeclaration(field.Declaration.Type, propertyName)
                .WithModifiers(modifiers)
                .WithAccessorList(
                    SyntaxFactory.AccessorList(SyntaxFactory.SingletonList(
                        SyntaxFactory.AccessorDeclaration(SyntaxKind.GetAccessorDeclaration, block)
                    ))
                ).WithAdditionalAnnotations(Formatter.Annotation);
        }

        static bool ContainsGetter(SemanticModel model, PropertyDeclarationSyntax property, VariableDeclaratorSyntax declarator)
        {
            var symbol = model.GetDeclaredSymbol(declarator);
            if (property.ExpressionBody != null)
                return model.GetSymbolInfo(property.ExpressionBody.Expression).Symbol == symbol;
            var getter = property.AccessorList?.Accessors.FirstOrDefault(a => a.IsKind(SyntaxKind.GetAccessorDeclaration));
            if (getter == null || getter.Body?.Statements.Count != 1)
                return false;
#if RE2017
#warning "Check for get => ExpressionBody!"
#endif
            var ret = getter.Body?.Statements.Single() as ReturnStatementSyntax;
            if (ret == null)
                return false;
            return model.GetSymbolInfo(ret.Expression).Symbol == symbol;
        }
    }
}

