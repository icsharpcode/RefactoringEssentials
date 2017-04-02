using RefactoringEssentials.CSharp.CodeRefactorings;
using Xunit;

namespace RefactoringEssentials.Tests.CSharp.CodeRefactorings
{
    public class ReplacePostfixExpressionWithAssignmentTests : CSharpCodeRefactoringTestBase
    {
        [Fact]
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

        [Fact]
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

        [Fact]
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

