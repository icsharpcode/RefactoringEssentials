using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.Simplification;

namespace RefactoringEssentials.CSharp.CodeRefactorings
{
    [ExportCodeRefactoringProvider(LanguageNames.CSharp, Name = "Convert implicit to explicit implementation")]
    public class ConvertImplicitToExplicitImplementationCodeRefactoringProvider : CodeRefactoringProvider
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
            var node = root.FindNode(span);
			while (node != null && !(node is MemberDeclarationSyntax))
                node = node.Parent;
            if (node == null)
                return;

            if (!node.IsKind(SyntaxKind.MethodDeclaration) &&
                !node.IsKind(SyntaxKind.PropertyDeclaration) &&
                !node.IsKind(SyntaxKind.IndexerDeclaration) &&
                !node.IsKind(SyntaxKind.EventDeclaration))
                return;
            //			if (!node.NameToken.Contains (context.Location))
            //				return null;

            var memberDeclaration = node as MemberDeclarationSyntax;
            var explicitSyntax = memberDeclaration.GetExplicitInterfaceSpecifierSyntax();
            if (explicitSyntax != null)
                return;

            var enclosingSymbol = model.GetDeclaredSymbol(memberDeclaration, cancellationToken);
            if (enclosingSymbol == null)
                return;

            var containingType = enclosingSymbol.ContainingType;
            if (containingType.TypeKind == TypeKind.Interface)
                return;

            var implementingInterface = GetImplementingInterface(enclosingSymbol, containingType);
            if (implementingInterface == null)
                return;

            context.RegisterRefactoring(
                CodeActionFactory.Create(
                    span,
                    DiagnosticSeverity.Info,
                    GettextCatalog.GetString("To explicit implementation"),
                    t2 =>
                    {
                        var newNode = memberDeclaration;
                        var nameSpecifier = SyntaxFactory.ExplicitInterfaceSpecifier(SyntaxFactory.ParseName(implementingInterface.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat)))
                            .WithAdditionalAnnotations(Simplifier.Annotation);
                        switch (newNode.Kind())
                        {
                            case SyntaxKind.MethodDeclaration:
                                var method = (MethodDeclarationSyntax)memberDeclaration;
                                newNode = method
                                    .WithModifiers(SyntaxFactory.TokenList())
                                    .WithExplicitInterfaceSpecifier(nameSpecifier);
                                break;
                            case SyntaxKind.PropertyDeclaration:
                                var property = (PropertyDeclarationSyntax)memberDeclaration;
                                newNode = property
                                    .WithModifiers(SyntaxFactory.TokenList())
                                    .WithExplicitInterfaceSpecifier(nameSpecifier);
                                break;
                            case SyntaxKind.IndexerDeclaration:
                                var indexer = (IndexerDeclarationSyntax)memberDeclaration;
                                newNode = indexer
                                    .WithModifiers(SyntaxFactory.TokenList())
                                    .WithExplicitInterfaceSpecifier(nameSpecifier);
                                break;
                            case SyntaxKind.EventDeclaration:
                                var evt = (EventDeclarationSyntax)memberDeclaration;
                                newNode = evt
                                    .WithModifiers(SyntaxFactory.TokenList())
                                    .WithExplicitInterfaceSpecifier(nameSpecifier);
                                break;
                        }
                        var newRoot = root.ReplaceNode((SyntaxNode)
                            memberDeclaration,
                            newNode.WithLeadingTrivia(memberDeclaration.GetLeadingTrivia()).WithAdditionalAnnotations(Formatter.Annotation)
                        );
                        return Task.FromResult(document.WithSyntaxRoot(newRoot));
                    }
                )
            );
        }

        static INamedTypeSymbol GetImplementingInterface(ISymbol enclosingSymbol, INamedTypeSymbol containingType)
        {
            INamedTypeSymbol result = null;
            foreach (var iface in containingType.AllInterfaces)
            {
                foreach (var member in iface.GetMembers())
                {
                    var implementation = containingType.FindImplementationForInterfaceMember(member);
                    if (implementation == enclosingSymbol)
                    {
                        if (result != null)
                            return null;
                        result = iface;
                    }
                }
            }
            return result;
        }
    }
}