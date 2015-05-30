using System.Linq;
using System.Threading;
using System.Collections.Generic;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.Text;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Formatting;

namespace RefactoringEssentials.CSharp.CodeRefactorings
{
    [ExportCodeRefactoringProvider(LanguageNames.CSharp, Name = "Split local variable declaration and assignment")]
    public class SplitLocalVariableDeclarationAndAssignmentCodeRefactoringProvider : SpecializedCodeRefactoringProvider<VariableDeclaratorSyntax>
    {
        protected override IEnumerable<CodeAction> GetActions(Document document, SemanticModel semanticModel, SyntaxNode root, TextSpan span, VariableDeclaratorSyntax node, CancellationToken cancellationToken)
        {
            var declaration = node.Parent as VariableDeclarationSyntax;
            if (declaration == null || node.Initializer == null || (!node.Identifier.Span.Contains(span) && !node.Initializer.EqualsToken.Span.Contains(span) && node.Initializer.Value.SpanStart != span.Start))
                yield break;
            var forStmt = declaration.Parent as ForStatementSyntax;
            if (forStmt != null)
            {
            }
            else
            {
                var variableDecl = declaration.Parent as LocalDeclarationStatementSyntax;
                if (variableDecl == null || variableDecl.Modifiers.Any(m => m.IsKind(SyntaxKind.ConstKeyword)))
                    yield break;
                var block = variableDecl.Parent as BlockSyntax;
                if (block == null)
                    yield break;
            }

            yield return
                CodeActionFactory.Create(
                    span,
                    DiagnosticSeverity.Info,
                    GettextCatalog.GetString("Split declaration and assignment"),
                    t2 =>
                    {
                        SyntaxNode newRoot;
                        if (forStmt != null)
                        {
                            root = root.TrackNodes(new SyntaxNode[] { forStmt, declaration });
                            newRoot = root.InsertNodesBefore(
                                root.GetCurrentNode(forStmt),
                                new[] {
                                    SyntaxFactory.LocalDeclarationStatement(
                                        SyntaxFactory.VariableDeclaration(
                                            declaration.Type,
                                            SyntaxFactory.SeparatedList(new [] {
                                                SyntaxFactory.VariableDeclarator(node.Identifier)
                                            })
                                        ).WithAdditionalAnnotations(Formatter.Annotation)
                                    ).WithAdditionalAnnotations(Formatter.Annotation)
                                }
                            );
                            newRoot = newRoot.ReplaceNode((SyntaxNode)
                                newRoot.GetCurrentNode(declaration),
                                SyntaxFactory.VariableDeclaration(
                                        SyntaxFactory.ParseTypeName(""),
                                    SyntaxFactory.SeparatedList(new[] {
                                            node
                                    })
                                ).WithAdditionalAnnotations(Formatter.Annotation)
                            );
                        }
                        else
                        {
                            root = root.TrackNodes(new SyntaxNode[] { declaration.Parent, declaration.Type, node });
                            newRoot = root.InsertNodesAfter(
                                root.GetCurrentNode(declaration.Parent),
                                new[] {
                                    SyntaxFactory.ExpressionStatement(
                                        SyntaxFactory.AssignmentExpression(
                                            SyntaxKind.SimpleAssignmentExpression,
                                            SyntaxFactory.IdentifierName(node.Identifier),
                                            node.Initializer.Value
                                        )
                                    ).WithAdditionalAnnotations(Formatter.Annotation)
                                }
                            );
                            newRoot = newRoot.ReplaceNode((SyntaxNode)newRoot.GetCurrentNode(node), SyntaxFactory.VariableDeclarator(node.Identifier).WithAdditionalAnnotations(Formatter.Annotation));

                            if (declaration.Type.ToString() == "var")
                            {
                                var type = semanticModel.GetTypeInfo(declaration.Type).Type;

                                newRoot = newRoot.ReplaceNode((SyntaxNode)
                                    newRoot.GetCurrentNode(declaration.Type),
                                    SyntaxFactory.ParseTypeName(type.ToMinimalDisplayString(semanticModel, declaration.SpanStart))
                                    .WithLeadingTrivia(declaration.Type.GetLeadingTrivia())
                                    .WithTrailingTrivia(declaration.Type.GetTrailingTrivia())
                                    .WithAdditionalAnnotations(Formatter.Annotation)
                                );
                            }
                        }
                        //Console.WriteLine (newRoot);
                        return Task.FromResult(document.WithSyntaxRoot(newRoot));
                    }
                );
        }

    }
}

