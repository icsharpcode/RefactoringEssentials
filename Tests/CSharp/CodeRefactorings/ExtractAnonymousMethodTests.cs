using NUnit.Framework;
using RefactoringEssentials.CSharp.CodeRefactorings;

namespace RefactoringEssentials.Tests.CSharp.CodeRefactorings
{
    [Ignore("Needs insertion cursor mode.")]
    [TestFixture]
    public class ExtractAnonymousMethodTests : CSharpCodeRefactoringTestBase
    {
        [Test]
        public void TestLambdaWithBodyStatement()
        {
            Test<ExtractAnonymousMethodAction>(@"
class TestClass
{
	void TestMethod ()
	{
		System.Action<int> a = i $=>  { i++; };
	}
}", @"
class TestClass
{
	void Method (int i)
	{
		i++;
	}
	void TestMethod ()
	{
		System.Action<int> a = Method;
	}
}");
        }

        [Test]
        public void TestLambdaWithBodyExpression()
        {
            Test<ExtractAnonymousMethodAction>(@"
class TestClass
{
	void TestMethod ()
	{
		System.Action<int> a = i $=> i++;
	}
}", @"
class TestClass
{
	void Method (int i)
	{
		i++;
	}
	void TestMethod ()
	{
		System.Action<int> a = Method;
	}
}");

            Test<ExtractAnonymousMethodAction>(@"
class TestClass
{
	void TestMethod ()
	{
		System.Func<int> a = () $=> 1;
	}
}", @"
class TestClass
{
	int Method ()
	{
		return 1;
	}
	void TestMethod ()
	{
		System.Func<int> a = Method;
	}
}");
        }

        [Test]
        public void TestAnonymousMethod()
        {
            Test<ExtractAnonymousMethodAction>(@"
class TestClass
{
	void TestMethod ()
	{
		System.Action<int> a = $delegate (int i) { i++; };
	}
}", @"
class TestClass
{
	void Method (int i)
	{
		i++;
	}
	void TestMethod ()
	{
		System.Action<int> a = Method;
	}
}");
        }

        [Test]
        public void TestContainLocalReference()
        {
            TestWrongContext<ExtractAnonymousMethodAction>(@"
class TestClass
{
	void TestMethod ()
	{
		int j = 1;
		System.Func<int, int> a = $delegate (int i) { return i + j; };
	}
}");
        }

        [Test]
        public void TestLambdaInField()
        {
            Test<ExtractAnonymousMethodAction>(@"
class TestClass
{
	System.Action<int> a = i $=>  { i++; };
}", @"
class TestClass
{
	void Method (int i)
	{
		i++;
	}
	System.Action<int> a = Method;
}");
        }
    }
}
