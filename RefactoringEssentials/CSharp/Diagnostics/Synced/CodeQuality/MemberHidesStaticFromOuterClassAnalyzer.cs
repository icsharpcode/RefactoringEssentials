using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Linq;
using System.Collections.Generic;

namespace RefactoringEssentials.CSharp.Diagnostics
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class MemberHidesStaticFromOuterClassAnalyzer : DiagnosticAnalyzer
    {
        static readonly DiagnosticDescriptor descriptor = new DiagnosticDescriptor(
            CSharpDiagnosticIDs.MemberHidesStaticFromOuterClassAnalyzerID,
            GettextCatalog.GetString("Member hides static member from outer class"),
            GettextCatalog.GetString("{0} '{1}' hides {2} from outer class"),
            DiagnosticAnalyzerCategories.CodeQualityIssues,
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            helpLinkUri: HelpLink.CreateFor(CSharpDiagnosticIDs.MemberHidesStaticFromOuterClassAnalyzerID)
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
            readonly List<List<ISymbol>> staticMembers = new List<List<ISymbol>>();
            SyntaxTreeAnalysisContext ctx;
            SemanticModel semanticModel;

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
                staticMembers.Add(new List<ISymbol>(type.GetMembers().Where(m => m.IsStatic)));
                base.VisitClassDeclaration(node);
                staticMembers.RemoveAt(staticMembers.Count - 1); 
            }

            public override void VisitStructDeclaration(Microsoft.CodeAnalysis.CSharp.Syntax.StructDeclarationSyntax node)
            {
                var type = semanticModel.GetDeclaredSymbol(node) as INamedTypeSymbol;
                staticMembers.Add(new List<ISymbol>(type.GetMembers().Where(m => m.IsStatic)));
                base.VisitStructDeclaration(node);
                staticMembers.RemoveAt(staticMembers.Count - 1); 
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

            void Check(string name, SyntaxToken nodeToMark, string memberType)
			{
				for (int i = 0; i < staticMembers.Count - 1; i++) {
					var member = staticMembers[i].FirstOrDefault(m => m.Name == name);
					if (member == null)
						continue;
					string outerMemberType;
                    switch (member.Kind) {
						case SymbolKind.Field:
                            outerMemberType = GettextCatalog.GetString("field");
							break;
						case SymbolKind.Property:
                            outerMemberType = GettextCatalog.GetString("property");
							break;
						case SymbolKind.Event:
                            outerMemberType = GettextCatalog.GetString("event");
							break;
						case SymbolKind.Method:
                            outerMemberType = GettextCatalog.GetString("method");
							break;
						default:
                            outerMemberType = GettextCatalog.GetString("member");
							break;
					}
                    ctx.ReportDiagnostic(Diagnostic.Create(
                        descriptor, 
                        nodeToMark.GetLocation(),
                        memberType, member.Name, outerMemberType
                    ));
					return;
				}
			}

            public override void VisitEventDeclaration(Microsoft.CodeAnalysis.CSharp.Syntax.EventDeclarationSyntax node)
            {
                Check(node.Identifier.ValueText, node.Identifier, GettextCatalog.GetString("Event"));
            }

            public override void VisitEventFieldDeclaration(Microsoft.CodeAnalysis.CSharp.Syntax.EventFieldDeclarationSyntax node)
            {
                foreach (var v in node.Declaration.Variables) {
                    Check(v.Identifier.ValueText, v.Identifier, GettextCatalog.GetString("Event"));
                }
            }

            public override void VisitFieldDeclaration(Microsoft.CodeAnalysis.CSharp.Syntax.FieldDeclarationSyntax node)
            {
                foreach (var v in node.Declaration.Variables) {
                    Check(v.Identifier.ValueText, v.Identifier, GettextCatalog.GetString("Field"));
                }
            }

            public override void VisitPropertyDeclaration(Microsoft.CodeAnalysis.CSharp.Syntax.PropertyDeclarationSyntax node)
            {
                Check(node.Identifier.ValueText, node.Identifier, GettextCatalog.GetString("Property"));
            }

            public override void VisitMethodDeclaration(Microsoft.CodeAnalysis.CSharp.Syntax.MethodDeclarationSyntax node)
            {
                Check(node.Identifier.ValueText, node.Identifier, GettextCatalog.GetString("Method"));
            }
        }
    }
}