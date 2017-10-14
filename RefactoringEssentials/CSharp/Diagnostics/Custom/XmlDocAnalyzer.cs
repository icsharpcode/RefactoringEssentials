using System;
using System.Linq;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using Microsoft.CodeAnalysis.Text;
using System.Text;
using RefactoringEssentials.Xml;

namespace RefactoringEssentials.CSharp.Diagnostics
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
	public class XmlDocAnalyzer : DiagnosticAnalyzer
    {
        static readonly DiagnosticDescriptor descriptor = new DiagnosticDescriptor(
            CSharpDiagnosticIDs.XmlDocAnalyzerID,
            GettextCatalog.GetString("Validate Xml docs"),
            "{0}",
            DiagnosticAnalyzerCategories.CompilerWarnings,
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            helpLinkUri: HelpLink.CreateFor(CSharpDiagnosticIDs.XmlDocAnalyzerID)
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
            readonly List<DocumentationCommentTriviaSyntax> storedXmlComment = new List<DocumentationCommentTriviaSyntax>();
            readonly SyntaxTreeAnalysisContext context;
            readonly StringBuilder xml = new StringBuilder();
            readonly SemanticModel semanticModel;

            public GatherVisitor(SyntaxTreeAnalysisContext context, SemanticModel semanticModel)
            {
                this.context = context;
                this.semanticModel = semanticModel;
            }

            public override void VisitDocumentationCommentTrivia(DocumentationCommentTriviaSyntax node)
            {
                storedXmlComment.Add(node);
            }

            void AddXmlIssue(int offset, int length, string str)
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    descriptor,
                    Location .Create(context.Tree, new TextSpan(offset, length)),
                    str
                ));
            }

            void CheckForInvalid(SyntaxNode node)
            {
                context.CancellationToken.ThrowIfCancellationRequested();
                foreach (var triva in node.GetLeadingTrivia())
                {
                    if (triva.IsKind(SyntaxKind.SingleLineDocumentationCommentTrivia))
                        storedXmlComment.Add((DocumentationCommentTriviaSyntax)triva.GetStructure());
                }

                if (storedXmlComment.Count == 0)
                    return;

                context.ReportDiagnostic(Diagnostic.Create(
                    descriptor,
                    Location .Create(context.Tree, storedXmlComment[0].FullSpan),
                    storedXmlComment.Skip(1).Select(cmt => Location .Create(context.Tree, cmt.FullSpan)),
                    GettextCatalog.GetString("Xml comment is not placed before a valid language element")
                ));
                storedXmlComment.Clear();
            }

            public override void VisitNamespaceDeclaration(NamespaceDeclarationSyntax node)
            {
                CheckForInvalid(node);
                base.VisitNamespaceDeclaration(node);
            }

            public override void VisitUsingDirective(UsingDirectiveSyntax node)
            {
                CheckForInvalid(node);
                base.VisitUsingDirective(node);
            }

            public override void VisitExternAliasDirective(ExternAliasDirectiveSyntax node)
            {
                CheckForInvalid(node);
                base.VisitExternAliasDirective(node);
            }

            const string firstline = "<root>\n";

            class CRefVisistor : CSharpSyntaxWalker
            {
                readonly GatherVisitor parent;

                public CRefVisistor(GatherVisitor parent)
                {
                    this.parent = parent;
                }

                public override void Visit(SyntaxNode node)
                {
                    base.Visit(node);
                }

                public override void VisitNameMemberCref(NameMemberCrefSyntax node)
                {
                    base.VisitNameMemberCref(node);
                    var sym = parent.semanticModel.GetSymbolInfo(node).Symbol;
                    if (sym == null)
                        parent.AddXmlIssue(node.Span.Start, node.Span.Length, string.Format(GettextCatalog.GetString("Cannot find reference '{0}'"), node.Name));
                }
            }

            void CheckXmlDocForErrors(SyntaxNode node, ISymbol member)
            {
                context.CancellationToken.ThrowIfCancellationRequested();

                foreach (var triva in node.GetLeadingTrivia())
                {
                    if (triva.IsKind(SyntaxKind.SingleLineDocumentationCommentTrivia))
                    {
                        storedXmlComment.Add((DocumentationCommentTriviaSyntax)triva.GetStructure());
                        new CRefVisistor(this).Visit(triva.GetStructure ());
                    }
                }

                if (storedXmlComment.Count == 0)
                    return;

                xml.Clear();
                xml.Append(firstline);
                var OffsetTable = new List<int>();
                foreach (var cmt in storedXmlComment)
                {
                    OffsetTable.Add(xml.Length - firstline.Length);
                    xml.Append(cmt.Content + "\n");
                }
                xml.Append("</root>\n");

                var doc = new AXmlParser().Parse(SourceText.From(xml.ToString()));

                var stack = new Stack<AXmlObject>();
                stack.Push(doc);
                foreach (var err in doc.SyntaxErrors)
                {
                    AddXmlIssue(CalculateRealStartOffset(OffsetTable, err.StartOffset), err.EndOffset - err.StartOffset, err.Description);
                }

                while (stack.Count > 0) {
                    var cur = stack.Pop();
                    var el = cur as AXmlElement;
                    if (el != null) {
                        switch (el.Name)
                        {
                            case "typeparam":
                            case "typeparamref":
                                var name = el.Attributes.FirstOrDefault(attr => attr.Name == "name");
                                if (name == null || name.ValueSegment.Length < 2)
                                    break;
                                if (member != null && member.IsKind(SymbolKind.NamedType))
                                {
                                    var type = (INamedTypeSymbol)member;
                                    if (!type.TypeArguments.Any(arg => arg.Name == name.Value))
                                    {
                                        AddXmlIssue(CalculateRealStartOffset(OffsetTable, name.ValueSegment.Start + 1), name.ValueSegment.Length - 2, string.Format(GettextCatalog.GetString("Type parameter '{0}' not found"), name.Value));
                                    }
                                }
                                break;
                            case "param":
                            case "paramref":
                                name = el.Attributes.FirstOrDefault(attr => attr.Name == "name");
                                if (name == null || name.ValueSegment.Length < 2)
                                    break;
                                var m = member as IMethodSymbol;
                                if (m != null)
                                {
                                    if (m.Parameters.Any(p => p.Name == name.Value))
                                        break;
                                    AddXmlIssue(CalculateRealStartOffset(OffsetTable, name.ValueSegment.Start + 1), name.ValueSegment.Length - 2, string.Format(GettextCatalog.GetString("Parameter '{0}' not found"), name.Value));
                                    break;
                                }
                                var prop = member as IPropertySymbol;
                                if (prop != null)
                                {
                                    if (prop.Parameters.Any(p => p.Name == name.Value))
                                        break;
                                    if (name.Value == "value")
                                        break;
                                    AddXmlIssue(CalculateRealStartOffset(OffsetTable, name.ValueSegment.Start + 1), name.ValueSegment.Length - 2, string.Format(GettextCatalog.GetString("Parameter '{0}' not found"), name.Value));
                                    break;
                                }
                                var evt = member as IEventSymbol;
                                if (evt != null)
                                {
                                    if (name.Value == "value")
                                        break;
                                    AddXmlIssue(CalculateRealStartOffset(OffsetTable, name.ValueSegment.Start + 1), name.ValueSegment.Length - 2, string.Format(GettextCatalog.GetString("Parameter '{0}' not found"), name.Value));
                                    break;
                                }
                                var named = member as INamedTypeSymbol;
                                if (named != null)
                                {
                                    if (named.DelegateInvokeMethod == null)
                                        break;
                                    if (named.DelegateInvokeMethod.Parameters.Any(p => p.Name == name.Value))
                                        break;
                                    AddXmlIssue(CalculateRealStartOffset(OffsetTable, name.ValueSegment.Start + 1), name.ValueSegment.Length - 2, string.Format(GettextCatalog.GetString("Parameter '{0}' not found"), name.Value));
                                    break;
                                }
                                AddXmlIssue(CalculateRealStartOffset(OffsetTable, name.ValueSegment.Start + 1), name.ValueSegment.Length - 2, string.Format(GettextCatalog.GetString("Parameter '{0}' not found"), name.Value));
                                break;
                        }
                    }
                    foreach (var child in cur.Children)
                        stack.Push(child);
                }


                storedXmlComment.Clear();
            }

            int CalculateRealStartOffset(List<int> OffsetTable, int offset)
            {
                int lineNumber = 0;
                for (int i = 0; i < OffsetTable.Count; i++)
                {
                    if (OffsetTable[i] > offset)
                        lineNumber = i - 1;
                }
                int realStartOffset = storedXmlComment[lineNumber].ParentTrivia.Span.Start + offset - firstline.Length - OffsetTable[lineNumber];
                return realStartOffset;
            }

            public override void VisitClassDeclaration(ClassDeclarationSyntax node)
            {
                CheckXmlDocForErrors(node, semanticModel.GetDeclaredSymbol(node));
                base.VisitClassDeclaration(node);
            }

            public override void VisitStructDeclaration(StructDeclarationSyntax node)
            {
                CheckXmlDocForErrors(node, semanticModel.GetDeclaredSymbol(node));
                base.VisitStructDeclaration(node);
            }

            public override void VisitInterfaceDeclaration(InterfaceDeclarationSyntax node)
            {
                CheckXmlDocForErrors(node, semanticModel.GetDeclaredSymbol(node));
                base.VisitInterfaceDeclaration(node);
            }

            public override void VisitEnumDeclaration(EnumDeclarationSyntax node)
            {
                CheckXmlDocForErrors(node, semanticModel.GetDeclaredSymbol(node));
                base.VisitEnumDeclaration(node);
            }

            public override void VisitDelegateDeclaration(DelegateDeclarationSyntax node)
            {
                CheckXmlDocForErrors(node, semanticModel.GetDeclaredSymbol(node));
            }

            public override void VisitMethodDeclaration(MethodDeclarationSyntax node)
            {
                CheckXmlDocForErrors(node, semanticModel.GetDeclaredSymbol(node));
            }

            public override void VisitConstructorDeclaration(ConstructorDeclarationSyntax node)
            {
                CheckXmlDocForErrors(node, semanticModel.GetDeclaredSymbol(node));
            }

            public override void VisitEventDeclaration(EventDeclarationSyntax node)
            {
                CheckXmlDocForErrors(node, semanticModel.GetDeclaredSymbol(node));
            }

            public override void VisitEventFieldDeclaration(EventFieldDeclarationSyntax node)
            {
                CheckXmlDocForErrors(node, semanticModel.GetDeclaredSymbol(node.Declaration.Variables.FirstOrDefault()));
            }

            public override void VisitDestructorDeclaration(DestructorDeclarationSyntax node)
            {
                CheckXmlDocForErrors(node, semanticModel.GetDeclaredSymbol(node));
            }

            public override void VisitEnumMemberDeclaration(EnumMemberDeclarationSyntax node)
            {
                CheckXmlDocForErrors(node, semanticModel.GetDeclaredSymbol(node));
            }

            public override void VisitFieldDeclaration(FieldDeclarationSyntax node)
            {
                CheckXmlDocForErrors(node, semanticModel.GetDeclaredSymbol(node.Declaration.Variables.FirstOrDefault()));
            }

            public override void VisitIndexerDeclaration(IndexerDeclarationSyntax node)
            {
                CheckXmlDocForErrors(node, semanticModel.GetDeclaredSymbol(node));
            }

            public override void VisitPropertyDeclaration(PropertyDeclarationSyntax node)
            {
                CheckXmlDocForErrors(node, semanticModel.GetDeclaredSymbol(node));
            }

            public override void VisitOperatorDeclaration(OperatorDeclarationSyntax node)
            {
                CheckXmlDocForErrors(node, semanticModel.GetDeclaredSymbol(node));
            }
		}
    }


}