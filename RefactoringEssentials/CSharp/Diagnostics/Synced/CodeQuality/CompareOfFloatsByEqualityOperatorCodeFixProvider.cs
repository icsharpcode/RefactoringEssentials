using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CodeFixes;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;
using Microsoft.CodeAnalysis.Formatting;

namespace RefactoringEssentials.CSharp.Diagnostics
{

    [ExportCodeFixProvider(LanguageNames.CSharp), System.Composition.Shared]
    public class CompareOfFloatsByEqualityOperatorCodeFixProvider : CodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds
        {
            get
            {
                return ImmutableArray.Create(CSharpDiagnosticIDs.CompareOfFloatsByEqualityOperatorAnalyzerID);
            }
        }

        // Does not make sense here, because the fixes produce code that is not compilable
        //public override FixAllProvider GetFixAllProvider()
        //{
        //	return WellKnownFixAllProviders.BatchFixer;
        //}

        public async override Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var document = context.Document;
            var cancellationToken = context.CancellationToken;
            var span = context.Span;
            var diagnostics = context.Diagnostics;
            var semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);
            var root = semanticModel.SyntaxTree.GetRoot(cancellationToken);
            var diagnostic = diagnostics.First();
            var node = root.FindNode(context.Span) as BinaryExpressionSyntax;
            if (node == null)
                return;
            CodeAction action;
            var floatType = diagnostic.Descriptor.CustomTags.ElementAt(1);
            switch (diagnostic.Descriptor.CustomTags.ElementAt(0))
            {
                case "1":
                    action = AddIsNaNIssue(document, semanticModel, root, node, node.Right, floatType);
                    break;
                case "2":
                    action = AddIsNaNIssue(document, semanticModel, root, node, node.Left, floatType);
                    break;
                case "3":
                    action = AddIsPositiveInfinityIssue(document, semanticModel, root, node, node.Right, floatType);
                    break;
                case "4":
                    action = AddIsPositiveInfinityIssue(document, semanticModel, root, node, node.Left, floatType);
                    break;
                case "5":
                    action = AddIsNegativeInfinityIssue(document, semanticModel, root, node, node.Right, floatType);
                    break;
                case "6":
                    action = AddIsNegativeInfinityIssue(document, semanticModel, root, node, node.Left, floatType);
                    break;
                case "7":
                    action = AddIsZeroIssue(document, semanticModel, root, node, node.Right, floatType);
                    break;
                case "8":
                    action = AddIsZeroIssue(document, semanticModel, root, node, node.Left, floatType);
                    break;
                default:
                    action = AddCompareIssue(document, semanticModel, root, node, floatType);

                    break;
            }

            if (action != null)
            {
                context.RegisterCodeFix(action, diagnostic);
            }
        }

        static CodeAction AddIsNaNIssue(Document document, SemanticModel semanticModel, SyntaxNode root, BinaryExpressionSyntax node, ExpressionSyntax argExpr, string floatType)
        {
            return CodeActionFactory.Create(node.Span, DiagnosticSeverity.Warning, string.Format(node.IsKind(SyntaxKind.EqualsExpression) ? "Replace with '{0}.IsNaN(...)' call" : "Replace with '!{0}.IsNaN(...)' call", floatType), token =>
            {
                SyntaxNode newRoot;
                ExpressionSyntax expr;
                var arguments = new SeparatedSyntaxList<ArgumentSyntax>();
                arguments = arguments.Add(SyntaxFactory.Argument(argExpr));
                expr = SyntaxFactory.InvocationExpression(
                    SyntaxFactory.MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        SyntaxFactory.ParseExpression(floatType),
                        SyntaxFactory.IdentifierName("IsNaN")
                    ),
                    SyntaxFactory.ArgumentList(
                        arguments
                    )
                );
                if (node.IsKind(SyntaxKind.NotEqualsExpression))
                    expr = SyntaxFactory.PrefixUnaryExpression(SyntaxKind.LogicalNotExpression, expr);
                expr = expr.WithAdditionalAnnotations(Formatter.Annotation);
                newRoot = root.ReplaceNode((SyntaxNode)node, expr);
                return Task.FromResult(document.WithSyntaxRoot(newRoot));
            });
        }

        static CodeAction AddIsPositiveInfinityIssue(Document document, SemanticModel semanticModel, SyntaxNode root, BinaryExpressionSyntax node, ExpressionSyntax argExpr, string floatType)
        {
            return CodeActionFactory.Create(node.Span, DiagnosticSeverity.Warning, string.Format(node.IsKind(SyntaxKind.EqualsExpression) ? "Replace with '{0}.IsPositiveInfinity(...)' call" : "Replace with '!{0}.IsPositiveInfinity(...)' call", floatType), token =>
            {
                SyntaxNode newRoot;
                ExpressionSyntax expr;
                var arguments = new SeparatedSyntaxList<ArgumentSyntax>();
                arguments = arguments.Add(SyntaxFactory.Argument(argExpr));
                expr = SyntaxFactory.InvocationExpression(
                    SyntaxFactory.MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        SyntaxFactory.ParseExpression(floatType),
                        SyntaxFactory.IdentifierName("IsPositiveInfinity")
                    ),
                    SyntaxFactory.ArgumentList(
                        arguments
                    )
                );
                if (node.IsKind(SyntaxKind.NotEqualsExpression))
                    expr = SyntaxFactory.PrefixUnaryExpression(SyntaxKind.LogicalNotExpression, expr);
                expr = expr.WithAdditionalAnnotations(Formatter.Annotation);
                newRoot = root.ReplaceNode((SyntaxNode)node, expr);
                return Task.FromResult(document.WithSyntaxRoot(newRoot));
            });
        }

        static CodeAction AddIsNegativeInfinityIssue(Document document, SemanticModel semanticModel, SyntaxNode root, BinaryExpressionSyntax node, ExpressionSyntax argExpr, string floatType)
        {
            return CodeActionFactory.Create(node.Span, DiagnosticSeverity.Warning, string.Format(node.IsKind(SyntaxKind.EqualsExpression) ? "Replace with '{0}.IsNegativeInfinity(...)' call" : "Replace with '!{0}.IsNegativeInfinity(...)' call", floatType), token =>
            {
                SyntaxNode newRoot;
                ExpressionSyntax expr;
                var arguments = new SeparatedSyntaxList<ArgumentSyntax>();
                arguments = arguments.Add(SyntaxFactory.Argument(argExpr));
                expr = SyntaxFactory.InvocationExpression(
                    SyntaxFactory.MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        SyntaxFactory.ParseExpression(floatType),
                        SyntaxFactory.IdentifierName("IsNegativeInfinity")
                    ),
                    SyntaxFactory.ArgumentList(
                        arguments
                    )
                );
                if (node.IsKind(SyntaxKind.NotEqualsExpression))
                    expr = SyntaxFactory.PrefixUnaryExpression(SyntaxKind.LogicalNotExpression, expr);
                expr = expr.WithAdditionalAnnotations(Formatter.Annotation);
                newRoot = root.ReplaceNode((SyntaxNode)node, expr);
                return Task.FromResult(document.WithSyntaxRoot(newRoot));
            });
        }

        static CodeAction AddIsZeroIssue(Document document, SemanticModel semanticModel, SyntaxNode root, BinaryExpressionSyntax node, ExpressionSyntax argExpr, string floatType)
        {
            return CodeActionFactory.Create(node.Span, DiagnosticSeverity.Warning, "Fix floating point number comparison", token =>
            {
                SyntaxNode newRoot;
                ExpressionSyntax expr;
                var arguments = new SeparatedSyntaxList<ArgumentSyntax>();
                arguments = arguments.Add(SyntaxFactory.Argument(argExpr));
                expr = SyntaxFactory.BinaryExpression(
                    node.IsKind(SyntaxKind.EqualsExpression) ? SyntaxKind.LessThanExpression : SyntaxKind.GreaterThanExpression,
                    SyntaxFactory.InvocationExpression(
                        SyntaxFactory.MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            SyntaxFactory.ParseTypeName("System.Math").WithAdditionalAnnotations(Microsoft.CodeAnalysis.Simplification.Simplifier.Annotation),
                            SyntaxFactory.IdentifierName("Abs")
                        ),
                        SyntaxFactory.ArgumentList(
                            arguments
                        )
                    ),
                    SyntaxFactory.IdentifierName(SyntaxFactory.Identifier("EPSILON").WithAdditionalAnnotations(RenameAnnotation.Create()))
                );
                expr = expr.WithAdditionalAnnotations(Formatter.Annotation);
                newRoot = root.ReplaceNode((SyntaxNode)node, expr);
                return Task.FromResult(document.WithSyntaxRoot(newRoot));
            });
        }

        static CodeAction AddCompareIssue(Document document, SemanticModel semanticModel, SyntaxNode root, BinaryExpressionSyntax node, string floatType)
        {
            return CodeActionFactory.Create(node.Span, DiagnosticSeverity.Warning, "Fix floating point number comparison", token =>
            {
                SyntaxNode newRoot;
                ExpressionSyntax expr;
                var arguments = new SeparatedSyntaxList<ArgumentSyntax>();
                arguments = arguments.Add(SyntaxFactory.Argument(SyntaxFactory.BinaryExpression(SyntaxKind.SubtractExpression, node.Left, node.Right)));
                expr = SyntaxFactory.BinaryExpression(
                        node.IsKind(SyntaxKind.EqualsExpression) ? SyntaxKind.LessThanExpression : SyntaxKind.GreaterThanExpression,
                    SyntaxFactory.InvocationExpression(
                        SyntaxFactory.MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            SyntaxFactory.ParseTypeName("System.Math").WithAdditionalAnnotations(Microsoft.CodeAnalysis.Simplification.Simplifier.Annotation),
                            SyntaxFactory.IdentifierName("Abs")
                        ),
                        SyntaxFactory.ArgumentList(
                            arguments
                        )
                    ),
                    SyntaxFactory.IdentifierName(SyntaxFactory.Identifier("EPSILON").WithAdditionalAnnotations(RenameAnnotation.Create()))
                );
                expr = expr.WithAdditionalAnnotations(Formatter.Annotation);
                newRoot = root.ReplaceNode((SyntaxNode)node, expr);
                return Task.FromResult(document.WithSyntaxRoot(newRoot));
            });
        }
    }
}
