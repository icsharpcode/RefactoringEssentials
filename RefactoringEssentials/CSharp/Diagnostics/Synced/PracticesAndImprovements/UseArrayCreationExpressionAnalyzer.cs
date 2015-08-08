using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace RefactoringEssentials.CSharp.Diagnostics
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class UseArrayCreationExpressionAnalyzer : DiagnosticAnalyzer
    {
        private static readonly DiagnosticDescriptor descriptor = new DiagnosticDescriptor(
            CSharpDiagnosticIDs.UseArrayCreationExpressionAnalyzerID,
            GettextCatalog.GetString("Use array creation expression"),
            GettextCatalog.GetString("Use array create expression"),
            DiagnosticAnalyzerCategories.PracticesAndImprovements,
            DiagnosticSeverity.Info,
            isEnabledByDefault: true,
            helpLinkUri: HelpLink.CreateFor(CSharpDiagnosticIDs.UseArrayCreationExpressionAnalyzerID)
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
                 SyntaxKind.InvocationExpression
            );
        }

        private static bool TryGetDiagnostic(SyntaxNodeAnalysisContext nodeContext, out Diagnostic diagnostic)
        {
            diagnostic = default(Diagnostic);
            if (nodeContext.IsFromGeneratedCode())
                return false;
            var node = nodeContext.Node as InvocationExpressionSyntax;
            if (node == null)
                return false;

            var invocationSymbol = nodeContext.SemanticModel.GetSymbolInfo(node).Symbol;
            if (invocationSymbol == null)
                return false;
            System.Array.CreateInstance(typeof(int), 10);
            new int[10, 20, i];

            if (invocationSymbol.Name != "CreateInstance" || invocationSymbol.IsArrayType())
                // IsArrayType not working -> using it wrong.. 
                return false;

            var firstArgument = node.ArgumentList.Arguments.FirstOrDefault().Expression as TypeOfExpressionSyntax;
            if (firstArgument == null)
                return false;

            var argumentChildLiteralExpression =
                node.ArgumentList.Arguments[1].ChildNodes().FirstOrDefault() as LiteralExpressionSyntax;
            if (argumentChildLiteralExpression == null)
                return false;

           
            if (!argumentChildLiteralExpression.IsKind(SyntaxKind.NumericLiteralExpression))
                return false;

            ////				var argRR = ctx.Resolve(invocationExpression.Arguments.ElementAt(1));
            ////				if (!argRR.Type.IsKnownType(KnownTypeCode.Int32))
            ////					return;

            diagnostic = Diagnostic.Create(descriptor, node.GetLocation());
            return true;
        }

        //		class GatherVisitor : GatherVisitorBase<UseArrayCreationExpressionAnalyzer>
        //		{
        //			public GatherVisitor(SemanticModel semanticModel, Action<Diagnostic> addDiagnostic, CancellationToken cancellationToken)
        //				: base (semanticModel, addDiagnostic, cancellationToken)
        //			{
        //			}

        ////			public override void VisitInvocationExpression(InvocationExpression invocationExpression)
        ////			{
        ////				base.VisitInvocationExpression(invocationExpression);
        ////
        ////				var rr = ctx.Resolve(invocationExpression) as CSharpInvocationResolveResult;
        ////				if (rr == null || rr.IsError)
        ////					return;
        ////
        ////				if (rr.Member.Name != "CreateInstance" ||
        ////				    !rr.Member.DeclaringType.IsKnownType(KnownTypeCode.Array))
        ////					return;
        ////				var firstArg = invocationExpression.Arguments.FirstOrDefault() as TypeOfExpression;
        ////				if (firstArg == null)
        ////					return;
        ////				var argRR = ctx.Resolve(invocationExpression.Arguments.ElementAt(1));
        ////				if (!argRR.Type.IsKnownType(KnownTypeCode.Int32))
        ////					return;
        ////
        ////				AddDiagnosticAnalyzer(new CodeIssue(
        ////					invocationExpression,
        ////					ctx.TranslateString(""),
        ////					ctx.TranslateString(""),
        ////					script => {
        ////						var ac = new ArrayCreateExpression {
        ////							Type = firstArg.Type.Clone()
        ////						};
        ////						foreach (var arg in invocationExpression.Arguments.Skip(1))
        ////							ac.Arguments.Add(arg.Clone()) ;
        ////						script.Replace(invocationExpression, ac);
        ////					}
        ////				));
        ////			}
        //		}
    }
}