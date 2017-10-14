using RefactoringEssentials.CSharp.CodeRefactorings;
using Xunit;

namespace RefactoringEssentials.Tests.CSharp.CodeRefactorings
{
    public class CheckArrayIndexValueTests : CSharpCodeRefactoringTestBase
    {
        [Fact]
        public void TestSimpleCase()
        {
            Test<CheckArrayIndexValueCodeRefactoringProvider>(@"
class Test
{
    public static void Main (string[] args)
    {
        Console.WriteLine(args[$5]);
    }
}", @"
class Test
{
    public static void Main (string[] args)
    {
        if (args.Length > 5)
            Console.WriteLine(args[5]);
    }
}");
        }

        [Fact]
        public void TestNestedCase()
        {
            Test<CheckArrayIndexValueCodeRefactoringProvider>(@"
class Test
{
    public static void Main (string[] args)
    {
        if (true)
            if (true)
                Console.WriteLine(args[$5 + 234 - 234]);
    }
}", @"
class Test
{
    public static void Main (string[] args)
    {
        if (true)
            if (true)
                if (args.Length > 5 + 234 - 234)
                    Console.WriteLine(args[5 + 234 - 234]);
    }
}");
        }
    }
}