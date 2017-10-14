using RefactoringEssentials.CSharp.CodeRefactorings;
using Xunit;

namespace RefactoringEssentials.Tests.CSharp.CodeRefactorings
{
    public class ReplaceAssignmentWithPostfixExpressionTests : CSharpCodeRefactoringTestBase
    {
        [Fact]
        public void TestAdd()
        {
            Test<ReplaceAssignmentWithPostfixExpressionCodeRefactoringProvider>(@"
class Test
{
	void Foo (int i)
	{
		i $+= 1;
	}
}", @"
class Test
{
	void Foo (int i)
	{
        i++;
	}
}");
        }

        [Fact]
        public void TestAddWithComment()
        {
            Test<ReplaceAssignmentWithPostfixExpressionCodeRefactoringProvider>(@"
class Test
{
	void Foo (int i)
	{
        // Some comment
		i $+= 1;
	}
}", @"
class Test
{
	void Foo (int i)
	{
        // Some comment
        i++;
	}
}");
        }

        [Fact]
        public void TestSub()
        {
            Test<ReplaceAssignmentWithPostfixExpressionCodeRefactoringProvider>(@"
class Test
{
	void Foo (int i)
	{
		i $-= 1;
	}
}", @"
class Test
{
	void Foo (int i)
	{
        i--;
	}
}");
        }


        [Fact]
        public void TestAddCase2()
        {
            Test<ReplaceAssignmentWithPostfixExpressionCodeRefactoringProvider>(@"
class Test
{
	void Foo (int i)
	{
		i $= i + 1;
	}
}", @"
class Test
{
	void Foo (int i)
	{
        i++;
	}
}");
        }

    }
}

