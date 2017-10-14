using RefactoringEssentials.CSharp.CodeRefactorings;
using Xunit;

namespace RefactoringEssentials.Tests.CSharp.CodeRefactorings
{
    public class ConvertBitwiseFlagComparisonToHasFlagsTests : CSharpCodeRefactoringTestBase
    {
        [Fact]
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

        [Fact]
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

        [Fact]
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

        [Fact]
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

        [Fact]
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

        [Fact]
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

        [Fact]
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

