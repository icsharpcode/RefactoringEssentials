using System.Text;
using Microsoft.CodeAnalysis.Text;
using RefactoringEssentials.Tests.Common;

namespace RefactoringEssentials.Tests
{
	public abstract class CodeRefactoringTestBase
    {
        internal static string HomogenizeEol(string str)
        {
            return Utils.HomogenizeEol(str);
        }

        protected static string ParseText(string input, out TextSpan selectedSpan, out TextSpan markedSpan)
        {
            int start = -1, end = -1;
            int start2 = -1, end2 = -1;
            var result = new StringBuilder(input.Length);
            int upper = input.Length - 1;
            for (int i = 0; i < upper; i++)
            {
                var ch = input[i];
                if (ch == '$' && (i + 1 >= upper || input[i + 1] != '"') || ch == '…' || (int)ch == 65533)
                {
                    start = end = i;
                    continue;
                }
                if (ch == '<' && input[i + 1] == '-')
                {
                    start = i;
                    i++;
                    continue;
                }
                if (ch == '-' && input[i + 1] == '>')
                {
                    end = i;
                    i++;
                    continue;
                }

                if (ch == '-' && input[i + 1] == '[')
                {
                    start2 = result.Length;
                    i++;
                    continue;
                }
                if (ch == ']' && input[i + 1] == '-')
                {
                    end2 = result.Length;
                    i++;
                    continue;
                }
                result.Append(ch);
            }

            if (upper >= 0)
            {
                var lastChar = input[upper];
                if (lastChar == '$')
                {
                    start = end = upper;
                }
                else
                {
                    result.Append(lastChar);
                }
            }
            selectedSpan = start < 0 ? new TextSpan(0, 0) : TextSpan.FromBounds(start, end);
            markedSpan = start2 < 0 ? new TextSpan(0, 0) : TextSpan.FromBounds(start2, end2);
            return result.ToString();
        }
    }
}
