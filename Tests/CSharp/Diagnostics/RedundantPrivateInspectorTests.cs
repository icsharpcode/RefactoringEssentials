using NUnit.Framework;
using RefactoringEssentials.CSharp.Diagnostics;

namespace RefactoringEssentials.Tests.CSharp.Diagnostics
{
    [TestFixture]
    public class RedundantPrivateInspectorTests : CSharpDiagnosticTestBase
    {
        [Test]
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


        [Test]
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

        [Test]
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