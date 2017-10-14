using RefactoringEssentials.CSharp.CodeRefactorings;
using Xunit;

namespace RefactoringEssentials.Tests.CSharp.CodeRefactorings
{
    public class SplitIfWithAndConditionInTwoTests : CSharpCodeRefactoringTestBase
    {
        [Fact]
        public void TestAndSimple()
        {
            Test<SplitIfWithAndConditionInTwoCodeRefactoringProvider>(@"
class Test
{
	void Foo (bool a, bool b)
	{
		if (a $&& b) {
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
            if (b)
            {
                return;
            }
        }
    }
}
");
        }

        [Fact]
        public void TestAndIfElse()
        {
            Test<SplitIfWithAndConditionInTwoCodeRefactoringProvider>(@"
class Test
{
	void Foo (bool a, bool b)
	{
		if (a $&& b)
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
            if (b)
            {
                return;
            }
            else
            {
                Something();
            }
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
        public void TestComplexAnd()
        {
            Test<SplitIfWithAndConditionInTwoCodeRefactoringProvider>(@"
class Test
{
	void Foo (bool a, bool b)
	{
		if (a && b $&& !a && !b)
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
        if (a && b)
        {
            if (!a && !b)
            {
                return;
            }
        }
    }
}
");
        }


        [Fact]
        public void TestInvalid()
        {
            TestWrongContext<SplitIfWithAndConditionInTwoCodeRefactoringProvider>(@"
class Test
{
	void Foo (bool a, bool b)
	{
		if (!b || a $&& b)
        {
			return;
		}
	}
}
");
        }
    }


}

