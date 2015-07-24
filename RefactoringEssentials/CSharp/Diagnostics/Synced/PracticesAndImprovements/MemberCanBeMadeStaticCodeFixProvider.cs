using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace RefactoringEssentials.CSharp.Diagnostics
{
    [ExportCodeFixProvider(LanguageNames.CSharp), System.Composition.Shared]
    public class MemberCanBeMadeStaticCodeFixProvider : CodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds
        {
            get
            {
                return ImmutableArray.Create(CSharpDiagnosticIDs.MemberCanBeMadeStaticAnalyzerID);
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
            var node = root.FindNode(context.Span);

            if(node is MethodDeclarationSyntax)
                MakeMethodStaticFix(node as MethodDeclarationSyntax, diagnostic,context,root);
            else if(node is PropertyDeclarationSyntax)
                MakePropertyStaticFix(node as PropertyDeclarationSyntax, diagnostic,context,root);
            else if(node is EventDeclarationSyntax)
                MakeEventStaticFix(node as EventDeclarationSyntax,diagnostic, context,root);

        }



        public void MakeMethodStaticFix(MethodDeclarationSyntax methodDeclaration, Diagnostic diagnostic, CodeFixContext context, SyntaxNode root)
        {
            context.RegisterCodeFix(CodeActionFactory.Create(methodDeclaration.Span, diagnostic.Severity, "Method can be made static",
                token =>
                {
                    var oldNode = methodDeclaration;
                    var newRoot = root.ReplaceNode(oldNode, methodDeclaration.WithModifiers(methodDeclaration.Modifiers.Add(SyntaxFactory.Token(SyntaxKind.StaticKeyword))).WithLeadingTrivia());
                    return Task.FromResult(context.Document.WithSyntaxRoot(newRoot)); // I don't know what to put there... 
                }), diagnostic);

        }

        public void MakePropertyStaticFix(PropertyDeclarationSyntax propertyDeclaration, Diagnostic diagnostic, CodeFixContext context, SyntaxNode root)
        {
            context.RegisterCodeFix(CodeActionFactory.Create(propertyDeclaration.Span, diagnostic.Severity, "Redundant explicit nullable type creation",
                token =>
                {
                    var oldNode = propertyDeclaration;
                    propertyDeclaration.WithModifiers(propertyDeclaration.Modifiers.Add(SyntaxFactory.Token(SyntaxKind.StaticKeyword)));
                    var newRoot = root.ReplaceNode(oldNode, propertyDeclaration.WithLeadingTrivia());
                    return Task.FromResult(context.Document.WithSyntaxRoot(newRoot)); // I don't know what to put there... 
                }), diagnostic);
        }

        public void MakeEventStaticFix(EventDeclarationSyntax eventDeclaration, Diagnostic diagnostic, CodeFixContext context, SyntaxNode root)
        {
            context.RegisterCodeFix(CodeActionFactory.Create(eventDeclaration.Span, diagnostic.Severity, "Redundant explicit nullable type creation",
                token =>
                {
                    var oldNode = eventDeclaration;
                    eventDeclaration.WithModifiers(eventDeclaration.Modifiers.Add(SyntaxFactory.Token(SyntaxKind.StaticKeyword)));
                    var newRoot = root.ReplaceNode(oldNode, eventDeclaration.WithLeadingTrivia());
                    return Task.FromResult(context.Document.WithSyntaxRoot(newRoot)); // I don't know what to put there... 
                }), diagnostic);
        }

    }
}