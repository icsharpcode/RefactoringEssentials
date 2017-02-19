using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using RefactoringEssentials.Util.Analysis;

namespace RefactoringEssentials.CSharp.Diagnostics
{
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class FunctionNeverReturnsAnalyzer : DiagnosticAnalyzer
    {
        static readonly DiagnosticDescriptor descriptor = new DiagnosticDescriptor(
            CSharpDiagnosticIDs.FunctionNeverReturnsAnalyzerID,
            GettextCatalog.GetString("Function does not reach its end or a 'return' statement by any of possible execution paths"),
            GettextCatalog.GetString("{0} never reaches end or a 'return' statement"),
            DiagnosticAnalyzerCategories.CodeQualityIssues,
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            helpLinkUri: HelpLink.CreateFor(CSharpDiagnosticIDs.FunctionNeverReturnsAnalyzerID)
        );

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(descriptor);

        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.RegisterCompilationStartAction(compilationContext =>
            {
                var compilation = compilationContext.Compilation;
                compilationContext.RegisterSyntaxTreeAction(delegate (SyntaxTreeAnalysisContext ctx)
                {
                    try
                    {
                        if (!compilation.SyntaxTrees.Contains(ctx.Tree))
                            return;
                        var semanticModel = compilation.GetSemanticModel(ctx.Tree);
                        var root = ctx.Tree.GetRoot(ctx.CancellationToken);
                        var model = compilationContext.Compilation.GetSemanticModel(ctx.Tree);
                        new GatherVisitor(ctx, semanticModel).Visit(root);
                    }
                    catch (OperationCanceledException) { }
                });
            });
        }

        class GatherVisitor : CSharpSyntaxWalker
        {
            readonly SyntaxTreeAnalysisContext context;
            readonly SemanticModel semanticModel;

            public GatherVisitor(SyntaxTreeAnalysisContext context, SemanticModel semanticModel)
            {
                this.context = context;
                this.semanticModel = semanticModel;
            }

            public override void VisitBlock(Microsoft.CodeAnalysis.CSharp.Syntax.BlockSyntax node)
            {
                context.CancellationToken.ThrowIfCancellationRequested();
                base.VisitBlock(node);
            }

            public override void VisitInterfaceDeclaration(Microsoft.CodeAnalysis.CSharp.Syntax.InterfaceDeclarationSyntax node)
            {
                // nothing to analyze here
            }

            public override void VisitMethodDeclaration(Microsoft.CodeAnalysis.CSharp.Syntax.MethodDeclarationSyntax node)
            {
                base.VisitMethodDeclaration(node);
                var body = node.Body;

                // partial/abstract method
                if (body == null)
                    return;
                VisitBody("Method", node.Identifier, body, semanticModel.GetDeclaredSymbol(node));
            }

            public override void VisitAnonymousMethodExpression(Microsoft.CodeAnalysis.CSharp.Syntax.AnonymousMethodExpressionSyntax node)
            {
                base.VisitAnonymousMethodExpression(node);
                VisitBody("Delegate", node.DelegateKeyword, node.Body as StatementSyntax, null);

            }

            public override void VisitSimpleLambdaExpression(Microsoft.CodeAnalysis.CSharp.Syntax.SimpleLambdaExpressionSyntax node)
            {
                base.VisitSimpleLambdaExpression(node);
                VisitBody("Lambda expression", node.ArrowToken, node.Body as StatementSyntax, null);
            }

            public override void VisitParenthesizedLambdaExpression(Microsoft.CodeAnalysis.CSharp.Syntax.ParenthesizedLambdaExpressionSyntax node)
            {
                base.VisitParenthesizedLambdaExpression(node);
                VisitBody("Lambda expression", node.ArrowToken, node.Body as StatementSyntax, null);
            }

            public override void VisitAccessorDeclaration(Microsoft.CodeAnalysis.CSharp.Syntax.AccessorDeclarationSyntax node)
            {
                base.VisitAccessorDeclaration(node);
                if (node.Body == null)
                    return;
                VisitBody("Accessor", node.Keyword, node.Body, semanticModel.GetDeclaredSymbol(node.Parent.Parent), node.Kind());
            }

            void VisitBody(string entityType, SyntaxToken markerToken, StatementSyntax body, ISymbol symbol, SyntaxKind accessorKind = SyntaxKind.UnknownAccessorDeclaration)
            {
                if (body == null)
                    return;
                var recursiveDetector = new RecursiveDetector(semanticModel, symbol, accessorKind);
                var reachability = ReachabilityAnalysis.Create((StatementSyntax)body, this.semanticModel, recursiveDetector, context.CancellationToken);
                bool hasReachableReturn = false;
                foreach (var statement in reachability.ReachableStatements)
                {
                    if (statement.IsKind(SyntaxKind.ReturnStatement) || statement.IsKind(SyntaxKind.ThrowStatement) || statement.IsKind(SyntaxKind.YieldBreakStatement))
                    {
                        if (!recursiveDetector.Visit(statement))
                        {
                            hasReachableReturn = true;
                            break;
                        }
                    }
                }
                if (!hasReachableReturn && !reachability.IsEndpointReachable(body))
                {
                    context.ReportDiagnostic(Diagnostic.Create(
                        descriptor,
                        markerToken.GetLocation(),
                        entityType
                    ));
                }
            }


