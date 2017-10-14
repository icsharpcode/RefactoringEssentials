using System.Collections.Generic;
using System.Globalization;

namespace RefactoringEssentials
{
#if NR6
    public
#endif
    static class WordParser
    {
        public static List<string> BreakWords(string identifier)
        {
            var words = new List<string>();
            int wordStart = 0;
            bool lastWasLower = false, lastWasUpper = false;
            for (int i = 0; i < identifier.Length; i++)
            {
                char c = identifier[i];
                var category = CharUnicodeInfo.GetUnicodeCategory(c);
                if (category == System.Globalization.UnicodeCategory.LowercaseLetter)
                {
                    if (lastWasUpper && (i - wordStart) > 2)
                    {
                        words.Add(identifier.Substring(wordStart, i - wordStart - 1));
                        wordStart = i - 1;
                    }
                    lastWasLower = true;
                    lastWasUpper = false;
                }
                else if (category == System.Globalization.UnicodeCategory.UppercaseLetter)
                {
                    if (lastWasLower)
                    {
                        words.Add(identifier.Substring(wordStart, i - wordStart));
                        wordStart = i;
                    }
                    lastWasLower = false;
                    lastWasUpper = true;
                }
                else
                {
                    if (c == '_')
                    {
                        if ((i - wordStart) > 0)
                            words.Add(identifier.Substring(wordStart, i - wordStart));
                        wordStart = i + 1;
                        lastWasLower = lastWasUpper = false;
                    }
                }
            }
            if (wordStart < identifier.Length)
                words.Add(identifier.Substring(wordStart));
            return words;
        }
    }
}

