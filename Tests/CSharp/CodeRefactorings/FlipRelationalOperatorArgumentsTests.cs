using RefactoringEssentials.CSharp.CodeRefactorings;
using Xunit;

namespace RefactoringEssentials.Tests.CSharp.CodeRefactorings
{
    public class FlipRelationalOperatorArgumentsTests : CSharpCodeRefactoringTestBase
    {
        [Fact]
        public void TestLessThan()
        {
            Test<FlipRelationalOperatorArgumentsCodeRefactoringProvider>(@"
class Foo
{
    public void FooFoo (int x, int y)
    {
        if (x $< y))
            Console.WriteLine (x);
    }
}", @"
class Foo
{
    public void FooFoo (int x, int y)
    {
        if (y > x))
            Console.WriteLine (x);
    }
}");
        }

        [Fact]
        public void TestLessThanOrEquals()
        {
            Test<FlipRelationalOperatorArgumentsCodeRefactoringProvider>(@"
class Foo
{
    public void FooFoo (int x, int y)
    {
        if (x $<= y))
            Console.WriteLine (x);
    }
}", @"
class Foo
{
    public void FooFoo (int x, int y)
    {
        if (y >= x))
            Console.WriteLine (x);
    }
}");
        }

        [Fact]
        public void TestGreaterThan()
        {
            Test<FlipRelationalOperatorArgumentsCodeRefactoringProvider>(@"
class Foo
{
    public void FooFoo (int x, int y)
    {
        if (x $> y))
            Console.WriteLine (x);
    }
}", @"
class Foo
{
    public void FooFoo (int x, int y)
    {
        if (y < x))
            Console.WriteLine (x);
    }
}");
        }

        [Fact]
        public void TestGreaterThanOrEquals()
        {
            Test<FlipRelationalOperatorArgumentsCodeRefactoringProvider>(@"
class Foo
{
    public void FooFoo (int x, int y)
    {
        if (x $>= y))
            Console.WriteLine (x);
    }
}", @"
class Foo
{
    public void FooFoo (int x, int y)
    {
        if (y <= x))
            Console.WriteLine (x);
    }
}");
        }


    }
}

