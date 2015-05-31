using NUnit.Framework;
using RefactoringEssentials.CSharp.CodeRefactorings;

namespace RefactoringEssentials.Tests.CSharp.CodeRefactorings
{
    [TestFixture]
    public class ReplacePostfixExpressionWithAssignmentTests : CSharpCodeRefactoringTestBase
    {
        [Test]
        public void TestAdd()
        {
            Test<ReplacePostfixExpressionWithAssignmentCodeRefactoringProvider>(@"
class Test
{
	void Foo (int i)
	{
		$i++;
	}
}", @"
class Test
{
	void Foo (int i)
	{
        i += 1;
	}
}");
        }

        [Ignore("broken")]
        [Test]
        public void TestAddWithComment()
        {
            Test<ReplacePostfixExpressionWithAssignmentCodeRefactoringProvider>(@"
class Test
{
	void Foo (int i)
	{
        // Some comment
		$i++;
	}
}", @"
class Test
{
	void Foo (int i)
	{
        // Some comment
        i += 1;
	}
}");
        }

        [Test]
        public void TestSub()
        {
            Test<ReplacePostfixExpressionWithAssignmentCodeRefactoringProvider>(@"
class Test
{
	void Foo (int i)
	{
		$i--;
	}
}", @"
class Test
{
	void Foo (int i)
	{
        i -= 1;
	}
}");
        }
    }
}

