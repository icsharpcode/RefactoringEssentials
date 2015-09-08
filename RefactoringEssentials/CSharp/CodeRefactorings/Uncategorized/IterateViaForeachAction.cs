using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.CodeActions;

namespace RefactoringEssentials.CSharp.CodeRefactorings
{
    [ExportCodeRefactoringProvider(LanguageNames.CSharp, Name = "Iterate via 'foreach'")]
    public class IterateViaForeachAction : CodeRefactoringProvider
    {
        public override async Task ComputeRefactoringsAsync(CodeRefactoringContext context)
        {
            var document = context.Document;
            var span = context.Span;
            var cancellationToken = context.CancellationToken;
            var model = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);
            if (model.IsFromGeneratedCode(cancellationToken))
                return;
            var root = await model.SyntaxTree.GetRootAsync(cancellationToken).ConfigureAwait(false);

            var node = root.FindNode(span);
            
            var assignment = node.GetAncestor<AssignmentExpressionSyntax>();
            if (assignment != null && IsEnumerable(model.GetTypeInfo(assignment.Left).ConvertedType) == true)
            {
                context.RegisterRefactoring(Handle(document, span, root, node, assignment.Left, false));
                return;
            }

            var usingStatement = node.GetAncestor<UsingStatementSyntax>();
            var variable = usingStatement?.Declaration?.ChildNodes()?.OfType<VariableDeclaratorSyntax>()?.FirstOrDefault();

            if (usingStatement != null && variable != null && IsEnumerable(model.GetDeclaredSymbol(variable)) == true)
            {
                context.RegisterRefactoring(HandleUsingStatement(document, span, root, usingStatement, variable));
                return;
            }

            var variableDeclarator = node.GetAncestorOrThis<VariableDeclaratorSyntax>();
            var localDeclaration = node.GetAncestor<LocalDeclarationStatementSyntax>();

            if (localDeclaration != null && variableDeclarator != null && IsEnumerable(model.GetDeclaredSymbol(variableDeclarator)) == true)
            {
                context.RegisterRefactoring(Handle(document, span, root, node, SyntaxFactory.IdentifierName(variableDeclarator.Identifier), false));
                return;
            }

            var expressionStatement = node.GetAncestor<ExpressionStatementSyntax>();
            if (expressionStatement != null && IsEnumerable(model.GetTypeInfo(expressionStatement.Expression).ConvertedType) == true)
            {
                context.RegisterRefactoring(Handle(document, span, root, node, expressionStatement.Expression.WithoutTrivia(), true));
                return;
            }
        }

        static CodeAction HandleUsingStatement(Document document, Microsoft.CodeAnalysis.Text.TextSpan span, SyntaxNode root, UsingStatementSyntax usingStatement, VariableDeclaratorSyntax variable)
        {
            return CodeActionFactory.Create(
                            span,
                            DiagnosticSeverity.Info,
                            "Iterate via 'foreach'",
                            ct =>
                            {
                                ForEachStatementSyntax foreachStmt = BuildForeach(SyntaxFactory.IdentifierName(variable.Identifier));

                                var innerBlock = usingStatement.Statement.EnsureBlock();

                                var newBlock = innerBlock.WithStatements(innerBlock.Statements.Insert(0, foreachStmt)).WithAdditionalAnnotations(Formatter.Annotation);
                                var newUsing = usingStatement.WithStatement(newBlock);
                                var newRoot = root.ReplaceNode(usingStatement, newUsing.WithTrailingTrivia(usingStatement.GetTrailingTrivia()));

                                return Task.FromResult(document.WithSyntaxRoot(newRoot));
                            }
                        );
        }           

        static CodeAction Handle(Document document, Microsoft.CodeAnalysis.Text.TextSpan span, SyntaxNode root, SyntaxNode node, ExpressionSyntax iterateOver, bool replaceNode)
        {
            return CodeActionFactory.Create(
                span,
                DiagnosticSeverity.Info,
                "Iterate via 'foreach'",
                ct =>
                {
                    ForEachStatementSyntax foreachStmt = BuildForeach(iterateOver);

                    SyntaxNode newRoot;
                    var ancestor = node.GetAncestor<StatementSyntax>();
                    if (replaceNode)
                    {
                        newRoot = root.ReplaceNode(ancestor, new[] { foreachStmt.WithTrailingTrivia(ancestor.GetTrailingTrivia()) });
                    }
                    else
                    {
                        newRoot = root.InsertNodesAfter(ancestor, new[] { foreachStmt.WithTrailingTrivia(ancestor.GetTrailingTrivia()) });
                    }
                    return Task.FromResult(document.WithSyntaxRoot(newRoot));
                }
            );
        }

        static ForEachStatementSyntax BuildForeach(ExpressionSyntax iterateOver)
        {
            var itemVariable = SyntaxFactory.Identifier("item").WithAdditionalAnnotations(RenameAnnotation.Create());

            return SyntaxFactory.ForEachStatement(SyntaxFactory.ParseTypeName("var"), itemVariable, iterateOver, SyntaxFactory.Block())
                                                        .WithAdditionalAnnotations(Formatter.Annotation);
        }

        static bool? IsEnumerable(ISymbol symbol)
        {
            return symbol?.GetSymbolType()?.IsIEnumerable();
        }
    }
}
