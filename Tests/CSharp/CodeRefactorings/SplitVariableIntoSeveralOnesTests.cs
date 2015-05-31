using NUnit.Framework;
using RefactoringEssentials.CSharp.CodeRefactorings;

namespace RefactoringEssentials.Tests.CSharp.CodeRefactorings
{
    [TestFixture]
    public class SplitDeclarationListTests : CSharpCodeRefactoringTestBase
    {
        [Test]
        public void TestLocalVariable()
        {
            Test<SplitDeclarationListCodeRefactoringProvider>(@"
class TestClass
{
	void TestMethod()
	{
		int $a, b, c;
	}
}", @"
class TestClass
{
	void TestMethod()
	{
		int a;
		int b;
		int c;
	}
}");
        }

        [Test]
        public void TestField()
        {
            Test<SplitDeclarationListCodeRefactoringProvider>(@"
class TestClass
{
	public int $a, b, c;
}", @"
class TestClass
{
	public int a;
	public int b;
	public int c;
}");
        }

        [Test]
        public void TestEvent()
        {
            Test<SplitDeclarationListCodeRefactoringProvider>(@"
class TestClass
{
	event System.EventHandler $a, b, c;
}", @"
class TestClass
{
	event System.EventHandler a;
	event System.EventHandler b;
	event System.EventHandler c;
}");
        }

        [Test]
        public void TestFixedField()
        {
            Test<SplitDeclarationListCodeRefactoringProvider>(@"
struct TestStruct
{
	unsafe fixed int $a[10], b[10], c[10];
}", @"
struct TestStruct
{
	unsafe fixed int a[10];
	unsafe fixed int b[10];
	unsafe fixed int c[10];
}");
        }

        [Test]
        public void TestVariableInFor()
        {
            TestWrongContext<SplitDeclarationListCodeRefactoringProvider>(@"
class TestClass
{
	void TestMethod ()
	{
		for (int a = 0, b = 0, $c = 0; a < 10; a++) {
		}
	}
}");
        }

        [Test]
        public void TestSingleVariable()
        {
            TestWrongContext<SplitDeclarationListCodeRefactoringProvider>(@"
class TestClass
{
	void TestMethod ()
	{
		int $a;
	}
}");
        }
    }
}
