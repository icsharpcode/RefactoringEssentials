using RefactoringEssentials;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;

namespace RefactoringEssentials.CSharp.CodeFixes
{
    [ExportCodeFixProvider(LanguageNames.CSharp), System.Composition.Shared]
    public class CS0108UseNewKeywordIfHidingIntendedCodeFixProvider : CodeFixProvider
    {
        const string CS0108 = "CS0108"; // Warning CS0108: Use the new keyword if hiding was intended

        public sealed override ImmutableArray<string> FixableDiagnosticIds
        {
            get { return ImmutableArray.Create(CS0108); }
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
            var model = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);
            if (model.IsFromGeneratedCode(cancellationToken))
                return;
            var root = await model.SyntaxTree.GetRootAsync(cancellationToken).ConfigureAwait(false);

            var diagnostic = diagnostics.First();
            var node = root.FindNode(context.Span);
            context.RegisterCodeFix(CodeActionFactory.Create(
                node.Span,
                diagnostic.Severity,
                GettextCatalog.GetString("Add 'new' modifier"),
                token =>
                {
                    SyntaxNode newRoot;
                    if (node.Kind() != SyntaxKind.VariableDeclarator)
                        newRoot = root.ReplaceNode(node, AddNewModifier(node));
                    else //this one wants to be awkward - you can't add modifiers to a variable declarator
                    {
                        SyntaxNode declaringNode = node.Parent.Parent;
                        if (declaringNode is FieldDeclarationSyntax)
                            newRoot = root.ReplaceNode(node.Parent.Parent, (node.Parent.Parent as FieldDeclarationSyntax).AddModifiers(SyntaxFactory.Token(SyntaxKind.NewKeyword)));
                        else //it's an event declaration
                            newRoot = root.ReplaceNode(node.Parent.Parent, (node.Parent.Parent as EventFieldDeclarationSyntax).AddModifiers(SyntaxFactory.Token(SyntaxKind.NewKeyword)));
                    }
                    return Task.FromResult(document.WithSyntaxRoot(newRoot.WithAdditionalAnnotations(Formatter.Annotation)));
                }), diagnostic);
        }

        SyntaxNode AddNewModifier(SyntaxNode node)
        {
            SyntaxToken newToken = SyntaxFactory.Token(SyntaxKind.NewKeyword);
            switch (node.Kind())
            {
                //couldn't find a common base
                case SyntaxKind.IndexerDeclaration:
                    var indexer = (IndexerDeclarationSyntax)node;
                    return indexer.AddModifiers(newToken);
                case SyntaxKind.ClassDeclaration:
                    var classDecl = (ClassDeclarationSyntax)node;
                    return classDecl.AddModifiers(newToken);
                case SyntaxKind.PropertyDeclaration:
                    var propDecl = (PropertyDeclarationSyntax)node;
                    return propDecl.AddModifiers(newToken);
                case SyntaxKind.MethodDeclaration:
                    var methDecl = (MethodDeclarationSyntax)node;
                    return methDecl.AddModifiers(newToken);
                case SyntaxKind.StructDeclaration:
                    var structDecl = (StructDeclarationSyntax)node;
                    return structDecl.AddModifiers(newToken);
                case SyntaxKind.EnumDeclaration:
                    var enumDecl = (EnumDeclarationSyntax)node;
                    return enumDecl.AddModifiers(newToken);
                case SyntaxKind.InterfaceDeclaration:
                    var intDecl = (InterfaceDeclarationSyntax)node;
                    return intDecl.AddModifiers(newToken);
                case SyntaxKind.DelegateDeclaration:
                    var deleDecl = (DelegateDeclarationSyntax)node;
                    return deleDecl.AddModifiers(newToken);
                default:
                    return node;
            }
        }
    }
}
