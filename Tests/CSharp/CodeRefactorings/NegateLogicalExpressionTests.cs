using NUnit.Framework;
using RefactoringEssentials.CSharp.CodeRefactorings;

namespace RefactoringEssentials.Tests.CSharp.CodeRefactorings
{
    [TestFixture]
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
