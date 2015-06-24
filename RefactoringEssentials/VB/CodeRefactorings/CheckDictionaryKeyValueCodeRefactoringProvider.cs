using System.Linq;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;
using Microsoft.CodeAnalysis.VisualBasic;
using Microsoft.CodeAnalysis.Formatting;

namespace RefactoringEssentials.VB.CodeRefactorings
{
    [ExportCodeRefactoringProvider(LanguageNames.VisualBasic, Name = "Check dictionary key value")]
    public class CheckDictionaryKeyValueCodeRefactoringProvider : CodeRefactoringProvider
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

            var bracketedList = token.Parent.AncestorsAndSelf().OfType<ArgumentListSyntax>().FirstOrDefault();
            if (bracketedList == null)
                return;
            var elementAccess = bracketedList.AncestorsAndSelf().OfType<InvocationExpressionSyntax>().FirstOrDefault();
            if (elementAccess == null)
                return;
            var elementType = model.GetTypeInfo(elementAccess.Expression);
            if (elementType.Type == null)
                return;
            if (!IsDictionary(elementType.Type as INamedTypeSymbol) && !elementType.Type.AllInterfaces.Any(IsDictionary))
                return;

            context.RegisterRefactoring(
                CodeActionFactory.Create(
                    span,
                    DiagnosticSeverity.Info,
                    string.Format(GettextCatalog.GetString("Check 'If {0}.ContainsKey({1})'"), elementAccess.Expression, elementAccess.ArgumentList.Arguments.First()),
                    t2 =>
                    {
                        var parentStatement = elementAccess.Parent.AncestorsAndSelf().OfType<StatementSyntax>().FirstOrDefault();

                        var newParent = SyntaxFactory.MultiLineIfBlock(
                            SyntaxFactory.IfStatement(
                                SyntaxFactory.Token(SyntaxKind.IfKeyword),
                                SyntaxFactory.InvocationExpression(
                                    SyntaxFactory.MemberAccessExpression(
                                        SyntaxKind.SimpleMemberAccessExpression,
                                        elementAccess.Expression,
                                        SyntaxFactory.Token(SyntaxKind.DotToken),
                                        SyntaxFactory.IdentifierName("ContainsKey")),
                                    SyntaxFactory.ArgumentList(elementAccess.ArgumentList.Arguments)
                                ),
                                SyntaxFactory.Token(SyntaxKind.ThenKeyword)),
                            SyntaxFactory.List(new[] { parentStatement }),
                            SyntaxFactory.List<ElseIfBlockSyntax>(), null
                        ).WithLeadingTrivia(parentStatement.GetLeadingTrivia());
                        
                        return Task.FromResult(document.WithSyntaxRoot(root.ReplaceNode(parentStatement, newParent.WithAdditionalAnnotations(Formatter.Annotation))));
                    }
                )
            );
        }

        static bool IsDictionary(INamedTypeSymbol type)
        {
            return type != null && type.Name == "IDictionary" && type.ContainingNamespace.ToDisplayString() == "System.Collections.Generic";
        }

    }
}