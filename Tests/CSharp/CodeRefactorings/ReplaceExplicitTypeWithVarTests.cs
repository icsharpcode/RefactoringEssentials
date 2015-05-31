using NUnit.Framework;
using RefactoringEssentials.CSharp.CodeRefactorings;

namespace RefactoringEssentials.Tests.CSharp.CodeRefactorings
{
    [TestFixture]
    public class ReplaceExplicitTypeWithVarTests : CSharpCodeRefactoringTestBase
    {
        [Test]
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
            Assert.AreEqual(@"class TestClass
{
	void Test ()
	{
		var aVar = this;
	}
}", result);
        }

        [Test]
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

        [Test]
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

        [Test]
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
            Assert.AreEqual(@"class TestClass
{
	void Test ()
	{
		foreach (var aVar in this) {
		}
	}
}", result);
        }

        [Test]
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
    }
}

