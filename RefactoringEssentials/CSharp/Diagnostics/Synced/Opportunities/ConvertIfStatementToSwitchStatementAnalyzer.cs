using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace RefactoringEssentials.CSharp.Diagnostics
{
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class ConvertIfStatementToSwitchStatementAnalyzer : DiagnosticAnalyzer
    {
        static readonly DiagnosticDescriptor descriptor = new DiagnosticDescriptor(
            CSharpDiagnosticIDs.ConvertIfStatementToSwitchStatementAnalyzerID,
            GettextCatalog.GetString("'if' statement can be re-written as 'switch' statement"),
            GettextCatalog.GetString("Convert to 'switch' statement"),
            DiagnosticAnalyzerCategories.Opportunities,
            DiagnosticSeverity.Info,
            isEnabledByDefault: true,
            helpLinkUri: HelpLink.CreateFor(CSharpDiagnosticIDs.ConvertIfStatementToSwitchStatementAnalyzerID)
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
                new SyntaxKind[] { SyntaxKind.IfStatement }
            );
        }

        static bool TryGetDiagnostic(SyntaxNodeAnalysisContext nodeContext, out Diagnostic diagnostic)
        {
            var node = nodeContext.Node as IfStatementSyntax;
            var semanticModel = nodeContext.SemanticModel;
            var cancellationToken = nodeContext.CancellationToken;

            diagnostic = default(Diagnostic);
            if (node.Parent is IfStatementSyntax || node.Parent is ElseClauseSyntax)
                return false;

            var switchExpr = GetSwitchExpression(semanticModel, node.Condition);
            if (switchExpr == null)
                return false;
            var switchSections = new List<SwitchSectionSyntax>();
            if (!CollectSwitchSections(switchSections, semanticModel, node, switchExpr))
                return false;
            if (switchSections.Count(s => !s.Labels.OfType<DefaultSwitchLabelSyntax>().Any()) <= 2)
                return false;

            diagnostic = Diagnostic.Create(
                descriptor,
                node.IfKeyword.GetLocation()
            );
            return true;
        }

        static readonly SpecialType[] validTypes = {
            SpecialType.System_String, SpecialType.System_Boolean, SpecialType.System_Char,
            SpecialType.System_Byte, SpecialType.System_SByte,
            SpecialType.System_Int16, SpecialType.System_Int32, SpecialType.System_Int64,
            SpecialType.System_UInt16, SpecialType.System_UInt32, SpecialType.System_UInt64
        };

        static bool IsValidSwitchType(ITypeSymbol type)
        {
            if (type == null || type is IErrorTypeSymbol)
                return false;
            if (type.TypeKind == TypeKind.Enum)
                return true;

            if (type.IsNullableType())
            {
                type = type.GetNullableUnderlyingType();
                if (type == null || type is IErrorTypeSymbol)
                    return false;
            }
            return validTypes.Contains(type.SpecialType);
        }

        internal static ExpressionSyntax GetSwitchExpression(SemanticModel context, ExpressionSyntax expr)
        {
            var binaryOp = expr as BinaryExpressionSyntax;
            if (binaryOp == null)
                return null;

            if (binaryOp.OperatorToken.IsKind(SyntaxKind.LogicalOrExpression))
                return GetSwitchExpression(context, binaryOp.Left);

            if (binaryOp.OperatorToken.IsKind(SyntaxKind.EqualsEqualsToken))
            {
                ExpressionSyntax switchExpr = null;
                if (IsConstantExpression(context, binaryOp.Right))
                    switchExpr = binaryOp.Left;
                if (IsConstantExpression(context, binaryOp.Left))
                    switchExpr = binaryOp.Right;
                if (switchExpr != null && IsValidSwitchType(context.GetTypeInfo(switchExpr).Type))
                    return switchExpr;
            }

            return null;
        }

        internal static bool CollectSwitchSections(List<SwitchSectionSyntax> result, SemanticModel context,
                                           IfStatementSyntax ifStatement, ExpressionSyntax switchExpr)
        {
            // if
            var labels = new List<SwitchLabelSyntax>();
            if (!CollectCaseLabels(labels, context, ifStatement.Condition, switchExpr))
                return false;
            var statements = new List<StatementSyntax>();
            CollectSwitchSectionStatements(statements, context, ifStatement.Statement);
            result.Add(SyntaxFactory.SwitchSection(new SyntaxList<SwitchLabelSyntax>().AddRange(labels), new SyntaxList<StatementSyntax>().AddRange(statements)));

            if (ifStatement.Statement.DescendantNodes().Any(n => n is BreakStatementSyntax))
                return false;

            if (ifStatement.Else == null)
                return true;

            // else if
            var falseStatement = ifStatement.Else.Statement as IfStatementSyntax;
            if (falseStatement != null)
                return CollectSwitchSections(result, context, falseStatement, switchExpr);

            if (ifStatement.Else.Statement.DescendantNodes().Any(n => n is BreakStatementSyntax))
                return false;
            // else (default label)
            labels = new List<SwitchLabelSyntax>();
            labels.Add(SyntaxFactory.DefaultSwitchLabel());
            statements = new List<StatementSyntax>();
            CollectSwitchSectionStatements(statements, context, ifStatement.Else.Statement);
            result.Add(SyntaxFactory.SwitchSection(new SyntaxList<SwitchLabelSyntax>().AddRange(labels), new SyntaxList<StatementSyntax>().AddRange(statements)));

            return true;
        }

        static bool CollectCaseLabels(List<SwitchLabelSyntax> result, SemanticModel context,
                                       ExpressionSyntax condition, ExpressionSyntax switchExpr)
        {
            if (condition is ParenthesizedExpressionSyntax)
                return CollectCaseLabels(result, context, ((ParenthesizedExpressionSyntax)condition).Expression, switchExpr);

            var binaryOp = condition as BinaryExpressionSyntax;
            if (binaryOp == null)
                return false;

            if (binaryOp.IsKind(SyntaxKind.LogicalOrExpression))
                return CollectCaseLabels(result, context, binaryOp.Left, switchExpr) &&
                       CollectCaseLabels(result, context, binaryOp.Right, switchExpr);

            if (binaryOp.IsKind(SyntaxKind.EqualsExpression))
            {
                if (switchExpr.IsEquivalentTo(binaryOp.Left, true))
                {
                    if (IsConstantExpression(context, binaryOp.Right))
                    {
                        result.Add(SyntaxFactory.CaseSwitchLabel(binaryOp.Right));
                        return true;
                    }
                }
                else if (switchExpr.IsEquivalentTo(binaryOp.Right, true))
                {
                    if (IsConstantExpression(context, binaryOp.Left))
                    {
                        result.Add(SyntaxFactory.CaseSwitchLabel(binaryOp.Left));
                        return true;
                    }
                }
            }

            return false;
        }

        static void CollectSwitchSectionStatements(List<StatementSyntax> result, SemanticModel context,
                                                    StatementSyntax statement)
        {
            var blockStatement = statement as BlockSyntax;
            if (blockStatement != null)
                result.AddRange(blockStatement.Statements);
            else
                result.Add(statement);

            // add 'break;' at end if necessary
            var reachabilityAnalysis = context.AnalyzeControlFlow(statement);
            if (reachabilityAnalysis.EndPointIsReachable)
                result.Add(SyntaxFactory.BreakStatement());
        }

        static bool IsConstantExpression(SemanticModel context, ExpressionSyntax expr)
        {
            if (expr is LiteralExpressionSyntax)
                return true;
            if (expr is DefaultExpressionSyntax)
                return true;
            return context.GetConstantValue(expr).HasValue;
        }
    }
}