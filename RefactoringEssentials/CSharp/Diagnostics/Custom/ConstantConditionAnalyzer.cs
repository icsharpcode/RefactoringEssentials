using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace RefactoringEssentials.CSharp.Diagnostics
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class ConstantConditionAnalyzer : DiagnosticAnalyzer
    {
        static readonly DiagnosticDescriptor descriptor = new DiagnosticDescriptor(
            CSharpDiagnosticIDs.ConstantConditionAnalyzerID,
            GettextCatalog.GetString("Condition is always 'true' or always 'false'"),
            GettextCatalog.GetString("Condition is always '{0}'"),
            DiagnosticAnalyzerCategories.CodeQualityIssues,
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            helpLinkUri: HelpLink.CreateFor(CSharpDiagnosticIDs.ConstantConditionAnalyzerID)
        );

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(descriptor);

        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.RegisterSyntaxNodeAction(
                (nodeContext) =>
                {
                    Check(nodeContext, ((IfStatementSyntax)nodeContext.Node).Condition);
                },
                new SyntaxKind[] { SyntaxKind.IfStatement }
            );

            context.RegisterSyntaxNodeAction(
                (nodeContext) =>
                {
                    Check(nodeContext, ((WhileStatementSyntax)nodeContext.Node).Condition);
                },
                new SyntaxKind[] { SyntaxKind.WhileStatement }
            );

            context.RegisterSyntaxNodeAction(
                (nodeContext) =>
                {
                    Check(nodeContext, ((DoStatementSyntax)nodeContext.Node).Condition);
                },
                new SyntaxKind[] { SyntaxKind.DoStatement }
            );

            context.RegisterSyntaxNodeAction(
                (nodeContext) =>
                {
                    Check(nodeContext, ((ConditionalExpressionSyntax)nodeContext.Node).Condition);
                },
                new SyntaxKind[] { SyntaxKind.ConditionalExpression }
            );

            context.RegisterSyntaxNodeAction(
                (nodeContext) =>
                {
                    Check(nodeContext, ((ForStatementSyntax)nodeContext.Node).Condition);
                },
                new SyntaxKind[] { SyntaxKind.ForStatement }
            );
        }

        void Check(SyntaxNodeAnalysisContext nodeContext, ExpressionSyntax condition)
        {
            if (condition == null)
                return;

            if (condition.IsKind(SyntaxKind.TrueLiteralExpression) || condition.IsKind(SyntaxKind.FalseLiteralExpression))
                return;

            var resolveResult = nodeContext.SemanticModel.GetConstantValue(condition);
            if (!resolveResult.HasValue || !(resolveResult.Value is bool))
                return;

            var value = (bool)resolveResult.Value;

            nodeContext.ReportDiagnostic(Diagnostic.Create(
                descriptor.Id,
                descriptor.Category,
                string.Format(descriptor.MessageFormat.ToString(), value),
                descriptor.DefaultSeverity,
                descriptor.DefaultSeverity,
                descriptor.IsEnabledByDefault,
                4,
                descriptor.Title,
                descriptor.Description,
                descriptor.HelpLinkUri,
                condition.GetLocation(),
                null,
                new[] { value.ToString() }
            ));
        }

        ////			void RemoveText(Script script, TextLocation start, TextLocation end)
        ////			{
        ////				var startOffset = script.GetCurrentOffset(start);
        ////				var endOffset = script.GetCurrentOffset(end);
        ////				if (startOffset < endOffset)
        ////					script.RemoveText(startOffset, endOffset - startOffset);
        ////			}
        //		}
    }
}