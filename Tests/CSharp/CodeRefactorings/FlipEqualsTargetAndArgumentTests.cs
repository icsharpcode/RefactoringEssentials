using RefactoringEssentials.CSharp.CodeRefactorings;
using Xunit;

namespace RefactoringEssentials.Tests.CSharp.CodeRefactorings
{
    public class FlipEqualsTargetAndArgumentTests : CSharpCodeRefactoringTestBase
    {
        [Fact]
        public void TestSimpleCase()
        {
            Test<FlipEqualsTargetAndArgumentCodeRefactoringProvider>(@"
class Foo
{
    public void FooFoo (object x, object y)
    {
        if (x.$Equals(y))
            Console.WriteLine (x);
    }
}", @"
class Foo
{
    public void FooFoo (object x, object y)
    {
        if (y.Equals(x))
            Console.WriteLine (x);
    }
}");
        }

        [Fact]
        public void TestSimpleCaseWithComment()
        {
            Test<FlipEqualsTargetAndArgumentCodeRefactoringProvider>(@"
class Foo
{
    public void FooFoo (object x, object y)
    {
        // Some comment
        if (x.$Equals(y))
            Console.WriteLine (x);
    }
}", @"
class Foo
{
    public void FooFoo (object x, object y)
    {
        // Some comment
        if (y.Equals(x))
            Console.WriteLine (x);
    }
}");
        }

        [Fact]
        public void TestComplexCase()
        {
            Test<FlipEqualsTargetAndArgumentCodeRefactoringProvider>(@"
class Foo
{
    public void FooFoo (object x, object y)
    {
        if (x.$Equals(1 + 2))
            Console.WriteLine (x);
    }
}", @"
class Foo
{
    public void FooFoo (object x, object y)
    {
        if ((1 + 2).Equals(x))
            Console.WriteLine (x);
    }
}");
        }

        [Fact]
        public void TestRemoveParens()
        {
            Test<FlipEqualsTargetAndArgumentCodeRefactoringProvider>(@"
class Foo
{
    public void FooFoo (object x, object y)
    {
        if ((1 + 2).$Equals(x))
            Console.WriteLine (x);
    }
}", @"
class Foo
{
    public void FooFoo (object x, object y)
    {
        if (x.Equals(1 + 2))
            Console.WriteLine (x);
    }
}");
        }

        [Fact]
        public void TestUnaryOperatorCase()
        {
            Test<FlipEqualsTargetAndArgumentCodeRefactoringProvider>(@"
class Foo
{
    public void FooFoo (object x, bool y)
    {
        if (x.$Equals(!y))
            Console.WriteLine (x);
    }
}", @"
class Foo
{
    public void FooFoo (object x, bool y)
    {
        if ((!y).Equals(x))
            Console.WriteLine (x);
    }
}");
        }

        [Fact]
        public void TestNullCase()
        {
            TestWrongContext<FlipEqualsTargetAndArgumentCodeRefactoringProvider>(@"
class Foo
{
    public void FooFoo (object x, object y)
    {
        if (x.$Equals(null))
            Console.WriteLine (x);
    }
}");
        }

        [Fact]
        public void TestStaticCase()
        {
            TestWrongContext<FlipEqualsTargetAndArgumentCodeRefactoringProvider>(@"
class Foo
{
    public static bool Equals (object a) { return false; }

    public void FooFoo (object x, object y)
    {
        if (Foo.$Equals(x))
            Console.WriteLine (x);
    }
}");
        }

        [Fact]
        public void TestCaretLocation()
        {
            TestWrongContext<FlipEqualsTargetAndArgumentCodeRefactoringProvider>(@"
class Foo
{
    public void FooFoo (object x, object y)
    {
        if (x$.Equals(y))
            Console.WriteLine (x);
    }
}");
            TestWrongContext<FlipEqualsTargetAndArgumentCodeRefactoringProvider>(@"
class Foo
{
    public void FooFoo (object x, object y)
    {
        if (x.Equals($y))
            Console.WriteLine (x);
    }
}");
        }
    }
}

