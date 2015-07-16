using System.Linq;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;
using Microsoft.CodeAnalysis.VisualBasic;
using Microsoft.CodeAnalysis.Simplification;
using Microsoft.CodeAnalysis.Formatting;
using RefactoringEssentials.Util;

namespace RefactoringEssentials.VB.CodeRefactorings
{
    [ExportCodeRefactoringProvider(LanguageNames.VisualBasic, Name = "Create changed event for property")]
    public class CreateChangedEventCodeRefactoringProvider : CodeRefactoringProvider
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
            var property = root.FindNode(span) as PropertyStatementSyntax;

            if (property == null || !property.Identifier.Span.Contains(span))
                return;

            var propertyBlock = property.Parent as PropertyBlockSyntax;

            var field = GetBackingField(model, propertyBlock);
            if (field == null)
                return;
            var type = propertyBlock.Parent as TypeBlockSyntax;
            if (type == null)
                return;

            var resolvedType = model.Compilation.GetTypeSymbol("System", "EventHandler", 0, cancellationToken);
            if (resolvedType == null)
                return;

            context.RegisterRefactoring(
                CodeActionFactory.Create(
                    span,
                    DiagnosticSeverity.Info,
                    GettextCatalog.GetString("Create changed event"),
                    t2 =>
                    {
                        var eventDeclaration = CreateChangedEventDeclaration(propertyBlock);
                        var methodDeclaration = CreateEventInvocator(
                            type.BlockStatement.Identifier.ToString(),
                            type.BlockStatement.Modifiers.Any(m => m.IsKind(SyntaxKind.NotInheritableKeyword)),
                            eventDeclaration.Modifiers.Any(m => m.IsKind(SyntaxKind.SharedKeyword)),
                            eventDeclaration.Identifier.ToString(),
                            resolvedType.GetDelegateInvokeMethod(),
                            false
                        );
                        var invocation = SyntaxFactory.ExpressionStatement(SyntaxFactory.InvocationExpression(
                            SyntaxFactory.IdentifierName(methodDeclaration.SubOrFunctionStatement.Identifier.ToString()),
                            SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>(new[] {
                                SyntaxFactory.SimpleArgument(SyntaxFactory.ParseExpression("System.EventArgs.Empty").WithAdditionalAnnotations(Simplifier.Annotation))
                            }))
                        ));

                        var marker = new SyntaxAnnotation();

                        var newPropertyBlockSetterAccessor = propertyBlock.Accessors.First(a => a.IsKind(SyntaxKind.SetAccessorBlock));
                        newPropertyBlockSetterAccessor =
                        newPropertyBlockSetterAccessor.WithStatements(
                            newPropertyBlockSetterAccessor.Statements.Add(invocation.WithAdditionalAnnotations(Formatter.Annotation, Simplifier.Annotation)));

                        var newPropertyBlock = propertyBlock.WithAccessors(
                            SyntaxFactory.List<AccessorBlockSyntax>(new[] {
                                propertyBlock.Accessors.First(a => a.IsKind(SyntaxKind.GetAccessorBlock)),
                                newPropertyBlockSetterAccessor
                            }));

                        var newRoot = root.ReplaceNode(propertyBlock, new SyntaxNode[] {
                            newPropertyBlock,
                            methodDeclaration.WithAdditionalAnnotations(Formatter.Annotation),
                            eventDeclaration.WithTrailingTrivia(propertyBlock.GetTrailingTrivia()).WithAdditionalAnnotations(Formatter.Annotation)
                        });

                        return Task.FromResult(document.WithSyntaxRoot(newRoot));
                    })
            );
        }

        internal static IFieldSymbol GetBackingField(SemanticModel model, PropertyBlockSyntax property)
        {
            var getter = property.Accessors.FirstOrDefault(a => a.IsKind(SyntaxKind.GetAccessorBlock));
            var setter = property.Accessors.FirstOrDefault(a => a.IsKind(SyntaxKind.SetAccessorBlock));

            // automatic properties always need getter & setter
            if (property == null || getter == null || setter == null || !getter.Statements.Any() || !setter.Statements.Any())
                return null;

            if (property.PropertyStatement.Modifiers.Any(m => m.IsKind(SyntaxKind.MustInheritKeyword)) || property.Parent is InterfaceBlockSyntax)
                return null;
            var getterField = getter.ScanGetter(model);
            if (getterField == null)
                return null;
            var setterField = setter.ScanSetter(model);
            if (setterField == null)
                return null;
            if (!getterField.Equals(setterField))
                return null;
            return getterField;
        }

        static EventStatementSyntax CreateChangedEventDeclaration(PropertyBlockSyntax propertyDeclaration)
        {
            bool isNonInheritable = propertyDeclaration.PropertyStatement.Modifiers.Any(m => m.IsKind(SyntaxKind.SharedKeyword));

            return SyntaxFactory.EventStatement(
                    SyntaxFactory.List<AttributeListSyntax>(),
                    isNonInheritable ? SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword), SyntaxFactory.Token(SyntaxKind.SharedKeyword)) :
                        SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword)), SyntaxFactory.Identifier(propertyDeclaration.PropertyStatement.Identifier + "Changed"),
                    null,
                    SyntaxFactory.SimpleAsClause(SyntaxFactory.ParseTypeName("System.EventHandler").WithAdditionalAnnotations(Simplifier.Annotation)), null);
        }

        static ArgumentSyntax GetInvokeArgument(bool isShared)
        {
            return SyntaxFactory.SimpleArgument(
                isShared ? (ExpressionSyntax)SyntaxFactory.NothingLiteralExpression(SyntaxFactory.Token(SyntaxKind.NothingKeyword)) : SyntaxFactory.MeExpression());
        }

        static MethodBlockSyntax CreateEventInvocator(string declaringTypeName, bool isNonInheritable, bool isShared, string eventName, IMethodSymbol invokeMethod, bool useExplictType)
        {
            var result = CreateMethodStub(isNonInheritable, isShared, eventName, invokeMethod);
            result = result.WithStatements(SyntaxFactory.List<StatementSyntax>(new[] {
                SyntaxFactory.RaiseEventStatement(
                    SyntaxFactory.IdentifierName(eventName),
                    SyntaxFactory.ArgumentList(
                            SyntaxFactory.SeparatedList<ArgumentSyntax>(new[] {
                                GetInvokeArgument(isShared),
                                SyntaxFactory.SimpleArgument(SyntaxFactory.IdentifierName(invokeMethod.Parameters[1].Name))
                            })
                        )
                    )}));

            return result;
        }

        static string GetEventMethodName(string eventName)
        {
            return "On" + char.ToUpper(eventName[0]) + eventName.Substring(1);
        }

        static MethodBlockSyntax CreateMethodStub(bool isNonInheritable, bool isShared, string eventName, IMethodSymbol invokeMethod)
        {
            var methodStatement = SyntaxFactory.MethodStatement(SyntaxKind.SubStatement, SyntaxFactory.Token(SyntaxKind.SubKeyword), GetEventMethodName(eventName));

            if (isShared)
            {
                // Shared properties: Make the method Shared, too
                methodStatement = methodStatement.WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.ProtectedKeyword), SyntaxFactory.Token(SyntaxKind.SharedKeyword)));
            }
            else if (isNonInheritable)
            {
                // No additional modifiers for non-inheritable properties
            }
            else
            {
                // "Normal" properties: Make the method virtual (Overridable)
                methodStatement = methodStatement.WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.ProtectedKeyword), SyntaxFactory.Token(SyntaxKind.OverridableKeyword)));
            }

            var parameterType = invokeMethod.Parameters[1].Type;
            if (parameterType == null)
                return null;

            methodStatement = methodStatement.WithParameterList(SyntaxFactory.ParameterList(SyntaxFactory.SeparatedList<ParameterSyntax>(new[] {
                SyntaxFactory.Parameter(SyntaxFactory.ModifiedIdentifier(invokeMethod.Parameters [1].Name)).WithAsClause(
                    SyntaxFactory.SimpleAsClause(SyntaxFactory.ParseTypeName(parameterType.GetFullName()).WithAdditionalAnnotations(Simplifier.Annotation)))
                })));

            var node = SyntaxFactory.MethodBlock(SyntaxKind.SubBlock,
                methodStatement,
                SyntaxFactory.EndBlockStatement(SyntaxKind.EndSubStatement, SyntaxFactory.Token(SyntaxKind.SubKeyword)));

            return node;
        }
    }
}

