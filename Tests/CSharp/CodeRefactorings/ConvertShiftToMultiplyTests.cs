using NUnit.Framework;
using RefactoringEssentials.CSharp.CodeRefactorings;

namespace RefactoringEssentials.Tests.CSharp.CodeRefactorings
{
    [TestFixture]
    public class ConvertShiftToMultiplyTests : CSharpCodeRefactoringTestBase
    {
        [Test]
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

        [Test]
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

