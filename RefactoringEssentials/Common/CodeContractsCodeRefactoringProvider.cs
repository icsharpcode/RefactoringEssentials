using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.CodeRefactorings;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Text;
using System.Threading;
using System.Diagnostics.Contracts;
using Microsoft.CodeAnalysis.CodeActions;
using System;

namespace RefactoringEssentials.CSharp.CodeRefactorings
{

    public class Context
    {
        readonly Document document;
        readonly TextSpan textSpan;
        readonly CancellationToken cancellationToken;
        readonly SemanticModel semanticModel;
        readonly SyntaxNode root;
        readonly SyntaxNode node;

        public Context(Document document, TextSpan textSpan, CancellationToken cancellationToken, SemanticModel semanticModel, SyntaxNode root, SyntaxNode node)
        {
            Contract.Requires(document != null);
            Contract.Requires(textSpan != null);
            Contract.Requires(cancellationToken != null);
            Contract.Requires(semanticModel != null);
            Contract.Requires(root != null);
            Contract.Requires(node != null);

            this.document = document;
            this.textSpan = textSpan;
            this.cancellationToken = cancellationToken;
            this.semanticModel = semanticModel;
            this.root = root;
            this.node = node;
        }

        public Document Document
        {
            get
            {
                Contract.Ensures(Contract.Result<Document>() != null);
                return document;
            }
        }

        public TextSpan TextSpan
        {
            get
            {
                Contract.Ensures(Contract.Result<TextSpan>() != null);
                return textSpan;
            }
        }

        public CancellationToken CancellationToken
        {
            get
            {
                Contract.Ensures(Contract.Result<CancellationToken>() != null);
                return cancellationToken;
            }
        }

        public SemanticModel SemanticModel
        {
            get
            {
                Contract.Ensures(Contract.Result<SemanticModel>() != null);
                return semanticModel;
            }
        }

        public SyntaxNode Root
        {
            get
            {
                Contract.Ensures(Contract.Result<SyntaxNode>() != null);
                return root;
            }
        }

        public SyntaxNode Node
        {
            get
            {
                Contract.Ensures(Contract.Result<SyntaxNode>() != null);
                return node;
            }
        }
    }

    public abstract class CodeContractsCodeRefactoringProvider : CodeRefactoringProvider
    {

        protected async Task<Context> CodeContractsContext(CodeRefactoringContext context)
        {
            var document = context.Document;
            if (document.Project.Solution.Workspace.Kind == WorkspaceKind.MiscellaneousFiles)
                return null;

            var textSpan = context.Span;
            if (!textSpan.IsEmpty)
                return null;

            var cancellationToken = context.CancellationToken;
            if (cancellationToken.IsCancellationRequested)
                return null;

            var semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);
            if (semanticModel.IsFromGeneratedCode(cancellationToken))
                return null;

            var root = await semanticModel.SyntaxTree.GetRootAsync(cancellationToken).ConfigureAwait(false);
            if (!root.Span.Contains(textSpan))
                return null;

            var node = root.FindNode(textSpan, false, true);
            if (node == null) return null;

            return new Context(document, textSpan, cancellationToken, semanticModel, root, node);
        }

        protected static CodeAction CreateAction(TextSpan textSpan, Func<CancellationToken, Task<Document>> changedDocument, string description)
        {
            return CodeActionFactory.Create(
                textSpan,
                DiagnosticSeverity.Info,
                GettextCatalog.GetString(description),
                changedDocument
            );
        }

        protected static bool UsingStatementNotPresent(CompilationUnitSyntax cu)
        {
            return !cu.Usings.Any((UsingDirectiveSyntax u) => u.Name.ToString() == "System.Diagnostics.Contracts");
        }

        protected static CompilationUnitSyntax AddUsingStatement(SyntaxNode node, CompilationUnitSyntax cu)
        {
            return cu.AddUsingDirective(
                SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("System.Diagnostics.Contracts")).WithAdditionalAnnotations(Formatter.Annotation)
                , node
                , true);
        }
    }
}
