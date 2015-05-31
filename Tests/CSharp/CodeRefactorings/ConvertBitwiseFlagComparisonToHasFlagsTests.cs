using NUnit.Framework;
using RefactoringEssentials.CSharp.CodeRefactorings;

namespace RefactoringEssentials.Tests.CSharp.CodeRefactorings
{
    [TestFixture]
    public class ConvertBitwiseFlagComparisonToHasFlagsTests : CSharpCodeRefactoringTestBase
    {
        [Test]
        public void TestComparisonWithNullInEqual()
        {
            Test<ConvertBitwiseFlagComparisonToHasFlagsCodeRefactoringProvider>(@"
[Flags]
enum Foo
{
	A, B
}

class FooBar
{
	public void Bar (Foo f)
	{
		Console.WriteLine((f & Foo.A) $!= 0);
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
		Console.WriteLine(f.HasFlag(Foo.A));
	}
}
");
        }

        [Test]
        public void TestComparisonWithNullEqual()
        {
            Test<ConvertBitwiseFlagComparisonToHasFlagsCodeRefactoringProvider>(@"
[Flags]
enum Foo
{
	A, B
}

class FooBar
{
	public void Bar (Foo f)
	{
		Console.WriteLine((f & Foo.A) $== 0);
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
		Console.WriteLine(!f.HasFlag(Foo.A));
	}
}
");
        }

        [Test]
        public void TestComparisonWithFlagInEqual()
        {
            Test<ConvertBitwiseFlagComparisonToHasFlagsCodeRefactoringProvider>(@"
[Flags]
enum Foo
{
	A, B
}

class FooBar
{
	public void Bar (Foo f)
	{
		Console.WriteLine((f & Foo.A) $!= Foo.A);
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
		Console.WriteLine(!f.HasFlag(Foo.A));
	}
}
");
        }

        [Test]
        public void TestComparisonWithFlagEqual()
        {
            Test<ConvertBitwiseFlagComparisonToHasFlagsCodeRefactoringProvider>(@"
[Flags]
enum Foo
{
	A, B
}

class FooBar
{
	public void Bar (Foo f)
	{
		Console.WriteLine((f & Foo.A) $== Foo.A);
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
		Console.WriteLine(f.HasFlag(Foo.A));
	}
}
");
        }

        [Test]
        public void TestMultipleFlags()
        {
            Test<ConvertBitwiseFlagComparisonToHasFlagsCodeRefactoringProvider>(@"
[Flags]
enum Foo
{
	A, B
}

class FooBar
{
	public void Bar (Foo f)
	{
		Console.WriteLine((f & (Foo.A | Foo.B)) $!= 0);
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
		Console.WriteLine(f.HasFlag(Foo.A) | f.HasFlag(Foo.B));
	}
}
");
        }

        [Test]
        public void TestMultipleFlagsCase2()
        {
            TestWrongContext<ConvertBitwiseFlagComparisonToHasFlagsCodeRefactoringProvider>(@"
[Flags]
enum Foo
{
	A, B
}

class FooBar
{
	public void Bar (Foo f)
	{
		Console.WriteLine((f & (Foo.A & Foo.B)) $!= 0);
	}
}
");
        }

        [Test]
        public void TestInvalid()
        {
            TestWrongContext<ConvertBitwiseFlagComparisonToHasFlagsCodeRefactoringProvider>(@"
[Flags]
enum Foo
{
	A, B
}

class FooBar
{
	public void Bar (Foo f)
	{
		Console.WriteLine((f & (Foo.A | Foo.B & 1)) $!= 0);
	}
}
");
        }
    }
}

