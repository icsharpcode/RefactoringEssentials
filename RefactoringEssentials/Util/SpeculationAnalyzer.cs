using System;
using System.Threading;
using System.Reflection;
using System.Runtime.ExceptionServices;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;

namespace RefactoringEssentials
{
    [RoslynReflectionUsage(RoslynReflectionAllowedContext.CodeFixes)]
#if NR6
    public
#endif
    class SpeculationAnalyzer
    {
        readonly object instance;

        public SpeculationAnalyzer(ExpressionSyntax expression, ExpressionSyntax newExpression, SemanticModel semanticModel, CancellationToken cancellationToken, bool skipVerificationForReplacedNode = false, bool failOnOverloadResolutionFailuresInOriginalCode = false)
        {
            instance = Activator.CreateInstance(RoslynReflection.SpeculationAnalyzer.TypeInfo, new object[] {
                expression,
                newExpression,
                semanticModel,
                cancellationToken,
                skipVerificationForReplacedNode,
                failOnOverloadResolutionFailuresInOriginalCode
            });
        }

        public static bool SymbolInfosAreCompatible(SymbolInfo originalSymbolInfo, SymbolInfo newSymbolInfo, bool performEquivalenceCheck, bool requireNonNullSymbols = false)
        {
            try
            {
                return (bool)RoslynReflection.AbstractSpeculationAnalyzer_9.SymbolInfosAreCompatibleMethod.Invoke(null, new object[] { originalSymbolInfo, newSymbolInfo, performEquivalenceCheck, requireNonNullSymbols });
            }
            catch (TargetInvocationException ex)
            {
                ExceptionDispatchInfo.Capture(ex.InnerException).Throw();
                return false;
            }
        }

        public bool SymbolsForOriginalAndReplacedNodesAreCompatible()
        {
            try
            {
                return (bool)RoslynReflection.AbstractSpeculationAnalyzer_9.SymbolsForOriginalAndReplacedNodesAreCompatibleMethod.Invoke(instance, new object[0]);
            }
            catch (TargetInvocationException ex)
            {
                ExceptionDispatchInfo.Capture(ex.InnerException).Throw();
                return false;
            }
        }

        public bool ReplacementChangesSemantics()
        {
            try
            {
                return (bool)RoslynReflection.AbstractSpeculationAnalyzer_9.ReplacementChangesSemanticsMethod.Invoke(instance, new object[0]);
            }
            catch (TargetInvocationException ex)
            {
                ExceptionDispatchInfo.Capture(ex.InnerException).Throw();
                return false;
            }
        }

        public static SemanticModel CreateSpeculativeSemanticModelForNode(SyntaxNode originalNode, SyntaxNode nodeToSpeculate, SemanticModel semanticModel)
        {
            return (SemanticModel)RoslynReflection.SpeculationAnalyzer.CreateSpeculativeSemanticModelForNodeMethod.Invoke(null, new object[] { originalNode, nodeToSpeculate, semanticModel });
        }

        public static bool CanSpeculateOnNode(SyntaxNode node)
        {
            return (node is StatementSyntax && node.Kind() != SyntaxKind.Block) ||
                node is TypeSyntax ||
                node is CrefSyntax ||
                node.Kind() == SyntaxKind.Attribute ||
                node.Kind() == SyntaxKind.ThisConstructorInitializer ||
                node.Kind() == SyntaxKind.BaseConstructorInitializer ||
                node.Kind() == SyntaxKind.EqualsValueClause ||
                node.Kind() == SyntaxKind.ArrowExpressionClause;
        }

    }

}
