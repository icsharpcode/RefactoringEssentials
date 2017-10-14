using System;
using System.Linq;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Threading;

namespace RefactoringEssentials.CSharp.Diagnostics
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class UnusedParameterAnalyzer : DiagnosticAnalyzer
    {
        static readonly DiagnosticDescriptor descriptor = new DiagnosticDescriptor(
            CSharpDiagnosticIDs.UnusedParameterAnalyzerID,
            GettextCatalog.GetString("Parameter is never used"),
            GettextCatalog.GetString("Parameter '{0}' is never used"),
            DiagnosticAnalyzerCategories.RedundanciesInDeclarations,
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            helpLinkUri: HelpLink.CreateFor(CSharpDiagnosticIDs.UnusedParameterAnalyzerID)
        );

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(descriptor);

        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.RegisterCompilationStartAction(nodeContext => Analyze(nodeContext));
        }

        void Analyze(CompilationStartAnalysisContext compilationContext)
        {
            var compilation = compilationContext.Compilation;
            compilationContext.RegisterSyntaxTreeAction(delegate (SyntaxTreeAnalysisContext context) {
                try {
                    if (!compilation.SyntaxTrees.Contains(context.Tree))
                        return;
                    var semanticModel = compilation.GetSemanticModel(context.Tree);
                    var root = context.Tree.GetRoot(context.CancellationToken);
                    var model = compilationContext.Compilation.GetSemanticModel(context.Tree);
                    if (model.IsFromGeneratedCode(compilationContext.CancellationToken))
                        return;
                    var usageVisitor = new GetDelgateUsagesVisitor(semanticModel, context.CancellationToken);
                    usageVisitor.Visit(root);

                    var analyzer = new NodeAnalyzer(context, model, usageVisitor);
                    analyzer.Visit(root);
                } catch (OperationCanceledException) { }
            });
        }

        // Collect all methods that are used as delegate
        class GetDelgateUsagesVisitor : CSharpSyntaxWalker
        {
            SemanticModel ctx;
            public readonly List<ISymbol> UsedMethods = new List<ISymbol>();
            readonly CancellationToken token;

            public GetDelgateUsagesVisitor(SemanticModel ctx, CancellationToken token)
            {
                this.token = token;
                this.ctx = ctx;
            }

            public override void VisitBlock(BlockSyntax node)
            {
                token.ThrowIfCancellationRequested();
                base.VisitBlock(node);
            }

            public override void VisitIdentifierName(IdentifierNameSyntax node)
            {
                base.VisitIdentifierName(node);
                if (!IsTargetOfInvocation(node)) {
                    var mgr = ctx.GetSymbolInfo(node);
                    if (mgr.Symbol?.IsKind(SymbolKind.Method) == true)
                        UsedMethods.Add(mgr.Symbol);
                }
            }

            public override void VisitMemberAccessExpression(MemberAccessExpressionSyntax node)
            {
                base.VisitMemberAccessExpression(node);
                if (!IsTargetOfInvocation(node)) {
                    var mgr = ctx.GetSymbolInfo(node);
                    if (mgr.Symbol?.IsKind(SymbolKind.Method) == true)
                        UsedMethods.Add(mgr.Symbol);
                }
            }

            static bool IsTargetOfInvocation(SyntaxNode node)
            {
                return node.Parent is InvocationExpressionSyntax;
            }
        }

        class NodeAnalyzer : CSharpSyntaxWalker
        {
            readonly SyntaxTreeAnalysisContext ctx;
            readonly SemanticModel model;
            readonly GetDelgateUsagesVisitor visitor;

            bool currentTypeIsPartial;

            public NodeAnalyzer(SyntaxTreeAnalysisContext ctx, SemanticModel model, GetDelgateUsagesVisitor visitor)
            {
                this.ctx = ctx;
                this.model = model;
                this.visitor = visitor;
            }

            public override void Visit(SyntaxNode node)
            {
                base.Visit(node);
            }

            public override void VisitBlock(BlockSyntax node)
            {
                ctx.CancellationToken.ThrowIfCancellationRequested();
                base.VisitBlock(node);
            }

            public override void VisitClassDeclaration(ClassDeclarationSyntax node)
            {
                bool outerTypeIsPartial = currentTypeIsPartial;
                currentTypeIsPartial = node.Modifiers.Any(m => m.IsKind(SyntaxKind.PartialKeyword));
                base.VisitClassDeclaration(node);
                currentTypeIsPartial = outerTypeIsPartial;
            }

            static INamedTypeSymbol GetImplementingInterface(ISymbol enclosingSymbol, INamedTypeSymbol containingType)
            {
                INamedTypeSymbol result = null;
                foreach (var iface in containingType.AllInterfaces) {
                    foreach (var member in iface.GetMembers()) {
                        var implementation = containingType.FindImplementationForInterfaceMember(member);
                        if (implementation == enclosingSymbol) {
                            if (result != null)
                                return null;
                            result = iface;
                        }
                    }
                }
                return result;
            }


            public override void VisitMethodDeclaration(MethodDeclarationSyntax node)
            {
                base.VisitMethodDeclaration(node);

                if (node.Modifiers.Any(m => m.IsKind(SyntaxKind.VirtualKeyword) || m.IsKind(SyntaxKind.NewKeyword) || m.IsKind(SyntaxKind.PartialKeyword)))
                    return;
                if ((node.Body == null) && (node.ExpressionBody == null))
                    return;

                var member = model.GetDeclaredSymbol(node);
                if (member.IsOverride)
                    return;
                if (member.ExplicitInterfaceImplementations().Length > 0)
                    return;
                if (GetImplementingInterface(member, member.ContainingType) != null)
                    return;

                foreach (var attr in member.GetAttributes()) {
                    if (attr.AttributeClass.Name == "ExportAttribute")
                        return;
                }

                if (visitor.UsedMethods.Contains(member))
                    return;
                if (currentTypeIsPartial && member.Parameters.Length == 2) {
                    if (member.Parameters[0].Name == "sender") {
                        // Looks like an event handler; the registration might be in the designer part
                        return;
                    }
                }

                Analyze(node.ParameterList.Parameters, new SyntaxNode[] { node.Body, node.ExpressionBody }, node.Kind());
            }

            public override void VisitConstructorDeclaration(ConstructorDeclarationSyntax node)
            {
                base.VisitConstructorDeclaration(node);
                if (node.ParameterList.Parameters.Count == 0)
                    return;

                Analyze(node.ParameterList.Parameters, new SyntaxNode[] { node.Body, node.Initializer }, node.Kind());
            }

            public override void VisitAnonymousMethodExpression(AnonymousMethodExpressionSyntax node)
            {
                base.VisitAnonymousMethodExpression(node);
                if (node.ParameterList != null)
                    Analyze(node.ParameterList.Parameters, new[] { node.Block }, node.Kind());
            }

            public override void VisitIndexerDeclaration(IndexerDeclarationSyntax node)
            {
                base.VisitIndexerDeclaration(node);
                if (node.Modifiers.Any(m => m.IsKind(SyntaxKind.VirtualKeyword) || m.IsKind(SyntaxKind.NewKeyword) || m.IsKind(SyntaxKind.PartialKeyword)))
                    return;
                if (node.GetBodies().IsEmpty())
                    return;
                var member = model.GetDeclaredSymbol(node);
                if (member.IsOverride)
                    return;

                if (member.ExplicitInterfaceImplementations().Length > 0)
                    return;
                if (GetImplementingInterface(member, member.ContainingType) != null)
                    return;
                Analyze(node.ParameterList.Parameters, node.GetBodies(), node.Kind());
            }

            void Analyze(SeparatedSyntaxList<ParameterSyntax> parameterList, IEnumerable<SyntaxNode> nodesToAnalyze, SyntaxKind containerKind)
            {
                var parameters = new List<IParameterSymbol>();
                var parameterNodes = new List<ParameterSyntax>();
                foreach (var param in parameterList) {
                    var resolveResult = model.GetDeclaredSymbol(param);
                    if (resolveResult == null)
                        continue;
                    if (containerKind == SyntaxKind.ConstructorDeclaration) {
                        if (resolveResult.Type.Name == "StreamingContext" && resolveResult.Type.ContainingNamespace.ToDisplayString() == "System.Runtime.Serialization") {
                            // commonly unused parameter in constructors associated with ISerializable
                            return;
                        }
                    }
                    parameters.Add(resolveResult);
                    parameterNodes.Add(param);
                }
                foreach (var node in nodesToAnalyze) {
                    if (node == null)
                        continue;
                    foreach (var child in node.DescendantNodes()) {
                        var identifierNameSyntax = child as IdentifierNameSyntax;
                        if (identifierNameSyntax == null) continue;
                        var sym = model.GetSymbolInfo(identifierNameSyntax).Symbol as IParameterSymbol;
                        if (sym == null || sym.Ordinal < 0) continue;
                        int idx = parameters.IndexOf(param => param.Ordinal == sym.Ordinal);
                        if (idx < 0) continue;
                        if (sym.GetContainingMemberOrThis() == parameters[idx].GetContainingMemberOrThis()) {
                            parameters.RemoveAt(idx);
                            parameterNodes.RemoveAt(idx);
                            if (parameters.Count == 0)
                                return;
                        }
                    }
                }
                foreach (var param in parameterNodes) {
                    ctx.ReportDiagnostic(Diagnostic.Create(descriptor, param.Identifier.GetLocation(), param.Identifier.ValueText));
                }
            }
        }
    }
}
