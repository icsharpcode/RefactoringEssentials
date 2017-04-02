using RefactoringEssentials.CSharp.Diagnostics;
using Xunit;

namespace RefactoringEssentials.Tests.CSharp.Diagnostics
{
    public class NegativeRelationalExpressionTests : CSharpDiagnosticTestBase
    {

        public void Test(string op, string negatedOp)
        {
            var input = @"
class TestClass
{
	void TestMethod ()
	{
		var x = !(1 " + op + @" 2);
	}
}";
            var output = @"
class TestClass
{
	void TestMethod ()
	{
		var x = 1 " + negatedOp + @" 2;
	}
}";
            Test<NegativeRelationalExpressionAnalyzer>(input, 1, output);
        }

        [Fact(Skip="TODO: Issue not ported yet")]
        public void TestEquality()
        {
            Test("==", "!=");
        }

        [Fact(Skip="TODO: Issue not ported yet")]
        public void TestInEquality()
        {
            Test("!=", "==");
        }

        [Fact(Skip="TODO: Issue not ported yet")]
        public void TestGreaterThan()
        {
            Test(">", "<=");
        }

        [Fact(Skip="TODO: Issue not ported yet")]
        public void TestGreaterThanOrEqual()
        {
            Test(">=", "<");
        }

        [Fact(Skip="TODO: Issue not ported yet")]
        public void TestLessThan()
        {
            Test("<", ">=");
        }

        [Fact(Skip="TODO: Issue not ported yet")]
        public void TestLessThanOrEqual()
        {
            Test("<=", ">");
        }

        [Fact(Skip="TODO: Issue not ported yet")]
        public void TestFloatingPoint()
        {
            var input = @"
class TestClass
{
	void TestMethod (double d)
	{
		var x = !(d > 0.1);
	}
}";
            Test<NegativeRelationalExpressionAnalyzer>(input, 0);
        }

        [Fact(Skip="TODO: Issue not ported yet")]
        public void TestFloatingPointEquality()
        {
            var input = @"
class TestClass
{
	void TestMethod (double d)
	{
		var x = !(d == 0.1);
	}
}";
            Test<NegativeRelationalExpressionAnalyzer>(input, 1);
        }

        [Fact(Skip="TODO: Issue not ported yet")]
        public void TestUserDefinedOperator()
        {
            var input = @"
struct LineChangeInfo
{
	public static bool operator ==(LineChangeInfo lhs, LineChangeInfo rhs)
	{
		return lhs.Equals(rhs);
	}
	
	public static bool operator !=(LineChangeInfo lhs, LineChangeInfo rhs)
	{
		return !(lhs == rhs);
	}
}";
            Test<NegativeRelationalExpressionAnalyzer>(input, 0);
        }

        [Fact(Skip="TODO: Issue not ported yet")]
        public void TestBug()
        {
            var input = @"
class TestClass
{
	void TestMethod (bool a, bool b)
	{
		var x = !(a || b);
	}
}";
            Analyze<NegativeRelationalExpressionAnalyzer>(input);
        }

    }
}
