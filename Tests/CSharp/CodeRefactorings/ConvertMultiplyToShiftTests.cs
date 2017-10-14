using RefactoringEssentials.CSharp.CodeRefactorings;
using Xunit;

namespace RefactoringEssentials.Tests.CSharp.CodeRefactorings
{
    public class ConvertMultiplyToShiftTests : CSharpCodeRefactoringTestBase
    {
        [Fact]
        public void TestMultiply()
        {
            Test<ConvertMultiplyToShiftCodeRefactoringProvider>(@"
class TestClass
{
	int TestMethod (int i)
	{
		return i $* 256;
	}
}", @"
class TestClass
{
	int TestMethod (int i)
	{
		return i << 8;
	}
}");
        }

        [Fact]
        public void TestDivide()
        {
            Test<ConvertMultiplyToShiftCodeRefactoringProvider>(@"
class TestClass
{
	int TestMethod (int i)
	{
		return i $/ 16;
	}
}", @"
class TestClass
{
	int TestMethod (int i)
	{
		return i >> 4;
	}
}");
        }

        [Fact]
        public void TestInvaid()
        {
            TestWrongContext<ConvertMultiplyToShiftCodeRefactoringProvider>(@"
class TestClass
{
	int TestMethod (int i)
	{
		return i $* 255;
	}
}");
        }
    }
}

