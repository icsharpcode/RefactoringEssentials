using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Linq;
using Microsoft.CodeAnalysis.Text;

namespace RefactoringEssentials.CSharp.Diagnostics
{
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class RedundantToStringCallAnalyzer : DiagnosticAnalyzer
    {
        static readonly DiagnosticDescriptor descriptor1 = new DiagnosticDescriptor(
            CSharpDiagnosticIDs.RedundantToStringCallAnalyzerID,
            GettextCatalog.GetString("Finds calls to ToString() which would be generated automatically by the compiler"),
            GettextCatalog.GetString("Redundant 'ToString()' call"),
            DiagnosticAnalyzerCategories.RedundanciesInCode,
            DiagnosticSeverity.Hidden,
            isEnabledByDefault: true,
            helpLinkUri: HelpLink.CreateFor(CSharpDiagnosticIDs.RedundantToStringCallAnalyzerID),
            customTags: DiagnosticCustomTags.Unnecessary
        );

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(descriptor1);

        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.RegisterSyntaxNodeAction(
                AnalyzeBinaryExpression,
                new SyntaxKind[] { SyntaxKind.AddExpression }
            );

            context.RegisterSyntaxNodeAction(
                AnalyzeInvocationExpression,
                new SyntaxKind[] { SyntaxKind.InvocationExpression }
            );

        }

        static void AnalyzeBinaryExpression(SyntaxNodeAnalysisContext nodeContext)
        {
            var node = nodeContext.Node as BinaryExpressionSyntax;
            var visitor = new BinaryExpressionVisitor(nodeContext);
            visitor.Visit(node);
        }

        static void AnalyzeInvocationExpression(SyntaxNodeAnalysisContext nodeContext)
        {
            var invocationExpression = nodeContext.Node as InvocationExpressionSyntax;

            if (invocationExpression.Parent is BinaryExpressionSyntax)
                return;

            var member = nodeContext.SemanticModel.GetSymbolInfo(invocationExpression).Symbol;
            if (member == null)
                return;
            // "".ToString()
            CheckTargetedObject(nodeContext, invocationExpression, member);

            // Check list of members that call ToString() automatically
            CheckAutomaticToStringCallers(nodeContext, invocationExpression, member);

            // Check formatting calls
            CheckFormattingCall(nodeContext, invocationExpression, member);
        }


        class BinaryExpressionVisitor : CSharpSyntaxWalker
        {
            readonly SyntaxNodeAnalysisContext nodeContext;

            int stringExpressionCount;
            ExpressionSyntax firstStringExpression;
            HashSet<SyntaxNode> processedNodes = new HashSet<SyntaxNode>();

            public BinaryExpressionVisitor(SyntaxNodeAnalysisContext nodeContext)
            {
                this.nodeContext = nodeContext;
            }

            public void Reset()
            {
                stringExpressionCount = 0;
                firstStringExpression = null;
            }

            void Check(ExpressionSyntax expression)
            {
                if (stringExpressionCount <= 1)
                {
                    var resolvedType = nodeContext.SemanticModel.GetTypeInfo(expression).Type;
                    if (resolvedType != null && resolvedType.SpecialType == SpecialType.System_String)
                    {
                        stringExpressionCount++;
                        if (stringExpressionCount == 1)
                        {
                            firstStringExpression = expression;
                        }
                        else {
                            CheckExpressionInAutoCallContext(firstStringExpression);
                            CheckExpressionInAutoCallContext(expression);
                        }
                    }
                }
                else {
                    CheckExpressionInAutoCallContext(expression);
                }
            }

            public override void VisitBinaryExpression(BinaryExpressionSyntax node)
            {
                Check(node.Left);
                Check(node.Right);
            }

            public override void VisitBaseExpression(BaseExpressionSyntax node)
            {
                base.VisitBaseExpression(node);
            }

            void CheckExpressionInAutoCallContext(ExpressionSyntax expression)
            {
                if (expression is InvocationExpressionSyntax && !processedNodes.Contains(expression))
                {
                    CheckInvocationInAutoCallContext((InvocationExpressionSyntax)expression);
                }
            }

            void CheckInvocationInAutoCallContext(InvocationExpressionSyntax invocationExpression)
            {
                var memberExpression = invocationExpression.Expression as MemberAccessExpressionSyntax;
                if (memberExpression == null)
                {
                    return;
                }
                if (memberExpression.Name.ToString() != "ToString" || invocationExpression.ArgumentList.Arguments.Any())
                {
                    return;
                }

                var resolveResult = nodeContext.SemanticModel.GetSymbolInfo(invocationExpression).Symbol;
                if (resolveResult == null)
                {
                    return;
                }
                if (!OverridesObjectToStringMethod(resolveResult))
                    return;
                var type = nodeContext.SemanticModel.GetTypeInfo(memberExpression.Expression).Type;
                if ((type != null) && type.IsValueType)
                    return;
                
                AddRedundantToStringIssue(memberExpression, invocationExpression);
            }

