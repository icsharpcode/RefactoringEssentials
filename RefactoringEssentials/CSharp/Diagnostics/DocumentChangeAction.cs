using System;
using System.Threading;
using Microsoft.CodeAnalysis;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Text;
using Microsoft.CodeAnalysis.Simplification;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.CSharp.Formatting;

namespace RefactoringEssentials
{
    public sealed class DocumentChangeAction : NRefactoryCodeAction
    {
        readonly string title;
        readonly Func<CancellationToken, Task<Document>> createChangedDocument;
        readonly Func<CancellationToken, Task<Solution>> createChangedSolution;

        public override string Title
        {
            get
            {
                return title;
            }
        }

        public DocumentChangeAction(TextSpan textSpan, DiagnosticSeverity severity, string title, Func<CancellationToken, Task<Document>> createChangedDocument) : base(textSpan, severity)
        {
            this.title = title;
            this.createChangedDocument = createChangedDocument;
        }

        public DocumentChangeAction(TextSpan textSpan, DiagnosticSeverity severity, string title, Func<CancellationToken, Task<Solution>> createChangedSolution) : base(textSpan, severity)
        {
            this.title = title;
            this.createChangedSolution = createChangedSolution;
        }

        protected override Task<Document> GetChangedDocumentAsync(CancellationToken cancellationToken)
        {
            if (createChangedDocument == null)
                return base.GetChangedDocumentAsync(cancellationToken);

            var task = createChangedDocument.Invoke(cancellationToken);
            return task;
        }

        protected override Task<Solution> GetChangedSolutionAsync(CancellationToken cancellationToken)
        {
            if (createChangedSolution == null)
                return base.GetChangedSolutionAsync(cancellationToken);

            var task = createChangedSolution.Invoke(cancellationToken);
            return task;
        }

        protected override async Task<Document> PostProcessChangesAsync(Document document, CancellationToken cancellationToken)
        {
            document = await Simplifier.ReduceAsync(document, Simplifier.Annotation, cancellationToken: cancellationToken).ConfigureAwait(false);

            var options = document.Project.Solution.Workspace.Options;

            if (document.Project.Language == LanguageNames.CSharp)
            {
                options = options.WithChangedOption(CSharpFormattingOptions.SpaceWithinSquareBrackets, false);
                options = options.WithChangedOption(CSharpFormattingOptions.SpaceBetweenEmptySquareBrackets, false);
                options = options.WithChangedOption(CSharpFormattingOptions.SpaceBetweenEmptyMethodCallParentheses, false);
                options = options.WithChangedOption(CSharpFormattingOptions.SpaceBetweenEmptyMethodDeclarationParentheses, false);
                options = options.WithChangedOption(CSharpFormattingOptions.SpaceWithinOtherParentheses, false);
            }

            document = await Formatter.FormatAsync(document, Formatter.Annotation, options: options, cancellationToken: cancellationToken).ConfigureAwait(false);

            document = await CaseCorrector.CaseCorrectAsync(document, CaseCorrector.Annotation, cancellationToken).ConfigureAwait(false);
            return document;
        }
    }
}