using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.Simplification;
using System.Collections.Generic;
using System.Linq;

namespace RefactoringEssentials.CSharp
{
    static class Manipulations
    {
        internal static CodeRefactoringStatementSyntax ParseStatement(string statement)
        {
            return new CodeRefactoringStatementSyntax(SyntaxFactory.ParseStatement(statement + "\r\n")).WithAdditionalAnnotations(Formatter.Annotation, Simplifier.Annotation);
        }

        internal static PropertyDeclarationSyntax PropertyWithReplacedAccessors(PropertyDeclarationSyntax existingProperty, AccessorDeclarationSyntax getter, AccessorDeclarationSyntax setter)
        {
            return existingProperty.WithAccessorList(
                SyntaxFactory.AccessorList(
                    new SyntaxList<AccessorDeclarationSyntax>()
                    .Add(getter)
                    .Add(setter)
                  )
                ).WithAdditionalAnnotations(Formatter.Annotation);
        }

        internal static AccessorDeclarationSyntax GetAccessor(BlockSyntax body)
        {
            return SyntaxFactory.AccessorDeclaration(SyntaxKind.GetAccessorDeclaration, body);
        }

        internal static AccessorDeclarationSyntax SetAccessor(BlockSyntax body)
        {
            return SyntaxFactory.AccessorDeclaration(SyntaxKind.SetAccessorDeclaration, body);
        }

        internal static SyntaxNode AddBefore(SyntaxNode root, SyntaxNode loationToAddBefore, SyntaxNode nodeToAdd)
        {
            return root.InsertNodesBefore(
                root.GetCurrentNode(loationToAddBefore)
                , new List<SyntaxNode> { nodeToAdd });
        }

        internal static SyntaxNode AddStatementToConstructorBody(SyntaxNode root, ConstructorDeclarationSyntax constructor, StatementSyntax statement)
        {
            var body = constructor.Body ?? SyntaxFactory.Block();

            return root.ReplaceNode(root.GetCurrentNode(constructor), constructor.WithBody(
                    body.WithStatements(SyntaxFactory.List(new[] { statement }.Concat(body.Statements)))
                ));
        }

        internal static ExpressionStatementSyntax CreateAssignmentStatement(string leftHandSidePropertyName, string rightHandSidePropertyName)
        {
            return SyntaxFactory.ExpressionStatement(
                SyntaxFactory.AssignmentExpression(
                    SyntaxKind.SimpleAssignmentExpression,
                    leftHandSidePropertyName != rightHandSidePropertyName ? (ExpressionSyntax)SyntaxFactory.IdentifierName(leftHandSidePropertyName) : SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, SyntaxFactory.ThisExpression(), SyntaxFactory.IdentifierName(rightHandSidePropertyName)),
                    SyntaxFactory.IdentifierName(rightHandSidePropertyName)
                )
            ).WithAdditionalAnnotations(Formatter.Annotation);
        }

        internal static PropertyDeclarationSyntax CreateAutoProperty(TypeSyntax type, string identifier, SyntaxList<AccessorDeclarationSyntax> accessors, SyntaxKind? accessibility)
        {
            var newProperty = SyntaxFactory.PropertyDeclaration(type, identifier)
                .WithAccessorList(SyntaxFactory.AccessorList(accessors));

            if (accessibility.HasValue)
                newProperty = newProperty.WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(accessibility.Value)));

            return newProperty.WithAdditionalAnnotations(Formatter.Annotation);
        }

        internal static SyntaxList<AccessorDeclarationSyntax> GetAutoAccessor()
        {
            return new SyntaxList<AccessorDeclarationSyntax>().Add(SyntaxFactory.AccessorDeclaration(SyntaxKind.GetAccessorDeclaration).WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken)));
        }

        internal static SyntaxList<AccessorDeclarationSyntax> GetAndSetAutoAccessors()
        {
            return new SyntaxList<AccessorDeclarationSyntax>().Add(SyntaxFactory.AccessorDeclaration(SyntaxKind.GetAccessorDeclaration).WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken))).Add(SyntaxFactory.AccessorDeclaration(SyntaxKind.SetAccessorDeclaration).WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken)));
        }
    }
}
