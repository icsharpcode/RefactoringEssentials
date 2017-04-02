using RefactoringEssentials.CSharp.CodeRefactorings;
using Xunit;

namespace RefactoringEssentials.Tests.CSharp.CodeRefactorings
{
    public class ReplaceExplicitTypeWithVarTests : CSharpCodeRefactoringTestBase
    {
        [Fact]
        public void SimpleVarDeclaration()
        {
            string result = RunContextAction(new ReplaceExplicitTypeWithVarCodeRefactoringProvider(),
@"class TestClass
{
	void Test ()
	{
		$TestClass aVar = this;
	}
}");
            Assert.Equal(@"class TestClass
{
	void Test ()
	{
		var aVar = this;
	}
}", result);
        }

        [Fact]
        public void AlreadyAVar()
        {
            TestWrongContext(new ReplaceExplicitTypeWithVarCodeRefactoringProvider(),
                @"class TestClass
{
	void Test ()
	{
		$var aVar = this;
	}
}");
        }

        [Fact]
        public void AlreadyAVarInForeach()
        {
            TestWrongContext(new ReplaceExplicitTypeWithVarCodeRefactoringProvider(),
                @"class TestClass
{
	void Test ()
	{
		foreach ($var aVar in this) {
		}
	}
}");
        }

        [Fact]
        public void ForeachDeclaration()
        {
            string result = RunContextAction(new ReplaceExplicitTypeWithVarCodeRefactoringProvider(),
                @"class TestClass
{
	void Test ()
	{
		foreach ($TestClass aVar in this) {
		}
	}
}");
            Assert.Equal(@"class TestClass
{
	void Test ()
	{
		foreach (var aVar in this) {
		}
	}
}", result);
        }

        [Fact]
        public void TestInvalidLocationBug()
        {
            TestWrongContext(new ReplaceExplicitTypeWithVarCodeRefactoringProvider(),
                @"class TestClass
{
	$void Test ()
	{
		var aVar = this;
	}
}");
        }

        [Fact]
        public void TestMultipleInitializers()
        {
            TestWrongContext(new ReplaceExplicitTypeWithVarCodeRefactoringProvider(),
                @"class TestClass
{
	void Test ()
	{
		$TestClass aVar1 = this, aVar2 = this;
	}
}");
        }
    }
}

