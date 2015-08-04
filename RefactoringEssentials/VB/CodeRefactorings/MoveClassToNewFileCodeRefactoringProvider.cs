// Strathweb.Roslyn/MoveClassToFile/MoveClassToFile/MoveClassToFileCodeRefactoringProvider.cs
// This is a port from the above adjusted to VB. All creds to StrathWeb

using System.Collections.Generic;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis.VisualBasic;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace MoveClassToFile
{


    [ExportCodeRefactoringProvider(RefactoringId, LanguageNames.VisualBasic), Shared]
    public class MoveClassToFileCodeRefactoringProvider : CodeRefactoringProvider
    {
        public const string RefactoringId = "MoveClassToNewFile";

        public sealed override async Task ComputeRefactoringsAsync(CodeRefactoringContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
            var node = root.FindNode(context.Span);

            // only for a type declaration node that doesn't match the current file name
            // also omit all private classes
            var typeDecl = node as ClassStatementSyntax;
            var className = typeDecl.Identifier.GetIdentifierText() + ".vb";

            if(typeDecl == null || context.Document.Name.ToLowerInvariant() == className.ToLowerInvariant() || typeDecl.Modifiers.Any(SyntaxKind.PrivateKeyword))
            { return; }

            var action = CodeAction.Create("Move class to file \"" + className + "\" ", c => MoveClassIntoNewFileAsync(context.Document, typeDecl, className, c));
            context.RegisterRefactoring(action);
        }

        async Task<Solution> MoveClassIntoNewFileAsync(Document document, ClassStatementSyntax typeDecl, string className, CancellationToken cancellationToken)
        {
            var identifierToken = typeDecl.Identifier;

            // symbol representing the type
            var semanticModel = await document.GetSemanticModelAsync(cancellationToken);
            var typeSymbol = semanticModel.GetDeclaredSymbol(typeDecl, cancellationToken);

            // remove type from current files
            var currentSyntaxTree = await document.GetSyntaxTreeAsync();
            var currentRoot = await currentSyntaxTree.GetRootAsync();


            //In VB you have to remove th parent of det declaration node. That is the "ClassBlockSyntax"
            var replacedRoot = currentRoot.RemoveNode(typeDecl.Parent, SyntaxRemoveOptions.KeepNoTrivia);
            document = document.WithSyntaxRoot(replacedRoot);


            // create new tree for a new file
            // we drag all the imports because we don't know which are needed
            // and there is no easy way to find out which
            var currentUsings = currentRoot.DescendantNodesAndSelf().Where(s => s is ImportsStatementSyntax);

            var currentNs = (NamespaceStatementSyntax)currentRoot.DescendantNodesAndSelf().First(s => s is NamespaceStatementSyntax);

            SyntaxList<StatementSyntax> c;

            if(currentNs != null)
            {
                var temp = SyntaxFactory.SingletonList(
                SyntaxFactory.NamespaceBlock(
                     SyntaxFactory.NamespaceStatement(SyntaxFactory.ParseName(currentNs.Name.ToString())),
                        SyntaxFactory.SingletonList(typeDecl.Parent),
                        SyntaxFactory.EndNamespaceStatement()
                     ));
                c = SyntaxFactory.List(temp.Select(i => ((StatementSyntax)i)));
            }
            else
            {
                c = SyntaxFactory.SingletonList(typeDecl.Parent);
            }

            var newFileTree = SyntaxFactory.CompilationUnit()
                .WithImports(SyntaxFactory.List(currentUsings.Select(i => ((ImportsStatementSyntax)i))))
                .WithMembers(c)
                .WithoutLeadingTrivia()
                .NormalizeWhitespace();

            var codeText = newFileTree.ToFullString();

            //TODO: handle name conflicts
            var newDocument = document.Project.AddDocument(className, SourceText.From(codeText), document.Folders);

            newDocument = await RemoveUnusedImportDirectivesAsync(newDocument, cancellationToken);

            return newDocument.Project.Solution;

        }

        static async Task<Document> RemoveUnusedImportDirectivesAsync(Document document, CancellationToken cancellationToken)
        {
            var root = await document.GetSyntaxRootAsync();
            var semanticModel = await document.GetSemanticModelAsync();

            root = RemoveUnusedImportDirectives(semanticModel, root, cancellationToken);
            document = document.WithSyntaxRoot(root);
            return document;
        }

        static SyntaxNode RemoveUnusedImportDirectives(SemanticModel semanticModel, SyntaxNode root, CancellationToken cancellationToken)
        {
            var oldUsings = root.DescendantNodesAndSelf().Where(s => s is ImportsStatementSyntax);
            var unusedUsings = GetUnusedImportDirectives(semanticModel, cancellationToken);
            SyntaxTriviaList leadingTrivia = root.GetLeadingTrivia();

            root = root.RemoveNodes(oldUsings, SyntaxRemoveOptions.KeepNoTrivia);
            var newUsings = SyntaxFactory.List(oldUsings.Except(unusedUsings));
            root = ((CompilationUnitSyntax)root)
                .WithImports(newUsings)
                .NormalizeWhitespace()
                .WithLeadingTrivia(leadingTrivia);

            return root;
        }

        static HashSet<SyntaxNode> GetUnusedImportDirectives(SemanticModel model, CancellationToken cancellationToken)
        {
            HashSet<SyntaxNode> unusedImportDirectives = new HashSet<SyntaxNode>();
            SyntaxNode root = model.SyntaxTree.GetRoot(cancellationToken);
            foreach(Diagnostic diagnostic in model.GetDiagnostics(null, cancellationToken).Where(d => d.Id == "BC50001"))
            {
                ImportsStatementSyntax usingDirectiveSyntax = root.FindNode(diagnostic.Location.SourceSpan, false, false) as ImportsStatementSyntax;
                if(usingDirectiveSyntax != null)
                {
                    unusedImportDirectives.Add(usingDirectiveSyntax);
                }
            }

            return unusedImportDirectives;
        }
    }
}