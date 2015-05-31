using NUnit.Framework;
using RefactoringEssentials.CSharp.CodeRefactorings;

namespace RefactoringEssentials.Tests.CSharp.CodeRefactorings
{
    [TestFixture]
    public class FlipRelationalOperatorArgumentsTests : CSharpCodeRefactoringTestBase
    {
        [Test]
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

        [Test]
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

        [Test]
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

        [Test]
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

