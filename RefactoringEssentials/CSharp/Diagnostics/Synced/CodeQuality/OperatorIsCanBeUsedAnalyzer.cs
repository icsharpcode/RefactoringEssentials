using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace RefactoringEssentials.CSharp.Diagnostics
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class OperatorIsCanBeUsedAnalyzer : DiagnosticAnalyzer
    {
        static readonly DiagnosticDescriptor descriptor = new DiagnosticDescriptor(
            CSharpDiagnosticIDs.OperatorIsCanBeUsedAnalyzerID,
            GettextCatalog.GetString("Operator Is can be used instead of comparing object GetType() and instances of System.Type object"),
            GettextCatalog.GetString("Operator 'is' can be used"),
            DiagnosticAnalyzerCategories.CodeQualityIssues,
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            helpLinkUri: HelpLink.CreateFor(CSharpDiagnosticIDs.OperatorIsCanBeUsedAnalyzerID)
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
                SyntaxKind.EqualsExpression
            );
        }

        static bool TryGetDiagnostic(SyntaxNodeAnalysisContext nodeContext, out Diagnostic diagnostic)
        {
            diagnostic = default(Diagnostic);
            var node = nodeContext.Node as BinaryExpressionSyntax;
            InvocationExpressionSyntax invocation;
            TypeOfExpressionSyntax typeOf;
            if (!Matches(node.Left, node.Right, out invocation, out typeOf) && !Matches(node.Right, node.Left, out invocation, out typeOf))
                return false;

            var type = nodeContext.SemanticModel.GetTypeInfo(typeOf.Type).Type;
            if (type == null || !type.IsSealed)
                return false;
            diagnostic = Diagnostic.Create(
                descriptor,
                node.GetLocation()
            );
            return true;
        }

        static bool Matches(ExpressionSyntax member, ExpressionSyntax typeofExpr, out InvocationExpressionSyntax invoc, out TypeOfExpressionSyntax typeOf)
        {
            invoc = member as InvocationExpressionSyntax;
            typeOf = typeofExpr as TypeOfExpressionSyntax;
            if (invoc == null || typeOf == null)
                return false;

            var memberAccess = invoc.Expression as MemberAccessExpressionSyntax;
            return memberAccess != null && memberAccess.Name.Identifier.ValueText == "GetType";
        }
    }
}