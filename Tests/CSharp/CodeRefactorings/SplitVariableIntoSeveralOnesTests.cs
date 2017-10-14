using RefactoringEssentials.CSharp.CodeRefactorings;
using Xunit;

namespace RefactoringEssentials.Tests.CSharp.CodeRefactorings
{
    public class SplitDeclarationListTests : CSharpCodeRefactoringTestBase
    {
        [Fact]
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

        [Fact]
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

        [Fact]
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

        [Fact]
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

        [Fact]
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

        [Fact]
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
