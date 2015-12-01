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
using System.Diagnostics.Contracts;

namespace RefactoringEssentials.CSharp.CodeRefactorings
{
    [ExportCodeRefactoringProvider(LanguageNames.CSharp, Name = "Add a Contract to specify the return value must not be null")]
    /// <summary>
    /// Creates a 'Contract.Ensures(return != null);' contract for a method return value.
    /// </summary>
    public class ContractEnsuresNotNullReturnCodeRefactoringProvider : CodeContractsCodeRefactoringProvider<MethodDeclarationSyntax>
    {
        protected override IEnumerable<CodeAction> GetActions(Document document, SemanticModel semanticModel, SyntaxNode root, TextSpan span, MethodDeclarationSyntax node, CancellationToken cancellationToken)
        {
            var nullableType = node.ChildNodes().OfType<NullableTypeSyntax>().FirstOrDefault();

            var objectType = node.ChildNodes().OfType<IdentifierNameSyntax>().FirstOrDefault();

            var returnType = (CSharpSyntaxNode) nullableType ?? objectType;
            if (returnType == null)
                return Enumerable.Empty<CodeAction>();

            var bodyStatement = node.ChildNodes().OfType<BlockSyntax>().FirstOrDefault();
            if (bodyStatement == null)
                return Enumerable.Empty<CodeAction>();

            if (HasReturnContract(bodyStatement, returnType.ToString()))
                return Enumerable.Empty<CodeAction>();

            return new[] { CodeActionFactory.Create(
                node.Identifier.Span,
                DiagnosticSeverity.Info,
                GettextCatalog.GetString ("Add a Contract to specify the return value must not be null"),
                t2 => {
                    var newBody = bodyStatement.WithStatements (SyntaxFactory.List<StatementSyntax>(new [] { CreateContractEnsuresCall(returnType.ToString()) }.Concat (bodyStatement.Statements)));

                    var newRoot = (CompilationUnitSyntax) root.ReplaceNode((SyntaxNode)bodyStatement, newBody);

                    if (UsingStatementNotPresent(newRoot)) newRoot = AddUsingStatement(node, newRoot);

                    return Task.FromResult(document.WithSyntaxRoot(newRoot));
                }
            ) };
        }

        static StatementSyntax CreateContractEnsuresCall(string returnType)
        {
            return SyntaxFactory.ParseStatement($"Contract.Ensures(Contract.Result<{returnType}>() != null);").WithAdditionalAnnotations(Formatter.Annotation, Simplifier.Annotation);
        }

        static bool HasReturnContract(BlockSyntax bodyStatement, string returnType)
        {
            var workspace = new AdhocWorkspace();

            foreach (var expression in bodyStatement.DescendantNodes().OfType<ExpressionStatementSyntax>())
            {
                var formatted = Formatter.Format(expression, workspace).ToString();

                if (formatted == $"Contract.Ensures(Contract.Result<{returnType}>() != null);")
                    return true;

                if (formatted == $"Contract.Ensures(null != Contract.Result<{returnType}>());")
                    return true;
            }
            return false;
        }
    }
}
