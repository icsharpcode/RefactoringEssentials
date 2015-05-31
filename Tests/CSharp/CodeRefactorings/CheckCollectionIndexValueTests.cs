using NUnit.Framework;
using RefactoringEssentials.CSharp.CodeRefactorings;

namespace RefactoringEssentials.Tests.CSharp.CodeRefactorings
{
    [TestFixture]
    public class CheckCollectionIndexValueTests : CSharpCodeRefactoringTestBase
    {
        [Test]
        public void TestSimpleCase()
        {

            Test<CheckCollectionIndexValueCodeRefactoringProvider>(@"
using System.Collections.Generic;
class Test
{
    public static void Main (List<int> args)
    {
        Console.WriteLine(args[$5]);
    }
}", @"
using System.Collections.Generic;
class Test
{
    public static void Main (List<int> args)
    {
        if (args.Count > 5)
            Console.WriteLine(args[5]);
    }
}");
        }

        [Test]
        public void TestNestedCase()
        {
            Test<CheckCollectionIndexValueCodeRefactoringProvider>(@"
using System.Collections.Generic;
class Test
{
    public static void Main (List<int> args)
    {
        if (true)
            if (true)
                Console.WriteLine(args[$5 + 234 - 234]);
    }
}", @"
using System.Collections.Generic;
class Test
{
    public static void Main (List<int> args)
    {
        if (true)
            if (true)
                if (args.Count > 5 + 234 - 234)
                    Console.WriteLine(args[5 + 234 - 234]);
    }
}");
        }
    }
}