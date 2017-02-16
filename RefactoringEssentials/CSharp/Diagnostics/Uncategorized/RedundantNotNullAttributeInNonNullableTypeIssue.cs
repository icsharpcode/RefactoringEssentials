using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CodeFixes;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.Text;
using System.Threading;
using ICSharpCode.NRefactory6.CSharp.Refactoring;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.FindSymbols;

namespace ICSharpCode.NRefactory6.CSharp.Diagnostics
{
    [DiagnosticAnalyzer]
    [ExportDiagnosticAnalyzer("", LanguageNames.CSharp)]
    [NRefactoryCodeDiagnosticAnalyzer(Description = "", AnalysisDisableKeyword = "")]
    [IssueDescription("Use of NotNullAttribute in non-nullable type is redundant.",
        Description = "Detects unnecessary usages of the NotNullAttribute.",
        Category = IssueCategories.RedundanciesInDeclarations,
        Severity = Severity.Warning)]
    public class RedundantNotNullAttributeInNonNullableTypeDiagnosticAnalyzer : GatherVisitorCodeIssueProvider
    {
        internal const string DiagnosticId = "";
        const string Description = "";
        const string MessageFormat = "";
        const string Category = IssueCategories.CodeQualityIssues;

        static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Description, MessageFormat, Category, DiagnosticSeverity.Warning);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                return ImmutableArray.Create(Rule);
            }
        }

        protected override CSharpSyntaxWalker CreateVisitor(SemanticModel semanticModel, Action<Diagnostic> addDiagnostic, CancellationToken cancellationToken)
        {
            return new GatherVisitor(semanticModel, addDiagnostic, cancellationToken);
        }

        class GatherVisitor : GatherVisitorBase<RedundantNotNullAttributeInNonNullableTypeIssue>
        {
            public GatherVisitor(SemanticModel semanticModel, Action<Diagnostic> addDiagnostic, CancellationToken cancellationToken)
                : base(semanticModel, addDiagnostic, cancellationToken)
            {
            }

            public override void VisitBlockStatement(BlockStatement blockStatement)
            {
                // This is just here to prevent the visitor from unnecessarily visiting the children.
            }

            public override void VisitAttribute(Attribute attribute)
            {
                var attributeType = ctx.Resolve(attribute).Type;
                if (attributeType.FullName == AnnotationNames.NotNullAttribute)
                {
                    if (IsAttributeRedundant(attribute))
                    {
                        AddDiagnosticAnalyzer(new CodeIssue(attribute,
                            ctx.TranslateString("NotNullAttribute is not needed for non-nullable types"),
                            ctx.TranslateString("Remove redundant NotNullAttribute"),
                            script => script.RemoveAttribute(attribute)));
                    }
                }
            }

            bool IsAttributeRedundant(Attribute attribute)
            {
                var section = attribute.GetParent<AttributeSection>();
                var targetDeclaration = section.Parent;

                if (targetDeclaration is FixedFieldDeclaration)
                {
                    //Fixed fields are never null
                    return true;
                }

                var fieldDeclaration = targetDeclaration as FieldDeclaration;
                if (fieldDeclaration != null)
                {
                    return fieldDeclaration.Variables.All(variable => NullableType.IsNonNullableValueType(ctx.Resolve(variable).Type));
                }

                var resolveResult = ctx.Resolve(targetDeclaration);
                var memberResolveResult = resolveResult as MemberResolveResult;
                if (memberResolveResult != null)
                {
                    return NullableType.IsNonNullableValueType(memberResolveResult.Member.ReturnType);
                }
                var localResolveResult = resolveResult as LocalResolveResult;
                if (localResolveResult != null)
                {
                    return NullableType.IsNonNullableValueType(localResolveResult.Type);
                }

                return false;
            }
        }
    }

    [ExportCodeFixProvider(.DiagnosticId, LanguageNames.CSharp)]
    public class FixProvider : ICodeFixProvider
    {
        public IEnumerable<string> GetFixableDiagnosticIds()
        {
            yield return .DiagnosticId;
        }

        public async Task<IEnumerable<CodeAction>> GetFixesAsync(Document document, TextSpan span, IEnumerable<Diagnostic> diagnostics, CancellationToken cancellationToken)
        {
            var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
            var result = new List<CodeAction>();
            foreach (var diagonstic in diagnostics)
            {
                var node = root.FindNode(diagonstic.Location.SourceSpan);
                //if (!node.IsKind(SyntaxKind.BaseList))
                //	continue;
                var newRoot = root.RemoveNode(node, SyntaxRemoveOptions.KeepNoTrivia);
                result.Add(CodeActionFactory.Create(node.Span, diagonstic.Severity, diagonstic.GetMessage(), document.WithSyntaxRoot(newRoot)));
            }
            return result;
        }
    }
}