using NUnit.Framework;
using RefactoringEssentials.CSharp.CodeRefactorings;

namespace RefactoringEssentials.Tests.CSharp.CodeRefactorings
{
    [TestFixture, Ignore("Not implemented!")]
    public class MoveToOuterScopeTests : CSharpCodeRefactoringTestBase
    {
        void TestStatements(string input, string output)
        {
            Test<MoveToOuterScopeAction>(@"
class A
{
	void F()
	{"
     + input +
@"	}
}", @"
class A
{
	void F()
	{"
     + output +
@"	}
}");
        }

        [Test]
        public void SimpleCase()
        {
            TestStatements(@"
	while (true) {
		int $i = 2;
	}
", @"
		int i = 2;
	while (true) {
	}
");
        }

        [Test]
        public void IgnoresDeclarationsDirectlyInABody()
        {
            TestWrongContext<MoveToOuterScopeAction>(@"
class A
{
	void F()
	{
		int $i = 2;
	}
}");
        }

        [Test]
        public void MovesOnlyTheCurrentVariableInitialization()
        {
            TestStatements(@"
	while (true) {
		int $i = 2, j = 3;
	}
", @"
	int i = 2;
	while (true) {
			int j = 3;
	}
");
        }

        [Test]
        public void MovesAllInitializersWhenOnType()
        {
            TestStatements(@"
	while (true) {
		i$nt i = 2, j = 3;
	}
", @"
		int i = 2, j = 3;
	while (true) {
	}
");
        }

        [Test]
        public void OnlyMovesDeclarationWhenInitializerDependsOnOtherStatements()
        {
            TestStatements(@"
	while (true) {
		int i = 2;
		int $j = i;
	}
", @"
		int j;
	while (true) {
		int i = 2;
		j = i;
	}
");
        }

        [Test]
        public void HandlesLambdaDelegate()
        {
            TestStatements(@"
	var action = new Action<int>(i => {
		int $j = 2;
	});
", @"
		int j = 2;
	var action = new Action<int>(i => {
	});
");
        }

        [Test]
        public void IgnoresDeclarationDirectlyInConstructorBody()
        {
            TestWrongContext<MoveToOuterScopeAction>(@"
class A
{
	public A()
	{
		int $i = 2;
	}
}");
        }
    }
}

