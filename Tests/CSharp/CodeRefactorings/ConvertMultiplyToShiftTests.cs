using NUnit.Framework;
using RefactoringEssentials.CSharp.CodeRefactorings;

namespace RefactoringEssentials.Tests.CSharp.CodeRefactorings
{
    [TestFixture]
    public class ConvertMultiplyToShiftTests : CSharpCodeRefactoringTestBase
    {
        [Test]
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

        [Test]
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

        [Test]
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

