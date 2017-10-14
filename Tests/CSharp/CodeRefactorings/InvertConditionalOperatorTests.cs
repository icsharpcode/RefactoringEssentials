using RefactoringEssentials.CSharp.CodeRefactorings;
using Xunit;

namespace RefactoringEssentials.Tests.CSharp.CodeRefactorings
{
    public class InvertConditionalOperatorTests : CSharpCodeRefactoringTestBase
    {
        [Fact]
        public void TestCase1()
        {
            Test<InvertConditionalOperatorCodeRefactoringProvider>(@"
class Foo
{
	void Bar (int i, int j)
	{
		Console.WriteLine (i > j $? i : j);
	}
}
", @"
class Foo
{
	void Bar (int i, int j)
	{
		Console.WriteLine (i <= j ? j : i);
	}
}
");
        }

        [Fact]
        public void TestConditionStart()
        {
            Test<InvertConditionalOperatorCodeRefactoringProvider>(@"
class Foo
{
	void Bar (int i, int j)
	{
		Console.WriteLine ($i > j ? i : j);
	}
}
", @"
class Foo
{
	void Bar (int i, int j)
	{
		Console.WriteLine (i <= j ? j : i);
	}
}
");
        }

        [Fact]
        public void TestTrueStart()
        {
            Test<InvertConditionalOperatorCodeRefactoringProvider>(@"
class Foo
{
	void Bar (int i, int j)
	{
		Console.WriteLine (i > j ? $i : j);
	}
}
", @"
class Foo
{
	void Bar (int i, int j)
	{
		Console.WriteLine (i <= j ? j : i);
	}
}
");
        }

        [Fact]
        public void TestFalseStart()
        {
            Test<InvertConditionalOperatorCodeRefactoringProvider>(@"
class Foo
{
	void Bar (int i, int j)
	{
		Console.WriteLine (i > j ? i : $j);
	}
}
", @"
class Foo
{
	void Bar (int i, int j)
	{
		Console.WriteLine (i <= j ? j : i);
	}
}
");
        }

        [Fact]
        public void TestPopupLocations()
        {
            TestWrongContext<InvertConditionalOperatorCodeRefactoringProvider>(@"
class Foo
{
	void Bar (int i, int j)
	{
		Console.WriteLine (i > $j ? i : j);
	}
}
");
            TestWrongContext<InvertConditionalOperatorCodeRefactoringProvider>(@"
class Foo
{
	void Bar (int i, int j)
	{
		Console.WriteLine (i > j ? i$i : j);
	}
}
");

            TestWrongContext<InvertConditionalOperatorCodeRefactoringProvider>(@"
class Foo
{
	void Bar (int i, int j)
	{
		Console.WriteLine (i > j ? ii : j$j);
	}
}
");
        }
    }
}

