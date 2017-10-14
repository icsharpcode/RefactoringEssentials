using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Formatting;

namespace RefactoringEssentials.CSharp.Diagnostics
{
    [ExportCodeFixProvider(LanguageNames.CSharp), System.Composition.Shared]
    public class NotResolvedInTextIssueCodeFixProvider : CodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds
        {
            get
            {
                return ImmutableArray.Create(CSharpDiagnosticIDs.NotResolvedInTextAnalyzerID, CSharpDiagnosticIDs.NotResolvedInTextAnalyzer_SwapID);
            }
        }

        public async override Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var document = context.Document;
            var cancellationToken = context.CancellationToken;
            var span = context.Span;
            var diagnostics = context.Diagnostics;
            var model = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);
            var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
            var diagnostic = diagnostics.First();
            var node = root.FindToken(context.Span.Start).Parent as LiteralExpressionSyntax;
            if (node == null)
                return;
            var argumentList = node.Parent.Parent as ArgumentListSyntax;

            var objectCreateExpression = argumentList.Parent as ObjectCreationExpressionSyntax;
            var validNames = NotResolvedInTextAnalyzer.GetValidParameterNames(objectCreateExpression);
            var guessName = NotResolvedInTextAnalyzer.GuessParameterName(model, objectCreateExpression, validNames);

            ExpressionSyntax paramNode;
            ExpressionSyntax altParamNode;
            bool canAddParameterName;

            NotResolvedInTextAnalyzer.CheckExceptionType(model, objectCreateExpression, out paramNode, out altParamNode, out canAddParameterName);

            if (diagnostic.Id == CSharpDiagnosticIDs.NotResolvedInTextAnalyzer_SwapID)
            {
                context.RegisterCodeFix(
                    CodeActionFactory.Create(
                        node.Span,
                        diagnostic.Severity,
                        GettextCatalog.GetString("Swap parameter"),
                        (token) =>
                        {
                            var list = new List<ArgumentSyntax>();
                            foreach (var arg in argumentList.Arguments)
                            {
                                if (arg.Expression == paramNode)
                                {
                                    list.Add((Microsoft.CodeAnalysis.CSharp.Syntax.ArgumentSyntax)altParamNode.Parent);
                                    continue;
                                }
                                if (arg.Expression == altParamNode)
                                {
                                    list.Add((Microsoft.CodeAnalysis.CSharp.Syntax.ArgumentSyntax)paramNode.Parent);
                                    continue;
                                }

                                list.Add(arg);
                            }
                            var newRoot = root.ReplaceNode(argumentList, SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList(list)).WithAdditionalAnnotations(Formatter.Annotation));
                            return Task.FromResult(document.WithSyntaxRoot(newRoot));
                        }
                    ),
                    diagnostic
                );
            }
            else if (diagnostic.Id == CSharpDiagnosticIDs.NotResolvedInTextAnalyzerID)
            {
                var fixes = new List<CodeAction>();
                if (canAddParameterName)
                {
                    fixes.Add(
                        CodeActionFactory.Create(
                            node.Span,
                            diagnostic.Severity,
                            string.Format(GettextCatalog.GetString("Add '\"{0}\"' parameter."), guessName),
                            (token) =>
                            {
                                var newArgument = SyntaxFactory.ParseExpression('"' + guessName + '"');
                                var newRoot = root.ReplaceNode(argumentList, argumentList.WithArguments(argumentList.Arguments.Insert(0, SyntaxFactory.Argument(newArgument))).WithAdditionalAnnotations(Formatter.Annotation));
                                return Task.FromResult(document.WithSyntaxRoot(newRoot));
                            }
                        )
                    );
                }

                fixes.Add(CodeActionFactory.Create(
                    node.Span,
                    diagnostic.Severity,
                    string.Format(GettextCatalog.GetString("Replace with '\"{0}\"'."), guessName),
                    (token) =>
                    {
                        var newArgument = SyntaxFactory.ParseExpression('"' + guessName + '"');
                        var newRoot = root.ReplaceNode(node, newArgument.WithLeadingTrivia(node.GetLeadingTrivia()).WithTrailingTrivia(node.GetTrailingTrivia()));
                        return Task.FromResult(document.WithSyntaxRoot(newRoot));
                    }
                ));

                context.RegisterFixes(
                    fixes,
                    diagnostic
                );
            }
        }
    }
}