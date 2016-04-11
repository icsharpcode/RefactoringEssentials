using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CodeFixes;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.Simplification;

namespace RefactoringEssentials.CSharp.CodeFixes
{
    [ExportCodeFixProvider(LanguageNames.CSharp), System.Composition.Shared]
    public class InvalidConversionCodeFixProvider : CodeFixProvider
    {
        const string CS0029 = "CS0029"; // Error CS0029: Cannot implicitly convert type 'type' to 'type'
        const string CS0266 = "CS0266"; // Error CS0266: Cannot implicitly convert type 'type1' to 'type2'. An explicit conversion exists (are you missing a cast?)
        const string CS1503 = "CS1503"; // Error CS1503: Argument 'number' cannot convert from TypeA to TypeB

        public sealed override ImmutableArray<string> FixableDiagnosticIds
        {
            get { return ImmutableArray.Create(CS0029, CS0266, CS1503); }
        }

        public override FixAllProvider GetFixAllProvider()
        {
            return WellKnownFixAllProviders.BatchFixer;
        }

        public async override Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var document = context.Document;
            if (document.Project.Solution.Workspace.Kind == WorkspaceKind.MiscellaneousFiles)
                return;
            var span = context.Span;
            var cancellationToken = context.CancellationToken;
            if (cancellationToken.IsCancellationRequested)
                return;
            var model = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);
            if (model.IsFromGeneratedCode(cancellationToken))
                return;
            var root = await model.SyntaxTree.GetRootAsync(cancellationToken).ConfigureAwait(false);
            var diagnostic = context.Diagnostics.First();
            var expression = root.FindNode(context.Span).SkipArgument() as ExpressionSyntax;
            if (expression == null)
                return;

            var message = diagnostic.GetMessage();

            var idx2 = message.LastIndexOf('\'');
            if (idx2 < 0)
            {
                idx2 = message.LastIndexOf('"');
            }
            if (idx2 < 0)
                return;
            var idx1 = message.LastIndexOf('\'', idx2 - 1);
            if (idx1 < 0)
            {
                idx1 = message.LastIndexOf('"', idx2 - 1);
            }
            if (idx1 < 0)
                return;
            string castToType = message.Substring(idx1 + 1, idx2 - idx1 - 1);

            // Explicit conversion exists
            if (diagnostic.Id == CS0266 || diagnostic.Id == CS1503)
            {
                context.RegisterCodeFix(CodeActionFactory.Create(
                    expression.Span,
                    diagnostic.Severity,
                    string.Format(GettextCatalog.GetString("Cast to '{0}'"), castToType),
                    token =>
                    {
                        var newRoot = root.ReplaceNode(expression, SyntaxFactory.CastExpression(SyntaxFactory.ParseTypeName(castToType), expression.Parenthesize()).WithAdditionalAnnotations(Formatter.Annotation, Simplifier.Annotation));
                        return Task.FromResult(document.WithSyntaxRoot(newRoot));
                    }), diagnostic);
            }
            if (expression.Parent.IsKind(SyntaxKind.EqualsValueClause) && expression.Parent.Parent.IsKind(SyntaxKind.VariableDeclarator))
            {
                idx1 = message.IndexOf('\'');
                if (idx1 < 0)
                {
                    idx1 = message.IndexOf('"');
                }
                if (idx1 >= 0)
                {
                    idx2 = message.IndexOf('\'', idx1 + 1);
                    if (idx2 < 0)
                    {
                        idx2 = message.IndexOf('"', idx1 + 1);
                    }
                    if (idx2 >= 0)
                    {
                        string castFromType = message.Substring(idx1 + 1, idx2 - idx1 - 1);

                        var fd = expression.Parent.Parent.Parent.Parent as FieldDeclarationSyntax;
                        if (fd != null)
                        {
                            context.RegisterCodeFix(CodeActionFactory.Create(
                                expression.Span,
                                diagnostic.Severity,
                                GettextCatalog.GetString("Change field type"),
                                token =>
                                {
                                    var newRoot = root.ReplaceNode(fd.Declaration, fd.Declaration.WithType(SyntaxFactory.ParseTypeName(castFromType).WithLeadingTrivia(fd.Declaration.GetLeadingTrivia()).WithTrailingTrivia(fd.Declaration.GetTrailingTrivia()).WithAdditionalAnnotations(Simplifier.Annotation)));
                                    return Task.FromResult(document.WithSyntaxRoot(newRoot));
                                }
                            ), diagnostic);
                        }

                        var lc = expression.Parent.Parent.Parent.Parent as LocalDeclarationStatementSyntax;
                        if (lc != null)
                        {
                            context.RegisterCodeFix(CodeActionFactory.Create(
                                expression.Span,
                                diagnostic.Severity,
                                GettextCatalog.GetString("Change local variable type"),
                                token =>
                                {
                                    var newRoot = root.ReplaceNode(lc.Declaration, lc.Declaration.WithType(SyntaxFactory.IdentifierName("var").WithLeadingTrivia(lc.Declaration.Type.GetLeadingTrivia()).WithTrailingTrivia(lc.Declaration.Type.GetTrailingTrivia()).WithAdditionalAnnotations(Simplifier.Annotation)));
                                    return Task.FromResult(document.WithSyntaxRoot(newRoot));
                                }
                            ), diagnostic);
                        }
                    }
                }
            }

            if (expression.Parent is ReturnStatementSyntax)
            {
                idx1 = message.IndexOf('\'');
                if (idx1 < 0)
                {
                    idx1 = message.IndexOf('"');
                }
                if (idx1 >= 0)
                {
                    idx2 = message.IndexOf('\'', idx1 + 1);
                    if (idx2 < 0)
                    {
                        idx2 = message.IndexOf('"', idx1 + 1);
                    }
                    if (idx2 >= 0)
                    {
                        string castFromType = message.Substring(idx1 + 1, idx2 - idx1 - 1);

                        var method = expression.Parent.AncestorsAndSelf().OfType<MethodDeclarationSyntax>().FirstOrDefault();
                        if (method != null)
                        {
                            context.RegisterCodeFix(CodeActionFactory.Create(
                                expression.Span,
                                diagnostic.Severity,
                                GettextCatalog.GetString("Change return type of method"),
                                token =>
                                {
                                    var newRoot = root.ReplaceNode(method, method.WithReturnType(SyntaxFactory.ParseTypeName(castFromType).WithLeadingTrivia(method.ReturnType.GetLeadingTrivia()).WithTrailingTrivia(method.ReturnType.GetTrailingTrivia()).WithAdditionalAnnotations(Simplifier.Annotation)));
                                    return Task.FromResult(document.WithSyntaxRoot(newRoot));
                                }
                            ), diagnostic);
                        }
                    }
                }
            }
        }
    }
}