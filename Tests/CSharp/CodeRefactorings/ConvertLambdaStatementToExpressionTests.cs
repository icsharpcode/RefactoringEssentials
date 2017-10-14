using RefactoringEssentials.CSharp.CodeRefactorings;
using Xunit;

namespace RefactoringEssentials.Tests.CSharp.CodeRefactorings
{
    public class ConvertLambdaStatementToExpressionTests : CSharpCodeRefactoringTestBase
    {
        [Fact]
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

        [Fact]
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

        [Fact]
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

        [Fact]
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
