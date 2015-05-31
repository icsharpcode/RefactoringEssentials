using NUnit.Framework;
using RefactoringEssentials.CSharp.CodeRefactorings;

namespace RefactoringEssentials.Tests.CSharp.CodeRefactorings
{
    [TestFixture]
    public class JoinLocalVariableDeclarationAndAssignmentTests : CSharpCodeRefactoringTestBase
    {
        [Test]
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

        [Test]
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

        [Test]
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
