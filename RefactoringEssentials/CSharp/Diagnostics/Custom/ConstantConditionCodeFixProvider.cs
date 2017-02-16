using System.Collections.Generic;
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
    public class ConstantConditionCodeFixProvider : CodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds
        {
            get
            {
                return ImmutableArray.Create(CSharpDiagnosticIDs.ConstantConditionAnalyzerID);
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
            var semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);
            var root = semanticModel.SyntaxTree.GetRoot(cancellationToken);
            var diagnostic = diagnostics.First();
            var node = root.FindNode(context.Span);
            //if (!node.IsKind(SyntaxKind.BaseList))
            //	continue;

            var value = bool.Parse(diagnostic.Descriptor.CustomTags.First());

            var conditionalExpr = node.Parent as ConditionalExpressionSyntax;
            var ifElseStatement = node.Parent as IfStatementSyntax;
            var valueStr = value.ToString().ToLowerInvariant();

            if (conditionalExpr != null)
            {
                context.RegisterCodeFix(CodeActionFactory.Create(node.Span, diagnostic.Severity, string.Format("Replace '?:' with '{0}' branch", valueStr), token =>
                {
                    var replaceWith = value ? conditionalExpr.WhenTrue : conditionalExpr.WhenFalse;
                    var newRoot = root.ReplaceNode((SyntaxNode)conditionalExpr, replaceWith.WithAdditionalAnnotations(Formatter.Annotation));
                    return Task.FromResult(document.WithSyntaxRoot(newRoot));
                }), diagnostic);
            }
            else if (ifElseStatement != null)
            {
                context.RegisterCodeFix(CodeActionFactory.Create(node.Span, diagnostic.Severity, string.Format("Replace 'if' with '{0}' branch", valueStr), token =>
                {
                    var list = new List<SyntaxNode>();
                    StatementSyntax branch;
                    if (value)
                    {
                        branch = ifElseStatement.Statement;
                    }
                    else
                    {
                        if (ifElseStatement.Else == null)
                            return Task.FromResult(document.WithSyntaxRoot(root.RemoveNode(ifElseStatement, SyntaxRemoveOptions.KeepNoTrivia)));
                        branch = ifElseStatement.Else.Statement;
                    }

                    var block = branch as BlockSyntax;
                    if (block != null)
                    {
                        foreach (var stmt in block.Statements)
                            list.Add(stmt.WithAdditionalAnnotations(Formatter.Annotation));
                    }
                    else
                    {
                        if (branch != null)
                            list.Add(branch.WithAdditionalAnnotations(Formatter.Annotation));
                    }
                    if (list.Count == 0)
                        return Task.FromResult(document);
                    var newRoot = root.ReplaceNode((SyntaxNode)ifElseStatement, list);
                    return Task.FromResult(document.WithSyntaxRoot(newRoot));
                }), diagnostic);
            }
            else
            {
                context.RegisterCodeFix(CodeActionFactory.Create(node.Span, diagnostic.Severity, string.Format("Replace expression with '{0}'", valueStr), token =>
                {
                    var replaceWith = SyntaxFactory.LiteralExpression(value ? SyntaxKind.TrueLiteralExpression : SyntaxKind.FalseLiteralExpression);
                    var newRoot = root.ReplaceNode((SyntaxNode)node, replaceWith.WithAdditionalAnnotations(Formatter.Annotation));
                    return Task.FromResult(document.WithSyntaxRoot(newRoot));
                }), diagnostic);
            }
        }
    }
}