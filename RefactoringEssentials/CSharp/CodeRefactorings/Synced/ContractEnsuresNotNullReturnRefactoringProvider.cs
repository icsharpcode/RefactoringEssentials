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
    [ExportCodeRefactoringProvider(LanguageNames.CSharp, Name = "Add a Contract to specify the return value must not be null")]
    /// <summary>
    /// Creates a 'Contract.Ensures(return != null);' contract for a method return value.
    /// </summary>
    public class ContractEnsuresNotNullReturnCodeRefactoringProvider : SpecializedCodeRefactoringProvider<MethodDeclarationSyntax>
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

            return new[] { CodeActionFactory.Create(
                node.Identifier.Span,
                DiagnosticSeverity.Info,
                GettextCatalog.GetString ("Add a Contract to specify the return value must not be null"),
                t2 => {
                    var newBody = bodyStatement.WithStatements (SyntaxFactory.List<StatementSyntax>(new [] { CreateContractEnsuresCall(returnType.ToString()) }.Concat (bodyStatement.Statements)));

                    var newRoot = (CompilationUnitSyntax) root.ReplaceNode((SyntaxNode)bodyStatement, newBody);

                    //if (UsingStatementNotPresent(newRoot)) newRoot = AddUsingStatement(node, newRoot);

                    return Task.FromResult(document.WithSyntaxRoot(newRoot));
                }
            ) };
        }

        static StatementSyntax CreateContractEnsuresCall(string returnType)
        {
            return SyntaxFactory.ParseStatement($"Contract.Ensures(Contract.Result<{returnType}>() != null);").WithAdditionalAnnotations(Formatter.Annotation, Simplifier.Annotation);
        }

        static bool UsingStatementNotPresent(CompilationUnitSyntax cu)
        {
            return !cu.Usings.Any((UsingDirectiveSyntax u) => u.Name.ToString() == "System.Diagnostics.Contracts");
        }

        static CompilationUnitSyntax AddUsingStatement(ParameterSyntax node, CompilationUnitSyntax cu)
        {
            return cu.AddUsingDirective(
                SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("System.Diagnostics.Contracts")).WithAdditionalAnnotations(Formatter.Annotation)
                , node
                , true);
        }

        static bool HasNotNullContract(SemanticModel semanticModel, IParameterSymbol parameterSymbol, BlockSyntax bodyStatement)
        {
            foreach (var expressions in bodyStatement.DescendantNodes().OfType<ExpressionStatementSyntax>())
            {
                var identifiers = expressions.DescendantNodes().OfType<IdentifierNameSyntax>();

                if (Enumerable.SequenceEqual(identifiers.Select(i => i.Identifier.Text), new List<string>() { "Contract", "Requires", parameterSymbol.Name }))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
