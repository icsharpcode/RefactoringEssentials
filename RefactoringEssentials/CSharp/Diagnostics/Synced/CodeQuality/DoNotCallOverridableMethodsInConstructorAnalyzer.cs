using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;

namespace RefactoringEssentials.CSharp.Diagnostics
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class DoNotCallOverridableMethodsInConstructorAnalyzer : DiagnosticAnalyzer
    {
        static readonly DiagnosticDescriptor descriptor = new DiagnosticDescriptor(
            CSharpDiagnosticIDs.DoNotCallOverridableMethodsInConstructorAnalyzerID,
            GettextCatalog.GetString("Warns about calls to virtual member functions occuring in the constructor"),
            GettextCatalog.GetString("Virtual member call in constructor"),
            DiagnosticAnalyzerCategories.CodeQualityIssues,
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            helpLinkUri: HelpLink.CreateFor(CSharpDiagnosticIDs.DoNotCallOverridableMethodsInConstructorAnalyzerID)
        );

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(descriptor);

        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.RegisterSyntaxNodeAction(
                (nodeContext) =>
                {
                    IEnumerable<Diagnostic> diagnostics;
                    if (TryGetDiagnostic(nodeContext, out diagnostics))
                        foreach (var diagnostic in diagnostics)
                            nodeContext.ReportDiagnostic(diagnostic);
                },
                SyntaxKind.ConstructorDeclaration
            );
        }

        static bool TryGetDiagnostic(SyntaxNodeAnalysisContext nodeContext, out IEnumerable<Diagnostic> diagnostic)
        {
            diagnostic = default(IEnumerable<Diagnostic>);

            var node = nodeContext.Node as ConstructorDeclarationSyntax;
            var type = node.Parent as TypeDeclarationSyntax;
            if (type == null || type.Modifiers.Any(m => m.IsKind(SyntaxKind.SealedKeyword) || m.IsKind(SyntaxKind.StaticKeyword)))
            {
                return false;
            }
            var visitor = new VirtualCallFinderVisitor(nodeContext, node, type);
            visitor.Visit(node.Body);
            diagnostic = visitor.Diagnostics;
            return true;
        }


        class VirtualCallFinderVisitor : CSharpSyntaxWalker
        {
            readonly SyntaxNodeAnalysisContext nodeContext;
            internal List<Diagnostic> Diagnostics = new List<Diagnostic>();
            //ConstructorDeclarationSyntax node;
            TypeDeclarationSyntax type;

            public VirtualCallFinderVisitor(SyntaxNodeAnalysisContext nodeContext, ConstructorDeclarationSyntax node, TypeDeclarationSyntax type)
            {
                this.nodeContext = nodeContext;
                //this.node = node;
                this.type = type;
            }

            void Check(SyntaxNode n, bool skipMethods)
            {
                var info = nodeContext.SemanticModel.GetSymbolInfo(n);
                var symbol = info.Symbol;
                if ((symbol == null) || (symbol.ContainingType == null) || symbol.ContainingType.Locations.Where(loc => loc.IsInSource && loc.SourceTree.FilePath == type.SyntaxTree.FilePath).All(loc => !type.Span.Contains(loc.SourceSpan)))
                    return;
                if (symbol is ITypeSymbol)
                    return;
                if (skipMethods && (symbol.Kind == SymbolKind.Method))
                    return;
                if (!symbol.IsSealed && (symbol.IsVirtual || symbol.IsAbstract || symbol.IsOverride))
                {
                    if (symbol.Kind == SymbolKind.Property)
                    {
                        var propertySymbol = symbol as IPropertySymbol;
                        if (propertySymbol != null)
                        {
                            if (n.Ancestors().Any(a => a is AssignmentExpressionSyntax))
                            {
                                var setterMethodSymbol = propertySymbol.SetMethod;
                                if ((setterMethodSymbol != null) && (setterMethodSymbol.DeclaredAccessibility == Accessibility.Private))
                                    return;
                            }
                            else
                            {
                                var getterMethodSymbol = propertySymbol.GetMethod;
                                if ((getterMethodSymbol != null) && (getterMethodSymbol.DeclaredAccessibility == Accessibility.Private))
                                    return;
                            }
                        }
                        else
                        {
                            return;
                        }
                    }
                    Diagnostics.Add(Diagnostic.Create(descriptor, n.GetLocation()));
                }
            }

            public override void VisitMemberAccessExpression(MemberAccessExpressionSyntax node)
            {
                base.VisitMemberAccessExpression(node);
                if (node.Parent is MemberAccessExpressionSyntax || node.Parent is InvocationExpressionSyntax)
                    return;
                if (node.Expression.IsKind(SyntaxKind.ThisExpression))
                    Check(node, true);
            }

            public override void VisitIdentifierName(IdentifierNameSyntax node)
            {
                base.VisitIdentifierName(node);
                var ancestors = node.Ancestors();
                if (ancestors.Any(n => (n is MemberAccessExpressionSyntax) || (n is InvocationExpressionSyntax)))
                    return;

                Check(node, true);
            }

            static bool IsSimpleThisCall(ExpressionSyntax expression)
            {
                var ma = expression as MemberAccessExpressionSyntax;
                if (ma == null)
                    return false;
                return ma.Expression.IsKind(SyntaxKind.ThisExpression);
            }

            public override void VisitInvocationExpression(InvocationExpressionSyntax node)
            {
                base.VisitInvocationExpression(node);
                if (node.Parent is MemberAccessExpressionSyntax || node.Parent is InvocationExpressionSyntax)
                    return;
                if (node.Expression.IsKind(SyntaxKind.IdentifierName) || IsSimpleThisCall(node.Expression))
                    Check(node, false);
            }

            public override void VisitParenthesizedLambdaExpression(ParenthesizedLambdaExpressionSyntax node)
            {
                // ignore lambdas
            }

            public override void VisitSimpleLambdaExpression(SimpleLambdaExpressionSyntax node)
            {
                // ignore lambdas
            }

            public override void VisitAnonymousMethodExpression(AnonymousMethodExpressionSyntax node)
            {
                // ignore anonymous methods
            }
        }
    }
}