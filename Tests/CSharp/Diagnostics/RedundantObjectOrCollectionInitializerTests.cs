using NUnit.Framework;
using RefactoringEssentials.CSharp.Diagnostics;

namespace RefactoringEssentials.Tests.CSharp.Diagnostics
{

    [TestFixture]
    [Ignore("TODO: Issue not ported yet")]
    public class RedundantObjectOrCollectionInitializerTests : CSharpDiagnosticTestBase
    {
        [Test]
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

        [Test]
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

        [Test]
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

        [Test]
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
