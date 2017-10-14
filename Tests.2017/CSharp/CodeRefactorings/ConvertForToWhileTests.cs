using RefactoringEssentials.CSharp.CodeRefactorings;
using Xunit;

namespace RefactoringEssentials.Tests.CSharp.CodeRefactorings
{
    public class ConvertForToWhileTests : CSharpCodeRefactoringTestBase
    {
        [Fact]
        public void TestSimpleFor()
        {
            Test<ConvertForToWhileCodeRefactoringProvider>(@"
class Test
{
    void Foo (object[] o)
    {
        $for (int i = 0, oLength = o.Length; i < oLength; i++)
        {
            var p = o[i];
            System.Console.WriteLine(p);
        }
    }
}", @"
class Test
{
    void Foo (object[] o)
    {
        int i = 0, oLength = o.Length;
        while (i < oLength)
        {
            var p = o[i];
            System.Console.WriteLine(p);
            i++;
        }
    }
}");
        }

        [Fact]
        public void TestSimpleForWithComment1()
        {
            Test<ConvertForToWhileCodeRefactoringProvider>(@"
class Test
{
    void Foo (object[] o)
    {
        // Some comment
        $for (int i = 0, oLength = o.Length; i < oLength; i++)
        {
            var p = o[i];
            System.Console.WriteLine(p);
        }
    }
}", @"
class Test
{
    void Foo (object[] o)
    {
        // Some comment
        int i = 0, oLength = o.Length;
        while (i < oLength)
        {
            var p = o[i];
            System.Console.WriteLine(p);
            i++;
        }
    }
}");
        }

        [Fact]
        public void TestSimpleForWithComment2()
        {
            Test<ConvertForToWhileCodeRefactoringProvider>(@"
class Test
{
    void Foo (object[] o)
    {
        $for (int i = 0, oLength = o.Length; i < oLength; i++)
        {
            // Some comment
            var p = o[i];
            System.Console.WriteLine(p);
        }
    }
}", @"
class Test
{
    void Foo (object[] o)
    {
        int i = 0, oLength = o.Length;
        while (i < oLength)
        {
            // Some comment
            var p = o[i];
            System.Console.WriteLine(p);
            i++;
        }
    }
}");
        }

        [Fact]
        public void TestMissingInitializer()
        {
            Test<ConvertForToWhileCodeRefactoringProvider>(@"
class Test
{
    void Foo ()
    {
        $for (; i < oLength; i++)
        {
            var p = o[i];
            System.Console.WriteLine(p);
        }
    }
}", @"
class Test
{
    void Foo ()
    {
        while (i < oLength)
        {
            var p = o[i];
            System.Console.WriteLine(p);
            i++;
        }
    }
}");
        }

        [Fact]
        public void TestMissingCondition()
        {
            Test<ConvertForToWhileCodeRefactoringProvider>(@"
class Test
{
    void Foo (object[] o)
    {
        $for (int i = 0, oLength = o.Length;; i++)
        {
            var p = o[i];
            System.Console.WriteLine(p);
        }
    }
}", @"
class Test
{
    void Foo (object[] o)
    {
        int i = 0, oLength = o.Length;
        while (true)
        {
            var p = o[i];
            System.Console.WriteLine(p);
            i++;
        }
    }
}");
        }

        [Fact]
        public void TestInfiniteLoop()
        {
            Test<ConvertForToWhileCodeRefactoringProvider>(@"
class Test
{
    void Foo (object[] o)
    {
        $for (;;)
        {
            var p = o[i];
            System.Console.WriteLine(p);
        }
    }
}", @"
class Test
{
    void Foo (object[] o)
    {
        while (true)
        {
            var p = o[i];
            System.Console.WriteLine(p);
        }
    }
}");
        }

        [Fact]
        public void TestMultipleInitializersAndIterators()
        {
            Test<ConvertForToWhileCodeRefactoringProvider>(@"
class Test
{
    void Foo (object[] o)
    {
        $for (i=0,j=0; i < oLength; i++,j++)
        {
            var p = o[i];
            System.Console.WriteLine(p);
        }
    }
}", @"
class Test
{
    void Foo (object[] o)
    {
        i = 0;
        j = 0;
        while (i < oLength)
        {
            var p = o[i];
            System.Console.WriteLine(p);
            i++;
            j++;
        }
    }
}");
        }
    }
}