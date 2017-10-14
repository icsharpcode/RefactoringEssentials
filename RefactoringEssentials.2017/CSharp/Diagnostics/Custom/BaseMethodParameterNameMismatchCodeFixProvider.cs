using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Rename;

namespace RefactoringEssentials.CSharp.Diagnostics
{

    [ExportCodeFixProvider(LanguageNames.CSharp), System.Composition.Shared]
    public class BaseMethodParameterNameMismatchCodeFixProvider : CodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds
        {
            get
            {
                return ImmutableArray.Create(CSharpDiagnosticIDs.BaseMethodParameterNameMismatchAnalyzerID);
            }
        }

        public override FixAllProvider GetFixAllProvider()
        {
            return WellKnownFixAllProviders.BatchFixer;
        }

        public async override Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var document = context.Document;
            var cancellationToken = context.CancellationToken;
            var span = context.Span;
            var diagnostics = context.Diagnostics;
            var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
            var diagnostic = diagnostics.First();
            var parameter = root.FindNode(context.Span);
            if (!parameter.IsKind(SyntaxKind.Parameter))
                return;
            var newName = diagnostic.Descriptor.CustomTags.First();
            var semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);
            var parameterSymbol = semanticModel.GetDeclaredSymbol(parameter, cancellationToken);
            var solution = document.Project.Solution;

            if (parameterSymbol == null || solution == null)
                return;
            context.RegisterCodeFix(CodeAction.Create($"Rename to '{newName}'", ct => Renamer.RenameSymbolAsync(solution, parameterSymbol, newName, solution.Workspace.Options, ct), CSharpDiagnosticIDs.BaseMethodParameterNameMismatchAnalyzerID), diagnostic);
        }
    }
}