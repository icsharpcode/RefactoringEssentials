using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Formatting;
using System.Collections.Generic;

namespace RefactoringEssentials.CSharp.Diagnostics
{
    [ExportCodeFixProvider(LanguageNames.CSharp), System.Composition.Shared]
    public class FieldCanBeMadeReadOnlyCodeFixProvider : CodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(CSharpDiagnosticIDs.FieldCanBeMadeReadOnlyAnalyzerID);

        public override FixAllProvider GetFixAllProvider()
        {
            return WellKnownFixAllProviders.BatchFixer;
        }

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
            var diagnostic = context.Diagnostics.First();
            var diagnosticSpan = diagnostic.Location.SourceSpan;
            var varDecl = root.FindToken(context.Span.Start).Parent.AncestorsAndSelf().OfType<VariableDeclaratorSyntax>().FirstOrDefault();
            if (varDecl == null)
                return;
            context.RegisterCodeFix(
                CodeActionFactory.Create(
                    context.Span,
                    diagnostic.Severity,
                    GettextCatalog.GetString("To 'readonly'"),
                    delegate (CancellationToken cancellationToken)
                    {
                        var fieldDeclaration = varDecl.Ancestors().OfType<FieldDeclarationSyntax>().First();
                        var nodes = new List<SyntaxNode>();
                        if (fieldDeclaration.Declaration.Variables.Count == 1)
                        {
                            nodes.Add(
                                fieldDeclaration
                                    .AddModifiers(SyntaxFactory.Token(SyntaxKind.ReadOnlyKeyword))
                                    .WithAdditionalAnnotations(Formatter.Annotation)
                            );
                        }
                        else
                        {
                            nodes.Add(fieldDeclaration.WithDeclaration(fieldDeclaration.Declaration.RemoveNode(varDecl, SyntaxRemoveOptions.KeepEndOfLine)));
                            nodes.Add(
                                fieldDeclaration.WithDeclaration(
                                    SyntaxFactory.VariableDeclaration(
                                        fieldDeclaration.Declaration.Type,
                                        SyntaxFactory.SeparatedList(new[] { varDecl })
                                    )
                                )
                                .AddModifiers(SyntaxFactory.Token(SyntaxKind.ReadOnlyKeyword))
                                .WithAdditionalAnnotations(Formatter.Annotation)
                            );
                        }
                        return Task.FromResult(context.Document.WithSyntaxRoot(root.ReplaceNode(fieldDeclaration, nodes)));
                    }
                ),
                diagnostic
            );
        }
    }
}