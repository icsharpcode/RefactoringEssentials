using NUnit.Framework;
using RefactoringEssentials.CSharp.CodeRefactorings;

namespace RefactoringEssentials.Tests.CSharp.CodeRefactorings
{
    [TestFixture]
    public class SplitIfWithOrConditionInTwoTests : CSharpCodeRefactoringTestBase
    {

        [Test]
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

        [Test]
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


        [Test]
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

