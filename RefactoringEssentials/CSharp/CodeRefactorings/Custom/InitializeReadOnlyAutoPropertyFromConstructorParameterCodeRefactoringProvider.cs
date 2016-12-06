using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis.Formatting;
using System.Diagnostics.Contracts;
using Microsoft.CodeAnalysis.Text;

namespace RefactoringEssentials.CSharp.CodeRefactorings
{
    public class ConstructorParameterContext
    {
        public string ParameterName { get; }
        public string PropertyName { get; }
        public TypeSyntax Type { get; }
        public ConstructorDeclarationSyntax Constructor { get; }
        public TextSpan TextSpan { get; }
        public SyntaxNode Root { get; }
        public Document Document { get; }

        public ConstructorParameterContext(Document document, string parameterName, string propertyName, TypeSyntax type, ConstructorDeclarationSyntax constructor, TextSpan textSpan, SyntaxNode root)
        {
            Contract.Requires(document != null);
            Contract.Requires(parameterName != null);
            Contract.Requires(propertyName != null);
            Contract.Requires(type != null);
            Contract.Requires(constructor != null);
            Contract.Requires(root != null);

            Document = document;
            Constructor = constructor;
            Type = type;
            ParameterName = parameterName;
            PropertyName = propertyName;
            TextSpan = textSpan;
            Root = root;
        }
    }

    public class ConstructorParameterContextFinder
    {
        public async Task<ConstructorParameterContext> Find(CodeRefactoringContext context)
        {
            var document = context.Document;
            if (document.Project.Solution.Workspace.Kind == WorkspaceKind.MiscellaneousFiles)
                return null;
            var span = context.Span;
            if (!span.IsEmpty)
                return null;
            var cancellationToken = context.CancellationToken;
            if (cancellationToken.IsCancellationRequested)
                return null;
            var model = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);
            if (model.IsFromGeneratedCode(cancellationToken))
                return null;
            var root = await model.SyntaxTree.GetRootAsync(cancellationToken).ConfigureAwait(false);
            var token = root.FindToken(span.Start);
            var parameter = token.Parent as ParameterSyntax;

            if (parameter == null)
                return null;

            var ctor = parameter.Parent.Parent as ConstructorDeclarationSyntax;
            if (ctor == null)
                return null;

            return new ConstructorParameterContext(document, parameter.Identifier.ToString(), GetPropertyName(parameter.Identifier.ToString()), parameter.Type, ctor, span, root);
        }

        static string GetPropertyName(string v)
        {
            return char.ToUpper(v[0]) + v.Substring(1);
        }
    }

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
                        constructorParameterContext.TextSpan,// parameter.Span,
                        DiagnosticSeverity.Info,
                        GettextCatalog.GetString("Initialize readonly auto-property from parameter"),
                        t2 => CreateAndInitialiseReadOnlyPropertyFromConstructorParameter(constructorParameterContext)
                    )
                );
        }

        static Task<Document> CreateAndInitialiseReadOnlyPropertyFromConstructorParameter(ConstructorParameterContext context)
        {
            var accessorDeclList = new SyntaxList<AccessorDeclarationSyntax>()
            .Add(SyntaxFactory.AccessorDeclaration(SyntaxKind.GetAccessorDeclaration).WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken)));
            var newProperty = SyntaxFactory.PropertyDeclaration(context.Type, context.PropertyName)
                .WithAccessorList(SyntaxFactory.AccessorList(accessorDeclList))
                .WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword)))
                .WithAdditionalAnnotations(Formatter.Annotation);

            var assignmentStatement = SyntaxFactory.ExpressionStatement(
                SyntaxFactory.AssignmentExpression(
                    SyntaxKind.SimpleAssignmentExpression,
                    context.PropertyName != context.ParameterName ? (ExpressionSyntax)SyntaxFactory.IdentifierName(context.PropertyName) : SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, SyntaxFactory.ThisExpression(), SyntaxFactory.IdentifierName(context.ParameterName)),
                    SyntaxFactory.IdentifierName(context.ParameterName)
                )
            ).WithAdditionalAnnotations(Formatter.Annotation);

            var trackedRoot = context.Root.TrackNodes(context.Constructor);
            var newRoot = trackedRoot.InsertNodesBefore(trackedRoot.GetCurrentNode(context.Constructor), new List<SyntaxNode>() {
                                newProperty
                            });
            var ctorBody = context.Constructor.Body ?? SyntaxFactory.Block();
            newRoot = newRoot.ReplaceNode(newRoot.GetCurrentNode(context.Constructor), context.Constructor.WithBody(
                ctorBody.WithStatements(SyntaxFactory.List(new[] { assignmentStatement }.Concat(ctorBody.Statements)))
            ));

            return Task.FromResult(context.Document.WithSyntaxRoot(newRoot));
        }
    }
}
