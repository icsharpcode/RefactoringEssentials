using System;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.VisualBasic;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;
using System.Collections.Generic;
using System.Linq;

namespace RefactoringEssentials.VB.Diagnostics
{
    [DiagnosticAnalyzer(LanguageNames.VisualBasic)]
    public class NameOfSuggestionAnalyzer : DiagnosticAnalyzer
    {
        static readonly DiagnosticDescriptor descriptor = new DiagnosticDescriptor(
            VBDiagnosticIDs.NameOfSuggestionAnalyzerID,
            GettextCatalog.GetString("Suggest the usage of the NameOf operator"),
            GettextCatalog.GetString("Use 'NameOf({0})' expression instead."),
            DiagnosticAnalyzerCategories.CodeQualityIssues,
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            helpLinkUri: HelpLink.CreateFor(VBDiagnosticIDs.NameOfSuggestionAnalyzerID)
        );
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(descriptor);

        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.RegisterSyntaxNodeAction(
                (nodeContext) =>
                {
                    Diagnostic diagnostic;
                    if (TryGetDiagnostic(nodeContext, out diagnostic))
                    {
                        nodeContext.ReportDiagnostic(diagnostic);
                    }
                },
                new SyntaxKind[] { SyntaxKind.ObjectCreationExpression }
            );
        }

        static bool TryGetDiagnostic(SyntaxNodeAnalysisContext nodeContext, out Diagnostic diagnostic)
        {
            diagnostic = default(Diagnostic);

            var options = nodeContext.SemanticModel.SyntaxTree.Options as VisualBasicParseOptions;
            if (options != null && options.LanguageVersion < LanguageVersion.VisualBasic14)
                return false;

            var objectCreateExpression = nodeContext.Node as ObjectCreationExpressionSyntax;

            ExpressionSyntax paramNode;
            if (!CheckExceptionType(nodeContext.SemanticModel, objectCreateExpression, out paramNode))
                return false;
            var paramName = GetArgumentParameterName(paramNode);
            if (paramName == null)
                return false;

            var validNames = GetValidParameterNames(objectCreateExpression);

            if (!validNames.Contains(paramName))
                return false;

            diagnostic = Diagnostic.Create(descriptor, paramNode.GetLocation(), paramName);
            return true;
        }

        internal static string GetArgumentParameterName(SyntaxNode expression)
        {
            var pExpr = expression as LiteralExpressionSyntax;
            if (pExpr != null)
                return pExpr.Token.Value.ToString();
            return null;
        }

        internal static List<string> GetValidParameterNames(ObjectCreationExpressionSyntax objectCreateExpression)
        {
            var names = new List<string>();
            var node = objectCreateExpression.Parent;
            while (node != null && !(node is TypeBlockSyntax) && !(node is AnonymousObjectCreationExpressionSyntax))
            {
                var lambda = node as LambdaExpressionSyntax;
                if (lambda != null)
                {
                    names.AddRange(lambda.SubOrFunctionHeader.ParameterList.Parameters.Select(p => p.Identifier.ToString()));
                    break;
                }

                var indexer = node as PropertyBlockSyntax;
                if ((indexer != null) && (indexer.PropertyStatement.ParameterList != null))
                {
                    names.AddRange(indexer.PropertyStatement.ParameterList.Parameters.Select(p => p.Identifier.ToString()));
                    break;
                }

                var methodDeclaration = node as MethodBlockSyntax;
                if (methodDeclaration != null)
                {
                    names.AddRange(methodDeclaration.SubOrFunctionStatement.ParameterList.Parameters.Select(p => p.Identifier.ToString()));
                    break;
                }

                var constructorDeclaration = node as ConstructorBlockSyntax;
                if (constructorDeclaration != null)
                {
                    names.AddRange(constructorDeclaration.SubNewStatement.ParameterList.Parameters.Select(p => p.Identifier.ToString()));
                    break;
                }
                var accessor = node as AccessorBlockSyntax;
                if (accessor != null)
                {
                    if (accessor.IsKind(SyntaxKind.SetAccessorBlock) ||
                        accessor.IsKind(SyntaxKind.AddHandlerAccessorBlock) ||
                        accessor.IsKind(SyntaxKind.RemoveHandlerAccessorBlock))
                    {
                        names.Add("value");
                    }

                    var propertyParent = node.Parent as PropertyBlockSyntax;
                    if ((propertyParent == null) || (propertyParent.PropertyStatement.ParameterList == null))
                        break;
                }
                node = node.Parent;
            }
            return names;
        }

        internal static bool CheckExceptionType(SemanticModel model, ObjectCreationExpressionSyntax objectCreateExpression, out ExpressionSyntax paramNode)
        {
            paramNode = null;
            var type = model.GetTypeInfo(objectCreateExpression).Type;
            if (type == null)
                return false;
            if (objectCreateExpression.ArgumentList == null)
                return false;
            if (type.Name == typeof(ArgumentException).Name)
            {
                if (objectCreateExpression.ArgumentList.Arguments.Count >= 2)
                {
                    paramNode = objectCreateExpression.ArgumentList.Arguments[1].GetExpression();
                }
                return paramNode != null;
            }
            if (type.Name == typeof(ArgumentNullException).Name ||
                type.Name == typeof(ArgumentOutOfRangeException).Name ||
                type.Name == "DuplicateWaitObjectException")
            {
                if (objectCreateExpression.ArgumentList.Arguments.Count >= 1)
                {
                    paramNode = objectCreateExpression.ArgumentList.Arguments[0].GetExpression();
                }
                return paramNode != null;
            }
            return false;
        }
    }
}