            void AddRedundantToStringIssue(MemberAccessExpressionSyntax memberExpression, InvocationExpressionSyntax invocationExpression)
            {
                // Simon Lindgren 2012-09-14: Previously there was a check here to see if the node had already been processed
                // This has been moved out to the callers, to check it earlier for a 30-40% run time reduction
                processedNodes.Add(invocationExpression);
                nodeContext.ReportDiagnostic(Diagnostic.Create(descriptor1, GetLocation (invocationExpression)));
            }

        }

        #region Invocation expression

        static bool OverridesObjectToStringMethod(ISymbol toStringSymbol)
        {
            ISymbol currentSymbol = toStringSymbol;
            while (currentSymbol != null)
            {
                var currentMethodSymbol = currentSymbol as IMethodSymbol;
                if ((currentMethodSymbol != null)
                    && (currentSymbol.ContainingType != null)
                    && (currentSymbol.ContainingType.SpecialType == SpecialType.System_Object))
                {
                    // Found object.ToString()
                    return true;
                }
                currentSymbol = currentSymbol.OverriddenMember();
            }

            return false;
        }

        static void CheckTargetedObject(SyntaxNodeAnalysisContext nodeContext, InvocationExpressionSyntax invocationExpression, ISymbol member)
        {
            var memberExpression = invocationExpression.Expression as MemberAccessExpressionSyntax;
            if (memberExpression != null)
            {
                var type = nodeContext.SemanticModel.GetTypeInfo(memberExpression.Expression).Type;

                if (type.SpecialType == SpecialType.System_String && member.Name == "ToString")
                {
                    nodeContext.ReportDiagnostic(Diagnostic.Create(descriptor1, GetLocation (invocationExpression)));
                }
            }
        }
        static string [] membersCallingToString = { "M:System.IO.TextWriter.Write", "M:System.Console.Write" };
        
        static void CheckAutomaticToStringCallers(SyntaxNodeAnalysisContext nodeContext, InvocationExpressionSyntax invocationExpression, ISymbol member)
        {
            if (member.IsOverride)
            {
                member = member.OverriddenMember();
                if (member == null)
                {
                    return;
                }
            }

            var method = member as IMethodSymbol;
            if (method == null)
                return;
            var id = method.GetDocumentationCommentId ();
            if (!membersCallingToString.Any (m => id.StartsWith (m, StringComparison.Ordinal)))
                return;

            var arguments = invocationExpression.ArgumentList.Arguments;
            for (int i = 0; i < arguments.Count; ++i)
            {
                CheckExpressionInAutoCallContext(nodeContext, arguments[i].Expression);
            }
        }

        static void CheckExpressionInAutoCallContext(SyntaxNodeAnalysisContext nodeContext, ExpressionSyntax expression)
        {
            var invocationExpressionSyntax = expression as InvocationExpressionSyntax;
            if (invocationExpressionSyntax != null)
            {
                CheckInvocationInAutoCallContext(nodeContext, invocationExpressionSyntax);
            }
        }

        static void CheckInvocationInAutoCallContext(SyntaxNodeAnalysisContext nodeContext, InvocationExpressionSyntax invocationExpression)
        {
            var memberExpression = invocationExpression.Expression as MemberAccessExpressionSyntax;
            if (memberExpression == null)
            {
                return;
            }
            if (memberExpression.Name.ToString() != "ToString" || invocationExpression.ArgumentList.Arguments.Any())
            {
                return;
            }

            var resolveResult = nodeContext.SemanticModel.GetSymbolInfo(invocationExpression).Symbol;
            if (resolveResult == null)
            {
                return;
            }
            if (!OverridesObjectToStringMethod(resolveResult))
                return;
            var type = nodeContext.SemanticModel.GetTypeInfo(memberExpression.Expression).Type;
            if ((type != null) && type.IsValueType)
                return;
            nodeContext.ReportDiagnostic(Diagnostic.Create(descriptor1, GetLocation(invocationExpression)));
        }

        static Location GetLocation(InvocationExpressionSyntax invocationExpression)
        {
            var memberExpression = invocationExpression.Expression as MemberAccessExpressionSyntax;

            return Location.Create(invocationExpression.SyntaxTree, TextSpan.FromBounds(memberExpression.Expression.Span.End, invocationExpression.Span.End));
        }

        static void CheckFormattingCall(SyntaxNodeAnalysisContext nodeContext, InvocationExpressionSyntax invocationExpression, ISymbol member)
        {
            ExpressionSyntax formatArgument;
            IList<ExpressionSyntax> formatArguments;
            // Only check parameters that are of type object: String means it is neccessary, others
            // means that there is another problem (ie no matching overload of the method).
            Func<IParameterSymbol, ExpressionSyntax, bool> predicate = (parameter, argument) =>
            {
                var type = parameter.Type;
/*                if (type is TypeWithElementType && parameter.IsParams)
                {
                    type = ((TypeWithElementType)type).ElementType;
                }*/
                return type.SpecialType == SpecialType.System_Object;
            };

            if (FormatStringHelper.TryGetFormattingParameters(nodeContext.SemanticModel, invocationExpression,
                                                              out formatArgument, out formatArguments, predicate))
            {
                foreach (var argument in formatArguments)
                {
                    CheckExpressionInAutoCallContext(nodeContext, argument);
                }
            }
        }
        #endregion
    }
}