using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace RefactoringEssentials.CSharp.Diagnostics
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class BitwiseOperatorOnEnumWithoutFlagsAnalyzer : DiagnosticAnalyzer
    {
        static readonly DiagnosticDescriptor descriptor = new DiagnosticDescriptor(
            CSharpDiagnosticIDs.BitwiseOperatorOnEnumWithoutFlagsAnalyzerID,
            GettextCatalog.GetString("Bitwise operation on enum which has no [Flags] attribute"),
            GettextCatalog.GetString("Bitwise operation on enum not marked with [Flags] attribute"),
            DiagnosticAnalyzerCategories.CodeQualityIssues,
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            helpLinkUri: HelpLink.CreateFor(CSharpDiagnosticIDs.BitwiseOperatorOnEnumWithoutFlagsAnalyzerID)
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
                        nodeContext.ReportDiagnostic(diagnostic);
                },
                SyntaxKind.BitwiseNotExpression,

                SyntaxKind.OrAssignmentExpression,
                SyntaxKind.AndAssignmentExpression,
                SyntaxKind.ExclusiveOrAssignmentExpression,

                SyntaxKind.BitwiseAndExpression,
                SyntaxKind.BitwiseOrExpression,
                SyntaxKind.ExclusiveOrExpression
            );
        }

        static bool TryGetDiagnostic(SyntaxNodeAnalysisContext nodeContext, out Diagnostic diagnostic)
        {
            diagnostic = default(Diagnostic);

            var prefixUnaryExpression = nodeContext.Node as PrefixUnaryExpressionSyntax;
            if (prefixUnaryExpression != null)
            {
                if (IsNonFlagsEnum(nodeContext.SemanticModel, prefixUnaryExpression.Operand))
                {
                    diagnostic = Diagnostic.Create(
                        descriptor,
                        prefixUnaryExpression.OperatorToken.GetLocation()
                    );
                    return true;
                }
            }

            var assignmentExpression = nodeContext.Node as AssignmentExpressionSyntax;
            if (assignmentExpression != null)
            {
                if (IsNonFlagsEnum(nodeContext.SemanticModel, assignmentExpression.Left) || IsNonFlagsEnum(nodeContext.SemanticModel, assignmentExpression.Right))
                {
                    diagnostic = Diagnostic.Create(
                        descriptor,
                        assignmentExpression.OperatorToken.GetLocation()
                    );
                    return true;
                }
            }

            var binaryExpression = nodeContext.Node as BinaryExpressionSyntax;
            if (binaryExpression != null)
            {
                if (IsNonFlagsEnum(nodeContext.SemanticModel, binaryExpression.Left) || IsNonFlagsEnum(nodeContext.SemanticModel, binaryExpression.Right))
                {
                    diagnostic = Diagnostic.Create(
                        descriptor,
                        binaryExpression.OperatorToken.GetLocation()
                    );
                    return true;
                }
            }
            return false;
        }

        static bool IsNonFlagsEnum(SemanticModel semanticModel, ExpressionSyntax expr)
        {
            var type = semanticModel.GetTypeInfo(expr).Type;
            if (type == null || type.TypeKind != TypeKind.Enum)
                return false;

            // check [Flags]
            return !type.GetAttributes().Any(attr => attr.AttributeClass.Name == "FlagsAttribute" && attr.AttributeClass.ContainingNamespace.ToDisplayString() == "System");
        }
    }
}