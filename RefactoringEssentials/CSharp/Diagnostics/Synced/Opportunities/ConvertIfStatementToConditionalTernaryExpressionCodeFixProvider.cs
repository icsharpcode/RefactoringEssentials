using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CodeFixes;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;
using Microsoft.CodeAnalysis.Formatting;
using RefactoringEssentials.CSharp.CodeRefactorings;
using System.Collections.Immutable;

namespace RefactoringEssentials.CSharp.Diagnostics
{
    [ExportCodeFixProvider(LanguageNames.CSharp), System.Composition.Shared]
    public class ConvertIfStatementToConditionalTernaryExpressionCodeFixProvider : CodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds
        {
            get
            {
                return ImmutableArray.Create(CSharpDiagnosticIDs.ConvertIfStatementToConditionalTernaryExpressionAnalyzerID);
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
            var root = await document.GetSyntaxRootAsync(cancellationToken);
            var diagnostic = diagnostics.First();
            var node = root.FindNode(context.Span) as IfStatementSyntax;

            ExpressionSyntax condition, target;
            AssignmentExpressionSyntax trueAssignment, falseAssignment;
            if (!ConvertIfStatementToConditionalTernaryExpressionCodeRefactoringProvider.ParseIfStatement(node, out condition, out target, out trueAssignment, out falseAssignment))
                return;
            var newRoot = root.ReplaceNode((SyntaxNode)node,
                SyntaxFactory.ExpressionStatement(
                    SyntaxFactory.AssignmentExpression(
                        trueAssignment.Kind(),
                        trueAssignment.Left,
                        SyntaxFactory.ConditionalExpression(condition, trueAssignment.Right, falseAssignment.Right)
                    )
                ).WithAdditionalAnnotations(Formatter.Annotation)
            );
            context.RegisterCodeFix(CodeActionFactory.Create(node.Span, diagnostic.Severity, "Convert to '?:' expression", document.WithSyntaxRoot(newRoot)), diagnostic);
        }
    }
}