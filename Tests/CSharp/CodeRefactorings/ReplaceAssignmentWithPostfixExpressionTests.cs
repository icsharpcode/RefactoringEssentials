using NUnit.Framework;
using RefactoringEssentials.CSharp.CodeRefactorings;

namespace RefactoringEssentials.Tests.CSharp.CodeRefactorings
{
    [TestFixture]
    public class ReplaceAssignmentWithPostfixExpressionTests : CSharpCodeRefactoringTestBase
    {
        [Test]
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

        [Ignore("broken")]
        [Test]
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

        [Test]
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


        [Test]
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

