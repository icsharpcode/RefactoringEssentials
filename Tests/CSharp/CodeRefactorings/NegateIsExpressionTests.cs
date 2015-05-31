using NUnit.Framework;
using RefactoringEssentials.CSharp.CodeRefactorings;

namespace RefactoringEssentials.Tests.CSharp.CodeRefactorings
{
    [TestFixture]
    public class NegateIsExpressionTests : CSharpCodeRefactoringTestBase
    {
        [Test]
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

        [Test]
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

