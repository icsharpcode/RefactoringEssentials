using RefactoringEssentials.CSharp.Diagnostics;
using Xunit;

namespace RefactoringEssentials.Tests.CSharp.Diagnostics
{
    public class RedundantBoolCompareTests : CSharpDiagnosticTestBase
    {
        [Fact(Skip="TODO: Issue not ported yet")]
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

        [Fact(Skip="TODO: Issue not ported yet")]
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

        [Fact(Skip="TODO: Issue not ported yet")]
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

        [Fact(Skip="TODO: Issue not ported yet")]
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
