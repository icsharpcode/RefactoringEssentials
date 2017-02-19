using RefactoringEssentials.CSharp.Diagnostics;
using Xunit;

namespace RefactoringEssentials.Tests.CSharp.Diagnostics
{

    public class RedundantObjectOrCollectionInitializerTests : CSharpDiagnosticTestBase
    {
        [Fact(Skip="TODO: Issue not ported yet")]
        public void Test()
        {
            var input = @"
class TestClass
{
	void TestMethod ()
	{
		var x = new TestClass () { };
	}
}";
            var output = @"
class TestClass
{
	void TestMethod ()
	{
		var x = new TestClass ();
	}
}";
            Test<RedundantObjectOrCollectionInitializerAnalyzer>(input, 1, output);
        }

        [Fact(Skip="TODO: Issue not ported yet")]
        public void TestDisable()
        {
            Analyze<RedundantObjectOrCollectionInitializerAnalyzer>(@" class TestClass
    {
        void TestMethod()
        {
// ReSharper disable once RedundantEmptyObjectOrCollectionInitializer
            var x = new TestClass() { };
        }
    }");
        }

        [Fact(Skip="TODO: Issue not ported yet")]
        public void TestNoArgumentList()
        {
            var input = @"
class TestClass
{
	void TestMethod ()
	{
		var x = new TestClass { };
	}
}";
            var output = @"
class TestClass
{
	void TestMethod ()
	{
		var x = new TestClass ();
	}
}";
            Test<RedundantObjectOrCollectionInitializerAnalyzer>(input, 1, output);
        }

        [Fact(Skip="TODO: Issue not ported yet")]
        public void TestNoIssue()
        {
            var input = @"
class TestClass
{
	public int Prop { get; set; }
	void TestMethod ()
	{
		var x = new TestClass ();
		var y = new TestClass () { Prop = 1 };
	}
}";
            Test<RedundantObjectOrCollectionInitializerAnalyzer>(input, 0);
        }
    }
}
