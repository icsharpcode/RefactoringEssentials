using RefactoringEssentials.CSharp.CodeRefactorings;
using Xunit;

namespace RefactoringEssentials.Tests.CSharp.CodeRefactorings
{
    public class ConvertShiftToMultiplyTests : CSharpCodeRefactoringTestBase
    {
        [Fact]
        public void TestShiftLeft()
        {
            Test<ConvertShiftToMultiplyCodeRefactoringProvider>(@"
class TestClass
{
	int TestMethod (int i)
	{
		return i $<< 8;
	}
}", @"
class TestClass
{
	int TestMethod (int i)
	{
		return i * 256;
	}
}");
        }

        [Fact]
        public void TestShiftRight()
        {
            Test<ConvertShiftToMultiplyCodeRefactoringProvider>(@"
class TestClass
{
	int TestMethod (int i)
	{
		return i $>> 4;
	}
}", @"
class TestClass
{
	int TestMethod (int i)
	{
		return i / 16;
	}
}");
        }
    }
}

