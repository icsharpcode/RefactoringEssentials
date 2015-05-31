using NUnit.Framework;
using RefactoringEssentials.CSharp.Diagnostics;

namespace RefactoringEssentials.Tests.CSharp.Diagnostics
{
    [TestFixture]
    public class CanBeReplacedWithTryCastAndCheckForNullTests : CSharpDiagnosticTestBase
    {
        [Test]
        public void SimpleCase()
        {
            Analyze<CanBeReplacedWithTryCastAndCheckForNullAnalyzer>(@"
class Bar
{
	public Bar Baz (object foo)
	{
		if (foo $is$ Bar) {
			Baz ((Bar)foo);
			return (Bar)foo;
		}
		return null;
	}
}
");
        }

        [Test]
        public void ComplexCase()
        {
            Analyze<CanBeReplacedWithTryCastAndCheckForNullAnalyzer>(@"
class Bar
{
	public IDisposable Baz (object foo)
	{
		if (((foo) $is$ Bar)) {
			Baz ((Bar)foo);
			Baz (foo as Bar);
			Baz (((foo) as Bar));
			Baz ((Bar)(foo));
			return (IDisposable)foo;
		}
		return null;
	}
}
");
        }

        [Test]
        public void IfElseCase()
        {
            Analyze<CanBeReplacedWithTryCastAndCheckForNullAnalyzer>(@"
class Bar
{
	public Bar Baz (object foo)
	{
		if (foo $is$ Bar) {
			Baz ((Bar)foo);
			return (Bar)foo;
		} else {
			Console.WriteLine (""Hello World "");
		}
		return null;
	}
}
");
        }

        [Test]
        public void InvalidIfNoTypeCast()
        {
            Analyze<CanBeReplacedWithTryCastAndCheckForNullAnalyzer>(@"
class Bar
{
	public Bar Baz (object foo)
	{
		if (foo is Bar) {
			Console.WriteLine (""Hello World "");
		}
		return null;
	}
}
");
        }

        [Test]
        public void NestedIf()
        {
            Analyze<CanBeReplacedWithTryCastAndCheckForNullAnalyzer>(@"
class Bar
{
	public Bar Baz (object foo)
	{
		if (foo is string) {
		} else if (foo $is$ Bar) {
			Baz ((Bar)foo);
			return (Bar)foo;
		}
		return null;
	}
}
");
        }

        [Test]
        public void TestNegatedCaseWithReturn()
        {
            Analyze<CanBeReplacedWithTryCastAndCheckForNullAnalyzer>(@"
class Bar
{
	public Bar Baz (object foo)
	{
		if (!(foo $is$ Bar))
			return null;
		Baz ((Bar)foo);
		return (Bar)foo;
	}
}
");
        }

        [Test]
        public void TestNegatedCaseWithBreak()
        {
            Analyze<CanBeReplacedWithTryCastAndCheckForNullAnalyzer>(@"
class Bar
{
	public Bar Baz (object foo)
	{
		for (int i = 0; i < 10; i++) {
			if (!(foo $is$ Bar))
				break;
			Baz ((Bar)foo);
		}
		return (Bar)foo;
	}
}
");
        }

        [Test]
        public void TestCaseWithContinue()
        {
            Analyze<CanBeReplacedWithTryCastAndCheckForNullAnalyzer>(@"
class Bar
{
	public Bar Baz (object foo)
	{
		for (int i = 0; i < 10; i++) {
			if (!(foo $is$ Bar)) {
				continue;
			} else {
				foo = new Bar ();
			}
			Baz ((Bar)foo);
		}
		return (Bar)foo;
	}
}
");
        }

        [Test]
        public void TestDisable()
        {
            Analyze<CanBeReplacedWithTryCastAndCheckForNullAnalyzer>(@"
class Bar
{
	public Bar Baz (object foo)
	{
#pragma warning disable " + CSharpDiagnosticIDs.CanBeReplacedWithTryCastAndCheckForNullAnalyzerID + @"
		if (foo is Bar) {
			Baz ((Bar)foo);
			return (Bar)foo;
		}
		return null;
	}
}
");
        }

        [Test]
        public void TestInvaludValueType()
        {
            Analyze<CanBeReplacedWithTryCastAndCheckForNullAnalyzer>(@"
class Bar
{
	public int Baz (object foo)
	{
		if (foo is int) {
			Baz ((int)foo);
			return (int)foo;
		}
		return 0;
	}
}
");
        }
    }
}