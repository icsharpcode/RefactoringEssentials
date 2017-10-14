using RefactoringEssentials.CSharp.CodeRefactorings;
using Xunit;

namespace RefactoringEssentials.Tests.CSharp.CodeRefactorings
{
    public class NegateIsExpressionTests : CSharpCodeRefactoringTestBase
    {
        [Fact]
        public void TestSimpleCase()
        {
            Test<NegateIsExpressionCodeRefactoringProvider>(@"
class TestClass
{
	void Test (object x)
	{
		var b = x $is TestClass;
	}
}", @"
class TestClass
{
	void Test (object x)
	{
		var b = !(x is TestClass);
	}
}");
        }

        [Fact]
        public void TestReverse()
        {
            Test<NegateIsExpressionCodeRefactoringProvider>(@"
class TestClass
{
	void Test (object x)
	{
		var b = !(x $is TestClass);
	}
}", @"
class TestClass
{
	void Test (object x)
	{
		var b = x is TestClass;
	}
}");
        }

    }
}

