using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq.Expressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace RefactoringEssentials.CSharp.Diagnostics
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class RedundantToStringCallAnalyzer : DiagnosticAnalyzer
    {
        static Tuple<int, int> onlyFirst = Tuple.Create(0, 0);

        static IDictionary<Tuple<string, int>, Tuple<int, int>> membersCallingToString = new Dictionary<Tuple<string, int>, Tuple<int, int>> {
            { Tuple.Create("System.IO.TextWriter.Write", 1), onlyFirst },
            { Tuple.Create("System.IO.TextWriter.WriteLine", 1), onlyFirst },
            { Tuple.Create("System.Console.Write", 1), onlyFirst },
            { Tuple.Create("System.Console.WriteLine", 1), onlyFirst }
        };

        static readonly DiagnosticDescriptor descriptor1 = new DiagnosticDescriptor(
            CSharpDiagnosticIDs.RedundantToStringCallAnalyzerID,
            GettextCatalog.GetString("Finds calls to ToString() which would be generated automatically by the compiler"),
            GettextCatalog.GetString("Redundant 'ToString()' call"),
            DiagnosticAnalyzerCategories.RedundanciesInCode,
            DiagnosticSeverity.Info,
            isEnabledByDefault: true,
            helpLinkUri: HelpLink.CreateFor(CSharpDiagnosticIDs.RedundantToStringCallAnalyzerID),
            customTags: DiagnosticCustomTags.Unnecessary
        );

        static readonly DiagnosticDescriptor descriptor2 = new DiagnosticDescriptor(
            CSharpDiagnosticIDs.RedundantToStringCallAnalyzer_ValueTypesID,
            GettextCatalog.GetString("Finds calls to ToString() which would be generated automatically by the compiler"),
            GettextCatalog.GetString("Redundant 'ToString()' call"),
            DiagnosticAnalyzerCategories.RedundanciesInCode,
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            helpLinkUri: HelpLink.CreateFor(CSharpDiagnosticIDs.RedundantToStringCallAnalyzerID),
            customTags: DiagnosticCustomTags.Unnecessary
        );

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(descriptor1, descriptor2);

        public override void Initialize(AnalysisContext context)
        {
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
            if (nodeContext.IsFromGeneratedCode())
                return;
            var node = nodeContext.Node as BinaryExpressionSyntax;
            var visitor = new BinaryExpressionVisitor(nodeContext);
            visitor.Visit(node);
        }

        static void AnalyzeInvocationExpression(SyntaxNodeAnalysisContext nodeContext)
        {
            if (nodeContext.IsFromGeneratedCode())
                return;
            var invocationExpression = nodeContext.Node as InvocationExpressionSyntax;

            if (invocationExpression.Parent is BinaryExpressionSyntax)
                return;

            var member = nodeContext.SemanticModel.GetSymbolInfo(invocationExpression).Symbol;
            if (member == null)
                return;
            var invocationResolveResult = nodeContext.SemanticModel.GetTypeInfo(invocationExpression).Type;

            // "".ToString()
            CheckTargetedObject(nodeContext, invocationExpression, invocationResolveResult, member);

            // Check list of members that call ToString() automatically
            CheckAutomaticToStringCallers(nodeContext, invocationExpression, member);

            // Check formatting calls
           // CheckFormattingCall(invocationExpression, invocationResolveResult));
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
                var type = nodeContext.SemanticModel.GetTypeInfo(memberExpression.Expression).Type;
                AddRedundantToStringIssue(memberExpression, invocationExpression, type?.IsValueType == true);
            }

            void AddRedundantToStringIssue(MemberAccessExpressionSyntax memberExpression, InvocationExpressionSyntax invocationExpression, bool isValueType)
            {
                // Simon Lindgren 2012-09-14: Previously there was a check here to see if the node had already been processed
                // This has been moved out to the callers, to check it earlier for a 30-40% run time reduction
                processedNodes.Add(invocationExpression);
                nodeContext.ReportDiagnostic(Diagnostic.Create(isValueType ? descriptor2 : descriptor1, memberExpression.Name.GetLocation()));
            }

        }

        #region Invocation expression

        static void CheckTargetedObject(SyntaxNodeAnalysisContext nodeContext, InvocationExpressionSyntax invocationExpression, ITypeSymbol type, ISymbol member)
        {
            var memberExpression = invocationExpression.Expression as MemberAccessExpressionSyntax;
            if (memberExpression != null)
            {
                if (type.SpecialType == SpecialType.System_String && member.Name == "ToString")
                {
                    nodeContext.ReportDiagnostic(Diagnostic.Create(descriptor1, memberExpression.Name.GetLocation()));
                }
            }
        }

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
            var key = new Tuple<string, int>(member.GetDocumentationCommentId(), invocationExpression.ArgumentList.Arguments.Count);
            Tuple<int, int> checkInfo;
            if (membersCallingToString.TryGetValue(key, out checkInfo))
            {
                var arguments = invocationExpression.ArgumentList.Arguments;
                for (int i = checkInfo.Item1; i < Math.Min(arguments.Count, checkInfo.Item2 + 1); ++i)
                {
                    CheckExpressionInAutoCallContext(nodeContext, arguments[i].Expression);
                }
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
            var type = nodeContext.SemanticModel.GetTypeInfo(memberExpression.Expression).Type;
            if (type?.IsValueType == true)
                nodeContext.ReportDiagnostic(Diagnostic.Create(descriptor1, memberExpression.Name.GetLocation()));
        }

        //static void CheckFormattingCall(InvocationExpressionSyntax invocationExpression, CSharpInvocationResolveResult invocationResolveResult)
        //{
        //    Expression formatArgument;
        //    IList<Expression> formatArguments;
        //    // Only check parameters that are of type object: String means it is neccessary, others
        //    // means that there is another problem (ie no matching overload of the method).
        //    Func<IParameter, Expression, bool> predicate = (parameter, argument) =>
        //    {
        //        var type = parameter.Type;
        //        if (type is TypeWithElementType && parameter.IsParams)
        //        {
        //            type = ((TypeWithElementType)type).ElementType;
        //        }
        //        var typeDefinition = type.GetDefinition();
        //        if (typeDefinition == null)
        //            return false;
        //        return typeDefinition.IsKnownType(KnownTypeCode.Object);
        //    };
        //    if (FormatStringHelper.TryGetFormattingParameters(invocationResolveResult, invocationExpression,
        //                                                      out formatArgument, out formatArguments, predicate))
        //    {
        //        foreach (var argument in formatArguments)
        //        {
        //            CheckExpressionInAutoCallContext(argument);
        //        }
        //    }
        //}
        #endregion
    }
}