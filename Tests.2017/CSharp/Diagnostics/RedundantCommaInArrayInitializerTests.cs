using RefactoringEssentials.CSharp.Diagnostics;
using Xunit;

namespace RefactoringEssentials.Tests.CSharp.Diagnostics
{
    public class RedundantCommaInArrayInitializerTests : CSharpDiagnosticTestBase
    {
        [Fact]
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

        [Fact]
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

        [Fact]
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

        [Fact]
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

        [Fact]
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

        [Fact]
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

        [Fact]
        public void TestPreserveTrivia()
        {
            Analyze<RedundantCommaInArrayInitializerAnalyzer>(@"
class TestClass
{
	void TestMethod()
	{
		var a = new int[] {
			1,
			2$,$
		};
	}
}", @"
class TestClass
{
	void TestMethod()
	{
		var a = new int[] {
			1,
			2
		};
	}
}");
        }
    }
}
