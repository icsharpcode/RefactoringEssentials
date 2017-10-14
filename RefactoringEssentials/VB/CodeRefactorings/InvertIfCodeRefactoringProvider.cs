using System.Linq;
using System.Collections.Generic;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Text;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.VisualBasic;

namespace RefactoringEssentials.VB.CodeRefactorings
{
    [ExportCodeRefactoringProvider(LanguageNames.VisualBasic, Name = "Invert if")]
    public class InvertIfCodeRefactoringProvider : CodeRefactoringProvider
    {
        static readonly string invertIfFixMessage = GettextCatalog.GetString("Invert 'if'");

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

            var complexIfBlock = GetMultiLineIfBlockComplex(root, span);
            if (complexIfBlock != null)
            {
                context.RegisterRefactoring(
                    CodeActionFactory.Create(
                        span,
                        DiagnosticSeverity.Info,
                        invertIfFixMessage,
                        t2 =>
                        {
                            var newRoot = root.ReplaceNode(
                                complexIfBlock,
                                GenerateNewScript(complexIfBlock)
                            );
                            return Task.FromResult(document.WithSyntaxRoot(newRoot));
                        }
                    )
                );
                return;
            }

            var simpleIfBlock = GetMultiLineIfBlockSimple(root, span);
            if (simpleIfBlock != null)
            {
                context.RegisterRefactoring(
                    CodeActionFactory.Create(
                        span,
                        DiagnosticSeverity.Info,
                        invertIfFixMessage,
                        t2 =>
                        {
                            var newRoot = root.ReplaceNode(
                                simpleIfBlock,
                                simpleIfBlock
                                    .WithIfStatement(simpleIfBlock.IfStatement.WithCondition(VBUtil.InvertCondition(simpleIfBlock.IfStatement.Condition)))
                                    .WithStatements(simpleIfBlock.ElseBlock.Statements)
                                    .WithElseBlock(simpleIfBlock.ElseBlock.WithStatements(simpleIfBlock.Statements))
                                    .WithAdditionalAnnotations(Formatter.Annotation)
                                );
                            return Task.FromResult(document.WithSyntaxRoot(newRoot));
                        }
                    )
                );
                return;
            }

            var inSubIfBlock = GetMultiLineIfBlockLastInSub(root, span);
            if (inSubIfBlock != null)
            {
                context.RegisterRefactoring(
                    CodeActionFactory.Create(
                        inSubIfBlock.Span,
                        DiagnosticSeverity.Info,
                        invertIfFixMessage,
                        t2 =>
                        {
                            var ifStatement = GenerateInvertedIfStatement(inSubIfBlock.IfStatement);
                            var invertedIfBlock = SyntaxFactory.MultiLineIfBlock(ifStatement)
                                .WithStatements(new SyntaxList<StatementSyntax>().Add(SyntaxFactory.ReturnStatement()))
                                .WithLeadingTrivia(inSubIfBlock.GetLeadingTrivia())
                                .WithTrailingTrivia(inSubIfBlock.GetTrailingTrivia())
                                .WithAdditionalAnnotations(Formatter.Annotation);
                            var newRoot = root.ReplaceNode(
                                inSubIfBlock,
                                new SyntaxNode[] { invertedIfBlock }.Concat(GetStatements(inSubIfBlock))
                            );
                            return Task.FromResult(document.WithSyntaxRoot(newRoot));
                        }
                    )
                );
            }


            var inLoopIfBlock = GetMultiLineIfBlockInLoop(root, span);
            if (inLoopIfBlock != null)
            {
                context.RegisterRefactoring(
                    CodeActionFactory.Create(
                        inLoopIfBlock.Span,
                        DiagnosticSeverity.Info,
                        invertIfFixMessage,
                        t2 =>
                        {
                            var ifStatement = GenerateInvertedIfStatement(inLoopIfBlock.IfStatement);
                            var invertedIfBlock = SyntaxFactory.MultiLineIfBlock(ifStatement)
                                .WithStatements(new SyntaxList<StatementSyntax>().Add(GetContinueStatement(inLoopIfBlock.Parent)))
                                .WithLeadingTrivia(inLoopIfBlock.GetLeadingTrivia())
                                .WithTrailingTrivia(inLoopIfBlock.GetTrailingTrivia())
                                .WithAdditionalAnnotations(Formatter.Annotation);
                            var newRoot = root.ReplaceNode(
                                inLoopIfBlock,
                                new SyntaxNode[] { invertedIfBlock }.Concat(GetStatements(inLoopIfBlock))
                            );
                            return Task.FromResult(document.WithSyntaxRoot(newRoot));
                        }
                    )
                );
                return;
            }
        }

        static SyntaxList<StatementSyntax> GenerateNewTrueStatement(SyntaxList<StatementSyntax> falseStatements)
        {
            if (falseStatements.Count == 1)
            {
                var stmt = falseStatements.First();
                if (stmt.GetLeadingTrivia().All(triva => triva.IsKind(SyntaxKind.WhitespaceTrivia)))
                    return new SyntaxList<StatementSyntax>().Add(stmt);
            }
            return falseStatements;
        }

