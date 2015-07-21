using System.Linq;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Formatting;

namespace RefactoringEssentials.CSharp.CodeRefactorings
{
    [ExportCodeRefactoringProvider(LanguageNames.CSharp, Name = "Check dictionary key value")]
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
            if (!IsDictionary(type as INamedTypeSymbol) && !type.AllInterfaces.Any(IsDictionary))
                return;
            context.RegisterRefactoring(
                CodeActionFactory.Create(
                    span,
                    DiagnosticSeverity.Info,
                    string.Format(GettextCatalog.GetString("Use 'if ({0}.TryGetValue({1}, out val))'"), elementAccess.Expression, elementAccess.ArgumentList.Arguments.First()),
                    t2 =>
                    {
                        var reservedNames = model.LookupSymbols(elementAccess.SpanStart).Select(s => s.Name);
                        string localVariableName = NameGenerator.EnsureUniqueness("val", reservedNames, true);
                        
                        var parentStatement = elementAccess.Parent.AncestorsAndSelf().OfType<StatementSyntax>().FirstOrDefault();
                        var newParent = SyntaxFactory.IfStatement(
                                SyntaxFactory.InvocationExpression(
                                    SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, elementAccess.Expression, SyntaxFactory.IdentifierName("TryGetValue")),
                                    SyntaxFactory.ArgumentList(elementAccess.ArgumentList.Arguments.Add(SyntaxFactory.Argument(SyntaxFactory.IdentifierName(localVariableName)).WithRefOrOutKeyword(SyntaxFactory.Token(SyntaxKind.OutKeyword))))
                                ),
                                parentStatement.ReplaceNode(elementAccess, SyntaxFactory.IdentifierName(localVariableName))
                            ).WithAdditionalAnnotations(Formatter.Annotation);
                        var dict = IsDictionary(elementType.Type as INamedTypeSymbol) ? elementType.Type : elementType.Type.AllInterfaces.First(IsDictionary);

                        var varDecl = SyntaxFactory.LocalDeclarationStatement(
                            SyntaxFactory.VariableDeclaration(
                                dict.GetTypeArguments()[1].GenerateTypeSyntax(),
                                SyntaxFactory.SeparatedList(new[] { SyntaxFactory.VariableDeclarator(localVariableName) })
                            )
                        ).WithAdditionalAnnotations(Formatter.Annotation);

                        SyntaxNode newRoot;

                        if (parentStatement.Parent.IsKind(SyntaxKind.Block))
                        {
                            newRoot = root.ReplaceNode(parentStatement, new SyntaxNode[] { varDecl, newParent });
                        }
                        else
                        {
                            newRoot = root.ReplaceNode(parentStatement, SyntaxFactory.Block(varDecl, newParent).WithAdditionalAnnotations(Formatter.Annotation));
                        }

                        return Task.FromResult(document.WithSyntaxRoot(newRoot));
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