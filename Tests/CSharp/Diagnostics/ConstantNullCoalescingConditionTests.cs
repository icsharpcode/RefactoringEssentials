using RefactoringEssentials.CSharp.Diagnostics;
using Xunit;

namespace RefactoringEssentials.Tests.CSharp.Diagnostics
{
    public class ConstantNullCoalescingConditionTests : CSharpDiagnosticTestBase
    {
        [Fact]
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

        [Fact]
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

        [Fact]
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

        [Fact]
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

        [Fact]
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

        [Fact]
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

        // "RECS0098: Remove redundant right side" reports incorrectly when left side is an equation
        [Fact]
        public void TestIssue172()
        {
            Analyze<ConstantNullCoalescingConditionAnalyzer>(@"
class TestClass
{
    void Foo()
    {
        decimal? a = null;
        decimal? b = 2;
        decimal? c = (a + 1) ?? b;
    }
}");
        }

        [Fact]
        public void TestNullableCreationOnLeftSide()
        {
            Analyze<ConstantNullCoalescingConditionAnalyzer>(@"
class TestClass
{
	void Foo()
	{
		int i = new int?() ?? 1;
	}
}");
        }
    }
}