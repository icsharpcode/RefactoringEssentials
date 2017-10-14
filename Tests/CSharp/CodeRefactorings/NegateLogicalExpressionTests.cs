using RefactoringEssentials.CSharp.CodeRefactorings;
using Xunit;

namespace RefactoringEssentials.Tests.CSharp.CodeRefactorings
{
    public class NegateLogicalExpressionTests : CSharpCodeRefactoringTestBase
    {
        public void Test(string op, string negatedOp)
        {
            Test<NegateLogicalExpressionCodeRefactoringProvider>(@"
class TestClass
{
	void Test ()
	{
		var b = 1 $" + op + @" 2;
	}
}", @"
class TestClass
{
	void Test ()
	{
		var b = 1 " + negatedOp + @" 2;
	}
}");
        }

        [Fact]
        public void TestEquality()
        {
            Test("==", "!=");
        }

        [Fact]
        public void TestInEquality()
        {
            Test("!=", "==");
        }

        [Fact]
        public void TestGreaterThan()
        {
            Test(">", "<=");
        }

        [Fact]
        public void TestGreaterThanOrEqual()
        {
            Test(">=", "<");
        }

        [Fact]
        public void TestLessThan()
        {
            Test("<", ">=");
        }

        [Fact]
        public void TestLessThanOrEqual()
        {
            Test("<=", ">");
        }

        [Fact]
        public void TestUnaryOperator()
        {
            Test<NegateLogicalExpressionCodeRefactoringProvider>(
                @"
class Foo 
{
	void Bar ()
	{
		var cond = $!(1 < 2);
	}
}
", @"
class Foo 
{
	void Bar ()
	{
		var cond = 1 < 2;
	}
}
");

        }
    }
}
