using System.Threading;
using System.Collections.Generic;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.Text;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Formatting;

namespace RefactoringEssentials.CSharp.CodeRefactorings
{
    [ExportCodeRefactoringProvider(LanguageNames.CSharp, Name = "Create overload without parameter")]
    public class CreateOverloadWithoutParameterCodeRefactoringProvider : SpecializedCodeRefactoringProvider<ParameterSyntax>
    {
        protected override IEnumerable<CodeAction> GetActions(Document document, SemanticModel semanticModel, SyntaxNode root, TextSpan span, ParameterSyntax node, CancellationToken cancellationToken)
        {
            if (node.Modifiers.Contains(t => t.IsKind(SyntaxKind.ThisKeyword) || t.IsKind(SyntaxKind.ParamsKeyword)))
                yield break;
            if (node.Default != null)
                yield break;
            if (!node.Identifier.Span.Contains(span))
                yield break;

            var methodDeclaration = node.Parent.Parent as MethodDeclarationSyntax;
            if (methodDeclaration == null)
                yield break;
            IMethodSymbol method = semanticModel.GetDeclaredSymbol(methodDeclaration);
            if (method == null)
                yield break;
            if (method.ExplicitInterfaceImplementations.Any())
                yield break;

            var parameters = method.Parameters.Where(param => param.Name != node.Identifier.ValueText);
            if (method.ContainingType.GetMembers().OfType<IMethodSymbol>().Any(
                m => (m.Name == method.Name)
                    && (m.TypeParameters.Count() == method.TypeParameters.Count())
                    && SignatureComparer.HaveSameSignature(m.Parameters.ToList(), parameters.ToList())))
                yield break;

            yield return CodeActionFactory.Create(node.Span, DiagnosticSeverity.Info, GettextCatalog.GetString("Create overload without parameter"),
                t2 =>
                {
                    BlockSyntax body = null;
                    if (methodDeclaration.Body != null)
                    {
                        var defaultExpression = GetDefaultValueExpression(semanticModel, methodDeclaration, node.Type);
                        List<StatementSyntax> bodyStatements = new List<StatementSyntax>();
                        ArgumentSyntax argumentExpression = null;

                        if (node.Modifiers.Any(m => m.IsKind(SyntaxKind.RefKeyword)))
                        {
                            bodyStatements.Add(SyntaxFactory.LocalDeclarationStatement(
                                SyntaxFactory.VariableDeclaration(
                                    node.Type,
                                    SyntaxFactory.SeparatedList(new[] {
                                    SyntaxFactory.VariableDeclarator(node.Identifier, null, SyntaxFactory.EqualsValueClause(defaultExpression))
                                    }))));
                            argumentExpression = GetArgumentExpression(node);
                        }
                        else if (node.Modifiers.Any(m => m.IsKind(SyntaxKind.OutKeyword)))
                        {
                            bodyStatements.Add(SyntaxFactory.LocalDeclarationStatement(
                                SyntaxFactory.VariableDeclaration(
                                    node.Type,
                                    SyntaxFactory.SeparatedList(new[] {
                                    SyntaxFactory.VariableDeclarator(node.Identifier)
                                    }))));
                            argumentExpression = GetArgumentExpression(node);
                        }
                        else
                        {
                            argumentExpression = SyntaxFactory.Argument(defaultExpression);
                        }

                        var methodInvocation = SyntaxFactory.InvocationExpression(
                            SyntaxFactory.IdentifierName(methodDeclaration.Identifier),
                            SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList(methodDeclaration.ParameterList.Parameters.Select(p => (p == node) ? argumentExpression : GetArgumentExpression(p)))));

                        if (method.ReturnType.SpecialType == SpecialType.System_Void)
                            bodyStatements.Add(SyntaxFactory.ExpressionStatement(methodInvocation));
                        else
                            bodyStatements.Add(SyntaxFactory.ReturnStatement(methodInvocation));


                        body = SyntaxFactory.Block(bodyStatements).WithTrailingTrivia(methodDeclaration.Body?.GetTrailingTrivia());
                    }

                    var newMethod = methodDeclaration
                        .WithParameterList(SyntaxFactory.ParameterList(
                            SyntaxFactory.SeparatedList(methodDeclaration.ParameterList.Parameters.Where(p => p.Identifier.ValueText != node.Identifier.ValueText))))
                        .WithBody(body)
                        .WithLeadingTrivia(methodDeclaration.GetLeadingTrivia().Where(t => !t.IsKind(SyntaxKind.SingleLineDocumentationCommentTrivia) && !t.IsKind(SyntaxKind.MultiLineDocumentationCommentTrivia)))
                        .WithTrailingTrivia(methodDeclaration.GetTrailingTrivia())
                        .WithAdditionalAnnotations(Formatter.Annotation);

                    var newRoot = root.InsertNodesBefore(methodDeclaration, new[] { newMethod });
                    return Task.FromResult(document.WithSyntaxRoot(newRoot));
                });
        }

        static ArgumentSyntax GetArgumentExpression(ParameterSyntax parameter)
        {
            var identifier = SyntaxFactory.IdentifierName(parameter.Identifier);
            if (parameter.Modifiers.Any(m => m.IsKind(SyntaxKind.OutKeyword)))
                return SyntaxFactory.Argument(null, SyntaxFactory.Token(SyntaxKind.OutKeyword), identifier);
            if (parameter.Modifiers.Any(m => m.IsKind(SyntaxKind.RefKeyword)))
                return SyntaxFactory.Argument(null, SyntaxFactory.Token(SyntaxKind.RefKeyword), identifier);

            return SyntaxFactory.Argument(identifier);
        }

        static ExpressionSyntax GetDefaultValueExpression(SemanticModel semanticModel, MethodDeclarationSyntax methodDeclaration, TypeSyntax type)
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
                    return SyntaxFactory.DefaultExpression(type);
                return SyntaxFactory.MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression, type, SyntaxFactory.IdentifierName(members.First().Identifier));
            }

            if (typeSymbol.IsKind(SymbolKind.DynamicType))
                return SyntaxFactory.LiteralExpression(SyntaxKind.NullLiteralExpression);
            if (typeSymbol.IsReferenceType)
            {
                if (typeSymbol.GetTypeParameters().Any() && (methodDeclaration.TypeParameterList != null) && methodDeclaration.TypeParameterList.Parameters.Any())
                    return SyntaxFactory.DefaultExpression(type);
                return SyntaxFactory.LiteralExpression(SyntaxKind.NullLiteralExpression);
            }

            switch (typeSymbol.SpecialType)
            {
                case SpecialType.System_Boolean:
                    return SyntaxFactory.LiteralExpression(SyntaxKind.FalseLiteralExpression);
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
                return SyntaxFactory.LiteralExpression(SyntaxKind.NullLiteralExpression);
            if (typeSymbol.IsValueType)
                return SyntaxFactory.ObjectCreationExpression(type, SyntaxFactory.ArgumentList(), null);

            return SyntaxFactory.DefaultExpression(type);
        }
    }
}
