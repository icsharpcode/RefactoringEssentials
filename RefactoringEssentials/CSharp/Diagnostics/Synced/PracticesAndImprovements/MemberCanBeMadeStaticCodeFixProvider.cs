using Microsoft.CodeAnalysis;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CodeFixes;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;

namespace RefactoringEssentials.CSharp.Diagnostics
{

    [ExportCodeFixProvider(LanguageNames.CSharp), System.Composition.Shared]
    public class MemberCanBeMadeStaticCodeFixProvider : CodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds
        {
            get
            {
                return ImmutableArray.Create(CSharpDiagnosticIDs.MemberCanBeMadeStaticAnalyzerID);
            }
        }

        public override FixAllProvider GetFixAllProvider()
        {
            return WellKnownFixAllProviders.BatchFixer;
        }

        public async override Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var document = context.Document;
            var cancellationToken = context.CancellationToken;
            var span = context.Span;
            var diagnostics = context.Diagnostics;
            var root = await document.GetSyntaxRootAsync(cancellationToken);
            var diagnostic = diagnostics.First();
            var node = root.FindNode(context.Span);
            //if (!node.IsKind(SyntaxKind.BaseList))
            //	continue;
            var newRoot = root.RemoveNode(node, SyntaxRemoveOptions.KeepNoTrivia);
            context.RegisterCodeFix(CodeActionFactory.Create(node.Span, diagnostic.Severity, "Make '{0}' static", document.WithSyntaxRoot(newRoot)), diagnostic);
        }

        public void MakeMethodStaticFix(MethodDeclarationSyntax methodDeclaration, Diagnostic diagnostic, CodeFixContext context)
        {
//            context.RegisterCodeFix(CodeActionFactory.Create(methodDeclaration.Span, diagnostic.Severity, "Redundant explicit nullable type creation",
//                token =>
//                {
////                methodDeclaration.Modifiers.Add(new SyntaxToken().)
////                //var newNode = SyntaxFactory.MethodDeclaration(methodDeclaration.AttributeLists,)
////                ObjectCreationExpression(objectCreation.NewKeyword, objectCreation.Type,
////argumentList, objectCreation.Initializer);

////              var newRoot = root.ReplaceNode(objectCreation,
////newNode.WithLeadingTrivia(objectCreation.GetLeadingTrivia())
////      .WithAdditionalAnnotations(Formatter.Annotation));

////              return Task.FromResult(document.WithSyntaxRoot(newRoot));
////          }), diagnostic);
//                }
        }
    }
}