using RefactoringEssentials.CSharp.CodeRefactorings;
using Xunit;

namespace RefactoringEssentials.Tests.CSharp.CodeRefactorings
{
    public class ConvertDoWhileToWhileLoopTests : CSharpCodeRefactoringTestBase
    {
        [Fact]
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

        [Fact]
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

        [Fact]
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

        [Fact]
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

