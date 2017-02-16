using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Formatting;

namespace RefactoringEssentials.CSharp.Diagnostics
{
    [ExportCodeFixProvider(LanguageNames.CSharp), System.Composition.Shared]
    public class RedundantArgumentNameCodeFixProvider : CodeFixProvider
    {
        const string CodeActionMessage = "Remove argument name specification";

        public override ImmutableArray<string> FixableDiagnosticIds
        {
            get
            {
                return ImmutableArray.Create(CSharpDiagnosticIDs.RedundantArgumentNameAnalyzerID);
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
            var diagnostic = diagnostics.First();
            var node = root.FindNode(context.Span);
            var argListSyntax = node.Parent.Parent as BaseArgumentListSyntax;
            if (node.IsKind(SyntaxKind.NameColon) && argListSyntax != null)
            {
                bool replace = true;
                var newRoot = root;
                var args = new List<ArgumentSyntax>();

                foreach (var arg in argListSyntax.Arguments)
                {
                    if (replace)
                    {
                        args.Add(arg);
                    }
                    replace &= arg != node.Parent;

                }
                newRoot = newRoot.ReplaceNodes(args, (arg, arg2) => SyntaxFactory.Argument(arg.Expression).WithTriviaFrom(arg).WithAdditionalAnnotations(Formatter.Annotation));

                context.RegisterCodeFix(CodeActionFactory.Create(node.Span, diagnostic.Severity, CodeActionMessage, document.WithSyntaxRoot(newRoot)), diagnostic);
                return;
            }
            var attrListSyntax = node.Parent.Parent as AttributeArgumentListSyntax;
            if (node.IsKind(SyntaxKind.NameColon) && attrListSyntax != null)
            {
                bool replace = true;
                var newRoot = root;
                var args = new List<AttributeArgumentSyntax>();

                foreach (var arg in attrListSyntax.Arguments)
                {
                    if (replace)
                    {
                        args.Add(arg);
                    }
                    replace &= arg != node.Parent;

                }
                newRoot = newRoot.ReplaceNodes(args, (arg, arg2) => SyntaxFactory.AttributeArgument(arg.Expression).WithTriviaFrom(arg).WithAdditionalAnnotations(Formatter.Annotation));

                context.RegisterCodeFix(CodeActionFactory.Create(node.Span, diagnostic.Severity, CodeActionMessage, document.WithSyntaxRoot(newRoot)), diagnostic);
                return;
            }
        }
    }

}