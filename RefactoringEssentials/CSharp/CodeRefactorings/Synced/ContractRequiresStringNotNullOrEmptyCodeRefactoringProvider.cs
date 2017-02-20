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
using Microsoft.CodeAnalysis.Simplification;
using Microsoft.CodeAnalysis.Formatting;

namespace RefactoringEssentials.CSharp.CodeRefactorings
{
    /// <summary>
    /// Creates a 'Contract.Requires(param != null);' contract for a parameter.
    /// </summary>
    [ExportCodeRefactoringProvider(LanguageNames.CSharp, Name = "Add a Contract to specify the string parameter must not be null or empty")]
    public class ContractRequiresStringNotNullOrEmptyCodeRefactoringProvider : CodeContractsCodeRefactoringProvider
    {
        public override async Task ComputeRefactoringsAsync(CodeRefactoringContext context)
        {
            var codeContractsContext = await CodeContractsContext(context);
            if (codeContractsContext == null)
                return;

            var foundNode = (ParameterSyntax)codeContractsContext.Node.AncestorsAndSelf().FirstOrDefault(n => n is ParameterSyntax);
            if (foundNode == null)
                return;

            foreach (var action in GetActions(codeContractsContext.Document, codeContractsContext.SemanticModel, codeContractsContext.Root, codeContractsContext.TextSpan, foundNode))
                context.RegisterRefactoring(action);
        }

        protected IEnumerable<CodeAction> GetActions(Document document, SemanticModel semanticModel, SyntaxNode root, TextSpan span, ParameterSyntax node)
        {
            if (!node.Identifier.Span.Contains(span))
                yield break;

            var parameter = node;
            var bodyStatement = parameter.Parent.Parent.ChildNodes().OfType<BlockSyntax>().FirstOrDefault();
            if (bodyStatement == null)
                yield break;

            var parameterSymbol = semanticModel.GetDeclaredSymbol(node);
            var type = parameterSymbol.Type;
            if (type.SpecialType != SpecialType.System_String || HasReturnContract(bodyStatement, parameterSymbol.Name))
                yield break;

            yield return CreateAction(
                node.Identifier.Span
                , t2 => {
                    var newBody = bodyStatement.WithStatements(SyntaxFactory.List<StatementSyntax>(new[] { CreateContractRequiresCall(node.Identifier.ToString()) }.Concat(bodyStatement.Statements)));

                    var newRoot = (CompilationUnitSyntax)root.ReplaceNode((SyntaxNode)bodyStatement, newBody);

                    if (UsingStatementNotPresent(newRoot)) newRoot = AddUsingStatement(node, newRoot);

                    return Task.FromResult(document.WithSyntaxRoot(newRoot));
                }
                , "Add contract requiring parameter must not be null or empty"
            );
        }

        static StatementSyntax CreateContractRequiresCall(string parameterName)
        {
            return SyntaxFactory.ParseStatement($"Contract.Requires(string.IsNullOrEmpty({parameterName}) == false);\r\n").WithAdditionalAnnotations(Formatter.Annotation, Simplifier.Annotation);
        }

        static bool HasReturnContract(BlockSyntax bodyStatement, string parameterName)
        {
            var workspace = new AdhocWorkspace();

            foreach (var expression in bodyStatement.DescendantNodes().OfType<ExpressionStatementSyntax>())
            {
                var formatted = Formatter.Format(expression, workspace).ToString();

                if (formatted == $"Contract.Requires(string.IsNullOrEmpty({parameterName}) == false);")
                    return true;

                if (formatted == $"Contract.Requires(false == string.IsNullOrEmpty({parameterName}));")
                    return true;

                if (formatted == $"Contract.Requires(!string.IsNullOrEmpty({parameterName}));")
                    return true;
            }
            return false;
        }
    }
}
