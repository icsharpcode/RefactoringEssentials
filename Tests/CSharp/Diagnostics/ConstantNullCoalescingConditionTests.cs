using NUnit.Framework;
using RefactoringEssentials.CSharp.Diagnostics;

namespace RefactoringEssentials.Tests.CSharp.Diagnostics
{
    [Ignore("TODO roslyn port.")]
    [TestFixture]
    public class ConstantNullCoalescingConditionTests : CSharpDiagnosticTestBase
    {
        [Test]
        public void TestNullRightSide()
        {
            Test<ConstantNullCoalescingConditionAnalyzer>(@"
class TestClass
{
	void Foo()
	{
		object o = new object () ?? null;
	}
}", @"
class TestClass
{
	void Foo()
	{
		object o = new object ();
	}
}");
        }

        [Test]
        public void TestNullLeftSide()
        {
            Test<ConstantNullCoalescingConditionAnalyzer>(@"
class TestClass
{
	void Foo()
	{
		object o = null ?? new object ();
	}
}", @"
class TestClass
{
	void Foo()
	{
		object o = new object ();
	}
}");
        }

        [Test]
        public void TestEqualExpressions()
        {
            Test<ConstantNullCoalescingConditionAnalyzer>(@"
class TestClass
{
	void Foo()
	{
		object o = new object () ?? new object ();
	}
}", @"
class TestClass
{
	void Foo()
	{
		object o = new object ();
	}
}");
        }

        [Test]
        public void TestSmartUsage()
        {
            //Previously, this was a "TestWrongContext".
            //However, since smart null coallescing was introduced, this can now be
            //detected as redundant
            Test<ConstantNullCoalescingConditionAnalyzer>(@"
class TestClass
{
	void Foo()
	{
		object o = new object () ?? """";
	}
}", @"
class TestClass
{
	void Foo()
	{
		object o = new object ();
	}
}");
        }

        [Test]
        public void TestSmartUsageInParam()
        {
            Analyze<ConstantNullCoalescingConditionAnalyzer>(@"
class TestClass
{
	void Foo(object o)
	{
		object p = o ?? """";
	}
}");
        }

        [Ignore("enable again")]
        [Test]
        public void TestDisable()
        {
            Analyze<ConstantNullCoalescingConditionAnalyzer>(@"
class TestClass
{
	void Foo()
	{
		// ReSharper disable once ConstantNullCoalescingCondition
		object o = new object () ?? null;
	}
}");
        }
    }
}