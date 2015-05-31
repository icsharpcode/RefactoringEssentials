using NUnit.Framework;
using RefactoringEssentials.CSharp.CodeRefactorings;

namespace RefactoringEssentials.Tests.CSharp.CodeRefactorings
{
    [TestFixture]
    public class CheckDictionaryKeyValueTests : CSharpCodeRefactoringTestBase
    {
        [Test]
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
        if (args.ContainsKey(5))
            Console.WriteLine(args[5]);
    }
}");
        }

        [Test]
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
                if (args.ContainsKey(5 + 234 - 234))
                    Console.WriteLine(args[5 + 234 - 234]);
    }
}");
        }
    }
}