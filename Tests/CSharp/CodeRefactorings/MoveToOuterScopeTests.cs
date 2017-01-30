using RefactoringEssentials.CSharp.CodeRefactorings;
using Xunit;

namespace RefactoringEssentials.Tests.CSharp.CodeRefactorings
{
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

        [Fact(Skip="Not implemented!")]
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

        [Fact(Skip="Not implemented!")]
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

        [Fact(Skip="Not implemented!")]
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

        [Fact(Skip="Not implemented!")]
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

        [Fact(Skip="Not implemented!")]
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

        [Fact(Skip="Not implemented!")]
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

        [Fact(Skip="Not implemented!")]
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

