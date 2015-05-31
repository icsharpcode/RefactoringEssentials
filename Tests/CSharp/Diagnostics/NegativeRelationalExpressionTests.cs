using NUnit.Framework;
using RefactoringEssentials.CSharp.Diagnostics;

namespace RefactoringEssentials.Tests.CSharp.Diagnostics
{
    [TestFixture]
    [Ignore("TODO: Issue not ported yet")]
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

        [Test]
        public void TestEquality()
        {
            Test("==", "!=");
        }

        [Test]
        public void TestInEquality()
        {
            Test("!=", "==");
        }

        [Test]
        public void TestGreaterThan()
        {
            Test(">", "<=");
        }

        [Test]
        public void TestGreaterThanOrEqual()
        {
            Test(">=", "<");
        }

        [Test]
        public void TestLessThan()
        {
            Test("<", ">=");
        }

        [Test]
        public void TestLessThanOrEqual()
        {
            Test("<=", ">");
        }

        [Test]
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

        [Test]
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

        [Test]
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

        [Test]
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
