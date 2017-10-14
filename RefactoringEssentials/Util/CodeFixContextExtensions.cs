using Microsoft.CodeAnalysis.CodeFixes;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CodeActions;


namespace RefactoringEssentials
{
#if NR6
	public
#endif
    static class CodeFixContextExtensions
    {
        /// <summary>
        /// Use this helper to register multiple fixes (<paramref name="actions"/>) each of which addresses / fixes the same supplied <paramref name="diagnostic"/>.
        /// </summary>
        public static void RegisterFixes(this CodeFixContext context, IEnumerable<CodeAction> actions, Diagnostic diagnostic)
        {
            foreach (var action in actions)
            {
                context.RegisterCodeFix(action, diagnostic);
            }
        }

        /// <summary>
        /// Use this helper to register multiple fixes (<paramref name="actions"/>) each of which addresses / fixes the same set of supplied <paramref name="diagnostics"/>.
        /// </summary>
        public static void RegisterFixes(this CodeFixContext context, IEnumerable<CodeAction> actions, ImmutableArray<Diagnostic> diagnostics)
        {
            foreach (var action in actions)
            {
                context.RegisterCodeFix(action, diagnostics);
            }
        }
    }

}

