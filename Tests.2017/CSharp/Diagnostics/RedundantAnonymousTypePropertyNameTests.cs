using RefactoringEssentials.CSharp.Diagnostics;
using Xunit;

namespace RefactoringEssentials.Tests.CSharp.Diagnostics
{
    public class RedundantAnonymousTypePropertyNameTests : CSharpDiagnosticTestBase
    {
        [Fact]
        public void TestSimpleCase()
        {
            Analyze<RedundantAnonymousTypePropertyNameAnalyzer>(@"
class FooBar
{
	public int Foo;
	public int Bar;
}
class TestClass
{
	public void Test(FooBar f)
	{
		var n = new { $Foo =$ f.Foo, b = 12 };
	}
}", @"
class FooBar
{
	public int Foo;
	public int Bar;
}
class TestClass
{
	public void Test(FooBar f)
	{
		var n = new { f.Foo, b = 12 };
	}
}");
        }

        [Fact]
        public void TestIgnoredCase()
        {
            Analyze<RedundantAnonymousTypePropertyNameAnalyzer>(@"
class FooBar
{
	public int Foo;
	public int Bar;
}
class TestClass
{
	public void Test(FooBar f)
	{
		var foo = f.Foo;
		var n = new { Foo = foo, b = 12 };
	}
}");
        }

        [Fact]
        public void TestDisable()
        {
            Analyze<RedundantAnonymousTypePropertyNameAnalyzer>(@"
class FooBar
{
	public int Foo;
	public int Bar;
}
class TestClass
{
	public void Test(FooBar f)
	{
#pragma warning disable " + CSharpDiagnosticIDs.RedundantAnonymousTypePropertyNameAnalyzerID + @"
		var n = new { Foo = f.Foo, b = 12 };
	}
}");
        }


    }
}

