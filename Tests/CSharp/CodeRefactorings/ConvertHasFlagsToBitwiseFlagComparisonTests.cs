using NUnit.Framework;
using RefactoringEssentials.CSharp.CodeRefactorings;

namespace RefactoringEssentials.Tests.CSharp.CodeRefactorings
{
    [TestFixture]
    public class ConvertHasFlagsToBitwiseFlagComparisonTests : CSharpCodeRefactoringTestBase
    {
        [Test]
        public void TestSimpleHasFlag()
        {
            Test<ConvertHasFlagsToBitwiseFlagComparisonCodeRefactoringProvider>(@"
[Flags]
enum Foo
{
	A, B
}

class FooBar
{
	public void Bar (Foo f)
	{
		Console.WriteLine (f.$HasFlag (Foo.A));
	}
}
", @"
[Flags]
enum Foo
{
	A, B
}

class FooBar
{
	public void Bar (Foo f)
	{
		Console.WriteLine ((f & Foo.A) != 0);
	}
}
");
        }

        [Test]
        public void TestNegatedSimpleHasFlag()
        {
            Test<ConvertHasFlagsToBitwiseFlagComparisonCodeRefactoringProvider>(@"
[Flags]
enum Foo
{
	A, B
}

class FooBar
{
	public void Bar (Foo f)
	{
		Console.WriteLine (!f.$HasFlag (Foo.A));
	}
}
", @"
[Flags]
enum Foo
{
	A, B
}

class FooBar
{
	public void Bar (Foo f)
	{
		Console.WriteLine ((f & Foo.A) == 0);
	}
}
");
        }


        [Test]
        public void TestMultipleFlagsCase2()
        {
            Test<ConvertHasFlagsToBitwiseFlagComparisonCodeRefactoringProvider>(@"
[Flags]
enum Foo
{
	A, B
}

class FooBar
{
	public void Bar (Foo f)
	{
		Console.WriteLine (f.$HasFlag (Foo.A | Foo.B));
	}
}
", @"
[Flags]
enum Foo
{
	A, B
}

class FooBar
{
	public void Bar (Foo f)
	{
		Console.WriteLine ((f & (Foo.A & Foo.B)) != 0);
	}
}
");
        }
    }
}

