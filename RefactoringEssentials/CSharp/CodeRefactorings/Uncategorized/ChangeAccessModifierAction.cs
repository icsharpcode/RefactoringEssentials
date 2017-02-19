using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace RefactoringEssentials.CSharp.CodeRefactorings
{
    [ExportCodeRefactoringProvider(LanguageNames.CSharp, Name = "Change access level")]
    /// <summary>
    /// Changes the access level of an entity declaration
    /// </summary>
    public class ChangeAccessModifierAction : CodeRefactoringProvider
    {
        public override async Task ComputeRefactoringsAsync(CodeRefactoringContext context)
        {
            var document = context.Document;
            if (document.Project.Solution.Workspace.Kind == WorkspaceKind.MiscellaneousFiles)
                return;
            var span = context.Span;
            if (!span.IsEmpty)
                return;
            var cancellationToken = context.CancellationToken;
            if (cancellationToken.IsCancellationRequested)
                return;
            var model = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);
            if (model.IsFromGeneratedCode(cancellationToken))
                return;
            var root = await model.SyntaxTree.GetRootAsync(cancellationToken).ConfigureAwait(false);
            var token = root.FindToken(span.Start);
            if (!token.IsIdentifierOrAccessorOrAccessibilityModifier())
                return;
            var node = token.Parent;
            while (node != null && !(node is MemberDeclarationSyntax || node is AccessorDeclarationSyntax))
                node = node.Parent;
            if (node == null || node.IsKind(SyntaxKind.InterfaceDeclaration, SyntaxKind.EnumMemberDeclaration))
                return;
            
            ISymbol symbol = null;
            var field = node as FieldDeclarationSyntax;
            if(field != null)
                symbol = model.GetDeclaredSymbol(field.Declaration.Variables.First(), cancellationToken);
            else
            {
                var member = node as MemberDeclarationSyntax;
                if(member != null)
                    symbol = model.GetDeclaredSymbol(member, cancellationToken);
                else
                {
                    var accessor = node as AccessorDeclarationSyntax;
                    if(accessor != null)
                        symbol = model.GetDeclaredSymbol(accessor, cancellationToken);
                }
            }
            if (!symbol.AccessibilityChangeable())
                return;
            
            foreach (var accessibility in GetPossibleAccessibilities(model, symbol, node, cancellationToken))
            {
                var modifiers = GetAccessibilityModifiers(accessibility);
                context.RegisterRefactoring(CodeActionFactory.Create(
                    token.Span,
                    DiagnosticSeverity.Info,
                    GettextCatalog.GetString("To " + String.Join(" ", modifiers)),
                    t =>
                    {
                        var newRoot = root.ReplaceNode(
                            node,
                            node.WithoutLeadingTrivia().WithModifiers(modifiers).WithLeadingTrivia(node.GetLeadingTrivia()));

                        return Task.FromResult(document.WithSyntaxRoot(newRoot));
                    }));
            }
        }

        static IEnumerable<Accessibility> GetPossibleAccessibilities(SemanticModel model, ISymbol member, SyntaxNode declaration, CancellationToken cancellationToken)
        {
            IEnumerable<Accessibility> result = null;
            var containingType = member.ContainingType;
            if (containingType == null)
            {
                if (member.IsPublic())
                    result = new [] { Accessibility.Internal };

                result = new [] { Accessibility.Public };
            }
            else if (containingType.IsValueType)
            {
                result = new []
                {
                    Accessibility.Private,
                    Accessibility.Internal,
                    Accessibility.Public
                };
            }
            else
            {
                result = new[]
                {
                    Accessibility.Private,
                    Accessibility.Protected,
                    Accessibility.Internal,
                    Accessibility.ProtectedOrInternal,
                    Accessibility.Public
                };
                if (member.IsAccessorMethod())
                {
                    var propertyDeclaration = declaration.Ancestors().OfType<PropertyDeclarationSyntax>().FirstOrDefault();
                    if (propertyDeclaration != null)
                    {
                        var property = model.GetDeclaredSymbol(propertyDeclaration, cancellationToken);
                        result = result.Where(a => a < property.DeclaredAccessibility);
                    }

                }
            }
            
            return result.Where(a => a != member.DeclaredAccessibility);
        }

        public static SyntaxTokenList GetAccessibilityModifiers(Accessibility accessibility)
        {
            var tokenList = new List<SyntaxToken>();
            switch (accessibility)
            {
                case Accessibility.Private:
                    tokenList.Add(SyntaxFactory.Token(SyntaxKind.PrivateKeyword));
                    break;

                case Accessibility.Protected:
                    tokenList.Add(SyntaxFactory.Token(SyntaxKind.ProtectedKeyword));
                    break;

                case Accessibility.Internal:
                    tokenList.Add(SyntaxFactory.Token(SyntaxKind.InternalKeyword));
                    break;

                case Accessibility.ProtectedOrInternal:
                    tokenList.Add(SyntaxFactory.Token(SyntaxKind.ProtectedKeyword));
                    tokenList.Add(SyntaxFactory.Token(SyntaxKind.InternalKeyword));
                    break;

                case Accessibility.Public:
                    tokenList.Add(SyntaxFactory.Token(SyntaxKind.PublicKeyword));
                    break;
            }
            return SyntaxFactory.TokenList(tokenList.Select(t => t.WithTrailingTrivia(SyntaxFactory.SyntaxTrivia(SyntaxKind.WhitespaceTrivia, " "))));
        }
    }
}

