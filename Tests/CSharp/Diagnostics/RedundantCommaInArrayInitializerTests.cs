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
    void TestMethod()
    {
        var a = new int[] { 1, 2$,$ };
    }
}";
            var output = @"
class TestClass
{
    void TestMethod()
    {
        var a = new int[] { 1, 2 };
    }
}";
            Analyze<RedundantCommaInArrayInitializerAnalyzer>(input, output);
        }

        [Test]
        public void TestArrayInitializerNoRedundance()
        {
            Analyze<RedundantCommaInArrayInitializerAnalyzer>(@"
class TestClass
{
	void TestMethod()
	{
		var a = new int[] { 1, 2 };
	}
}");
        }

        [Test]
        public void TestArrayInitializerDescription()
        {
            Analyze<RedundantCommaInArrayInitializerAnalyzer>(@"
class TestClass
{
	void TestMethod()
	{
		var a = new int[] { 1, 2$,$ };
	}
}");
        }

        [Test]
        public void TestObjectInitializerDescription()
        {
            Analyze<RedundantCommaInArrayInitializerAnalyzer>(@"
class TestClass
{
	int Prop { get; set; }
	void TestMethod ()
	{
		var a = new TestClass { Prop = 1$,$ };
	}
}");
        }

        [Test]
        public void TestCollectionInitializerDescrition()
        {
            Analyze<RedundantCommaInArrayInitializerAnalyzer>(@"
class TestClass
{
	void TestMethod ()
	{
		var a = new TestClass { 1$,$ };
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
#pragma warning disable " + CSharpDiagnosticIDs.RedundantCommaInArrayInitializerAnalyzerID + @"
		var a = new TestClass { 1, };
	}
}";
            Analyze<RedundantCommaInArrayInitializerAnalyzer>(input);
        }
    }
}