        static IEnumerable<SyntaxNode> GenerateNewScript(MultiLineIfBlockSyntax ifBlock)
        {
            var ifStatement = GenerateInvertedIfStatement(ifBlock.IfStatement);
            yield return SyntaxFactory.MultiLineIfBlock(ifStatement)
                .WithStatements(GenerateNewTrueStatement(ifBlock.ElseBlock.Statements))
                .WithLeadingTrivia(ifBlock.GetLeadingTrivia())
                .WithTrailingTrivia(ifBlock.GetTrailingTrivia())
                .WithAdditionalAnnotations(Formatter.Annotation);

            foreach (var stmt in ifBlock.Statements)
            {
                yield return stmt.WithAdditionalAnnotations(Formatter.Annotation);
            }
        }

        static IfStatementSyntax GenerateInvertedIfStatement(IfStatementSyntax ifStatement)
        {
            var condition = VBUtil.InvertCondition(ifStatement.Condition);
            return SyntaxFactory.IfStatement(condition)
                .WithThenKeyword(SyntaxFactory.Token(SyntaxKind.ThenKeyword))
                .WithAdditionalAnnotations(Formatter.Annotation);
        }

        static MultiLineIfBlockSyntax GetMultiLineIfBlockSyntax(SyntaxNode root, TextSpan span)
        {
            var ifStatement = root.FindNode(span) as IfStatementSyntax;
            if (ifStatement == null)
                return null;

            var result = ifStatement.Parent as MultiLineIfBlockSyntax;
            if (result == null || result.ElseIfBlocks.Count > 0)
                return null;
            return result;
        }


        static MultiLineIfBlockSyntax GetMultiLineIfBlockSimple(SyntaxNode root, TextSpan span)
        {
            var result = GetMultiLineIfBlockSyntax(root, span);
            if (result == null || result.ElseBlock == null)
                return null;
            return result;
        }

        static MultiLineIfBlockSyntax GetMultiLineIfBlockComplex(SyntaxNode root, TextSpan span)
        {
            var result = GetMultiLineIfBlockSyntax(root, span);
            if (result == null)
                return null;
            if (result.ElseBlock == null || result.ElseBlock.Statements.Count == 0)
                return null;

            var isQuitingStatement = result.ElseBlock.Statements.FirstOrDefault();
            if (isQuitingStatement.IsKind(SyntaxKind.ReturnStatement) ||
                isQuitingStatement.IsKind(SyntaxKind.ContinueDoStatement) ||
                isQuitingStatement.IsKind(SyntaxKind.ContinueForStatement) ||
                isQuitingStatement.IsKind(SyntaxKind.ContinueWhileStatement))
                return result;
            return null;
        }


        static MultiLineIfBlockSyntax GetMultiLineIfBlockLastInSub(SyntaxNode root, TextSpan span)
        {
            var result = GetMultiLineIfBlockSyntax(root, span);
            if (result == null || result.ElseBlock != null)
                return null;

            if (result.Parent is MethodBlockSyntax)
            {
                var parent = result.Parent as MethodBlockSyntax;
                if (parent.SubOrFunctionStatement.SubOrFunctionKeyword.Text != "Sub")
                    return null;
                if (IsLastStatement(parent.Statements, result))
                    return result;
            }
            return null;
        }

        static MultiLineIfBlockSyntax GetMultiLineIfBlockInLoop(SyntaxNode root, TextSpan span)
        {
            var result = GetMultiLineIfBlockSyntax(root, span);
            if (result == null || result.ElseBlock != null)
                return null;

            if (result.Parent is DoLoopBlockSyntax)
            {
                var parent = result.Parent as DoLoopBlockSyntax;
                if (IsLastStatement(parent.Statements, result))
                    return result;
            }

            if (result.Parent is WhileBlockSyntax)
            {
                var parent = result.Parent as WhileBlockSyntax;
                if (IsLastStatement(parent.Statements, result))
                    return result;
            }

            if (result.Parent is ForOrForEachBlockSyntax)
            {
                var parent = result.Parent as ForOrForEachBlockSyntax;
                if (IsLastStatement(parent.Statements, result))
                    return result;
            }
            return null;
        }

        static bool IsLastStatement(SyntaxList<StatementSyntax> statements, MultiLineIfBlockSyntax ifBlock)
        {
            return statements.IndexOf(ifBlock) + 1 >= statements.Count;
        }

        static IEnumerable<SyntaxNode> GetStatements(MultiLineIfBlockSyntax ifBlock)
        {
            foreach (var stmt in ifBlock.Statements)
                yield return stmt.WithAdditionalAnnotations(Formatter.Annotation);
        }

        static ContinueStatementSyntax GetContinueStatement(SyntaxNode parent)
        {
            if (parent is DoLoopBlockSyntax)
                return SyntaxFactory.ContinueDoStatement();

            if (parent is WhileBlockSyntax)
                return SyntaxFactory.ContinueWhileStatement();

            if (parent is ForOrForEachBlockSyntax)
                return SyntaxFactory.ContinueForStatement();

            return null;
        }
    }
}