            class RecursiveDetector : ReachabilityAnalysis.RecursiveDetectorVisitor
            {
                SemanticModel ctx;
                ISymbol member;
                SyntaxKind accessorRole;

                internal RecursiveDetector(SemanticModel ctx, ISymbol member, SyntaxKind accessorRole)
                {
                    this.ctx = ctx;
                    this.member = member;
                    this.accessorRole = accessorRole;
                }

                public override bool DefaultVisit(SyntaxNode node)
                {
                    foreach (var child in node.ChildNodes())
                    {
                        if (Visit(child))
                            return true;
                    }
                    return false;
                }

                public override bool VisitBinaryExpression(BinaryExpressionSyntax node)
                {
                    switch (node.Kind())
                    {
                        case SyntaxKind.LogicalAndExpression:
                        case SyntaxKind.LogicalOrExpression:
                            return Visit(node.Left);
                    }
                    return base.VisitBinaryExpression(node);
                }

                public override bool VisitAssignmentExpression(AssignmentExpressionSyntax node)
                {
                    if (accessorRole != SyntaxKind.UnknownAccessorDeclaration)
                    {
                        if (accessorRole == SyntaxKind.SetAccessorDeclaration)
                        {
                            return Visit(node.Left);
                        }
                        return Visit(node.Right);
                    }
                    return base.VisitAssignmentExpression(node);
                }

                public override bool VisitAnonymousMethodExpression(AnonymousMethodExpressionSyntax node)
                {
                    return false;
                }

                public override bool VisitSimpleLambdaExpression(SimpleLambdaExpressionSyntax node)
                {
                    return false;
                }

                public override bool VisitParenthesizedLambdaExpression(ParenthesizedLambdaExpressionSyntax node)
                {
                    return false;
                }

                public override bool VisitIdentifierName(IdentifierNameSyntax node)
                {
                    return CheckRecursion((SyntaxNode)(node.Parent as MemberAccessExpressionSyntax) ?? node);
                }


                public override bool VisitMemberAccessExpression(MemberAccessExpressionSyntax node)
                {
                    if (base.VisitMemberAccessExpression(node))
                        return true;
                    return CheckRecursion(node);
                }

                public override bool VisitInvocationExpression(InvocationExpressionSyntax node)
                {
                    // There seems to be no better way to detect a "nameof" expression
                    var invocationIdentifier = node.ChildNodes().OfType<IdentifierNameSyntax>().FirstOrDefault();
                    if ((invocationIdentifier != null) && (invocationIdentifier.Identifier.ValueText == "nameof"))
                        return false;

                    if (base.VisitInvocationExpression(node))
                        return true;
                    return CheckRecursion(node);
                }

                bool CheckRecursion(SyntaxNode node)
                {
                    if (member == null)
                    {
                        return false;
                    }

                    var resolveResult = ctx.GetSymbolInfo(node);

                    //We'll ignore Method groups here
                    //If the invocation expressions will be dealt with later anyway
                    //and properties are never in "method groups".
                    var memberResolveResult = resolveResult.Symbol;
                    if (memberResolveResult == null || memberResolveResult != this.member)
                    {
                        return false;
                    }

                    // Exit on method groups
                    if (memberResolveResult.IsKind(SymbolKind.Method))
                    {
                        var invocation = (node as InvocationExpressionSyntax) ?? (node.Parent as InvocationExpressionSyntax);
                        if (invocation == null)
                            return false;
                        var memberAccExpr = (invocation.Expression ?? node) as MemberAccessExpressionSyntax;
                        if (memberAccExpr != null)
                        {
                            if (!memberAccExpr.Expression.IsKind(SyntaxKind.ThisExpression))
                                return false;
                        }
                    }

                    //Now check for virtuals
                    if (memberResolveResult.IsVirtual && !memberResolveResult.ContainingType.IsSealed)
                    {
                        return false;
                    }

                    var parentAssignment = node.Parent as AssignmentExpressionSyntax;
                    if (parentAssignment != null)
                    {
                        if (accessorRole == SyntaxKind.AddAccessorDeclaration)
                        {
                            return parentAssignment.IsKind(SyntaxKind.AddAssignmentExpression);
                        }
                        if (accessorRole == SyntaxKind.RemoveAccessorDeclaration)
                        {
                            return parentAssignment.IsKind(SyntaxKind.SubtractAssignmentExpression);
                        }
                        if (accessorRole == SyntaxKind.GetAccessorDeclaration)
                        {
                            return !parentAssignment.IsKind(SyntaxKind.SimpleAssignmentExpression);
                        }

                        return true;
                    }

                    if (node.Parent.IsKind(SyntaxKind.PreIncrementExpression) ||
                        node.Parent.IsKind(SyntaxKind.PreDecrementExpression) ||
                        node.Parent.IsKind(SyntaxKind.PostIncrementExpression) ||
                        node.Parent.IsKind(SyntaxKind.PostDecrementExpression))
                    {

                        return true;
                    }

                    return accessorRole == SyntaxKind.UnknownAccessorDeclaration || accessorRole == SyntaxKind.GetAccessorDeclaration;
                }
            }
        }
    }
}