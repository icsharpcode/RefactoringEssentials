using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;

namespace RefactoringEssentials.CSharp.Diagnostics
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class UseMethodAnyAnalyzer : DiagnosticAnalyzer
    {
        static readonly DiagnosticDescriptor descriptor = new DiagnosticDescriptor(
            CSharpDiagnosticIDs.UseMethodAnyAnalyzerID,
            GettextCatalog.GetString("Replace usages of 'Count()' with call to 'Any()'"),
            GettextCatalog.GetString("Use '{0}' for increased performance"),
            DiagnosticAnalyzerCategories.PracticesAndImprovements,
            DiagnosticSeverity.Info,
            isEnabledByDefault: true,
            helpLinkUri: HelpLink.CreateFor(CSharpDiagnosticIDs.UseMethodAnyAnalyzerID)
        );

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(descriptor);

        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.RegisterSyntaxNodeAction(
                (nodeContext) => {
                    Diagnostic diagnostic;
                    if (TryGetDiagnostic(nodeContext, out diagnostic)) {
                        nodeContext.ReportDiagnostic(diagnostic);
                    }
                },
                SyntaxKind.EqualsExpression,
                SyntaxKind.NotEqualsExpression,
                SyntaxKind.GreaterThanExpression,
                SyntaxKind.GreaterThanOrEqualExpression,
                SyntaxKind.LessThanExpression,
                SyntaxKind.LessThanOrEqualExpression
            );
        }

        static bool TryGetDiagnostic(SyntaxNodeAnalysisContext nodeContext, out Diagnostic diagnostic)
        {
            diagnostic = default(Diagnostic);
            var node = nodeContext.Node as BinaryExpressionSyntax;
            ExpressionSyntax target;
            ArgumentListSyntax arguments;
            bool isNot;
            if (!MatchCount0Or1(node, nodeContext.SemanticModel, out target, out arguments, out isNot))
                return false;

            diagnostic = Diagnostic.Create(
                descriptor,
                node.GetLocation(),
                "Any()"
            );
            return true;
        }

        static bool MatchCount0Or1(BinaryExpressionSyntax node, SemanticModel semanticModel, out ExpressionSyntax target, out ArgumentListSyntax arguments, out bool isNot)
        {
            var left = node.Left.SkipParens();
            var right = node.Right.SkipParens();
            target = null;
            arguments = null;
            isNot = false;

            InvocationExpressionSyntax invocation = left as InvocationExpressionSyntax ?? right as InvocationExpressionSyntax;
            SyntaxToken? constant = (right as LiteralExpressionSyntax)?.Token ?? (left as LiteralExpressionSyntax)?.Token;

            if (invocation == null || !(constant?.Value is int))
                return false;
            if (semanticModel != null && !CheckInvocation(semanticModel, invocation))
                return false;
            int constantValue = (int)constant?.Value;
            bool callLeft = left == invocation;

            switch (node.Kind()) {
                // invocation == 0
                // 0 == invocation
                case SyntaxKind.EqualsExpression:
                    isNot = true;
                    if (constantValue != 0)
                        return false;
                    break;
                // invocation != 0
                // 0 != invocation
                case SyntaxKind.NotEqualsExpression:
                    if (constantValue != 0)
                        return false;
                    break;
                // invocation > 0
                // 1 > invocation
                case SyntaxKind.GreaterThanExpression:
                    if (callLeft) {
                        if (constantValue != 0)
                            return false;
                    } else {
                        isNot = true;
                        if (constantValue != 1)
                            return false;
                    }
                    break;
                // invocation >= 1
                // 0 >= invocation
                case SyntaxKind.GreaterThanOrEqualExpression:
                    if (callLeft) {
                        if (constantValue != 1)
                            return false;
                    } else {
                        isNot = true;
                        if (constantValue != 0)
                            return false;
                    }
                    break;
                // 0 < invocation
                // invocation < 1
                case SyntaxKind.LessThanExpression:
                    if (callLeft) {
                        isNot = true;
                        if (constantValue != 1)
                            return false;
                    } else {
                        if (constantValue != 0)
                            return false;
                    }
                    break;
                // 1 <= invocation
                // invocation <= 0
                case SyntaxKind.LessThanOrEqualExpression:
                    if (callLeft) {
                        isNot = true;
                        if (constantValue != 0)
                            return false;
                    } else {
                        if (constantValue != 1)
                            return false;
                    }
                    break;
                default:
                    return false;
            }

            target = (invocation.Expression as MemberAccessExpressionSyntax)?.Expression;
            if (target == null)
                return false;

            arguments = invocation.ArgumentList;
            return true;
        }

        private static bool CheckInvocation(SemanticModel semanticModel, InvocationExpressionSyntax invocation)
        {
            var symbol = semanticModel.GetSymbolInfo(invocation).Symbol;
            return symbol != null
                && symbol.Name == "Count"
                && symbol.IsExtensionMethod()
                && symbol.ContainingType.GetFullName() == "System.Linq.Enumerable";
        }

        internal static ExpressionSyntax MakeAnyCall(BinaryExpressionSyntax node)
        {
            ExpressionSyntax target;
            ArgumentListSyntax arguments;
            bool isNot;
            if (MatchCount0Or1(node, null, out target, out arguments, out isNot)) {
                var anyIdentifier = ((SimpleNameSyntax)SyntaxFactory.ParseName("Any"));
                var invocation = SyntaxFactory.InvocationExpression(
                    SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, target, anyIdentifier),
                    SyntaxFactory.ArgumentList(arguments.Arguments)
                );
                if (isNot)
                    return SyntaxFactory.PrefixUnaryExpression(SyntaxKind.LogicalNotExpression, invocation);
                return invocation;
            }

            return null;
        }
    }
}