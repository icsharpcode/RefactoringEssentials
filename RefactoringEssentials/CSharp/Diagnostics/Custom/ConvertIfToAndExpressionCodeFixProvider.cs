using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CodeFixes;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;
using Microsoft.CodeAnalysis.Formatting;

namespace RefactoringEssentials.CSharp.Diagnostics
{
    [ExportCodeFixProvider(LanguageNames.CSharp), System.Composition.Shared]
    public class ConvertIfToAndExpressionCodeFixProvider : CodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds
        {
            get
            {
                return ImmutableArray.Create(CSharpDiagnosticIDs.ConvertIfToAndExpressionAnalyzerID);
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
            var diagnostic = context.Diagnostics.First();
            var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
            var node = root.FindToken(context.Span.Start).Parent as IfStatementSyntax;
            if (node == null)
                return;
            ExpressionSyntax target;
            SyntaxTriviaList assignmentTrailingTriviaList;
            ConvertIfToOrExpressionAnalyzer.MatchIfElseStatement(node, SyntaxKind.FalseLiteralExpression, out target, out assignmentTrailingTriviaList);
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
                                                        SyntaxFactory.BinaryExpression(SyntaxKind.LogicalAndExpression, ConvertIfToOrExpressionAnalyzer.AddParensToComplexExpression(varDeclarator.Initializer.Value), ConvertIfToOrExpressionAnalyzer.AddParensToComplexExpression(CSharpUtil.InvertCondition(node.Condition))))
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
                            SyntaxKind.AndAssignmentExpression,
                            ConvertIfToOrExpressionAnalyzer.AddParensToComplexExpression(target),
                            ConvertIfToOrExpressionAnalyzer.AddParensToComplexExpression(CSharpUtil.InvertCondition(node.Condition)).WithAdditionalAnnotations(Formatter.Annotation)
                        )
                    ).WithLeadingTrivia(node.GetLeadingTrivia()).WithTrailingTrivia(node.GetTrailingTrivia()));
            }

            context.RegisterCodeFix(CodeActionFactory.Create(node.Span, diagnostic.Severity, diagnostic.GetMessage(), document.WithSyntaxRoot(newRoot)), diagnostic);
        }
    }
}