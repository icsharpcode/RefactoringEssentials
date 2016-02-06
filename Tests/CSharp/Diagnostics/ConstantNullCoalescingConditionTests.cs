using NUnit.Framework;
using RefactoringEssentials.CSharp.Diagnostics;

namespace RefactoringEssentials.Tests.CSharp.Diagnostics
{
    [TestFixture]
    public class ConstantNullCoalescingConditionTests : CSharpDiagnosticTestBase
    {
        [Test]
        public void TestNullRightSide()
        {
            Analyze<ConstantNullCoalescingConditionAnalyzer>(@"
class TestClass
{
	void Foo()
	{
		object o = new object() ?? $null$;
	}
}", @"
class TestClass
{
	void Foo()
	{
		object o = new object();
	}
}");
        }

        [Test]
        public void TestNullLeftSide()
        {
            Analyze<ConstantNullCoalescingConditionAnalyzer>(@"
class TestClass
{
	void Foo()
	{
		object o = $null$ ?? new object();
	}
}", @"
class TestClass
{
	void Foo()
	{
		object o = new object();
	}
}");
        }

        [Test]
        public void TestEqualExpressions()
        {
            Analyze<ConstantNullCoalescingConditionAnalyzer>(@"
class TestClass
{
	void Foo()
	{
		object o = new object() ?? $new object()$;
	}
}", @"
class TestClass
{
	void Foo()
	{
		object o = new object();
	}
}");
        }

        [Test]
        public void TestSmartUsage()
        {
            //Previously, this was a "TestWrongContext".
            //However, since smart null coallescing was introduced, this can now be
            //detected as redundant
            Analyze<ConstantNullCoalescingConditionAnalyzer>(@"
class TestClass
{
	void Foo()
	{
		object o = new object() ?? $""""$;
	}
}", @"
class TestClass
{
	void Foo()
	{
		object o = new object();
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

        [Test]
        public void TestDisable()
        {
            Analyze<ConstantNullCoalescingConditionAnalyzer>(@"
class TestClass
{
	void Foo()
	{
#pragma warning disable " + CSharpDiagnosticIDs.ConstantNullCoalescingConditionAnalyzerID + @"
		object o = new object() ?? null;
	}
}");
        }
    }
}