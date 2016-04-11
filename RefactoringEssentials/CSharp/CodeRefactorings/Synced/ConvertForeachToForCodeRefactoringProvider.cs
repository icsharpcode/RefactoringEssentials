using System.Linq;
using System.Collections.Generic;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Text;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Formatting;

namespace RefactoringEssentials.CSharp.CodeRefactorings
{
    /// <summary>
    /// Converts a foreach loop to for.
    /// </summary>

    [ExportCodeRefactoringProvider(LanguageNames.CSharp, Name = "Convert 'foreach' loop to 'for'")]
    public class ConvertForeachToForCodeRefactoringProvider : CodeRefactoringProvider
    {
        static readonly string[] VariableNames = { "i", "j", "k", "l", "n", "m", "x", "y", "z" };
        static readonly string[] CollectionNames = { "list" };
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

            bool hasIndexAccess;
            var foreachStatement = GetForeachStatement(model, root, span, out hasIndexAccess);
            if (foreachStatement == null || foreachStatement.Statement == null)
                return;

            string name = GetName(model, span, VariableNames);
            if (name == null) // very unlikely, but just in case ...
                return;
            context.RegisterRefactoring(CodeActionFactory.Create(
                foreachStatement.ForEachKeyword.Span,
                DiagnosticSeverity.Info,
                GettextCatalog.GetString("To 'for'"),
                t2 =>
                {
                    var expressionTypeInfo = model.GetTypeInfo(foreachStatement.Expression);
                    var countProperty = GetCountProperty(expressionTypeInfo.Type);
                    var inExpression = foreachStatement.Expression;
                    var initializer = hasIndexAccess ?
                        SyntaxFactory.VariableDeclaration(
                            SyntaxFactory.ParseTypeName("int"),
                            SyntaxFactory.SeparatedList<VariableDeclaratorSyntax>(
                                SyntaxFactory.SeparatedList<VariableDeclaratorSyntax>(
                                    new[] {
                                        SyntaxFactory.VariableDeclarator(
                                            SyntaxFactory.Identifier(name),
                                            null,
                                            SyntaxFactory.EqualsValueClause(SyntaxFactory.ParseExpression("0"))
                                        )
                                    }
                                )
                            )
                        )
                        :
                        SyntaxFactory.VariableDeclaration(
                            SyntaxFactory.ParseTypeName("var"),
                            SyntaxFactory.SeparatedList<VariableDeclaratorSyntax>(
                                SyntaxFactory.SeparatedList<VariableDeclaratorSyntax>(
                                    new[] {
                                        SyntaxFactory.VariableDeclarator(
                                            SyntaxFactory.Identifier(name),
                                            null,
                                            SyntaxFactory.EqualsValueClause(SyntaxFactory.InvocationExpression(SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, inExpression,  SyntaxFactory.IdentifierName("GetEnumerator"))))
                                        )
                                    }
                                )
                            )
                        );
                    var id1 = SyntaxFactory.Identifier(name);
                    var id2 = id1;
                    var id3 = id1;
                    StatementSyntax declarationStatement = null;

                    if (inExpression is ObjectCreationExpressionSyntax || inExpression.IsKind(SyntaxKind.ImplicitArrayCreationExpression))
                    {
                        string listName = GetName(model, span, CollectionNames) ?? "col";

                        declarationStatement = SyntaxFactory.LocalDeclarationStatement(SyntaxFactory.VariableDeclaration(
                            SyntaxFactory.ParseTypeName("var"),
                            SyntaxFactory.SeparatedList<VariableDeclaratorSyntax>(
                                SyntaxFactory.SeparatedList<VariableDeclaratorSyntax>(
                                    new[] {
                                        SyntaxFactory.VariableDeclarator(
                                            SyntaxFactory.Identifier(listName),
                                            null,
                                            SyntaxFactory.EqualsValueClause(inExpression)
                                        )
                                    }
                                )
                            )
                        ));

                        inExpression = SyntaxFactory.IdentifierName(listName);
                    }

                    var variableDeclarationStatement = SyntaxFactory.LocalDeclarationStatement(SyntaxFactory.VariableDeclaration(
                        foreachStatement.Type,
                        SyntaxFactory.SeparatedList<VariableDeclaratorSyntax>(
                            SyntaxFactory.SeparatedList<VariableDeclaratorSyntax>(
                                new[] {
                                    SyntaxFactory.VariableDeclarator(
                                        foreachStatement.Identifier,
                                        null,
                                        SyntaxFactory.EqualsValueClause(
                                            hasIndexAccess ?
                                            (ExpressionSyntax)SyntaxFactory.ElementAccessExpression(inExpression, SyntaxFactory.BracketedArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>(new [] { SyntaxFactory.Argument(SyntaxFactory.IdentifierName(id3)) } ))) :
                                            SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, SyntaxFactory.IdentifierName(id2), SyntaxFactory.IdentifierName("Current"))
                                        )
                                    )
                                }
                            )
                        )
                    ));

