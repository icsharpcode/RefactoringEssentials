using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CodeFixes;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;

namespace RefactoringEssentials.CSharp.Diagnostics
{

    [ExportCodeFixProvider(LanguageNames.CSharp), System.Composition.Shared]
    public class RedundantInternalCodeFixProvider : CodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds
        {
            get
            {
                return ImmutableArray.Create(CSharpDiagnosticIDs.RedundantInternalAnalyzerID);
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
            var newRoot = root.ReplaceNode(node, RemoveInternalModifier(node));
            context.RegisterCodeFix(CodeActionFactory.Create(node.Span, diagnostic.Severity, "Remove redundant 'internal' modifier", document.WithSyntaxRoot(newRoot)), diagnostic);
        }

        public static SyntaxNode RemoveInternalModifier(SyntaxNode node)
        {
            Func<SyntaxToken, bool> isNotInternal = (m => !m.IsKind(SyntaxKind.InternalKeyword));
            var classNode = node as ClassDeclarationSyntax;
            if (classNode != null)
                return classNode.WithModifiers(SyntaxFactory.TokenList(classNode.Modifiers.Where(isNotInternal)))
                    .WithLeadingTrivia(classNode.GetLeadingTrivia());

            var structNode = node as StructDeclarationSyntax;
            if (structNode != null)
                return structNode.WithModifiers(SyntaxFactory.TokenList(structNode.Modifiers.Where(isNotInternal)))
                    .WithLeadingTrivia(structNode.GetLeadingTrivia());

            var interNode = node as InterfaceDeclarationSyntax;
            if (interNode != null)
                return interNode.WithModifiers(SyntaxFactory.TokenList(interNode.Modifiers.Where(isNotInternal)))
                    .WithLeadingTrivia(interNode.GetLeadingTrivia());

            var delegateNode = node as DelegateDeclarationSyntax;
            if (delegateNode != null)
                return delegateNode.WithModifiers(SyntaxFactory.TokenList(delegateNode.Modifiers.Where(isNotInternal)))
                    .WithLeadingTrivia(delegateNode.GetLeadingTrivia());

            var enumNode = node as EnumDeclarationSyntax;
            if (enumNode != null)
                return enumNode.WithModifiers(SyntaxFactory.TokenList(enumNode.Modifiers.Where(isNotInternal)))
                    .WithLeadingTrivia(enumNode.GetLeadingTrivia());
            return node;
        }
    }
}