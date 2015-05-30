using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using RefactoringEssentials.CSharp.CodeRefactorings;

namespace RefactoringEssentials.CSharp.Diagnostics
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class CanBeReplacedWithTryCastAndCheckForNullAnalyzer : DiagnosticAnalyzer
    {
        static readonly DiagnosticDescriptor descriptor = new DiagnosticDescriptor(
            NRefactoryDiagnosticIDs.CanBeReplacedWithTryCastAndCheckForNullAnalyzerID,
            GettextCatalog.GetString("Type check and casts can be replaced with 'as' and null check"),
            GettextCatalog.GetString("Type check and casts can be replaced with 'as' and null check"),
            DiagnosticAnalyzerCategories.CodeQualityIssues,
            DiagnosticSeverity.Info,
            isEnabledByDefault: true,
            helpLinkUri: HelpLink.CreateFor(NRefactoryDiagnosticIDs.CanBeReplacedWithTryCastAndCheckForNullAnalyzerID)
        );

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(descriptor);

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(
                (nodeContext) =>
                {
                    Diagnostic diagnostic;
                    if (TryGetDiagnostic(nodeContext, out diagnostic))
                    {
                        nodeContext.ReportDiagnostic(diagnostic);
                    }
                },
                new SyntaxKind[] { SyntaxKind.IfStatement }
            );
        }

        static bool TryGetDiagnostic(SyntaxNodeAnalysisContext nodeContext, out Diagnostic diagnostic)
        {
            diagnostic = default(Diagnostic);
            if (nodeContext.IsFromGeneratedCode())
                return false;
            var node = nodeContext.Node as IfStatementSyntax;
            BinaryExpressionSyntax isExpression;

            if (!CheckIfElse(
                nodeContext.SemanticModel,
                nodeContext.SemanticModel.SyntaxTree.GetRoot(nodeContext.CancellationToken),
                node,
                out isExpression))
                return false;

            diagnostic = Diagnostic.Create(descriptor, isExpression.OperatorToken.GetLocation());
            return true;
        }


        internal static bool CheckIfElse(SemanticModel ctx, SyntaxNode root, IfStatementSyntax ifElseStatement, out BinaryExpressionSyntax isExpression)
        {
            isExpression = null;
            var embeddedStatment = ifElseStatement.Statement;
            TypeInfo rr;
            ExpressionSyntax castToType;

            List<SyntaxNode> foundCasts;
            var innerCondition = ifElseStatement.Condition.SkipParens();
            if (innerCondition != null && innerCondition.IsKind(SyntaxKind.LogicalNotExpression))
            {

                var c2 = ((PrefixUnaryExpressionSyntax)innerCondition).Operand.SkipParens();
                if (c2.IsKind(SyntaxKind.IsExpression))
                {
                    isExpression = c2 as BinaryExpressionSyntax;
                    castToType = isExpression.Right;
                    rr = ctx.GetTypeInfo(castToType);
                    if (rr.Type == null || !rr.Type.IsReferenceType)
                        return false;

                    SyntaxNode searchStmt = embeddedStatment;
                    if (UseAsAndNullCheckCodeRefactoringProvider.IsControlFlowChangingStatement(searchStmt))
                    {
                        searchStmt = ifElseStatement.Parent;
                        foundCasts = searchStmt.DescendantNodesAndSelf(n => !UseAsAndNullCheckCodeRefactoringProvider.IsCast(ctx, n, rr.Type)).Where(arg => arg.SpanStart >= ifElseStatement.SpanStart && UseAsAndNullCheckCodeRefactoringProvider.IsCast(ctx, arg, rr.Type)).ToList();
                        foundCasts.AddRange(ifElseStatement.Condition.DescendantNodesAndSelf(n => !UseAsAndNullCheckCodeRefactoringProvider.IsCast(ctx, n, rr.Type)).Where(arg => arg.SpanStart > c2.Span.End && UseAsAndNullCheckCodeRefactoringProvider.IsCast(ctx, arg, rr.Type)));
                    }
                    else
                    {
                        foundCasts = new List<SyntaxNode>();
                    }
                    return foundCasts.Count > 0;

                }
                return false;
            }

            isExpression = innerCondition as BinaryExpressionSyntax;
            if (isExpression == null)
                return false;
            castToType = isExpression.Right;
            rr = ctx.GetTypeInfo(castToType);
            if (rr.Type == null || !rr.Type.IsReferenceType)
                return false;

            foundCasts = embeddedStatment.DescendantNodesAndSelf(n => !UseAsAndNullCheckCodeRefactoringProvider.IsCast(ctx, n, rr.Type)).Where(arg => UseAsAndNullCheckCodeRefactoringProvider.IsCast(ctx, arg, rr.Type)).ToList();
            foundCasts.AddRange(ifElseStatement.Condition.DescendantNodesAndSelf(n => !UseAsAndNullCheckCodeRefactoringProvider.IsCast(ctx, n, rr.Type)).Where(arg => arg.SpanStart > innerCondition.Span.End && UseAsAndNullCheckCodeRefactoringProvider.IsCast(ctx, arg, rr.Type)));
            return foundCasts.Count > 0;
        }
    }
}