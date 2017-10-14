using System.Linq;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Formatting;

namespace RefactoringEssentials.CSharp.CodeRefactorings
{
    [ExportCodeRefactoringProvider(LanguageNames.CSharp, Name = "Check collection index value")]
    public class CheckCollectionIndexValueCodeRefactoringProvider : CodeRefactoringProvider
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
            if (token.Parent == null)
                return;

            var bracketedList = token.Parent.AncestorsAndSelf().OfType<BracketedArgumentListSyntax>().FirstOrDefault();
            if (bracketedList == null)
                return;
            var elementAccess = bracketedList.AncestorsAndSelf().OfType<ElementAccessExpressionSyntax>().FirstOrDefault();
            if (elementAccess == null)
                return;
            var elementType = model.GetTypeInfo(elementAccess.Expression);
            var type = elementType.Type;
            if (type == null)
                return;
            if (!IsCollection(type as INamedTypeSymbol) && !type.AllInterfaces.Any(IsCollection))
                return;
            var argument = elementAccess.ArgumentList?.Arguments.FirstOrDefault();
            if (argument == null)
                return;
            context.RegisterRefactoring(
                CodeActionFactory.Create(
                    span,
                    DiagnosticSeverity.Info,
                    string.Format(GettextCatalog.GetString("Check 'if ({0}.Count > {1})'"), elementAccess.Expression, argument),
                    t2 =>
                    {
                        var parentStatement = elementAccess.Parent.AncestorsAndSelf().OfType<StatementSyntax>().FirstOrDefault();

                        var newParent = SyntaxFactory.IfStatement(
                            SyntaxFactory.BinaryExpression(
                                SyntaxKind.GreaterThanExpression,
                                SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, elementAccess.Expression, SyntaxFactory.IdentifierName("Count")),
                                argument.Expression
                            ),
                            parentStatement
                        );

                        return Task.FromResult(document.WithSyntaxRoot(root.ReplaceNode((SyntaxNode)parentStatement, newParent.WithAdditionalAnnotations(Formatter.Annotation))));
                    }
                )
            );
        }

        static bool IsCollection(INamedTypeSymbol type)
        {
            return type != null && type.Name == "ICollection" && type.ContainingNamespace.ToDisplayString() == "System.Collections";
        }
    }
}