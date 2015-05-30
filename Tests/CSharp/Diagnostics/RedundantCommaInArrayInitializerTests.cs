using NUnit.Framework;
using RefactoringEssentials.CSharp.Diagnostics;

namespace RefactoringEssentials.Tests.CSharp.Diagnostics
{
    [TestFixture]
    [Ignore("TODO: Issue not ported yet")]
    public class RedundantCommaInArrayInitializerTests : InspectionActionTestBase
    {
        [Test]
        public void Test()
        {
            var input = @"
class TestClass
{
	void TestMethod ()
	{
		var a = new int[] { 1, 2, };
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
            Test<RedundantCommaInArrayInitializerAnalyzer>(input, 1, output);
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
