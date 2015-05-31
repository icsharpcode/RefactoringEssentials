using NUnit.Framework;
using RefactoringEssentials.CSharp.Diagnostics;

namespace RefactoringEssentials.Tests.CSharp.Diagnostics
{
    [TestFixture]
    [Ignore("TODO: Issue not ported yet")]
    public class RedundantObjectCreationArgumentListTests : CSharpDiagnosticTestBase
    {
        [Test]
        public void Test()
        {
            var input = @"
class TestClass
{
	public int Prop { get; set; }
	void TestMethod ()
	{
		var x = new TestClass () {
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
		var x = new TestClass {
			Prop = 1
		};
	}
}";
            Test<RedundantObjectCreationArgumentListAnalyzer>(input, 1, output);
        }

        [Test]
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
            Test<RedundantObjectCreationArgumentListAnalyzer>(input, 0);
        }

        [Test]
        public void TestDisable()
        {
            var input = @"
class TestClass
{
	public int Prop { get; set; }
	void TestMethod ()
	{
		// ReSharper disable once RedundantEmptyObjectCreationArgumentList
		var x = new TestClass () {
			Prop = 1
		};
	}
}";
            Test<RedundantObjectCreationArgumentListAnalyzer>(input, 0);
        }

    }

}
