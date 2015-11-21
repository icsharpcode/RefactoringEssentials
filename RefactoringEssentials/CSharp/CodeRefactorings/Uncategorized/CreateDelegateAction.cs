using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp;
using System.Linq;
using System.Diagnostics;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.Formatting;

namespace RefactoringEssentials.CSharp.CodeRefactorings
{
    /// <summary>
    /// Create a delegate from an anonymous event declaration.
    /// </summary>
    /// <remarks>
    /// Language assumptions based on the C# 5.0 Language Specification.
    /// https://msdn.microsoft.com/en-us/library/ms228593.aspx
    /// </remarks>
    [ExportCodeRefactoringProvider(LanguageNames.CSharp, Name = "Create delegate")]
    public class CreateDelegateAction : CodeRefactoringProvider
    {
        public override async Task ComputeRefactoringsAsync(CodeRefactoringContext context)
        {
            var document = context.Document;
            var span = context.Span;
            var cancellationToken = context.CancellationToken;

            var model = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);
            if (model.IsFromGeneratedCode(cancellationToken))
                return;

            var root = await model.SyntaxTree.GetRootAsync(cancellationToken).ConfigureAwait(false);
            if (root == null)
                return;

            var node = root.FindNode(span);
            if (node == null)
                return;

            // Get the entire event declaration.
            var eventFieldDeclaration = (EventFieldDeclarationSyntax)node.AncestorsAndSelf().FirstOrDefault(a => a.IsKind(SyntaxKind.EventFieldDeclaration));

            // If the event handler type is defined then we should not create a delegate.
            var eventHandlerTypeInfo = model.GetTypeInfo(eventFieldDeclaration.Declaration.Type);
            if (eventHandlerTypeInfo.ConvertedType != null && !eventHandlerTypeInfo.ConvertedType.IsErrorType())
                return;

            // C# 5.0 Language Spec 9.6; class, interface or struct
            // Ensure there is a type declaring this event.
            var typeDeclarationNode = eventFieldDeclaration.Ancestors().FirstOrDefault(a => a.IsKind(
                SyntaxKind.ClassDeclaration,
                SyntaxKind.InterfaceDeclaration,
                SyntaxKind.StructDeclaration));

            if (typeDeclarationNode == null)
                return;
            
            // Prepare delegate parameters.
            ParameterListSyntax evtParams = SyntaxFactory.ParseParameterList("(object sender, System.EventArgs e)");

            // Prepare delegate modifiers.
            var modifiers = eventFieldDeclaration.Modifiers;

            if (modifiers.Any())
            {
                // C# 5.0 Language Spec 15.1 Delegate Declarations; possible modifiers excluding "new".
                modifiers = SyntaxFactory.TokenList(modifiers.Where(m => m.IsKind(
                    SyntaxKind.PublicKeyword,
                    SyntaxKind.ProtectedKeyword,
                    SyntaxKind.InternalKeyword,
                    SyntaxKind.PrivateKeyword)));
            }

            // Create delegate node.
            var newDelegateNode = SyntaxFactory.DelegateDeclaration(
                            SyntaxFactory.List<AttributeListSyntax>(),
                            modifiers,
                            SyntaxFactory.Token(SyntaxKind.DelegateKeyword),
                            SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.VoidKeyword)),
                            (eventFieldDeclaration.Declaration.Type as IdentifierNameSyntax).Identifier,
                            null,
                            evtParams.WithAdditionalAnnotations(Formatter.Annotation),
                            SyntaxFactory.List<TypeParameterConstraintClauseSyntax>(),
                            SyntaxFactory.Token(SyntaxKind.SemicolonToken)).WithAdditionalAnnotations(Formatter.Annotation);

            // Insert delegate just above the type declaration (class, interface, struct).
            var newRoot = root.InsertNodesBefore(typeDeclarationNode, new[] { newDelegateNode });

            // All clear, register the refactoring.
            context.RegisterRefactoring(
               CodeActionFactory.Create(
                   node.Span,
                   DiagnosticSeverity.Info,
                   GettextCatalog.GetString("Create delegate"),
                   document.WithSyntaxRoot(newRoot)));

            return;
        }
    }
}