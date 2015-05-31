using NUnit.Framework;
using RefactoringEssentials.CSharp.CodeRefactorings;

namespace RefactoringEssentials.Tests.CSharp.CodeRefactorings
{
    [TestFixture]
    public class ConvertDoWhileToWhileLoopTests : CSharpCodeRefactoringTestBase
    {
        [Test]
        public void TestSimple()
        {
            Test<ConvertDoWhileToWhileLoopCodeRefactoringProvider>(@"
class Foo {
	void TestMethod() {
		int x = 1;
		do
			x++;
		$while (x != 1);
	}
}", @"
class Foo {
	void TestMethod() {
		int x = 1;
        while (x != 1)
            x++;
    }
}");
        }

        [Test]
        public void TestSimpleWithComment1()
        {
            Test<ConvertDoWhileToWhileLoopCodeRefactoringProvider>(@"
class Foo {
	void TestMethod() {
		int x = 1;
		// Some comment
		do
			x++;
		$while (x != 1);
	}
}", @"
class Foo {
	void TestMethod() {
		int x = 1;
        // Some comment
        while (x != 1)
            x++;
    }
}");
        }

        [Test]
        public void TestSimpleWithComment2()
        {
            Test<ConvertDoWhileToWhileLoopCodeRefactoringProvider>(@"
class Foo {
	void TestMethod() {
		int x = 1;
		do
			x++; // Some comment
		$while (x != 1);
	}
}", @"
class Foo {
	void TestMethod() {
		int x = 1;
        while (x != 1)
            x++; // Some comment
    }
}");
        }

        [Test]
        public void TestDisabledInContent()
        {
            TestWrongContext<ConvertDoWhileToWhileLoopCodeRefactoringProvider>(@"
class Foo {
	void TestMethod() {
		int x = 1;
		do
			$x++;
		while (x != 1);
	}
}");
        }
    }
}

