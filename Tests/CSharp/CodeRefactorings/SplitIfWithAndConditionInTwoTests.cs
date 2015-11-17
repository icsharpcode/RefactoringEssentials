using NUnit.Framework;
using RefactoringEssentials.CSharp.CodeRefactorings;

namespace RefactoringEssentials.Tests.CSharp.CodeRefactorings
{
    [TestFixture]
    public class SplitIfWithAndConditionInTwoTests : CSharpCodeRefactoringTestBase
    {
        [Test]
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

        [Test]
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

        [Test]
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


        [Test]
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

