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
    [ExportCodeRefactoringProvider(LanguageNames.CSharp, Name = "Convert anonymous method to lambda expression")]
    public class ConvertAnonymousMethodToLambdaCodeRefactoringProvider : SpecializedCodeRefactoringProvider<AnonymousMethodExpressionSyntax>
    {
        protected override IEnumerable<CodeAction> GetActions(Document document, SemanticModel semanticModel, SyntaxNode root, TextSpan span, AnonymousMethodExpressionSyntax node, CancellationToken cancellationToken)
        {
            if (!node.DelegateKeyword.Span.Contains(span))
                return Enumerable.Empty<CodeAction>();

            ExpressionSyntax convertExpression = null;
            if (node.Block.Statements.Count == 1)
            {
                var stmt = node.Block.Statements.FirstOrDefault() as ExpressionStatementSyntax;
                if (stmt != null)
                    convertExpression = stmt.Expression;
            }

            ITypeSymbol guessedType = null;
            if (node.ParameterList == null)
            {
                var info = semanticModel.GetTypeInfo(node);
                guessedType = info.ConvertedType ?? info.Type;
                if (guessedType == null)
                    return Enumerable.Empty<CodeAction>();
            }
            return new[]  {
                CodeActionFactory.Create(
                    node.DelegateKeyword.Span,
                    DiagnosticSeverity.Info,
                    GettextCatalog.GetString ("To lambda expression"),
                    t2 => {
                        var parent = node.Parent.Parent.Parent;
                        bool explicitLambda = parent is VariableDeclarationSyntax && ((VariableDeclarationSyntax)parent).Type.IsVar;
                        ParameterListSyntax parameterList;
                        if (node.ParameterList != null) {
                            if (explicitLambda) {
                                parameterList = node.ParameterList;
                            } else {
                                parameterList = SyntaxFactory.ParameterList(
                                    SyntaxFactory.SeparatedList(node.ParameterList.Parameters.Select(p => SyntaxFactory.Parameter(p.AttributeLists, p.Modifiers, null, p.Identifier, p.Default)))
                                );
                            }
                        } else {
                            var invokeMethod = guessedType.GetDelegateInvokeMethod();
                            parameterList = SyntaxFactory.ParameterList(
                                SyntaxFactory.SeparatedList(
                                    invokeMethod.Parameters.Select(p =>
                                        SyntaxFactory.Parameter(SyntaxFactory.Identifier(p.Name))
                                    )
                                )
                            );
                        }
                        var lambdaExpression = explicitLambda || parameterList.Parameters.Count != 1 ?
                            (SyntaxNode)SyntaxFactory.ParenthesizedLambdaExpression(parameterList, (CSharpSyntaxNode)convertExpression ?? node.Block) :
                            SyntaxFactory.SimpleLambdaExpression(parameterList.Parameters[0], (CSharpSyntaxNode)convertExpression ?? node.Block);
                        var newRoot = root.ReplaceNode((SyntaxNode)node, lambdaExpression.WithAdditionalAnnotations(Formatter.Annotation));
                        return Task.FromResult(document.WithSyntaxRoot(newRoot));
                    }
                )
            };
        }
    }
}