using NUnit.Framework;
using RefactoringEssentials.CSharp.Diagnostics;

namespace RefactoringEssentials.Tests.CSharp.Diagnostics
{
    [TestFixture]
    public class ArrayCreationCanBeReplacedWithArrayInitializerTests : CSharpDiagnosticTestBase
    {
        [Test]
        public void TestVariableDeclarationCase1()
        {
            Analyze<ArrayCreationCanBeReplacedWithArrayInitializerAnalyzer>(@"
class TestClass
{
	void TestMethod ()
	{
		int[] foo = $new int[] ${1, 2, 3};
	}
}", @"
class TestClass
{
	void TestMethod ()
	{
		int[] foo = {1, 2, 3};
	}
}");
        }

        [Test]
        public void TestFieldCase1()
        {
            Analyze<ArrayCreationCanBeReplacedWithArrayInitializerAnalyzer>(@"
class TestClass
{
	int[] foo = $new int[] ${1, 2, 3};
}", @"
class TestClass
{
	int[] foo = {1, 2, 3};
}");
        }


        [Test]
        public void TestVariableDeclarationCase2()
        {
            Analyze<ArrayCreationCanBeReplacedWithArrayInitializerAnalyzer>(@"
class TestClass
{
	void TestMethod ()
	{
		int[] foo = $new [] ${1, 2, 3};
	}
}", @"
class TestClass
{
	void TestMethod ()
	{
		int[] foo = {1, 2, 3};
	}
}");
        }

        [Test]
        public void TestFieldCase2()
        {
            Analyze<ArrayCreationCanBeReplacedWithArrayInitializerAnalyzer>(@"
class TestClass
{
	public int[] filed = $new [] ${1,2,3};
}", @"
class TestClass
{
	public int[] filed = {1,2,3};
}");
        }

        [Test]
        public void TestNoProblem1()
        {
            Analyze<ArrayCreationCanBeReplacedWithArrayInitializerAnalyzer>(@"
class TestClass
{
	void TestMethod ()
	{
		var foo = new[] {1, 2, 3};
	}
}");
        }

        [Test]
        public void TestNoProblem2()
        {
            Analyze<ArrayCreationCanBeReplacedWithArrayInitializerAnalyzer>(@"
class TestClass
{
	void TestMethod ()
	{
		var foo = new int[] {1, 2, 3};
	}
}");
        }

        [Test]
        public void TestNoProblem3()
        {
            Analyze<ArrayCreationCanBeReplacedWithArrayInitializerAnalyzer>(@"
class TestClass
{
	Void Foo(int[] a)
	{}
	void TestMethod ()
	{
		Foo(new int[]{1,2,3});
	}
}");
        }
    }
}
