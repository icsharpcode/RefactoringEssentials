using NUnit.Framework;
using RefactoringEssentials.CSharp.CodeRefactorings;

namespace RefactoringEssentials.Tests.CSharp.CodeRefactorings
{
    [TestFixture]
    public class ConvertWhileToDoWhileLoopTests : CSharpCodeRefactoringTestBase
    {
        [Test]
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

        [Test]
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

        [Test]
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

        [Test]
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

        [Test]
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
