using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System;

namespace RefactoringEssentials.CSharp.Diagnostics
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class FieldCanBeMadeReadOnlyAnalyzer : DiagnosticAnalyzer
    {
        static readonly DiagnosticDescriptor descriptor = new DiagnosticDescriptor(
            CSharpDiagnosticIDs.FieldCanBeMadeReadOnlyAnalyzerID,
            GettextCatalog.GetString("Convert field to readonly"),
            GettextCatalog.GetString("Convert field to readonly"),
            DiagnosticAnalyzerCategories.PracticesAndImprovements,
            DiagnosticSeverity.Info,
            isEnabledByDefault: true,
            helpLinkUri: HelpLink.CreateFor(CSharpDiagnosticIDs.FieldCanBeMadeReadOnlyAnalyzerID)
        );

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(descriptor);

        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.RegisterCompilationStartAction(nodeContext => Analyze(nodeContext));
        }

        void Analyze(CompilationStartAnalysisContext compilationContext)
        {
            var compilation = compilationContext.Compilation;
            compilationContext.RegisterSyntaxTreeAction(delegate (SyntaxTreeAnalysisContext context)
            {
                try
                {
                    if (!compilation.SyntaxTrees.Contains(context.Tree))
                        return;
                    var semanticModel = compilation.GetSemanticModel(context.Tree);
                    var root = context.Tree.GetRoot(context.CancellationToken);
                    var model = compilationContext.Compilation.GetSemanticModel(context.Tree);
                    foreach (var type in root.DescendantNodesAndSelf(SkipMembers).OfType<ClassDeclarationSyntax>())
                    {
                        var allMembers = type.GetMembersFromAllParts(model);
                        if (allMembers == null)
                            continue;

                        var fieldDeclarations = type
                            .ChildNodes()
                            .OfType<FieldDeclarationSyntax>()
                            .Where(f => FieldFilter(model, f))
                            .SelectMany(fd => fd.Declaration.Variables.Select(v => new { Field = fd, Variable = v, Symbol = semanticModel.GetDeclaredSymbol(v, context.CancellationToken) }));
                        foreach (var candidateField in fieldDeclarations)
                        {
                            context.CancellationToken.ThrowIfCancellationRequested();
                            // handled by ConvertToConstantIssue
                            if (candidateField?.Variable?.Initializer != null && semanticModel.GetConstantValue(candidateField.Variable.Initializer.Value, context.CancellationToken).HasValue)
                                continue;

                            // user-defined value type -- might be mutable
                            var field = candidateField.Symbol;
                            if (field != null && !field.GetReturnType().IsReferenceType)
                            {
                                if (field.GetReturnType().IsDefinedInSource())
                                {
                                    continue;
                                }
                            }
                            if (field.GetAttributes().Any(ad => ad.AttributeClass.Name == "SerializableAttribute" && ad.AttributeClass.ContainingNamespace.GetFullName() == "System"))
                                continue;
                            bool wasAltered = false;
                            bool wasUsed = false;
                            foreach (var member in allMembers)
                            {
                                if (member == candidateField.Field)
                                    continue;
                                if (IsAltered(model, member, candidateField.Symbol, context.CancellationToken, out wasUsed))
                                {
                                    wasAltered = true;
                                    break;
                                }
                            }
                            if (!wasAltered && wasUsed)
                            {
                                context.CancellationToken.ThrowIfCancellationRequested();
                                context.ReportDiagnostic(Diagnostic.Create(descriptor, candidateField.Variable.Identifier.GetLocation()));
                            }
                        }
                    }
                }
                catch (Exception) { }
            });
        }

        bool IsAltered(SemanticModel model, MemberDeclarationSyntax member, ISymbol symbol, CancellationToken token, out bool wasUsed)
        {
            wasUsed = false;
            foreach (var usage in member.DescendantNodesAndSelf().Where(n => n.IsKind(SyntaxKind.IdentifierName)).OfType<ExpressionSyntax>())
            {
                var info = model.GetSymbolInfo(usage).Symbol;
                if (info == symbol)
                    wasUsed = true;
                if (!usage.IsWrittenTo())
                {
                    // Special case: If variable is of a value type, check if one of its members is altered.
                    var memberAccExpr = usage.Parent as MemberAccessExpressionSyntax;
                    if (symbol.GetReturnType().IsReferenceType
                        || (memberAccExpr == null)
                        || (info != symbol)
                        || (memberAccExpr.Name == usage)
                        || !memberAccExpr.IsWrittenTo())
                        continue;
                }

                if (member.IsKind(SyntaxKind.ConstructorDeclaration) && !usage.Ancestors().Any(a => a.IsKind(SyntaxKind.AnonymousMethodExpression) || a.IsKind(SyntaxKind.SimpleLambdaExpression) || a.IsKind(SyntaxKind.ParenthesizedLambdaExpression)))
                {
                    if (member.GetModifiers().Any(m => m.IsKind(SyntaxKind.StaticKeyword)) == info.IsStatic)
                        continue;
                }
                if (info == symbol)
                    return true;
            }
            return false;
        }

        bool FieldFilter(SemanticModel model, FieldDeclarationSyntax fieldDeclaration)
        {
            if (fieldDeclaration.Modifiers.Any(
                p => p.IsKind(SyntaxKind.ConstKeyword) || p.IsKind(SyntaxKind.ReadOnlyKeyword) ||
                     p.IsKind(SyntaxKind.PublicKeyword) || p.IsKind(SyntaxKind.ProtectedKeyword) || p.IsKind(SyntaxKind.InternalKeyword)))
                return false;

            return true;
        }

        bool SkipMembers(SyntaxNode arg)
        {
            return !arg.IsKind(SyntaxKind.Block);
        }
    }
}