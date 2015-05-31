using NUnit.Framework;
using RefactoringEssentials.CSharp.CodeRefactorings;

namespace RefactoringEssentials.Tests.CSharp.CodeRefactorings
{
    [TestFixture]
    public class ConvertLambdaStatementToExpressionTests : CSharpCodeRefactoringTestBase
    {
        [Test]
        public void TestReturn()
        {
            Test<ConvertLambdaStatementToExpressionCodeRefactoringProvider>(@"
class TestClass
{
	void TestMethod ()
	{
		System.Func<int, int> f = i $=> {
			return i + 1;
		};
	}
}", @"
class TestClass
{
	void TestMethod ()
	{
		System.Func<int, int> f = i => i + 1;
	}
}");
        }

        [Test]
        public void TestParenthesizedLambdaReturn()
        {
            Test<ConvertLambdaStatementToExpressionCodeRefactoringProvider>(@"
class TestClass
{
	void TestMethod ()
	{
		System.Func<int, int> f = (i) $=> {
			return i + 1;
		};
	}
}", @"
class TestClass
{
	void TestMethod ()
	{
		System.Func<int, int> f = (i) => i + 1;
	}
}");
        }

        [Test]
        public void TestExpressionStatement()
        {
            Test<ConvertLambdaStatementToExpressionCodeRefactoringProvider>(@"
class TestClass
{
	void TestMethod ()
	{
		System.Action<int> f = i $=> {
			System.Console.Write(i);
		};
	}
}", @"
class TestClass
{
	void TestMethod ()
	{
		System.Action<int> f = i => System.Console.Write(i);
	}
}");
        }

        [Test]
        public void TestExpressionStatementWithComment()
        {
            Test<ConvertLambdaStatementToExpressionCodeRefactoringProvider>(@"
class TestClass
{
	void TestMethod ()
	{
		// Some comment
		System.Action<int> f = i $=> {
			System.Console.Write(i);
		};
	}
}", @"
class TestClass
{
	void TestMethod ()
	{
		// Some comment
		System.Action<int> f = i => System.Console.Write(i);
	}
}");
        }
    }
}
