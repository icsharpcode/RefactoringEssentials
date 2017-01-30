using RefactoringEssentials.CSharp.CodeRefactorings;
using Xunit;

namespace RefactoringEssentials.Tests.CSharp.CodeRefactorings
{
    public class CheckDictionaryKeyValueTests : CSharpCodeRefactoringTestBase
    {
        [Fact]
        public void TestSimpleCase()
        {
            Test<CheckDictionaryKeyValueCodeRefactoringProvider>(@"
class Test
{
    public static void Main (System.Collections.Generic.IDictionary<int, int> args)
    {
        Console.WriteLine(args[$5]);
    }
}", @"
class Test
{
    public static void Main (System.Collections.Generic.IDictionary<int, int> args)
    {
        int val;
        if (args.TryGetValue(5, out val))
            Console.WriteLine(val);
    }
}");
        }

        [Fact]
        public void TestNestedCase()
        {
            Test<CheckDictionaryKeyValueCodeRefactoringProvider>(@"
class Test
{
    public static void Main (System.Collections.Generic.IDictionary<int, int> args)
    {
        if (true)
            if (true)
                Console.WriteLine(args[$5 + 234 - 234]);
    }
}", @"
class Test
{
    public static void Main (System.Collections.Generic.IDictionary<int, int> args)
    {
        if (true)
            if (true)
            {
                int val;
                if (args.TryGetValue(5 + 234 - 234, out val))
                    Console.WriteLine(val);
            }
    }
}");
        }

		[Fact]
		public void TestNameClash()
		{
			Test<CheckDictionaryKeyValueCodeRefactoringProvider>(@"
class Test
{
    static int val;

    public static void Main (System.Collections.Generic.IDictionary<int, int> args)
    {
        Console.WriteLine(args[$5]);
    }
}", @"
class Test
{
    static int val;

    public static void Main (System.Collections.Generic.IDictionary<int, int> args)
    {
        int val1;
        if (args.TryGetValue(5, out val1))
            Console.WriteLine(val1);
    }
}");
		}
    }
}