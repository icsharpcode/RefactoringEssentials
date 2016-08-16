using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RefactoringEssentials.CSharp.Diagnostics
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class StaticFieldOrAutoPropertyInGenericTypeAnalyzer : DiagnosticAnalyzer
    {
        static readonly DiagnosticDescriptor descriptor = new DiagnosticDescriptor(
            CSharpDiagnosticIDs.StaticFieldInGenericTypeAnalyzerID,
            GettextCatalog.GetString("Warns about static fields in generic types"),
            GettextCatalog.GetString("Static field in generic type"),
            DiagnosticAnalyzerCategories.CodeQualityIssues,
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            helpLinkUri: HelpLink.CreateFor(CSharpDiagnosticIDs.StaticFieldInGenericTypeAnalyzerID)
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
            SyntaxTreeAnalysisContext ctx;
            SemanticModel semanticModel;

            IList<ITypeParameterSymbol> availableTypeParameters = new List<ITypeParameterSymbol>();

            public GatherVisitor(SyntaxTreeAnalysisContext ctx, SemanticModel semanticModel)
            {
                this.ctx = ctx;
                this.semanticModel = semanticModel;
            }

            public override void VisitBlock(Microsoft.CodeAnalysis.CSharp.Syntax.BlockSyntax node)
            {
                ctx.CancellationToken.ThrowIfCancellationRequested();
            }

            public override void VisitClassDeclaration(Microsoft.CodeAnalysis.CSharp.Syntax.ClassDeclarationSyntax node)
            {
                var type = semanticModel.GetDeclaredSymbol(node) as INamedTypeSymbol;
                var oldTypeParameters = availableTypeParameters; 
                if (type != null)
                {
                    availableTypeParameters = Concat(availableTypeParameters, type.TypeParameters);
                }
                base.VisitClassDeclaration(node);
                availableTypeParameters = oldTypeParameters;
            }

            public override void VisitStructDeclaration(Microsoft.CodeAnalysis.CSharp.Syntax.StructDeclarationSyntax node)
            {
                var type = semanticModel.GetDeclaredSymbol(node) as INamedTypeSymbol;
                var oldTypeParameters = availableTypeParameters; 
                if (type != null)
                {
                    availableTypeParameters = Concat(availableTypeParameters, type.TypeParameters);
                }
                base.VisitStructDeclaration(node);
                availableTypeParameters = oldTypeParameters;
            }
            static IList<ITypeParameterSymbol> Concat(params IList<ITypeParameterSymbol>[] lists)
            {
                return lists.SelectMany(l => l).ToList();
            }

            public override void VisitEnumDeclaration(Microsoft.CodeAnalysis.CSharp.Syntax.EnumDeclarationSyntax node)
            {
            }

            public override void VisitInterfaceDeclaration(Microsoft.CodeAnalysis.CSharp.Syntax.InterfaceDeclarationSyntax node)
            {
            }

            public override void VisitDelegateDeclaration(Microsoft.CodeAnalysis.CSharp.Syntax.DelegateDeclarationSyntax node)
            {
            }


            bool UsesAllTypeParameters(TypeSyntax type)
			{
                if (type == null)
                    return false;
				if (availableTypeParameters.Count == 0)
					return true;

                var fieldType = semanticModel.GetTypeInfo(type).Type;
				if (fieldType == null)
					return false;

				// Check that all current type parameters are used in the field type
                var fieldTypeParameters = fieldType.GetAllTypeArguments();
                foreach (var typeParameter in availableTypeParameters) {
                    if (!fieldTypeParameters.Contains(typeParameter))
						return false;
				}
				return true;
			}
            
            public override void VisitFieldDeclaration(FieldDeclarationSyntax node)
            {
                base.VisitFieldDeclaration(node);
                if (node.Modifiers.Any(m => m.IsKind(SyntaxKind.StaticKeyword)) && !UsesAllTypeParameters(node.Declaration.Type))
                {
                    foreach (var v in node.Declaration.Variables)
                    {
                        ctx.ReportDiagnostic(Diagnostic.Create(descriptor, v.Identifier.GetLocation()));
                    }
                }
            }

            public override void VisitPropertyDeclaration(PropertyDeclarationSyntax node)
            {
                base.VisitPropertyDeclaration(node);
                if (node.AccessorList?.Accessors.Count == 2 &&
                    node.Modifiers.Any(m => m.IsKind(SyntaxKind.StaticKeyword)) && !UsesAllTypeParameters(node.Type) && 
                    node.AccessorList.Accessors[0].Body == null && node.AccessorList.Accessors[1].Body == null) {
                    ctx.ReportDiagnostic(Diagnostic.Create(descriptor, node.Identifier.GetLocation()));
                }
            }
        }
    }
}