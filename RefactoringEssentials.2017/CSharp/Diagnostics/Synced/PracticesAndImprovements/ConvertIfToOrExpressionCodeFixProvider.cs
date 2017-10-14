using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Formatting;
using System.Collections.Immutable;

namespace RefactoringEssentials.CSharp.Diagnostics
{
    [ExportCodeFixProvider(LanguageNames.CSharp), System.Composition.Shared]
    public class ConvertIfToOrExpressionCodeFixProvider : CodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds
        {
            get
            {
                return ImmutableArray.Create(CSharpDiagnosticIDs.ConvertIfToOrExpressionAnalyzerID);
            }
        }

        public override FixAllProvider GetFixAllProvider()
        {
            return WellKnownFixAllProviders.BatchFixer;
        }

        public async override Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var document = context.Document;
            var cancellationToken = context.CancellationToken;
            var span = context.Span;
            var diagnostics = context.Diagnostics;
            var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
            foreach (var diagnostic in diagnostics)
            {
                var node = root.FindNode(context.Span) as IfStatementSyntax;
                ExpressionSyntax target;
                SyntaxTriviaList assignmentTrailingTriviaList;
                ConvertIfToOrExpressionAnalyzer.MatchIfElseStatement(node, SyntaxKind.TrueLiteralExpression, out target, out assignmentTrailingTriviaList);
                SyntaxNode newRoot = null;
                var varDeclaration = ConvertIfToOrExpressionAnalyzer.FindPreviousVarDeclaration(node);
                if (varDeclaration != null)
                {
                    var varDeclarator = varDeclaration.Declaration.Variables[0];
                    newRoot = root.ReplaceNodes(new SyntaxNode[] { varDeclaration, node }, (arg, arg2) =>
                    {
                        if (arg is LocalDeclarationStatementSyntax)
                            return SyntaxFactory.LocalDeclarationStatement(
                                    SyntaxFactory.VariableDeclaration(varDeclaration.Declaration.Type,
                                        SyntaxFactory.SeparatedList(
                                            new[] {
                                                SyntaxFactory.VariableDeclarator(varDeclarator.Identifier.ValueText)
                                                    .WithInitializer(
                                                        SyntaxFactory.EqualsValueClause(
                                                            SyntaxFactory.BinaryExpression(SyntaxKind.LogicalOrExpression, ConvertIfToOrExpressionAnalyzer.AddParensToComplexExpression(varDeclarator.Initializer.Value), ConvertIfToOrExpressionAnalyzer.AddParensToComplexExpression(node.Condition)))
                                                                .WithAdditionalAnnotations(Formatter.Annotation)
                                                    )
                                            }
                                        ))
                                ).WithLeadingTrivia(varDeclaration.GetLeadingTrivia()).WithTrailingTrivia(node.GetTrailingTrivia());
                        return null;
                    });
                }
                else
                {
                    newRoot = root.ReplaceNode((SyntaxNode)node,
                        SyntaxFactory.ExpressionStatement(
                            SyntaxFactory.AssignmentExpression(
                                SyntaxKind.OrAssignmentExpression,
                                ConvertIfToOrExpressionAnalyzer.AddParensToComplexExpression(target),
                                ConvertIfToOrExpressionAnalyzer.AddParensToComplexExpression(node.Condition).WithAdditionalAnnotations(Formatter.Annotation)
                            )
                        ).WithLeadingTrivia(node.GetLeadingTrivia()).WithTrailingTrivia(node.GetTrailingTrivia()));
                }

                context.RegisterCodeFix(CodeActionFactory.Create(node.Span, diagnostic.Severity, diagnostic.GetMessage(), document.WithSyntaxRoot(newRoot)), diagnostic);
            }
        }
    }
}