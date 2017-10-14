using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis;

namespace RefactoringEssentials
{
    internal static class SyntaxTriviaListExtensions
    {
        public static bool Any(this SyntaxTriviaList triviaList, params SyntaxKind[] kinds)
        {
            foreach (var trivia in triviaList)
            {
                if (trivia.MatchesKind(kinds))
                {
                    return true;
                }
            }

            return false;
        }

        public static SyntaxTrivia? GetFirstNewLine(this SyntaxTriviaList triviaList)
        {
            return triviaList
                .Where(t => t.Kind() == SyntaxKind.EndOfLineTrivia)
                .FirstOrNullable();
        }

        public static SyntaxTrivia? GetLastComment(this SyntaxTriviaList triviaList)
        {
            return triviaList
                .Where(t => t.MatchesKind(SyntaxKind.SingleLineCommentTrivia, SyntaxKind.MultiLineCommentTrivia))
                .LastOrNullable();
        }

        public static IEnumerable<SyntaxTrivia> SkipInitialWhitespace(this SyntaxTriviaList triviaList)
        {
            return triviaList.SkipWhile(t => t.Kind() == SyntaxKind.WhitespaceTrivia);
        }
    }
}
