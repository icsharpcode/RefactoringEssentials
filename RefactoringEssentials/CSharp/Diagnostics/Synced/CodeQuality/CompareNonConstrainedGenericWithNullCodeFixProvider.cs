//using Microsoft.CodeAnalysis;
//using Microsoft.CodeAnalysis.CSharp;
//using Microsoft.CodeAnalysis.CodeFixes;
//using System.Threading.Tasks;
//using Microsoft.CodeAnalysis.CSharp.Syntax;
//using System.Linq;
//using System.Collections.Immutable;

//namespace RefactoringEssentials.CSharp.Diagnostics
//{
//    [ExportCodeFixProvider(LanguageNames.CSharp), System.Composition.Shared]
//    public class CompareNonConstrainedGenericWithNullCodeFixProvider : CodeFixProvider
//    {
//        public override ImmutableArray<string> FixableDiagnosticIds
//        {
//            get
//            {
//                return ImmutableArray.Create(CSharpDiagnosticIDs.CompareNonConstrainedGenericWithNullAnalyzerID);
//            }
//        }

//        public override FixAllProvider GetFixAllProvider()
//        {
//            return WellKnownFixAllProviders.BatchFixer;
//        }

//        public async override Task RegisterCodeFixesAsync(CodeFixContext context)
//        {
//            var document = context.Document;
//            var cancellationToken = context.CancellationToken;
//            var span = context.Span;
//            var diagnostics = context.Diagnostics;
//            var semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);
//            var root = semanticModel.SyntaxTree.GetRoot(cancellationToken);
//            var diagnostic = diagnostics.First();
//            var node = root.FindNode(context.Span);
//            if (!node.IsKind(SyntaxKind.NullLiteralExpression))
//                return;
//            context.RegisterCodeFix(CodeActionFactory.Create(
//                node.Span,
//                diagnostic.Severity,
//                "Replace with 'default'",
//                token =>
//                {
//                    var bOp = (BinaryExpressionSyntax)node.Parent;
//                    var n = node == bOp.Left.SkipParens() ? bOp.Right : bOp.Left;
//                    var info = semanticModel.GetTypeInfo(n);

//                    var newRoot = root.ReplaceNode(
//                        node,
//                        SyntaxFactory.DefaultExpression(
//                            SyntaxFactory.ParseTypeName(info.Type.ToMinimalDisplayString(semanticModel, node.SpanStart)))
//                            .WithLeadingTrivia(node.GetLeadingTrivia()
//                        )
//                    );

//                    return Task.FromResult(document.WithSyntaxRoot(newRoot));
//                }), diagnostic
//            );
//        }
//    }
//}