using RefactoringEssentials.CSharp.Diagnostics;
using Xunit;

namespace RefactoringEssentials.Tests.CSharp.Diagnostics
{
    public class RedundantPrivateInspectorTests : CSharpDiagnosticTestBase
    {
        [Fact]
        public void TestInspectorCase1()
        {
            Analyze<RedundantPrivateAnalyzer>(@"class Foo
{
	static $private$ int foo;
	$private$ void Bar (string str)
	{
	}
}", @"class Foo
{
    static int foo;
    void Bar(string str)
    {
    }
}");
        }


        [Fact]
        public void TestNestedClass()
        {
            Analyze<RedundantPrivateAnalyzer>(@"class Foo
{
	$private$ class Nested
	{
	}
}", @"class Foo
{
    class Nested
    {
    }
}");
        }

        [Fact]
        public void TestNoModifiers()
        {
            Analyze<RedundantPrivateAnalyzer>(@"class Foo
{
	static int foo;
	void Bar (string str)
	{
	}
}");
        }
    }
}