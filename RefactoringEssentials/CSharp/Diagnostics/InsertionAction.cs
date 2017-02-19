using System;
using System.Threading;
using Microsoft.CodeAnalysis;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Text;
using Microsoft.CodeAnalysis.Simplification;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.CSharp.Formatting;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;

namespace RefactoringEssentials
{
    public sealed class InsertionAction : NRefactoryCodeAction
    {
        readonly string title;
        readonly Func<CancellationToken, Task<InsertionResult>> createInsertion;

        public Func<CancellationToken, Task<InsertionResult>> CreateInsertion
        {
            get
            {
                return createInsertion;
            }
        }

        public override string Title
        {
            get
            {
                return title;
            }
        }

        public InsertionAction(TextSpan textSpan, DiagnosticSeverity severity, string title, Func<CancellationToken, Task<InsertionResult>> createInsertion) : base(textSpan, severity)
        {
            this.title = title;
            this.createInsertion = createInsertion;
        }

        protected override Task<Document> GetChangedDocumentAsync(CancellationToken cancellationToken)
        {
            return createInsertion.Invoke(cancellationToken).ContinueWith(t => CreateChangedDocument(t, cancellationToken).GetAwaiter().GetResult());
        }

        static async Task<Document> CreateChangedDocument(Task<InsertionResult> task, CancellationToken cancellationToken)
        {
            var insertionResult = task.GetAwaiter().GetResult();
            var document = insertionResult.Context.Document;
            var model = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);
            var root = await model.SyntaxTree.GetRootAsync(cancellationToken).ConfigureAwait(false);

            var targetType = root.FindNode(insertionResult.Location.SourceSpan).AncestorsAndSelf().OfType<TypeDeclarationSyntax>().FirstOrDefault();

            var childNodes = targetType.Members;
            var memberToInsert = (MemberDeclarationSyntax)insertionResult.Node.WithAdditionalAnnotations(Formatter.Annotation);

            SyntaxNode newRoot;
            if (childNodes.Count > 0)
            {
                newRoot = root.InsertNodesBefore(childNodes.First(n => n.Span.End > insertionResult.Context.Span.Start), new[] {
                    memberToInsert
                });
            }
            else
            {
                var newType = targetType;

                var classDecl = targetType as ClassDeclarationSyntax;
                if (classDecl != null)
                {
                    newType = classDecl.WithMembers(SyntaxFactory.List(new[] { memberToInsert }));
                }

                var ifaceDecl = targetType as InterfaceDeclarationSyntax;
                if (ifaceDecl != null)
                {
                    newType = ifaceDecl.WithMembers(SyntaxFactory.List(new[] { memberToInsert }));
                }

                var structDecl = targetType as StructDeclarationSyntax;
                if (structDecl != null)
                {
                    newType = structDecl.WithMembers(SyntaxFactory.List(new[] { memberToInsert }));
                }

                newRoot = root.ReplaceNode(targetType, newType);
            }

            return document.WithSyntaxRoot(newRoot);
        }

        protected override async Task<Document> PostProcessChangesAsync(Document document, CancellationToken cancellationToken)
        {
            document = await Simplifier.ReduceAsync(document, Simplifier.Annotation, cancellationToken: cancellationToken).ConfigureAwait(false);

            var options = document.Project.Solution.Workspace.Options;

            options = options.WithChangedOption(CSharpFormattingOptions.SpaceWithinSquareBrackets, false);
            options = options.WithChangedOption(CSharpFormattingOptions.SpaceBetweenEmptySquareBrackets, false);
            options = options.WithChangedOption(CSharpFormattingOptions.SpaceBetweenEmptyMethodCallParentheses, false);
            options = options.WithChangedOption(CSharpFormattingOptions.SpaceBetweenEmptyMethodDeclarationParentheses, false);
            options = options.WithChangedOption(CSharpFormattingOptions.SpaceWithinOtherParentheses, false);

            document = await Formatter.FormatAsync(document, Formatter.Annotation, options: options, cancellationToken: cancellationToken).ConfigureAwait(false);
            document = await CaseCorrector.CaseCorrectAsync(document, CaseCorrector.Annotation, cancellationToken).ConfigureAwait(false);
            return document;
        }
    }
}

