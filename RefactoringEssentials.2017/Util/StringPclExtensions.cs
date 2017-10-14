using System;
using System.Collections.Generic;

namespace RefactoringEssentials
{
    public static class StringPclExtensions
    {
        public static bool Any(this string source, Func<char, bool> predicate)
        {
            foreach (char ch in source)
            {
                if (predicate(ch))
                    return true;
            }
            return false;
        }

        public static IEnumerable<char> Take(this string source, int count)
        {
            if (count > source.Length)
                count = source.Length;
            for (int i = 0; i < count; i++)
            {
                yield return source[i];
            }
        }
    }
}
