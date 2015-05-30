using System.Linq;
using System.Threading;
using System.Collections.Generic;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.Text;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Formatting;

namespace RefactoringEssentials.CSharp.CodeRefactorings
{
    [ExportCodeRefactoringProvider(LanguageNames.CSharp, Name = "Add an exception description to the xml documentation")]
    public class AddExceptionDescriptionCodeRefactoringProvider : SpecializedCodeRefactoringProvider<ThrowStatementSyntax>
    {
        protected override IEnumerable<CodeAction> GetActions(Document document, SemanticModel semanticModel, SyntaxNode root, TextSpan span, ThrowStatementSyntax node, CancellationToken cancellationToken)
        {
            var entity = node.FirstAncestorOrSelf<MemberDeclarationSyntax>();
            if (entity == null)
                yield break;

            var rr = semanticModel.GetDeclaredSymbol(entity);
            if (rr == null)
                yield break;
            var expr = semanticModel.GetTypeInfo(node.Expression);
            if (expr.Type == null)
                yield break;

            bool hadDescription = false;
            foreach (var trivia in entity.GetLeadingTrivia())
            {
                if (!trivia.IsKind(SyntaxKind.SingleLineDocumentationCommentTrivia))
                    continue;
                hadDescription = true;
                if (trivia.HasStructure)
                {
                    var structure = trivia.GetStructure();
                    foreach (var d in structure.DescendantNodesAndSelf().OfType<XmlElementSyntax>())
                    {
                        if (d.StartTag.Name.LocalName.ToString() == "exception")
                        {
                            foreach (var n in d.StartTag.Attributes)
                            {
                                if (n.Name.LocalName.ToString() == "cref")
                                {
                                    // TODO: That's not a correct cref matching.
                                    if (n.ToString().Contains(expr.Type.Name))
                                        yield break;
                                }
                            }
                        }
                    }
                }
            }
            if (!hadDescription)
                yield break;
            yield return CodeActionFactory.Create(node.Span, DiagnosticSeverity.Info, GettextCatalog.GetString("Add exception description"),
                t2 =>
                {
                    var newComment = SyntaxFactory.ParseLeadingTrivia(string.Format("/// <exception cref=\"{0}\"></exception>\r\n", expr.Type.GetDocumentationCommentId()));
                    var list = entity.GetLeadingTrivia();
                    list = list.Add(newComment.First());
                    var newRoot = root.ReplaceNode((SyntaxNode)entity, entity.WithLeadingTrivia(list).WithAdditionalAnnotations(Formatter.Annotation));
                    return Task.FromResult(document.WithSyntaxRoot(newRoot));
                }
            );
        }
    }
}

