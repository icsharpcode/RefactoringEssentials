using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis.Formatting;

namespace RefactoringEssentials.CSharp.CodeRefactorings
{
    [ExportCodeRefactoringProvider(LanguageNames.CSharp, Name = "Initialize readonly auto property from constructor parameter")]
    public class InitializeReadOnlyAutoPropertyFromConstructorParameterCodeRefactoringProvider : CodeRefactoringProvider
    {
        ConstructorParameterContextFinder ConstructorParameterContextFinder { get; }

        public InitializeReadOnlyAutoPropertyFromConstructorParameterCodeRefactoringProvider()
        {
            ConstructorParameterContextFinder = new ConstructorParameterContextFinder();
        }

        public override async Task ComputeRefactoringsAsync(CodeRefactoringContext context)
        {
            var constructorParameterContext = await ConstructorParameterContextFinder.Find(context);

            if (constructorParameterContext == null)
                return;

            context.RegisterRefactoring(
                    CodeActionFactory.Create(
                        constructorParameterContext.TextSpan,
                        DiagnosticSeverity.Info,
                        GettextCatalog.GetString("Initialize readonly auto-property from parameter"),
                        t2 => CreateAndInitialiseReadOnlyPropertyFromConstructorParameter(constructorParameterContext)
                    )
                );
        }

        static Task<Document> CreateAndInitialiseReadOnlyPropertyFromConstructorParameter(ConstructorParameterContext context)
        {
            var trackedRoot = context.Root.TrackNodes(context.Constructor);

            var rootWithNewProperty = AddBefore(
                root: trackedRoot, 
                loationToAddBefore: context.Constructor, 
                nodeToAdd: CreateAutoProperty(
                    type: context.Type, 
                    identifier: context.PropertyName, 
                    getOnly: true, 
                    accessibility: SyntaxKind.PublicKeyword));

            var rootWithAssignmentAndProperty = AddStatementToConstructorBody(
                root: rootWithNewProperty, 
                constructor: context.Constructor, 
                statement: CreateAssignmentStatement(
                    leftHandSidePropertyName: context.PropertyName, 
                    rightHandSidePropertyName: context.ParameterName));

            return Task.FromResult(context.Document.WithSyntaxRoot(rootWithAssignmentAndProperty));
        }

        static SyntaxNode AddBefore(SyntaxNode root, SyntaxNode loationToAddBefore, SyntaxNode nodeToAdd)
        {
            return root.InsertNodesBefore(
                root.GetCurrentNode(loationToAddBefore)
                , new List<SyntaxNode> { nodeToAdd });
        }

        static SyntaxNode AddStatementToConstructorBody(SyntaxNode root, ConstructorDeclarationSyntax constructor, StatementSyntax statement)
        {
            var body = constructor.Body ?? SyntaxFactory.Block();

            return root.ReplaceNode(root.GetCurrentNode(constructor), constructor.WithBody(
                    body.WithStatements(SyntaxFactory.List(new[] { statement }.Concat(body.Statements)))
                ));
        }

        static ExpressionStatementSyntax CreateAssignmentStatement(string leftHandSidePropertyName, string rightHandSidePropertyName)
        {
            return SyntaxFactory.ExpressionStatement(
                SyntaxFactory.AssignmentExpression(
                    SyntaxKind.SimpleAssignmentExpression,
                    leftHandSidePropertyName != rightHandSidePropertyName ? (ExpressionSyntax)SyntaxFactory.IdentifierName(leftHandSidePropertyName) : SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, SyntaxFactory.ThisExpression(), SyntaxFactory.IdentifierName(rightHandSidePropertyName)),
                    SyntaxFactory.IdentifierName(rightHandSidePropertyName)
                )
            ).WithAdditionalAnnotations(Formatter.Annotation);
        }

        static PropertyDeclarationSyntax CreateAutoProperty(TypeSyntax type, string identifier, bool getOnly, SyntaxKind? accessibility)
        {
            var accessorDeclList = new SyntaxList<AccessorDeclarationSyntax>()
            .Add(SyntaxFactory.AccessorDeclaration(SyntaxKind.GetAccessorDeclaration).WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken)));

            if (getOnly == false)
                accessorDeclList.Add(SyntaxFactory.AccessorDeclaration(SyntaxKind.SetAccessorDeclaration).WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken)));

            var newProperty = SyntaxFactory.PropertyDeclaration(type, identifier)
                .WithAccessorList(SyntaxFactory.AccessorList(accessorDeclList));

            if (accessibility.HasValue)
                newProperty = newProperty.WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(accessibility.Value)));

            return newProperty.WithAdditionalAnnotations(Formatter.Annotation);
        }
    }
}
