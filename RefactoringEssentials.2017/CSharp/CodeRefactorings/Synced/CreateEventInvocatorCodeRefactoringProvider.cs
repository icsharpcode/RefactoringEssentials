using System.Linq;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace RefactoringEssentials.CSharp.CodeRefactorings
{
    [ExportCodeRefactoringProvider(LanguageNames.CSharp, Name = "Create event invocator")]
    public class CreateEventInvocatorCodeRefactoringProvider : CodeRefactoringProvider
    {
        public override async Task ComputeRefactoringsAsync(CodeRefactoringContext context)
        {
            var document = context.Document;
            if (document.Project.Solution.Workspace.Kind == WorkspaceKind.MiscellaneousFiles)
                return;
            var span = context.Span;
            if (!span.IsEmpty)
                return;
            var cancellationToken = context.CancellationToken;
            if (cancellationToken.IsCancellationRequested)
                return;
            var model = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);
            if (model.IsFromGeneratedCode(cancellationToken))
                return;
            var root = await model.SyntaxTree.GetRootAsync(cancellationToken).ConfigureAwait(false);
            var token = root.FindToken(span.Start);
            if (!token.IsKind(SyntaxKind.IdentifierToken))
                return;

            var node = token.Parent as VariableDeclaratorSyntax;
            if (node == null)
                return;
            var declaredSymbol = model.GetDeclaredSymbol(node, cancellationToken);
            if (declaredSymbol == null || !declaredSymbol.IsKind(SymbolKind.Event))
                return;
            if (declaredSymbol.ContainingSymbol.IsInterfaceType())
                return;
            var invokeMethod = declaredSymbol.GetReturnType().GetDelegateInvokeMethod();
            if (invokeMethod == null)
                return;

            context.RegisterRefactoring(
                CodeActionFactory.CreateInsertion(
                    span,
                    DiagnosticSeverity.Info,
                    GettextCatalog.GetString("Create event invocator"),
                    t2 =>
                    {
                        SyntaxNode eventInvocator = CreateEventInvocator(document, declaredSymbol);
                        return Task.FromResult(new InsertionResult(context, eventInvocator, declaredSymbol.ContainingType, declaredSymbol.Locations.First()));
                    }
                )
            );
        }

        public static MethodDeclarationSyntax CreateEventInvocator(Document document, ISymbol eventMember, bool useExplictType = false)
        {
            var options = document.Project.ParseOptions as CSharpParseOptions;

            if (options != null && options.LanguageVersion < LanguageVersion.CSharp6)
                return CreateOldEventInvocator(eventMember.ContainingType.Name, eventMember.ContainingType.IsSealed, eventMember.IsStatic, eventMember.Name, eventMember.GetReturnType().GetDelegateInvokeMethod(), useExplictType);

            return CreateEventInvocator(eventMember.ContainingType.Name, eventMember.ContainingType.IsSealed, eventMember.IsStatic, eventMember.Name, eventMember.GetReturnType().GetDelegateInvokeMethod(), useExplictType);
        }

        public static MethodDeclarationSyntax CreateEventInvocator(Document document, string declaringTypeName, bool isSealed, bool isStatic, string eventName, IMethodSymbol invokeMethod, bool useExplictType = false)
        {
            var options = document.Project.ParseOptions as CSharpParseOptions;

            if (options != null && options.LanguageVersion < LanguageVersion.CSharp6)
                return CreateOldEventInvocator(declaringTypeName, isSealed, isStatic, eventName, invokeMethod, useExplictType);
            return CreateEventInvocator(declaringTypeName, isSealed, isStatic, eventName, invokeMethod, useExplictType);
        }

        static MethodDeclarationSyntax CreateMethodStub(bool isSealed, bool isStatic, string eventName, IMethodSymbol invokeMethod, int parameterIndex)
        {
            var node = SyntaxFactory.MethodDeclaration(SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.VoidKeyword)), SyntaxFactory.Identifier(GetEventMethodName(eventName)));
            if (isStatic)
            {
                node = node.WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.StaticKeyword)));
            }
            else if (isSealed)
            {

            }
            else
            {
                node = node.WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.ProtectedKeyword), SyntaxFactory.Token(SyntaxKind.VirtualKeyword)));
            }
            node = node.WithParameterList(SyntaxFactory.ParameterList(SyntaxFactory.SeparatedList<ParameterSyntax>(new[] {
                SyntaxFactory.Parameter (SyntaxFactory.Identifier (invokeMethod.Parameters [parameterIndex].Name)).WithType (invokeMethod.Parameters [parameterIndex].Type.GenerateTypeSyntax ())
            })));
            return node;
        }

        static ExpressionSyntax GetTargetExpression(string declaringTypeName, bool isStatic, string eventName)
        {
            if (eventName != "e")
                return (ExpressionSyntax)SyntaxFactory.IdentifierName(eventName);

            if (isStatic)
                return SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, SyntaxFactory.IdentifierName(declaringTypeName), SyntaxFactory.IdentifierName(eventName));

            return SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, SyntaxFactory.ThisExpression(), SyntaxFactory.IdentifierName(eventName));
        }

        static IEnumerable<ArgumentSyntax> GetInvokeArguments(bool isStatic, ImmutableArray<IParameterSymbol> parameters, int parameterIndex)
        {
            if (parameters.Length > 1)
            {
                yield return SyntaxFactory.Argument(isStatic ? (ExpressionSyntax)SyntaxFactory.LiteralExpression(SyntaxKind.NullLiteralExpression) : SyntaxFactory.ThisExpression());
            }

            yield return SyntaxFactory.Argument(SyntaxFactory.IdentifierName(parameters[parameterIndex].Name));
        }

        static MethodDeclarationSyntax CreateEventInvocator(string declaringTypeName, bool isSealed, bool isStatic, string eventName, IMethodSymbol invokeMethod, bool useExplictType)
        {
            int parameterIndex = (invokeMethod.Parameters.Length) > 1 ? 1 : 0;
            var result = CreateMethodStub(isSealed, isStatic, eventName, invokeMethod, parameterIndex);
            var targetExpr = GetTargetExpression(declaringTypeName, isStatic, eventName);
            result = result.WithBody(SyntaxFactory.Block(
                SyntaxFactory.ExpressionStatement(
                    SyntaxFactory.InvocationExpression(
                        SyntaxFactory.ConditionalAccessExpression(
                            targetExpr,
                            SyntaxFactory.MemberBindingExpression(SyntaxFactory.IdentifierName("Invoke"))
                        ),
                        SyntaxFactory.ArgumentList(
                            SyntaxFactory.SeparatedList<ArgumentSyntax>(GetInvokeArguments(isStatic, invokeMethod.Parameters, parameterIndex))
                        )
                    )
                )
            ));

            return result;
        }


        static MethodDeclarationSyntax CreateOldEventInvocator(string declaringTypeName, bool isSealed, bool isStatic, string eventName, IMethodSymbol invokeMethod, bool useExplictType)
        {
            int parameterIndex = (invokeMethod.Parameters.Length) > 1 ? 1 : 0;
            //	var invokeMethod = member.GetReturnType ().GetDelegateInvokeMethod ();
            var result = CreateMethodStub(isSealed, isStatic, eventName, invokeMethod, parameterIndex);
            const string handlerName = "handler";
            var targetExpr = GetTargetExpression(declaringTypeName, isStatic, eventName);
            result = result.WithBody(SyntaxFactory.Block(
                SyntaxFactory.LocalDeclarationStatement(
                    SyntaxFactory.VariableDeclaration(
                        SyntaxFactory.ParseTypeName("var"),
                        SyntaxFactory.SeparatedList<VariableDeclaratorSyntax>(new[] {
                            SyntaxFactory.VariableDeclarator (SyntaxFactory.Identifier (handlerName)).WithInitializer (
                                SyntaxFactory.EqualsValueClause (
                                    targetExpr
                                )
                            )
                        })
                    )
                ),
                SyntaxFactory.IfStatement(
                    SyntaxFactory.BinaryExpression(
                        SyntaxKind.NotEqualsExpression,
                        SyntaxFactory.IdentifierName(handlerName),
                        SyntaxFactory.LiteralExpression(SyntaxKind.NullLiteralExpression)
                    ),
                    SyntaxFactory.ExpressionStatement(
                        SyntaxFactory.InvocationExpression(
                            SyntaxFactory.IdentifierName(handlerName),
                            SyntaxFactory.ArgumentList(
                                SyntaxFactory.SeparatedList<ArgumentSyntax>(GetInvokeArguments(isStatic, invokeMethod.Parameters, parameterIndex))
                            )
                        )
                    )
                )
            ));
            return result;
        }

        static string GetEventMethodName(string eventName)
        {
            return NameProposalService.GetNameProposal("On" + char.ToUpper(eventName[0]) + eventName.Substring(1), SyntaxKind.MethodDeclaration);
        }
    }
}