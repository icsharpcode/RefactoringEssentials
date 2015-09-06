using System.Linq;
using System.Threading;
using System.Collections.Generic;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.Text;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;
using Microsoft.CodeAnalysis.VisualBasic;
using Microsoft.CodeAnalysis.Simplification;
using Microsoft.CodeAnalysis.Formatting;
using RefactoringEssentials.Util;

namespace RefactoringEssentials.VB.CodeRefactorings
{
    [ExportCodeRefactoringProvider(LanguageNames.VisualBasic, Name = "Check if parameter is Nothing")]
    /// <summary>
    /// Creates a 'If param Is Nothing Then Throw New System.ArgumentNullException();' contruct for a parameter.
    /// </summary>
    public class CheckIfParameterIsNothingCodeRefactoringProvider : SpecializedCodeRefactoringProvider<ParameterSyntax>
    {
        protected override IEnumerable<CodeAction> GetActions(Document document, SemanticModel semanticModel, SyntaxNode root, TextSpan span, ParameterSyntax node, CancellationToken cancellationToken)
        {
            if (!node.Identifier.Span.Contains(span))
                return Enumerable.Empty<CodeAction>();
            var parameter = node;
            var bodyStatement = parameter.Parent.Parent.Parent as MethodBlockBaseSyntax;
            var lambdaBody = parameter.Parent.Parent.Parent as MultiLineLambdaExpressionSyntax;
            if (bodyStatement == null && lambdaBody == null)
                return Enumerable.Empty<CodeAction>();

            var parameterSymbol = semanticModel.GetDeclaredSymbol(node);
            var type = parameterSymbol.Type;
            if (type == null || type.IsValueType || HasNullCheck(semanticModel, parameterSymbol, (SyntaxNode)bodyStatement ?? lambdaBody))
                return Enumerable.Empty<CodeAction>();
            return new[] { CodeActionFactory.Create(
                node.Identifier.Span,
                DiagnosticSeverity.Info,
                GettextCatalog.GetString ("Add 'Is Nothing' check for parameter"),
                t2 => {
                    var paramName = node.Identifier.ToString();

                    ExpressionSyntax parameterExpr;

                    var parseOptions = root.SyntaxTree.Options as VisualBasicParseOptions;
                    if (parseOptions != null && parseOptions.LanguageVersion < LanguageVersion.VisualBasic14) {
                        parameterExpr = SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal(paramName));
                    } else {
                        parameterExpr = SyntaxFactory.ParseExpression("NameOf(" + paramName + ")");
                    }

                    var ifStatement = SyntaxFactory.IfStatement(SyntaxFactory.IsExpression(SyntaxFactory.IdentifierName(paramName), SyntaxFactory.Token(SyntaxKind.IsKeyword), SyntaxFactory.NothingLiteralExpression(SyntaxFactory.Token(SyntaxKind.NothingKeyword))))
                        .WithThenKeyword(SyntaxFactory.Token(SyntaxKind.ThenKeyword));

                    var statements = SyntaxFactory.SingletonList<StatementSyntax>(SyntaxFactory.ThrowStatement(
                            SyntaxFactory.ObjectCreationExpression(
                                SyntaxFactory.ParseTypeName("System.ArgumentNullException")
                            )
                            .WithArgumentList(SyntaxFactory.ArgumentList(
                                SyntaxFactory.SingletonSeparatedList<ArgumentSyntax>(
                                    SyntaxFactory.SimpleArgument(
                                        parameterExpr
                                    )
                                )
                            ))
                    ));

                    var ifBlock = SyntaxFactory.MultiLineIfBlock(ifStatement, statements, default(SyntaxList<ElseIfBlockSyntax>), null)
                        .WithAdditionalAnnotations(Formatter.Annotation, Simplifier.Annotation);

                    SyntaxNode newRoot;
                    if (bodyStatement != null)
                    {
                        var newStatements = bodyStatement.Statements.Insert(0, ifBlock);
                        var newBody = bodyStatement.WithStatements(newStatements);
                        newRoot = root.ReplaceNode(bodyStatement, newBody);
                    }
                    else
                    {
                        var newStatements = lambdaBody.Statements.Insert(0, ifBlock);
                        var newBody = lambdaBody.WithStatements(newStatements);
                        newRoot = root.ReplaceNode(lambdaBody, newBody);
                    }
                    return Task.FromResult(document.WithSyntaxRoot(newRoot));
                }
            ) };
        }

        static bool HasNullCheck(SemanticModel semanticModel, IParameterSymbol parameterSymbol, SyntaxNode body)
        {
            foreach (var ifStmt in body.DescendantNodes().OfType<MultiLineIfBlockSyntax>())
            {
                var cond = ifStmt.IfStatement.Condition as BinaryExpressionSyntax;
                if (cond == null || !cond.IsKind(SyntaxKind.IsExpression) && !cond.IsKind(SyntaxKind.IsNotExpression))
                    continue;
                ExpressionSyntax checkParam;
                if (cond.Left.IsKind(SyntaxKind.NothingLiteralExpression))
                {
                    checkParam = cond.Right;
                }
                else if (cond.Right.IsKind(SyntaxKind.NothingLiteralExpression))
                {
                    checkParam = cond.Left;
                }
                else
                {
                    continue;
                }
                var stmt = ifStmt.Statements.OfType<StatementSyntax>().FirstOrDefault();
                if (!(stmt is ThrowStatementSyntax))
                    continue;

                var param = semanticModel.GetSymbolInfo(checkParam);
                if (param.Symbol == parameterSymbol)
                    return true;
            }

            foreach (var ifStmt in body.DescendantNodes().OfType<SingleLineIfStatementSyntax>())
            {
                var cond = ifStmt.Condition as BinaryExpressionSyntax;
                if (cond == null || !cond.IsKind(SyntaxKind.IsExpression) && !cond.IsKind(SyntaxKind.IsNotExpression))
                    continue;
                ExpressionSyntax checkParam;
                if (cond.Left.IsKind(SyntaxKind.NothingLiteralExpression))
                {
                    checkParam = cond.Right;
                }
                else if (cond.Right.IsKind(SyntaxKind.NothingLiteralExpression))
                {
                    checkParam = cond.Left;
                }
                else
                {
                    continue;
                }
                var stmt = ifStmt.ChildNodes().OfType<StatementSyntax>().FirstOrDefault();
                if (!(stmt is ThrowStatementSyntax))
                    continue;

                var param = semanticModel.GetSymbolInfo(checkParam);
                if (param.Symbol == parameterSymbol)
                    return true;
            }
            return false;
        }
    }
}
