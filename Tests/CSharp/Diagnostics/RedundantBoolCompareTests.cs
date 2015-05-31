using NUnit.Framework;
using RefactoringEssentials.CSharp.Diagnostics;

namespace RefactoringEssentials.Tests.CSharp.Diagnostics
{
    [TestFixture]
    [Ignore("TODO: Issue not ported yet")]
    public class RedundantBoolCompareTests : CSharpDiagnosticTestBase
    {
        [Test]
        public void Test()
        {
            var input = @"
class TestClass
{
	void TestMethod (bool x)
	{
		bool y;
		y = x == true;
		y = x == false;
		y = x != false;
		y = x != true;
	}
}";
            var output = @"
class TestClass
{
	void TestMethod (bool x)
	{
		bool y;
		y = x;
		y = !x;
		y = x;
		y = !x;
	}
}";
            Test<RedundantBoolCompareAnalyzer>(input, 4, output);
        }

        [Test]
        public void TestInsertParentheses()
        {
            var input = @"
class TestClass
{
	void TestMethod ()
	{
		bool y = 2 > 1 == false;
	}
}";
            var output = @"
class TestClass
{
	void TestMethod ()
	{
		bool y = !(2 > 1);
	}
}";
            Test<RedundantBoolCompareAnalyzer>(input, 1, output);
        }

        [Test]
        public void TestInvalid()
        {
            Analyze<RedundantBoolCompareAnalyzer>(@"
class TestClass
{
	void TestMethod (bool? x)
	{
		bool y;
		y = x == true;
		y = x == false;
		y = x != false;
		y = x != true;
	}
}");
        }

        [Test]
        public void TestNullable()
        {
            var input = @"
class TestClass
{
	void TestMethod (bool? x)
	{
		var y = x == false;
	}
}";
            Test<RedundantBoolCompareAnalyzer>(input, 0);
        }
    }
}
