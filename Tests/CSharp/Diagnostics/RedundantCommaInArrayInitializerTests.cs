using NUnit.Framework;
using RefactoringEssentials.CSharp.Diagnostics;

namespace RefactoringEssentials.Tests.CSharp.Diagnostics
{
    [TestFixture]
    public class RedundantCommaInArrayInitializerTests : CSharpDiagnosticTestBase
    {
        [Test]
        public void Test()
        {
            var input = @"
class TestClass
{
	void TestMethod ()
	{
		var a = new int[] ${ 1, 2, }$;
	}
}";
            var output = @"
class TestClass
{
	void TestMethod ()
	{
		var a = new int[] { 1, 2 };
	}
}";
            Analyze<RedundantCommaInArrayInitializerAnalyzer>(input, output);
        }

        [Test]
        public void TestArrayInitializerDescription()
        {
            TestIssue<RedundantCommaInArrayInitializerAnalyzer>(@"
class TestClass
{
	void TestMethod ()
	{
		var a = new int[] { 1, 2, };
	}
}");
        }

        [Test]
        public void TestObjectInitializerDescription()
        {
            TestIssue<RedundantCommaInArrayInitializerAnalyzer>(@"
class TestClass
{
	int Prop { get; set; }
	void TestMethod ()
	{
		var a = new TestClass { Prop = 1, };
	}
}");
        }

        [Test]
        public void TestCollectionInitializerDescrition()
        {
            TestIssue<RedundantCommaInArrayInitializerAnalyzer>(@"
class TestClass
{
	void TestMethod ()
	{
		var a = new TestClass { 1, };
	}
}");
        }

        [Test]
        public void TestDisable()
        {
            var input = @"
class TestClass
{
	void TestMethod ()
	{            
// ReSharper disable once RedundantCommaInArrayInitializer
		var a = new TestClass { 1, };
	}
}";
            Analyze<RedundantCommaInArrayInitializerAnalyzer>(input);
        }
    }
}
