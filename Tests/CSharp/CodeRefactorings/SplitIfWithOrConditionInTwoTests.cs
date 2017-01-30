using RefactoringEssentials.CSharp.CodeRefactorings;
using Xunit;

namespace RefactoringEssentials.Tests.CSharp.CodeRefactorings
{
    public class SplitIfWithOrConditionInTwoTests : CSharpCodeRefactoringTestBase
    {

        [Fact]
        public void TestOrSimple()
        {
            Test<SplitIfWithOrConditionInTwoCodeRefactoringProvider>(@"
class Test
{
	void Foo (bool a, bool b)
	{
		if (a $|| b) {
			return;
		}
	}
}
", @"
class Test
{
	void Foo (bool a, bool b)
	{
        if (a)
        {
            return;
        }
        else if (b)
        {
            return;
        }
    }
}
");
        }

        [Fact]
        public void TestOrIfElse()
        {
            Test<SplitIfWithOrConditionInTwoCodeRefactoringProvider>(@"
class Test
{
	void Foo (bool a, bool b)
	{
		if (a $|| b)
        {
			return;
		}
        else
        {
			Something ();
		}
	}
}
", @"
class Test
{
	void Foo (bool a, bool b)
	{
        if (a)
        {
            return;
        }
        else if (b)
        {
            return;
        }
        else
        {
            Something();
        }
    }
}
");
        }


        [Fact]
        public void TestAndOr()
        {
            Test<SplitIfWithOrConditionInTwoCodeRefactoringProvider>(@"
class Test
{
	void Foo (bool a, bool b)
	{
		if (!b $|| a && b)
        {
			return;
		}
	}
}
", @"
class Test
{
	void Foo (bool a, bool b)
	{
        if (!b)
        {
            return;
        }
        else if (a && b)
        {
            return;
        }
    }
}
");
        }
    }


}

