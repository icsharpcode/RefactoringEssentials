using System;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace RefactoringEssentials.CSharp.Diagnostics
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class NameOfSuggestionAnalyzer : DiagnosticAnalyzer
    {
        static readonly DiagnosticDescriptor descriptor = new DiagnosticDescriptor(
            CSharpDiagnosticIDs.NameOfSuggestionAnalyzerID,
            GettextCatalog.GetString("Suggest the usage of the nameof operator"),
            GettextCatalog.GetString("Use 'nameof({0})' expression instead."),
            DiagnosticAnalyzerCategories.CodeQualityIssues,
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            helpLinkUri: HelpLink.CreateFor(CSharpDiagnosticIDs.NameOfSuggestionAnalyzerID)
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
            var options = nodeContext.SemanticModel.SyntaxTree.Options as CSharpParseOptions;
            if (options != null && options.LanguageVersion < LanguageVersion.CSharp6)
                return false;

            var objectCreateExpression = nodeContext.Node as ObjectCreationExpressionSyntax;

            ExpressionSyntax paramNode;
            if (!CheckExceptionType(nodeContext.SemanticModel, objectCreateExpression, out paramNode))
                return false;
            var paramName = NotResolvedInTextAnalyzer.GetArgumentParameterName(paramNode);
            if (paramName == null)
                return false;

            var validNames = NotResolvedInTextAnalyzer.GetValidParameterNames(objectCreateExpression);

            if (!validNames.Contains(paramName))
                return false;

            diagnostic = Diagnostic.Create(descriptor, paramNode.GetLocation(), paramName);
            return true;
        }

        static SyntaxToken? GetTypeName (TypeSyntax syntax)
        {
            if (syntax is SimpleNameSyntax sns)
            {
                // sns.GetRightmostName ?
                return sns.Identifier;
            }
            if (syntax is QualifiedNameSyntax qns)
            {
                return GetTypeName(qns.Right);
            }
            if (syntax is AliasQualifiedNameSyntax aqns)
            {
                return GetTypeName(aqns.Name);
            }
            return null;
        }

        internal static bool CheckExceptionType(SemanticModel model, ObjectCreationExpressionSyntax objectCreateExpression, out ExpressionSyntax paramNode)
        {
            paramNode = null;
            
            var typeName = GetTypeName (objectCreateExpression.Type)?.Text;
            if (typeName == null)
                return false;

            if (typeName == nameof(ArgumentException))
            {
                if (objectCreateExpression.ArgumentList.Arguments.Count >= 2)
                {
                    paramNode = objectCreateExpression.ArgumentList.Arguments[1].Expression;
                }
                return paramNode != null;
            }
            if (typeName == nameof(ArgumentNullException) ||
                typeName == nameof(ArgumentOutOfRangeException) ||
                typeName == "DuplicateWaitObjectException")
            {
                if (objectCreateExpression.ArgumentList.Arguments.Count >= 1)
                {
                    paramNode = objectCreateExpression.ArgumentList.Arguments[0].Expression;
                }
                return paramNode != null;
            }
            return false;
        }
    }
}

