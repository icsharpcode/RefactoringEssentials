using RefactoringEssentials.CSharp.CodeRefactorings;
using Xunit;

namespace RefactoringEssentials.Tests.CSharp.CodeRefactorings
{
    public class JoinLocalVariableDeclarationAndAssignmentTests : CSharpCodeRefactoringTestBase
    {
        [Fact]
        public void Test()
        {
            Test<JoinLocalVariableDeclarationAndAssignmentCodeRefactoringProvider>(@"
class TestClass
{
	void TestMethod ()
	{
		int $a;
		a = 1;
	}
}", @"
class TestClass
{
	void TestMethod ()
	{
		int a = 1;
	}
}");
        }

        [Fact]
        public void TestWithComment()
        {
            Test<JoinLocalVariableDeclarationAndAssignmentCodeRefactoringProvider>(@"
class TestClass
{
	void TestMethod ()
	{
		// Some comment
		int $a;
		a = 1;
	}
}", @"
class TestClass
{
	void TestMethod ()
	{
		// Some comment
		int a = 1;
	}
}");
        }

        [Fact]
        public void TestDeclarationList()
        {
            Test<JoinLocalVariableDeclarationAndAssignmentCodeRefactoringProvider>(@"
class TestClass
{
	void TestMethod ()
	{
		int a, $b;
		b = 1;
	}
}", @"
class TestClass
{
	void TestMethod ()
	{
		int a, b = 1;
	}
}");
        }
    }
}
