using NUnit.Framework;
using RefactoringEssentials.CSharp.CodeRefactorings;

namespace RefactoringEssentials.Tests.CSharp.CodeRefactorings
{
    [TestFixture]
    public class InvertConditionalOperatorTests : CSharpCodeRefactoringTestBase
    {
        [Test]
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

        [Test]
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

        [Test]
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

        [Test]
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

        [Test]
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

