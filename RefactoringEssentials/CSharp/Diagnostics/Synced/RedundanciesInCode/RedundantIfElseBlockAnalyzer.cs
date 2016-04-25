using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System.Linq;
using System.Collections.Generic;

namespace RefactoringEssentials.CSharp.Diagnostics
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class RedundantIfElseBlockAnalyzer : DiagnosticAnalyzer
    {
        static readonly DiagnosticDescriptor descriptor = new DiagnosticDescriptor(
            CSharpDiagnosticIDs.RedundantIfElseBlockAnalyzerID,
            GettextCatalog.GetString("Redundant 'else' keyword"),
            GettextCatalog.GetString("Redundant 'else' keyword"),
            DiagnosticAnalyzerCategories.RedundanciesInCode,
            DiagnosticSeverity.Hidden,
            isEnabledByDefault: true,
            helpLinkUri: HelpLink.CreateFor(CSharpDiagnosticIDs.RedundantIfElseBlockAnalyzerID),
            customTags: DiagnosticCustomTags.Unnecessary
        );

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(descriptor);

        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.RegisterSyntaxNodeAction(
                (nodeContext) =>
                {
                    Diagnostic diagnostic;
                    if (TryGetDiagnostic(nodeContext, out diagnostic))
                    {
                        nodeContext.ReportDiagnostic(diagnostic);
                    }
                },
                SyntaxKind.IfStatement
            );
        }

        static bool TryGetDiagnostic(SyntaxNodeAnalysisContext nodeContext, out Diagnostic diagnostic)
        {
            diagnostic = default(Diagnostic);
            var ifElseStatement = nodeContext.Node as IfStatementSyntax;
            if (ifElseStatement == null)
                return false;

            if (!ElseIsRedundantControlFlow(ifElseStatement, nodeContext) || HasConflictingNames(nodeContext, ifElseStatement))
                return false;

            diagnostic = Diagnostic.Create(descriptor, ifElseStatement.Else.ElseKeyword.GetLocation());
            return true;
        }

        static bool ElseIsRedundantControlFlow(IfStatementSyntax ifElseStatement, SyntaxNodeAnalysisContext syntaxNode)
        {
            if (ifElseStatement.Else == null || ifElseStatement.Parent is ElseClauseSyntax)
                return false;

            var blockSyntax = ifElseStatement.Else.Statement as BlockSyntax;
            if (blockSyntax != null && blockSyntax.Statements.Count == 0)
                return true;

            var result = syntaxNode.SemanticModel.AnalyzeControlFlow(ifElseStatement.Statement);
            return !result.EndPointIsReachable;
        }

        static bool HasConflictingNames(SyntaxNodeAnalysisContext nodeContext, IfStatementSyntax ifElseStatement)
        {
            var block = ifElseStatement.Else.Statement as BlockSyntax;
            if (block == null || block.Statements.Count == 0)
                return false;

            var member = ifElseStatement.Ancestors().FirstOrDefault(a => a is MemberDeclarationSyntax);

            var priorLocalDeclarations = new List<string>();
            foreach (var localDecl in member.DescendantNodes().Where(n => n.SpanStart < ifElseStatement.Else.SpanStart).OfType<LocalDeclarationStatementSyntax>()) {
                foreach (var v in localDecl.Declaration.Variables)
                    priorLocalDeclarations.Add(v.Identifier.ValueText);
            }

            foreach (var sym in block.Statements)
            {
                var decl = sym as LocalDeclarationStatementSyntax;
                if (decl == null)
                    continue;
                
                if (priorLocalDeclarations.Contains(s => decl.Declaration.Variables.Any(v => v.Identifier.ValueText == s)))
                    return true;
            }
            return false;
        }
    }
}