using NUnit.Framework;
using RefactoringEssentials.CSharp.Diagnostics;

namespace RefactoringEssentials.Tests.CSharp.Diagnostics
{
    [TestFixture]
    [Ignore("TODO: Issue not ported yet")]
    public class EqualExpressionComparisonTests : CSharpDiagnosticTestBase
    {
        [Test]
        public void TestEquality()
        {
            Test<EqualExpressionComparisonAnalyzer>(@"class Foo
{
	static int Bar (object o)
	{
		if (o == o) {
		}
		return 5;
	}
}", @"class Foo
{
	static int Bar (object o)
	{
		if (true) {
		}
		return 5;
	}
}");
        }


        [Test]
        public void TestInequality()
        {
            Test<EqualExpressionComparisonAnalyzer>(@"class Foo
{
	static int Bar (object o)
	{
		if (o != o) {
		}
		return 5;
	}
}", @"class Foo
{
	static int Bar (object o)
	{
		if (false) {
		}
		return 5;
	}
}");
        }


        [Test]
        public void TestEquals()
        {
            Test<EqualExpressionComparisonAnalyzer>(@"class Foo
{
	static int Bar (object o)
	{
		if ((1 + 2).Equals(1 + 2)) {
		}
		return 5;
	}
}", @"class Foo
{
	static int Bar (object o)
	{
		if (true) {
		}
		return 5;
	}
}");
        }

        [Test]
        public void TestNotEquals()
        {
            Test<EqualExpressionComparisonAnalyzer>(@"class Foo
{
	static int Bar (object o)
	{
		if (!(1 + 2).Equals(1 + 2)) {
		}
		return 5;
	}
}", @"class Foo
{
	static int Bar (object o)
	{
		if (false) {
		}
		return 5;
	}
}");
        }

        [Test]
        public void TestStaticEquals()
        {
            Test<EqualExpressionComparisonAnalyzer>(@"class Foo
{
	static int Bar (object o)
	{
		if (Equals(o, o)) {
		}
		return 5;
	}
}", @"class Foo
{
	static int Bar (object o)
	{
		if (true) {
		}
		return 5;
	}
}");
        }

        [Test]
        public void TestNotStaticEquals()
        {
            Test<EqualExpressionComparisonAnalyzer>(@"class Foo
{
	static int Bar (object o)
	{
		if (!Equals(o, o)) {
		}
		return 5;
	}
}", @"class Foo
{
	static int Bar (object o)
	{
		if (false) {
		}
		return 5;
	}
}");
        }

        [Test]
        public void TestDisable()
        {
            Analyze<EqualExpressionComparisonAnalyzer>(@"class Foo
{
	static int Bar (object o)
	{
		// ReSharper disable once EqualExpressionComparison
		if (o == o) {
		}
		return 5;
	}
}");
        }

    }
}