                    var statements = new List<StatementSyntax>();
                    statements.Add(variableDeclarationStatement);

                    var incrementors = new List<ExpressionSyntax>();

                    if (hasIndexAccess)
                        incrementors.Add(SyntaxFactory.PostfixUnaryExpression(SyntaxKind.PostIncrementExpression, SyntaxFactory.IdentifierName(id2)));

                    var block = foreachStatement.Statement as BlockSyntax;
                    if (block != null)
                    {
                        foreach (var stmt in block.Statements)
                        {
                            statements.Add(stmt);
                        }
                    }
                    else
                    {
                        statements.Add(foreachStatement.Statement);
                    }

                    var forStatement = SyntaxFactory.ForStatement(
                        initializer,
                        SyntaxFactory.SeparatedList<ExpressionSyntax>(),
                        hasIndexAccess ? (ExpressionSyntax)SyntaxFactory.BinaryExpression(SyntaxKind.LessThanExpression, SyntaxFactory.IdentifierName(id1), SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, inExpression, SyntaxFactory.IdentifierName(countProperty))) :
                        SyntaxFactory.InvocationExpression(SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, SyntaxFactory.IdentifierName(id2), SyntaxFactory.IdentifierName("MoveNext"))),
                        SyntaxFactory.SeparatedList<ExpressionSyntax>(incrementors),
                        SyntaxFactory.Block(statements)
                    );

                    //					if (hasIndexAccess) {
                    //						script.Link (initializer.Variables.First ().NameToken, id1, id2, id3);
                    //					} else {
                    //						script.Link (initializer.Variables.First ().NameToken, id1, id2);
                    //					}

                    SyntaxNode newRoot;
                    if (declarationStatement != null)
                    {
                        newRoot = root.ReplaceNode((SyntaxNode)foreachStatement,
                            new[] {
                                declarationStatement.WithAdditionalAnnotations(Formatter.Annotation).WithLeadingTrivia(foreachStatement.GetLeadingTrivia()),
                                forStatement.WithAdditionalAnnotations(Formatter.Annotation)
                            });
                    }
                    else
                    {
                        newRoot = root.ReplaceNode((SyntaxNode)foreachStatement,
                            forStatement.WithAdditionalAnnotations(Formatter.Annotation).WithLeadingTrivia(foreachStatement.GetLeadingTrivia()));
                    }
                    return Task.FromResult(document.WithSyntaxRoot(newRoot));
                }
            ));

            if (hasIndexAccess)
            {
                context.RegisterRefactoring(CodeActionFactory.Create(
                    span,
                    DiagnosticSeverity.Info,
                    GettextCatalog.GetString("Convert 'foreach' loop to optimized 'for'"),
                    t2 =>
                    {
                        var expressionTypeInfo = model.GetTypeInfo(foreachStatement.Expression);
                        var countProperty = GetCountProperty(expressionTypeInfo.Type);
                        var inExpression = foreachStatement.Expression;

                        string optimizedUpperBound = GetBoundName(inExpression) + countProperty;

                        var initializer =
                            SyntaxFactory.VariableDeclaration(
                                SyntaxFactory.ParseTypeName("int"),
                                SyntaxFactory.SeparatedList<VariableDeclaratorSyntax>(
                                    SyntaxFactory.SeparatedList<VariableDeclaratorSyntax>(
                                        new[] {
                                            SyntaxFactory.VariableDeclarator(
                                                SyntaxFactory.Identifier(name),
                                                null,
                                                SyntaxFactory.EqualsValueClause(SyntaxFactory.ParseExpression("0"))
                                            ),
                                            SyntaxFactory.VariableDeclarator(
                                                SyntaxFactory.Identifier(optimizedUpperBound),
                                                null,
                                                SyntaxFactory.EqualsValueClause(SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, inExpression, SyntaxFactory.IdentifierName(countProperty)))
                                            )
                                        }
                                    )
                                )
                            );
                        var id1 = SyntaxFactory.Identifier(name);
                        var id2 = id1;
                        var id3 = id1;
                        StatementSyntax declarationStatement = null;

                        if (inExpression is ObjectCreationExpressionSyntax || inExpression.IsKind(SyntaxKind.ImplicitArrayCreationExpression))
                        {
                            string listName = GetName(model, span, CollectionNames) ?? "col";

                            declarationStatement = SyntaxFactory.LocalDeclarationStatement(SyntaxFactory.VariableDeclaration(
                                SyntaxFactory.ParseTypeName("var"),
                                SyntaxFactory.SeparatedList<VariableDeclaratorSyntax>(
                                    SyntaxFactory.SeparatedList<VariableDeclaratorSyntax>(
                                        new[] {
                                            SyntaxFactory.VariableDeclarator(
                                                SyntaxFactory.Identifier(listName),
                                                null,
                                                SyntaxFactory.EqualsValueClause(inExpression)
                                            )
                                        }
                                    )
                                )
                            ));

                            inExpression = SyntaxFactory.IdentifierName(listName);
                        }

                        var variableDeclarationStatement = SyntaxFactory.LocalDeclarationStatement(SyntaxFactory.VariableDeclaration(
                            foreachStatement.Type,
                            SyntaxFactory.SeparatedList<VariableDeclaratorSyntax>(
                                SyntaxFactory.SeparatedList<VariableDeclaratorSyntax>(
                                    new[] {
                                        SyntaxFactory.VariableDeclarator(
                                            foreachStatement.Identifier,
                                            null,
                                            SyntaxFactory.EqualsValueClause(
                                                hasIndexAccess ?
                                                (ExpressionSyntax)SyntaxFactory.ElementAccessExpression(inExpression, SyntaxFactory.BracketedArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>(new [] { SyntaxFactory.Argument(SyntaxFactory.IdentifierName(id3)) } ))) :
                                                SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, SyntaxFactory.IdentifierName(id2), SyntaxFactory.IdentifierName("Current"))
                                            )
                                        )
                                    }
                                )
                            )
                        ));

                        var statements = new List<StatementSyntax>();
                        statements.Add(variableDeclarationStatement);

                        var incrementors = new List<ExpressionSyntax>();

                        if (hasIndexAccess)
                            incrementors.Add(SyntaxFactory.PostfixUnaryExpression(SyntaxKind.PostIncrementExpression, SyntaxFactory.IdentifierName(id2)));

                        var block = foreachStatement.Statement as BlockSyntax;
                        if (block != null)
                        {
                            foreach (var stmt in block.Statements)
                            {
                                statements.Add(stmt);
                            }
                        }
                        else
                        {
                            statements.Add(foreachStatement.Statement);
                        }

                        var forStatement = SyntaxFactory.ForStatement(
                            initializer,
                            SyntaxFactory.SeparatedList<ExpressionSyntax>(),
                            SyntaxFactory.BinaryExpression(SyntaxKind.LessThanExpression, SyntaxFactory.IdentifierName(id1), SyntaxFactory.IdentifierName(optimizedUpperBound)),
                            SyntaxFactory.SeparatedList<ExpressionSyntax>(incrementors),
                            SyntaxFactory.Block(statements)
                        );

                        //						script.Link (initializer.Variables.First ().NameToken, id1, id2, id3);

                        SyntaxNode newRoot;
                        if (declarationStatement != null)
                        {
                            newRoot = root.ReplaceNode((SyntaxNode)foreachStatement,
                                new[] {
                                    declarationStatement.WithAdditionalAnnotations(Formatter.Annotation).WithLeadingTrivia(foreachStatement.GetLeadingTrivia()),
                                    forStatement.WithAdditionalAnnotations(Formatter.Annotation)
                                });
                        }
                        else
                        {
                            newRoot = root.ReplaceNode((SyntaxNode)foreachStatement,
                                forStatement.WithAdditionalAnnotations(Formatter.Annotation).WithLeadingTrivia(foreachStatement.GetLeadingTrivia()));
                        }
                        return Task.FromResult(document.WithSyntaxRoot(newRoot));
                    }
                ));
            }
        }

        static string GetCountProperty(ITypeSymbol type)
        {
            return type.TypeKind == TypeKind.Array || type.SpecialType == SpecialType.System_String ? "Length" : "Count";
        }

        static ForEachStatementSyntax GetForeachStatement(SemanticModel context, SyntaxNode root, TextSpan span, out bool hasIndexAccess)
        {
            var token = root.FindToken(span.Start, false);
            if (!token.IsKind(SyntaxKind.ForEachKeyword) || !token.Span.Contains(span))
            {
                hasIndexAccess = false;
                return null;
            }

            var result = token.Parent as ForEachStatementSyntax;
            if (result == null)
            {
                hasIndexAccess = false;
                return null;
            }

            var collection = context.GetTypeInfo(result.Expression);
            if (collection.Type == null)
            {
                hasIndexAccess = false;
                return null;
            }

            hasIndexAccess = collection.Type.TypeKind == TypeKind.Array || collection.Type.GetMembers().OfType<IPropertySymbol>().Any(p => p.IsIndexer);
            return result;
        }

        static string GetName(SemanticModel model, TextSpan span, string[] variableNames)
        {
            var symbols = model.LookupSymbols(span.Start).ToList();
            for (int i = 0; i < 1000; i++)
            {
                foreach (var vn in variableNames)
                {
                    string id = i > 0 ? vn + i : vn;
                    if (symbols.All(s => s.Name != id))
                        return id;
                }
            }
            return null;
        }

        static string GetBoundName(ExpressionSyntax inExpression)
        {
            var ie = inExpression as IdentifierNameSyntax;
            if (ie != null)
                return ie.ToString();
            var mre = inExpression as MemberAccessExpressionSyntax;
            if (mre != null)
                return GetBoundName(mre.Expression) + mre.Name;
            return "max";
        }
    }
}
