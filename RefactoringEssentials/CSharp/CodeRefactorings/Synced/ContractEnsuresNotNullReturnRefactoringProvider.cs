using System.Linq;
using System.Collections.Generic;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Simplification;
using Microsoft.CodeAnalysis.Formatting;

namespace RefactoringEssentials.CSharp.CodeRefactorings
{
    /// <summary>
    /// Creates a 'Contract.Ensures(return != null);' contract for a method return value.
    /// </summary>
    [ExportCodeRefactoringProvider(LanguageNames.CSharp, Name = "Add a Contract to specify the return value must not be null")]
    public class ContractEnsuresNotNullReturnCodeRefactoringProvider : CodeContractsCodeRefactoringProvider
    {
        #region ICodeActionProvider implementation
        public override async Task ComputeRefactoringsAsync(CodeRefactoringContext context)
        {
            var codeContractsContext = await CodeContractsContext(context).ConfigureAwait(false);
            if (codeContractsContext == null)
                return;

            var methodNode = (MethodDeclarationSyntax)codeContractsContext.Node.AncestorsAndSelf().FirstOrDefault(n => n is MethodDeclarationSyntax);
            var getterNode = (AccessorDeclarationSyntax)codeContractsContext.Node.AncestorsAndSelf().FirstOrDefault(n => n is AccessorDeclarationSyntax && n.Kind() == SyntaxKind.GetAccessorDeclaration);
                        
            if (methodNode == null && getterNode == null)
                return;

            foreach (var action in GetActions(codeContractsContext.Document, codeContractsContext.Root, methodNode, getterNode))
                context.RegisterRefactoring(action);
        }
        #endregion

        protected IEnumerable<CodeAction> GetActions(Document document, SyntaxNode root, MethodDeclarationSyntax methodNode, AccessorDeclarationSyntax getterNode)
        {
            if (methodNode != null)
                return GetActions(document, root, methodNode);

            if (getterNode != null)
                return GetActions(document, root, getterNode);

            return Enumerable.Empty<CodeAction>();
        }

        protected IEnumerable<CodeAction> GetActions(Document document, SyntaxNode root, AccessorDeclarationSyntax node)
        {
            var propertyOrIndexerDeclaration = node.Ancestors().FirstOrDefault(n => n.GetType().Equals(typeof(PropertyDeclarationSyntax)) || n.GetType().Equals(typeof(IndexerDeclarationSyntax)));

            var nullableType = propertyOrIndexerDeclaration.ChildNodes().OfType<NullableTypeSyntax>().FirstOrDefault();
            
            var objectType = propertyOrIndexerDeclaration.ChildNodes().OfType<IdentifierNameSyntax>().FirstOrDefault();

            return GetActions(document, root, node, nullableType, objectType);
        }

        protected IEnumerable<CodeAction> GetActions(Document document, SyntaxNode root, MethodDeclarationSyntax node)
        {
            var nullableType = node.ChildNodes().OfType<NullableTypeSyntax>().FirstOrDefault();

            var objectType = node.ChildNodes().OfType<IdentifierNameSyntax>().FirstOrDefault();

            return GetActions(document, root, node, nullableType, objectType);
        }

        private static IEnumerable<CodeAction> GetActions(Document document, SyntaxNode root, SyntaxNode node, CSharpSyntaxNode nullableType, CSharpSyntaxNode objectType)
        {
            var returnType = (CSharpSyntaxNode)nullableType ?? objectType;
            if (returnType == null)
                yield break;

            var bodyStatement = node.ChildNodes().OfType<BlockSyntax>().FirstOrDefault();
            if (bodyStatement == null)
                yield break; 

            if (HasReturnContract(bodyStatement, returnType.ToString()))
                yield break;

            yield return CreateAction(
                node.Span
                ,t2 => {
                    var newBody = bodyStatement.WithStatements(SyntaxFactory.List<StatementSyntax>(new[] { CreateContractEnsuresCall(returnType.ToString()) }.Concat(bodyStatement.Statements)));

                    var newRoot = (CompilationUnitSyntax)root.ReplaceNode((SyntaxNode)bodyStatement, newBody);

                    if (UsingStatementNotPresent(newRoot)) newRoot = AddUsingStatement(node, newRoot);

                    return Task.FromResult(document.WithSyntaxRoot(newRoot));
                }
                ,"Add a Contract to specify the return value must not be null"
            );
        }

        static StatementSyntax CreateContractEnsuresCall(string returnType)
        {
            return SyntaxFactory.ParseStatement($"Contract.Ensures(Contract.Result<{returnType}>() != null);\r\n").WithAdditionalAnnotations(Formatter.Annotation, Simplifier.Annotation);
        }

        static bool ValidateContractAccess(MemberAccessExpressionSyntax access, string method, string genericTypeName)
        {
            if (access == null)
                return false;

            var className = access.Expression.GetRightmostName() as SimpleNameSyntax;
            if (className == null || className.Identifier.ValueText != "Contract")
                return false;

            var ensures = access.GetRightmostName() as SimpleNameSyntax;
            if (ensures == null || ensures.Identifier.ValueText != method)
                return false;

            if (genericTypeName!= null)
            {
                var generic = ensures as GenericNameSyntax;
                if (generic == null || generic.Arity != 1)
                    return false;

                var typeName = generic.TypeArgumentList.Arguments[0].GetRightmostName() as SimpleNameSyntax;
                if (typeName == null || typeName.Identifier.ValueText != genericTypeName)
                    return false;
            }
            return true;
        }

        static bool HasReturnContract(BlockSyntax bodyStatement, string returnType)
        {
            var rhsEnsures = SyntaxFactory.ParseStatement($"Contract.Ensures(Contract.Result<{returnType}>() != null);");
            var lhsEnsures = SyntaxFactory.ParseStatement($"Contract.Ensures(null != Contract.Result<{returnType}>());");
            foreach (var expression in bodyStatement.DescendantNodes().OfType<ExpressionStatementSyntax>())
            {
                var ies = expression.Expression as InvocationExpressionSyntax;
                if (ies == null)
                    continue;

                var access = ies.Expression as MemberAccessExpressionSyntax;
                if (access == null)
                    continue;

                if (!ValidateContractAccess(access, "Ensures", null))
                    continue;

                if (ies.ArgumentList.Arguments.Count != 1)
                    continue;

                var arg = ies.ArgumentList.Arguments[0].Expression as BinaryExpressionSyntax;
                if (arg == null || !arg.OperatorToken.IsKind(SyntaxKind.ExclamationEqualsToken))
                    continue;

                if (arg.Left.IsKind(SyntaxKind.NullLiteralExpression))
                {
                    ies = arg.Right as InvocationExpressionSyntax;
                }
                else if (arg.Right.IsKind(SyntaxKind.NullLiteralExpression))
                {
                    ies = arg.Left as InvocationExpressionSyntax;
                }
                access = ies?.Expression as MemberAccessExpressionSyntax;
                if (!ValidateContractAccess(access, "Result", returnType))
                    continue;
                return true;
            }
            return false;
        }
    }
}
