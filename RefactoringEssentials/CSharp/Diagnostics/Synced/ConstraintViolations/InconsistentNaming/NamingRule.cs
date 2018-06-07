using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace RefactoringEssentials.CSharp.Diagnostics
{
    public class NamingRule : IEquatable<NamingRule>
    {
        public string Name { get; set; }

        /// <summary>
        /// If set, identifiers are required to be prefixed with one of these values.
        /// </summary>
        public string[] RequiredPrefixes { get; set; }

        /// <summary>
        /// If set, identifiers are allowed to be prefixed with one of these values.
        /// </summary>
        public string[] AllowedPrefixes { get; set; }

        /// <summary>
        /// If set, identifiers are required to be suffixed with one of these values.
        /// </summary>
        public string[] RequiredSuffixes { get; set; }

        /// <summary>
        /// If set, identifiers cannot be prefixed by any of these values.
        /// </summary>
        public string[] ForbiddenPrefixes { get; set; }

        /// <summary>
        /// If set, identifiers cannot be suffixed by with any of these values.
        /// </summary>
        public string[] ForbiddenSuffixes { get; set; }

        /// <summary>
        /// Gets or sets the affected entity.
        /// </summary>
        public AffectedEntity AffectedEntity { get; set; }

        /// <summary>
        /// Gets or sets the modifiers mask
        /// </summary>
        public Modifiers VisibilityMask { get; set; }

        /// <summary>
        /// The way that the identifier is cased and that words are separated.
        /// </summary>
        public NamingStyle NamingStyle { get; set; }

        public bool IncludeStaticEntities { get; set; }

        public bool IncludeInstanceMembers { get; set; }

        public bool IsValid(string name)
        {
            string id = name;
            bool foundPrefix = false;
            if (RequiredPrefixes != null && RequiredPrefixes.Length > 0)
            {
                var prefix = RequiredPrefixes.FirstOrDefault(p => id.StartsWith(p, StringComparison.Ordinal));
                if (prefix == null)
                {
                    return false;
                }
                id = id.Substring(prefix.Length);
                foundPrefix = true;
            }

            if (!foundPrefix && AllowedPrefixes != null && AllowedPrefixes.Length > 0)
            {
                var prefix = AllowedPrefixes.FirstOrDefault(p => id.StartsWith(p, StringComparison.Ordinal));
                if (prefix != null)
                {
                    id = id.Substring(prefix.Length);
                    foundPrefix = true;
                }
            }

            if (!foundPrefix && ForbiddenPrefixes != null && ForbiddenPrefixes.Length > 0)
            {
                if (ForbiddenPrefixes.Any(p => id.StartsWith(p, StringComparison.Ordinal)))
                {
                    return false;
                }
            }

            if (RequiredSuffixes != null && RequiredSuffixes.Length > 0)
            {
                var suffix = RequiredSuffixes.FirstOrDefault(s => id.EndsWith(s, StringComparison.Ordinal));
                if (suffix == null)
                {
                    return false;
                }
                id = id.Substring(0, id.Length - suffix.Length);
            }
            else if (ForbiddenSuffixes != null && ForbiddenSuffixes.Length > 0)
            {
                if (ForbiddenSuffixes.Any(p => id.EndsWith(p, StringComparison.Ordinal)))
                {
                    return false;
                }
            }

            switch (NamingStyle)
            {
                case NamingStyle.AllLower:
                    for (int i = 0; i < id.Length; i++)
                    {
                        char ch = id[i];
                        if (ch == '_' && !HandleUnderscore(UnderscoreHandling.Allow, id, ref i))
                            return false;
                        if (char.IsLetter(ch) && char.IsUpper(ch))
                            return false;
                    }
                    return true;
                case NamingStyle.AllUpper:
                    for (int i = 0; i < id.Length; i++)
                    {
                        char ch = id[i];
                        if (ch == '_' && !HandleUnderscore(UnderscoreHandling.Allow, id, ref i))
                            return false;
                        if (char.IsLetter(ch) && char.IsLower(ch))
                            return false;
                    }
                    return true;
                case NamingStyle.CamelCase:
                    if (id.Length > 0)
                    {
                        if (char.IsUpper(id[0]) || id[0] == '_')
                            return false;
                        for (int i = 1; i < id.Length; i++)
                        {
                            char ch = id[i];
                            if (ch == '_' && !HandleUnderscore(UnderscoreHandling.Forbid, id, ref i))
                                return false;
                        }
                    }
                    return true;
                case NamingStyle.CamelCaseWithLowerLetterUnderscore:
                    if (id.Length > 0)
                    {
                        if (char.IsUpper(id[0]) || id[0] == '_')
                            return false;
                        for (int i = 1; i < id.Length; i++)
                        {
                            char ch = id[i];
                            if (ch == '_' && !HandleUnderscore(UnderscoreHandling.AllowWithLowerStartingLetter, id, ref i))
                                return false;
                        }
                    }
                    return true;

                case NamingStyle.CamelCaseWithUpperLetterUnderscore:
                    if (id.Length > 0)
                    {
                        if (char.IsUpper(id[0]) || id[0] == '_')
                            return false;
                        for (int i = 1; i < id.Length; i++)
                        {
                            char ch = id[i];
                            if (ch == '_' && !HandleUnderscore(UnderscoreHandling.AllowWithUpperStartingLetter, id, ref i))
                                return false;
                        }
                    }
                    return true;

                case NamingStyle.PascalCase:
                    if (id.Length > 0)
                    {
                        if (char.IsLower(id[0]) || id[0] == '_')
                            return false;
                        for (int i = 1; i < id.Length; i++)
                        {
                            char ch = id[i];
                            if (ch == '_' && !HandleUnderscore(UnderscoreHandling.Forbid, id, ref i))
                                return false;
                        }
                    }
                    return true;
                case NamingStyle.PascalCaseWithLowerLetterUnderscore:
                    if (id.Length > 0)
                    {
                        if (char.IsLower(id[0]) || id[0] == '_')
                            return false;
                        for (int i = 1; i < id.Length; i++)
                        {
                            char ch = id[i];
                            if (ch == '_' && !HandleUnderscore(UnderscoreHandling.AllowWithLowerStartingLetter, id, ref i))
                                return false;
                        }
                    }
                    return true;
                case NamingStyle.PascalCaseWithUpperLetterUnderscore:
                    if (id.Length > 0)
                    {
                        if (char.IsLower(id[0]) || id[0] == '_')
                            return false;
                        for (int i = 1; i < id.Length; i++)
                        {
                            char ch = id[i];
                            if (ch == '_' && !HandleUnderscore(UnderscoreHandling.AllowWithUpperStartingLetter, id, ref i))
                                return false;
                        }
                    }
                    return true;
                case NamingStyle.FirstUpper:
                    if (id.Length > 0)
                    {
                        if (char.IsLower(id[0]) || id[0] == '_')
                            return false;

                        for (int i = 1; i < id.Length; i++)
                        {
                            char ch = id[i];
                            if (ch == '_' && !HandleUnderscore(UnderscoreHandling.Allow, id, ref i))
                                return false;
                            if (char.IsLetter(ch) && char.IsUpper(ch))
                                return false;
                        }
                    }
                    return true;
            }
            return true;
        }

        public NamingRule(AffectedEntity affectedEntity)
        {
            AffectedEntity = affectedEntity;
            VisibilityMask = Modifiers.VisibilityMask;
            IncludeStaticEntities = true;
            IncludeInstanceMembers = true;
        }

        static bool CheckUnderscore(string id, UnderscoreHandling handling)
        {
            for (int i = 1; i < id.Length; i++)
            {
                char ch = id[i];
                if (ch == '_' && !HandleUnderscore(handling, id, ref i))
                    return false;
            }
            return true;
        }

        enum UnderscoreHandling
        {
            Forbid,
            Allow,
            AllowWithLowerStartingLetter,
            AllowWithUpperStartingLetter
        }

        static bool HandleUnderscore(UnderscoreHandling handling, string id, ref int i)
        {
            switch (handling)
            {
                case UnderscoreHandling.Forbid:
                    if (i + 1 < id.Length)
                    {
                        char ch = id[i + 1];
                        if (char.IsDigit(ch))
                        {
                            i++;
                            return true;
                        }
                    }
                    return false;
                case UnderscoreHandling.Allow:
                    return true;
                case UnderscoreHandling.AllowWithLowerStartingLetter:
                    if (i + 1 < id.Length)
                    {
                        char ch = id[i + 1];
                        if (char.IsLetter(ch) && !char.IsLower(ch) || ch == '_')
                            return false;
                        i++;
                    }
                    return true;
                case UnderscoreHandling.AllowWithUpperStartingLetter:
                    if (i + 1 < id.Length)
                    {
                        char ch = id[i + 1];
                        if (char.IsLetter(ch) && !char.IsUpper(ch) || ch == '_')
                            return false;
                        i++;
                    }
                    return true;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// Applies a suffix to a name and tries to reuse the suffix of the suffix.
        /// MyArgs + EventArgs -> MyEventArgs instead of MyArgsEventArgs
        /// </summary>
        static string ApplySuffix(string name, string suffix)
        {
            var words = WordParser.BreakWords(suffix);

            bool found = false;
            for (int j = words.Count - 1; j >= 0; j--)
            {
                if (name.EndsWith(words[j], StringComparison.Ordinal))
                {
                    name = name.Substring(0, name.Length - words[j].Length);
                    found = true;
                }
                else
                {
                    if (found)
                        return name + string.Join("", words.ToArray(), j, words.Count - j);
                    break;
                }
            }
            return name + suffix;
        }

        static string TranslateString(string originalString)
        {
            // TODO
            return originalString;
        }

        public string GetErrorMessage(SemanticModel ctx, string name, out IList<string> suggestedNames)
        {
            suggestedNames = new List<string>();
            string id = name;

            string errorMessage = null;

            bool missingRequiredPrefix = false;
            bool missingRequiredSuffix = false;
            string requiredPrefix = null;
            string allowedPrefix = null;
            string suffix = null;

            if (AllowedPrefixes != null && AllowedPrefixes.Length > 0)
            {
                allowedPrefix = AllowedPrefixes.FirstOrDefault(p => id.StartsWith(p, StringComparison.Ordinal));
                if (allowedPrefix != null)
                    id = id.Substring(allowedPrefix.Length);

            }

            if (RequiredPrefixes != null && RequiredPrefixes.Length > 0)
            {
                requiredPrefix = RequiredPrefixes.FirstOrDefault(p => id.StartsWith(p, StringComparison.Ordinal));
                if (requiredPrefix == null)
                {
                    errorMessage = string.Format(TranslateString("Name should have prefix '{0}'. (Rule '{1}')."), RequiredPrefixes[0], Name);
                    missingRequiredPrefix = true;
                }
                else
                {
                    id = id.Substring(requiredPrefix.Length);
                }
            }
            else if (ForbiddenPrefixes != null && ForbiddenPrefixes.Length > 0)
            {
                requiredPrefix = ForbiddenPrefixes.FirstOrDefault(p => id.StartsWith(p, StringComparison.Ordinal));
                if (requiredPrefix != null)
                {
                    errorMessage = string.Format(TranslateString("Name has forbidden prefix '{0}'. (Rule '{1}')"), requiredPrefix, Name);
                    id = id.Substring(requiredPrefix.Length);
                }
            }

            if (RequiredSuffixes != null && RequiredSuffixes.Length > 0)
            {
                suffix = RequiredSuffixes.FirstOrDefault(s => id.EndsWith(s, StringComparison.Ordinal));
                if (suffix == null)
                {
                    errorMessage = string.Format(TranslateString("Name should have suffix '{0}'. (Rule '{1}')"), RequiredSuffixes[0], Name);
                    missingRequiredSuffix = true;
                }
                else
                {
                    id = id.Substring(0, id.Length - suffix.Length);
                }
            }
            else if (ForbiddenSuffixes != null && ForbiddenSuffixes.Length > 0)
            {
                suffix = ForbiddenSuffixes.FirstOrDefault(p => id.EndsWith(p, StringComparison.Ordinal));
                if (suffix != null)
                {
                    errorMessage = string.Format(TranslateString("Name has forbidden suffix '{0}'. (Rule '{1}')"), suffix, Name);
                    id = id.Substring(0, id.Length - suffix.Length);
                }
            }

            switch (NamingStyle)
            {
                case NamingStyle.AllLower:
                    if (id.Any(ch => char.IsLetter(ch) && char.IsUpper(ch)))
                    {
                        errorMessage = string.Format(TranslateString("'{0}' contains upper case letters. (Rule '{1}')"), name, Name);
                        suggestedNames.Add(LowerCaseIdentifier(WordParser.BreakWords(id)));
                    }
                    else
                    {
                        suggestedNames.Add(id);
                    }
                    break;
                case NamingStyle.AllUpper:
                    if (id.Any(ch => char.IsLetter(ch) && char.IsLower(ch)))
                    {
                        errorMessage = string.Format(TranslateString("'{0}' contains lower case letters. (Rule '{1}')"), name, Name);
                        suggestedNames.Add(UpperCaseIdentifier(WordParser.BreakWords(id)));
                    }
                    else
                    {
                        suggestedNames.Add(id);
                    }
                    break;

                case NamingStyle.CamelCase:
                    if (id.Length > 0 && char.IsUpper(id[0]))
                    {
                        errorMessage = string.Format(TranslateString("'{0}' should start with a lower case letter. (Rule '{1}')"), name, Name);
                    }
                    else if (!CheckUnderscore(id, UnderscoreHandling.Forbid))
                    {
                        errorMessage = string.Format(TranslateString("'{0}' should not separate words with an underscore. (Rule '{1}')"), name, Name);
                    }
                    else
                    {
                        suggestedNames.Add(id);
                        break;
                    }
                    suggestedNames.Add(CamelCaseIdentifier(id));
                    break;
                case NamingStyle.CamelCaseWithLowerLetterUnderscore:
                    if (id.Length > 0 && char.IsUpper(id[0]))
                    {
                        errorMessage = string.Format(TranslateString("'{0}' should start with a lower case letter. (Rule '{1}')"), name, Name);
                    }
                    else if (!CheckUnderscore(id, UnderscoreHandling.AllowWithLowerStartingLetter))
                    {
                        errorMessage = string.Format(TranslateString("after '_' a lower letter should follow. (Rule '{0}')"), Name);
                    }
                    else
                    {
                        suggestedNames.Add(id);
                        break;
                    }
                    suggestedNames.Add(CamelCaseWithLowerLetterUnderscore(id));
                    break;
                case NamingStyle.CamelCaseWithUpperLetterUnderscore:
                    if (id.Length > 0 && char.IsUpper(id[0]))
                    {
                        errorMessage = string.Format(TranslateString("'{0}' should start with a lower case letter. (Rule '{1}')"), name, Name);
                    }
                    else if (!CheckUnderscore(id, UnderscoreHandling.AllowWithUpperStartingLetter))
                    {
                        errorMessage = string.Format(TranslateString("after '_' an upper letter should follow. (Rule '{0}')"), Name);
                    }
                    else
                    {
                        suggestedNames.Add(id);
                        break;
                    }
                    suggestedNames.Add(CamelCaseWithUpperLetterUnderscore(id));
                    break;

                case NamingStyle.PascalCase:
                    if (id.Length > 0 && char.IsLower(id[0]))
                    {
                        errorMessage = string.Format(TranslateString("'{0}' should start with an upper case letter. (Rule '{1}')"), name, Name);
                    }
                    else if (!CheckUnderscore(id, UnderscoreHandling.Forbid))
                    {
                        errorMessage = string.Format(TranslateString("'{0}' should not separate words with an underscore. (Rule '{1}')"), name, Name);
                    }
                    else
                    {
                        suggestedNames.Add(id);
                        break;
                    }
                    suggestedNames.Add(PascalCaseIdentifier(id));
                    break;
                case NamingStyle.PascalCaseWithLowerLetterUnderscore:
                    if (id.Length > 0 && char.IsLower(id[0]))
                    {
                        errorMessage = string.Format(TranslateString("'{0}' should start with an upper case letter. (Rule '{1}')"), name, Name);
                    }
                    else if (!CheckUnderscore(id, UnderscoreHandling.AllowWithLowerStartingLetter))
                    {
                        errorMessage = string.Format(TranslateString("after '_' a lower letter should follow. (Rule '{0}')"), Name);
                    }
                    else
                    {
                        suggestedNames.Add(id);
                        break;
                    }
                    suggestedNames.Add(PascalCaseWithLowerLetterUnderscore(id));
                    break;
                case NamingStyle.PascalCaseWithUpperLetterUnderscore:
                    if (id.Length > 0 && char.IsLower(id[0]))
                    {
                        errorMessage = string.Format(TranslateString("'{0}' should start with an upper case letter. (Rule '{1}')"), name, Name);
                    }
                    else if (!CheckUnderscore(id, UnderscoreHandling.AllowWithUpperStartingLetter))
                    {
                        errorMessage = string.Format(TranslateString("after '_' an upper letter should follow. (Rule '{0}')"), Name);
                    }
                    else
                    {
                        suggestedNames.Add(id);
                        break;
                    }
                    suggestedNames.Add(PascalCaseWithUpperLetterUnderscore(id));
                    break;
                case NamingStyle.FirstUpper:
                    if (id.Length > 0 && char.IsLower(id[0]))
                    {
                        errorMessage = string.Format(TranslateString("'{0}' should start with an upper case letter. (Rule '{1}')"), name, Name);
                    }
                    else if (id.Take(1).Any(ch => char.IsLetter(ch) && char.IsUpper(ch)))
                    {
                        errorMessage = string.Format(TranslateString("'{0}' contains an upper case letter after the first. (Rule '{1}')"), name, Name);
                    }
                    else
                    {
                        suggestedNames.Add(id);
                        break;
                    }
                    suggestedNames.Add(FirstUpperIdentifier(WordParser.BreakWords(id)));
                    break;
            }

            if (requiredPrefix != null)
            {
                for (int i = 0; i < suggestedNames.Count; i++)
                {
                    suggestedNames[i] = requiredPrefix + suggestedNames[i];
                }
            }
            else if (allowedPrefix != null)
            {
                int count = suggestedNames.Count;
                for (int i = 0; i < count; i++)
                {
                    suggestedNames.Add(suggestedNames[i]);
                    suggestedNames[i] = allowedPrefix + suggestedNames[i];
                }
            }
            else if (missingRequiredPrefix)
            {
                for (int i = 0; i < suggestedNames.Count; i++)
                {
                    var n = suggestedNames[i];
                    bool first = true;
                    foreach (var p in RequiredPrefixes)
                    {
                        if (first)
                        {
                            first = false;
                            suggestedNames[i] = p + n;
                        }
                        else
                        {
                            suggestedNames.Add(p + n);
                        }
                    }
                }
            }

            if (suffix != null)
            {
                for (int i = 0; i < suggestedNames.Count; i++)
                {
                    suggestedNames[i] = ApplySuffix(suggestedNames[i], suffix);
                }
            }
            else if (missingRequiredSuffix)
            {
                for (int i = 0; i < suggestedNames.Count; i++)
                {
                    var n = suggestedNames[i];
                    bool first = true;
                    foreach (var s in RequiredSuffixes)
                    {
                        if (first)
                        {
                            first = false;
                            suggestedNames[i] = ApplySuffix(n, s);
                        }
                        else
                        {
                            suggestedNames.Add(ApplySuffix(n, s));
                        }
                    }
                }
            }

            return errorMessage
                // should never happen.
                ?? "no known errors.";
        }

        static string CamelCaseIdentifier(string id)
        {
            return ConvertToValidName(id, ch => char.ToLower(ch), ch => char.ToUpper(ch));
        }

        static string CamelCaseWithLowerLetterUnderscore(string id)
        {
            return ConvertToValidNameWithSpecialUnderscoreHandling(id, ch => char.ToLower(ch), ch => char.ToLower(ch));
        }

        static string CamelCaseWithUpperLetterUnderscore(string id)
        {
            return ConvertToValidNameWithSpecialUnderscoreHandling(id, ch => char.ToLower(ch), ch => char.ToUpper(ch));
        }

        static string ConvertToValidName(string id, Func<char, char> firstCharFunc, Func<char, char> followingCharFunc)
        {
            var sb = new StringBuilder();
            bool first = true;
            for (int i = 0; i < id.Length; i++)
            {
                char ch = id[i];
                if (first && char.IsLetter(ch))
                {
                    sb.Append(firstCharFunc(ch));
                    firstCharFunc = followingCharFunc;
                    first = false;
                    continue;
                }
                if (ch == '_')
                {
                    if (first)
                        continue;
                    if (i + 1 < id.Length && id[i + 1] == '_')
                        continue;

                    if (i + 1 < id.Length)
                    {
                        if (char.IsDigit(id[i + 1]))
                        {
                            sb.Append('_');
                        }
                        else
                        {
                            first = true;
                        }
                    }
                    continue;
                }
                sb.Append(ch);
            }
            return sb.ToString();
        }

        static string ConvertToValidNameWithSpecialUnderscoreHandling(string id, Func<char, char> firstCharFunc, Func<char, char> afterUnderscoreLetter)
        {
            var sb = new StringBuilder();
            bool first = true;
            for (int i = 0; i < id.Length; i++)
            {
                char ch = id[i];
                if (first && char.IsLetter(ch))
                {
                    sb.Append(firstCharFunc(ch));
                    first = false;
                    continue;
                }
                if (ch == '_')
                {
                    if (first)
                        continue;
                    if (i + 1 < id.Length && id[i + 1] == '_')
                        continue;
                    sb.Append('_');
                    i++;
                    if (i < id.Length)
                        sb.Append(afterUnderscoreLetter(id[i]));
                    continue;
                }
                sb.Append(ch);
            }
            return sb.ToString();
        }


        static string PascalCaseIdentifier(string id)
        {
            return ConvertToValidName(id, ch => char.ToUpper(ch), ch => char.ToUpper(ch));
        }

        static string PascalCaseWithLowerLetterUnderscore(string id)
        {
            return ConvertToValidNameWithSpecialUnderscoreHandling(id, ch => char.ToUpper(ch), ch => char.ToLower(ch));
        }

        static string PascalCaseWithUpperLetterUnderscore(string id)
        {
            return ConvertToValidNameWithSpecialUnderscoreHandling(id, ch => char.ToUpper(ch), ch => char.ToUpper(ch));
        }

        static string LowerCaseIdentifier(List<string> words)
        {
            var sb = new StringBuilder();
            sb.Append(words[0].ToLower());
            for (int i = 1; i < words.Count; i++)
            {
                sb.Append('_');
                sb.Append(words[i].ToLower());
            }
            return sb.ToString();
        }

        static string UpperCaseIdentifier(List<string> words)
        {
            var sb = new StringBuilder();
            sb.Append(words[0].ToUpper());
            for (int i = 1; i < words.Count; i++)
            {
                sb.Append('_');
                sb.Append(words[i].ToUpper());
            }
            return sb.ToString();
        }

        static string FirstUpperIdentifier(List<string> words)
        {
            var sb = new StringBuilder();
            AppendCapitalized(words[0], sb);
            for (int i = 1; i < words.Count; i++)
            {
                sb.Append('_');
                sb.Append(words[i].ToLower());
            }
            return sb.ToString();
        }

        static void AppendCapitalized(string word, StringBuilder sb)
        {
            sb.Append(word.ToLower());
            sb[sb.Length - word.Length] = char.ToUpper(sb[sb.Length - word.Length]);
        }

        #region IEquatable implementation

        public bool Equals(NamingRule other)
        {
            return Name == other.Name &&
                AffectedEntity == other.AffectedEntity &&
                VisibilityMask == other.VisibilityMask &&
                NamingStyle == other.NamingStyle;
        }

        #endregion

        /// <summary>
        /// Gets an identifier (or comma separated list of identifiers) which apply to this naming rule.
        /// </summary>
        public string GetPreview()
        {
            var result = new StringBuilder();
            if (RequiredPrefixes != null && RequiredPrefixes.Length > 0)
                result.Append(RequiredPrefixes[0]);
            switch (NamingStyle)
            {
                case NamingStyle.PascalCase:
                    result.Append("PascalCase");
                    break;
                case NamingStyle.PascalCaseWithLowerLetterUnderscore:
                    result.Append("PascalCase_underscoreTolerant");
                    break;
                case NamingStyle.PascalCaseWithUpperLetterUnderscore:
                    result.Append("PascalCase_UnderscoreTolerant");
                    break;

                case NamingStyle.CamelCase:
                    result.Append("camelCase");
                    break;
                case NamingStyle.CamelCaseWithLowerLetterUnderscore:
                    result.Append("camelCase_underscoreTolerant");
                    break;
                case NamingStyle.CamelCaseWithUpperLetterUnderscore:
                    result.Append("camelCase_UnderscoreTolerant");
                    break;

                case NamingStyle.AllUpper:
                    result.Append("ALL_UPPER");
                    break;
                case NamingStyle.AllLower:
                    result.Append("all_Lower");
                    break;
                case NamingStyle.FirstUpper:
                    result.Append("First_Upper");
                    break;
            }
            if (RequiredSuffixes != null && RequiredSuffixes.Length > 0)
                result.Append(RequiredSuffixes[0]);
            if (AllowedPrefixes != null)
            {
                string baseString = result.ToString();
                foreach (var str in AllowedPrefixes)
                {
                    result.Append(", " + str + baseString);
                }
            }
            return result.ToString();
        }

        public NamingRule Clone()
        {
            return (NamingRule)MemberwiseClone();
        }

        public override string ToString()
        {
            return string.Format("[NamingRule: Name={0}, AffectedEntity={1}, VisibilityMask={2}, NamingStyle={3}, IncludeStaticEntities={4}, IncludeInstanceMembers={5}]", Name, AffectedEntity, VisibilityMask, NamingStyle, IncludeStaticEntities, IncludeInstanceMembers);
        }
    }
}

