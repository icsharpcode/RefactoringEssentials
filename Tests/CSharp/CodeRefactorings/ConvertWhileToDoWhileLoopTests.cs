using RefactoringEssentials.CSharp.CodeRefactorings;
using Xunit;

namespace RefactoringEssentials.Tests.CSharp.CodeRefactorings
{
    public class ConvertWhileToDoWhileLoopTests : CSharpCodeRefactoringTestBase
    {
        [Fact]
        public void TestSimple()
        {
            Test<ConvertWhileToDoWhileLoopCodeRefactoringProvider>(@"
class Foo {
	void Bar(int x) {
		$while (x > 0) x++;
	}
}", @"
class Foo {
	void Bar(int x) {
        do
            x++;
        while (x > 0);
    }
}");
        }

        [Fact]
        public void TestSimpleWithComment()
        {
            Test<ConvertWhileToDoWhileLoopCodeRefactoringProvider>(@"
class Foo {
	void Bar(int x) {
		// Some comment
		$while (x > 0) x++;
	}
}", @"
class Foo {
	void Bar(int x) {
        // Some comment
        do
            x++;
        while (x > 0);
    }
}");
        }

        [Fact]
        public void TestBlock()
        {
            Test<ConvertWhileToDoWhileLoopCodeRefactoringProvider>(@"
class Foo {
	void Bar(int x) {
		$while (x > 0) { x++; }
	}
}", @"
class Foo {
	void Bar(int x) {
        do
        { x++; }
        while (x > 0);
    }
}");
        }

        [Fact]
        public void TestBlockWithComment()
        {
            Test<ConvertWhileToDoWhileLoopCodeRefactoringProvider>(@"
class Foo {
	void Bar(int x) {
		// Some comment
		$while (x > 0) { x++; }
	}
}", @"
class Foo {
	void Bar(int x) {
        // Some comment
        do
        { x++; }
        while (x > 0);
    }
}");
        }

        [Fact]
        public void TestDisabledOutOfToken()
        {
            TestWrongContext<ConvertWhileToDoWhileLoopCodeRefactoringProvider>(@"
class Foo {
	void Bar(int x) {
		while (x > 0) $x++;
	}
}");
        }
    }
}
