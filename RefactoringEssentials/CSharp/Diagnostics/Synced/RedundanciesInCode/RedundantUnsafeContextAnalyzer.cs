using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;

namespace RefactoringEssentials.CSharp.Diagnostics
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class RedundantUnsafeContextAnalyzer : DiagnosticAnalyzer
    {
        static readonly DiagnosticDescriptor descriptor = new DiagnosticDescriptor(
            CSharpDiagnosticIDs.RedundantUnsafeContextAnalyzerID,
            GettextCatalog.GetString("Unsafe modifier in redundant in unsafe context or when no unsafe constructs are used"),
            GettextCatalog.GetString("'unsafe' modifier is redundant"),
            DiagnosticAnalyzerCategories.RedundanciesInCode,
            DiagnosticSeverity.Info,
            isEnabledByDefault: true,
            helpLinkUri: HelpLink.CreateFor(CSharpDiagnosticIDs.RedundantUnsafeContextAnalyzerID),
            customTags: DiagnosticCustomTags.Unnecessary
        );

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(descriptor);


        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.RegisterCompilationStartAction(compilationContext =>
            {
                var compilation = compilationContext.Compilation;
                compilationContext.RegisterSyntaxTreeAction(async delegate (SyntaxTreeAnalysisContext ctx)
                {
                    try
                    {
                        if (!compilation.SyntaxTrees.Contains(ctx.Tree))
                            return;
                        var semanticModel = compilation.GetSemanticModel(ctx.Tree);
                        var root = await ctx.Tree.GetRootAsync(ctx.CancellationToken).ConfigureAwait(false);
                        var model = compilationContext.Compilation.GetSemanticModel(ctx.Tree);
                        if (model.IsFromGeneratedCode(compilationContext.CancellationToken))
                            return;
                        new GatherVisitor(ctx).Visit(root);
                    }
                    catch (OperationCanceledException) { }
                });
            });
        }

        class GatherVisitor : CSharpSyntaxWalker
        {
            SyntaxTreeAnalysisContext ctx;

            public GatherVisitor(SyntaxTreeAnalysisContext ctx)
            {
                this.ctx = ctx;
            }

            class UnsafeState
            {
                public bool InUnsafeContext;
                public bool UseUnsafeConstructs;

                public UnsafeState(bool inUnsafeContext)
                {
                    this.InUnsafeContext = inUnsafeContext;
                    this.UseUnsafeConstructs = false;
                }

                public override string ToString()
                {
                    return string.Format("[UnsafeState: InUnsafeContext={0}, UseUnsafeConstructs={1}]", InUnsafeContext, UseUnsafeConstructs);
                }
            }

            readonly Stack<UnsafeState> unsafeStateStack = new Stack<UnsafeState>();


            void MarkUnsafe(SyntaxTokenList modifiers, bool isUnsafe)
            {
                var state = unsafeStateStack.Pop();
                if (isUnsafe && !state.UseUnsafeConstructs)
                {
                    ctx.ReportDiagnostic(Diagnostic.Create(
                        descriptor,
                        modifiers.First(m => m.IsKind(SyntaxKind.UnsafeKeyword)).GetLocation()
                    ));
                }
            }

            bool CheckModifiers(SyntaxTokenList modifiers)
            {
                var isUnsafe = modifiers.Any(m => m.IsKind(SyntaxKind.UnsafeKeyword));
                if (unsafeStateStack.Count > 0)
                {
                    var curState = unsafeStateStack.Peek();
                    unsafeStateStack.Push(new UnsafeState(curState.InUnsafeContext));
                }
                else {
                    unsafeStateStack.Push(new UnsafeState(isUnsafe));
                }
                return isUnsafe;
            }


            void MarkUnsafe()
            {
                if (unsafeStateStack.Count == 0)
                    return;
                unsafeStateStack.Peek().UseUnsafeConstructs = true;
            }


            public override void VisitClassDeclaration(ClassDeclarationSyntax node)
            {
                bool isUnsafe = CheckModifiers(node.Modifiers);
                base.VisitClassDeclaration(node);
                MarkUnsafe(node.Modifiers, isUnsafe);
            }

            public override void VisitStructDeclaration(StructDeclarationSyntax node)
            {
                bool isUnsafe = CheckModifiers(node.Modifiers);
                base.VisitStructDeclaration(node);
                MarkUnsafe(node.Modifiers, isUnsafe);
            }

            public override void VisitFieldDeclaration(FieldDeclarationSyntax node)
            {
                base.VisitFieldDeclaration(node);
                MarkUnsafe();
            }

            public override void VisitPointerType(PointerTypeSyntax node)
            {
                base.VisitPointerType(node);
                MarkUnsafe();
            }


            public override void VisitFixedStatement(FixedStatementSyntax node)
            {
                base.VisitFixedStatement(node);
                MarkUnsafe();
            }

            public override void VisitSizeOfExpression(SizeOfExpressionSyntax node)
            {
                base.VisitSizeOfExpression(node);
                MarkUnsafe();
            }

            public override void VisitPrefixUnaryExpression(PrefixUnaryExpressionSyntax node)
            {
                base.VisitPrefixUnaryExpression(node);
                if (node.IsKind(SyntaxKind.AddressOfExpression) || node.IsKind(SyntaxKind.PointerIndirectionExpression))  // TODO: Check
                    MarkUnsafe();
            }

            public override void VisitUnsafeStatement(UnsafeStatementSyntax node)
            {
                MarkUnsafe();
                bool isRedundant = unsafeStateStack.Peek().InUnsafeContext;
                unsafeStateStack.Push(new UnsafeState(true));
                base.VisitUnsafeStatement(node);
                isRedundant |= !unsafeStateStack.Pop().UseUnsafeConstructs;

                if (isRedundant)
                {
                    ctx.ReportDiagnostic(Diagnostic.Create(
                        descriptor,
                        node.UnsafeKeyword.GetLocation()
                    ));
                }
            }
        }
    }
}