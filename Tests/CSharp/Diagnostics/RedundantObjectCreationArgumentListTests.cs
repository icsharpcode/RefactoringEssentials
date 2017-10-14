using RefactoringEssentials.CSharp.Diagnostics;
using Xunit;

namespace RefactoringEssentials.Tests.CSharp.Diagnostics
{
    public class RedundantObjectCreationArgumentListTests : CSharpDiagnosticTestBase
    {
        [Fact]
        public void Test()
        {
            var input = @"
class TestClass
{
	public int Prop { get; set; }
	void TestMethod ()
	{
		var x = new TestClass $()$ {
			Prop = 1
		};
	}
}";
            var output = @"
class TestClass
{
	public int Prop { get; set; }
	void TestMethod ()
	{
		var x = new TestClass  {
			Prop = 1
		};
	}
}";
            Analyze<RedundantObjectCreationArgumentListAnalyzer>(input, output);
        }

        [Fact]
        public void TestNoIssue()
        {
            var input = @"
class TestClass
{
	public TestClass () { }
	public TestClass (int i) { }

	void TestMethod ()
	{
		var x = new TestClass { };
		var y = new TestClass (1) { };
	}
}";
            Analyze<RedundantObjectCreationArgumentListAnalyzer>(input);
        }

        [Fact]
        public void TestDisable()
        {
            var input = @"
class TestClass
{
	public int Prop { get; set; }
	void TestMethod ()
	{
		// ReSharper disable once RedundantEmptyObjectCreationArgumentList
#pragma warning disable " + CSharpDiagnosticIDs.RedundantObjectCreationArgumentListAnalyzerID + @"
		var x = new TestClass () {
			Prop = 1
		};
	}
}";
            Analyze<RedundantObjectCreationArgumentListAnalyzer>(input);
        }

    }

}
