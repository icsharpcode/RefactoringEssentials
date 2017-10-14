using System.Threading;
using System.Collections.Generic;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.Text;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;
using Microsoft.CodeAnalysis.VisualBasic;

namespace RefactoringEssentials.VB.CodeRefactorings
{
    [ExportCodeRefactoringProvider(LanguageNames.VisualBasic, Name = "Create overload without parameter")]
    public class CreateOverloadWithoutParameterCodeRefactoringProvider : SpecializedCodeRefactoringProvider<ParameterSyntax>
    {
        protected override IEnumerable<CodeAction> GetActions(Document document, SemanticModel semanticModel, SyntaxNode root, TextSpan span, ParameterSyntax node, CancellationToken cancellationToken)
        {
            if (node.Modifiers.Contains(t => t.IsKind(SyntaxKind.ParamArrayKeyword)))
                yield break;
            if (node.Default != null)
                yield break;
            if (!node.Identifier.Span.Contains(span))
                yield break;

            var methodStatement = node.Parent.Parent as MethodStatementSyntax;
            var methodBlock = methodStatement?.Parent as MethodBlockSyntax;
            if (methodStatement == null)
                yield break;
            IMethodSymbol method = semanticModel.GetDeclaredSymbol(methodStatement);
            if (method == null)
                yield break;
            if (method.ExplicitInterfaceImplementations.Any())
                yield break;

            var parameters = method.Parameters.Where(param => param.Name != node.Identifier.Identifier.ValueText);
            if (method.ContainingType.GetMembers().OfType<IMethodSymbol>().Any(
                m => (m.Name == method.Name)
                    && (m.TypeParameters.Count() == method.TypeParameters.Count())
                    && SignatureComparer.HaveSameSignature(m.Parameters.ToList(), parameters.ToList())))
                yield break;

            yield return CodeActionFactory.Create(node.Span, DiagnosticSeverity.Info, GettextCatalog.GetString("Create overload without parameter"),
                t2 =>
                {
                    var newMethodStatement = methodStatement
                        .WithParameterList(SyntaxFactory.ParameterList(
                            SyntaxFactory.SeparatedList(methodStatement.ParameterList.Parameters.Where(
                                p => p.Identifier.Identifier.ValueText != node.Identifier.Identifier.ValueText))));

                    if (methodBlock != null)
                    {
                        var defaultExpression = GetDefaultValueExpression(semanticModel, methodStatement, node.AsClause.Type);
                        List<StatementSyntax> bodyStatements = new List<StatementSyntax>();
                        ArgumentSyntax argumentExpression = null;

                        if (node.Modifiers.Any(m => m.IsKind(SyntaxKind.ByRefKeyword)))
                        {
                            bodyStatements.Add(SyntaxFactory.LocalDeclarationStatement(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.DimKeyword)),
                                    SyntaxFactory.SeparatedList(new[] {
                                    SyntaxFactory.VariableDeclarator(SyntaxFactory.SeparatedList(new[] { node.Identifier }),
                                    node.AsClause,
                                    SyntaxFactory.EqualsValue(defaultExpression))
                                    })));
                            argumentExpression = GetArgumentExpression(node);
                        }
                        else
                        {
                            argumentExpression = SyntaxFactory.SimpleArgument(defaultExpression);
                        }

                        var methodInvocation = SyntaxFactory.InvocationExpression(
                            SyntaxFactory.IdentifierName(methodStatement.Identifier),
                            SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList(methodStatement.ParameterList.Parameters.Select(p => p == node ? argumentExpression : GetArgumentExpression(p)))));

                        if (method.ReturnType.SpecialType == SpecialType.System_Void)
                            bodyStatements.Add(SyntaxFactory.ExpressionStatement(methodInvocation));
                        else
                            bodyStatements.Add(SyntaxFactory.ReturnStatement(methodInvocation));

                        var newMethod = methodBlock
                            .WithSubOrFunctionStatement(newMethodStatement)
                            .WithStatements(SyntaxFactory.List(bodyStatements))
                            .WithTrailingTrivia(methodBlock.GetTrailingTrivia())
                            .WithLeadingTrivia(methodStatement.GetLeadingTrivia().Where(t => !t.IsKind(SyntaxKind.DocumentationCommentTrivia)))
                            .WithAdditionalAnnotations(Formatter.Annotation);

                        var newRoot = root.InsertNodesBefore(methodBlock, new[] { newMethod });
                        return Task.FromResult(document.WithSyntaxRoot(newRoot));
                    }
                    else
                    {
                        var newRoot = root.InsertNodesBefore(methodStatement, new[] {
                            newMethodStatement
                                .WithTrailingTrivia(methodStatement.GetTrailingTrivia())
                                .WithLeadingTrivia(methodStatement.GetLeadingTrivia().Where(t => !t.IsKind(SyntaxKind.DocumentationCommentTrivia)))
                                .WithAdditionalAnnotations(Formatter.Annotation)
                        });
                        return Task.FromResult(document.WithSyntaxRoot(newRoot));
                    }
                });
        }

        static ArgumentSyntax GetArgumentExpression(ParameterSyntax parameter)
        {
            var identifier = SyntaxFactory.IdentifierName(parameter.Identifier.Identifier);
            return SyntaxFactory.SimpleArgument(identifier);
        }

        static ExpressionSyntax GetDefaultValueExpression(SemanticModel semanticModel, MethodStatementSyntax methodStatement, TypeSyntax type)
        {
            var typeSymbol = semanticModel.GetTypeInfo(type).Type;
            if (typeSymbol == null)
                return null;

            if (typeSymbol.IsArrayType())
                return SyntaxFactory.ObjectCreationExpression(type);

            if (typeSymbol.IsEnumType())
            {
                var members = type.GetMembers().OfType<EnumMemberDeclarationSyntax>();
                if (!members.Any())
                    return GetDefaultTypeExpression(type);
                return SyntaxFactory.MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression, SyntaxFactory.Token(SyntaxKind.DotToken), SyntaxFactory.IdentifierName(members.First().Identifier));
            }

            if (typeSymbol.IsKind(SymbolKind.DynamicType))
                return GetNothingExpression();
            if (typeSymbol.IsReferenceType)
            {
                if (typeSymbol.GetTypeParameters().Any() && (methodStatement.TypeParameterList != null) && methodStatement.TypeParameterList.Parameters.Any())
                    return GetDefaultTypeExpression(type);
                return GetNothingExpression();
            }

            switch (typeSymbol.SpecialType)
            {
                case SpecialType.System_Boolean:
                    return SyntaxFactory.FalseLiteralExpression(SyntaxFactory.Token(SyntaxKind.FalseKeyword));
                case SpecialType.System_Char:
                    return SyntaxFactory.LiteralExpression(SyntaxKind.CharacterLiteralExpression, SyntaxFactory.Literal('\0'));
                case SpecialType.System_SByte:
                case SpecialType.System_Byte:
                case SpecialType.System_Int16:
                case SpecialType.System_UInt16:
                case SpecialType.System_Int32:
                case SpecialType.System_UInt32:
                case SpecialType.System_Int64:
                case SpecialType.System_UInt64:
                case SpecialType.System_Single:
                case SpecialType.System_Double:
                case SpecialType.System_Decimal:
                    return SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal(0));
            }
            if (typeSymbol.IsNullableType())
                return GetNothingExpression();
            if (typeSymbol.IsValueType)
                return SyntaxFactory.ObjectCreationExpression(type);

            return GetDefaultTypeExpression(type);
        }

        static ExpressionSyntax GetDefaultTypeExpression(TypeSyntax type)
        {
            return SyntaxFactory.CTypeExpression(GetNothingExpression(), type);
        }

        static ExpressionSyntax GetNothingExpression()
        {
            return SyntaxFactory.NothingLiteralExpression(SyntaxFactory.Token(SyntaxKind.NothingKeyword));
        }
    }
}
