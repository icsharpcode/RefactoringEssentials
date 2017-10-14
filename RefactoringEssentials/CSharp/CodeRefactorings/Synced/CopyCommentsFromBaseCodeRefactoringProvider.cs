using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp;
using System.Threading;
using System.Xml.Linq;

namespace RefactoringEssentials.CSharp.CodeRefactorings
{
    [ExportCodeRefactoringProvider(LanguageNames.CSharp, Name = "Copy comments from base")]
    public class CopyCommentsFromBaseCodeRefactoringProvider : CodeRefactoringProvider
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
            if (!token.IsKind(SyntaxKind.IdentifierToken))
                return;

            var node = token.Parent as MemberDeclarationSyntax;
            if (node == null)
                return;
            var declaredSymbol = model.GetDeclaredSymbol(node, cancellationToken);
            if (declaredSymbol == null || !string.IsNullOrEmpty(declaredSymbol.GetDocumentationCommentXml(null, false, cancellationToken)))
                return;

            string documentation;
            var baseMember = GetBaseMember(declaredSymbol, out documentation, cancellationToken);
            if (baseMember == null || string.IsNullOrEmpty(documentation))
                return;
            XDocument doc = XDocument.Parse(documentation);
            var rootElement = doc.Elements().First();
            var inner = string.Join(System.Environment.NewLine, rootElement.Nodes().Select(n => n.ToString())).Trim();
            if (string.IsNullOrEmpty(inner))
                return;

            // "Copy comments from interface"
            context.RegisterRefactoring(
                CodeActionFactory.Create(
                    span,
                    DiagnosticSeverity.Info,
                    baseMember.ContainingType != null && baseMember.ContainingType.TypeKind == TypeKind.Interface ? GettextCatalog.GetString("Copy comments from interface") : GettextCatalog.GetString("Copy comments from base"),
                    t2 =>
                    {
                        var triva = node.GetLeadingTrivia();

                        var indentTrivia = triva.FirstOrDefault(t => t.IsKind(SyntaxKind.WhitespaceTrivia));
                        var indent = indentTrivia.ToString();

                        string[] lines = NewLine.SplitLines(inner);
                        for (int i = 0; i < lines.Length; i++)
                        {
                            lines[i] = indent + "/// " + lines[i].Trim();
                        }


                        var eol = "\r\n";
                        int idx = 0;
                        while (idx < triva.Count && triva[idx].IsKind(SyntaxKind.EndOfLineTrivia))
                            idx++;
                        triva = triva.Insert(idx, SyntaxFactory.SyntaxTrivia(SyntaxKind.SingleLineCommentTrivia, string.Join(eol, lines) + eol));
                        var newRoot = root.ReplaceNode(node, node.WithLeadingTrivia(triva));
                        return Task.FromResult(document.WithSyntaxRoot(newRoot));
                    }
                )
            );
        }

        static string RemoveLineBreaksFromXml(string innerXml)
        {
            return innerXml.TrimStart('\r').TrimStart('\n').TrimEnd('\n').TrimEnd('\r');
        }

        static ISymbol GetBaseMember(ISymbol declaredSymbol, out string documentation, CancellationToken cancellationToken)
        {
            var overriddenMember = declaredSymbol.OverriddenMember();
            documentation = overriddenMember != null ? overriddenMember.GetDocumentationCommentXml(null, false, cancellationToken) : "";

            if (!string.IsNullOrEmpty(documentation) || declaredSymbol.Kind == SymbolKind.NamedType)
                return overriddenMember;

            var containingType = declaredSymbol.ContainingType;
            if (containingType == null)
            {
                documentation = null;
                return null;
            }
            foreach (var iface in containingType.AllInterfaces)
            {
                foreach (var member in iface.GetMembers())
                {
                    var implementation = containingType.FindImplementationForInterfaceMember(member);
                    if (implementation == declaredSymbol)
                    {
                        documentation = member.GetDocumentationCommentXml(null, false, cancellationToken);
                        if (!string.IsNullOrEmpty(documentation))
                            return implementation;
                    }
                }
            }
            return null;
        }
    }
}