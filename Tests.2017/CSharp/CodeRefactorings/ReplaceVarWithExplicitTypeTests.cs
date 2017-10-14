using RefactoringEssentials.CSharp.CodeRefactorings;
using Xunit;

namespace RefactoringEssentials.Tests.CSharp.CodeRefactorings
{
    public class ReplaceVarWithExplicitTypeTests : CSharpCodeRefactoringTestBase
    {
        [Fact]
        public void SimpleVarDeclaration()
        {
            string result = RunContextAction(new ReplaceVarWithExplicitTypeCodeRefactoringProvider(),
@"class TestClass
{
	void Test ()
	{
		$var aVar = this;
	}
}");
            Assert.Equal(@"class TestClass
{
	void Test ()
	{
		TestClass aVar = this;
	}
}", result);
        }

        [Fact]
        public void ForeachDeclaration()
        {
            string result = RunContextAction(new ReplaceVarWithExplicitTypeCodeRefactoringProvider(),
@"class TestClass
{
	void Test ()
	{
		foreach ($var aVar in new TestClass[] { }) {
		}
	}
}");
            Assert.Equal(@"class TestClass
{
	void Test ()
	{
		foreach (TestClass aVar in new TestClass[] { }) {
		}
	}
}", result);
        }

        [Fact]
        public void SimpleAnonymousTypeDeclaration()
        {
            TestWrongContext(new ReplaceVarWithExplicitTypeCodeRefactoringProvider(), @"class TestClass
{
	void Test()
	{
		$var aVar = new { A = 1 };
	}
}");
        }

        [Fact]
        public void ForeachAnonymousTypeDeclaration()
        {
            TestWrongContext(new ReplaceVarWithExplicitTypeCodeRefactoringProvider(), @"class TestClass
{
	void Test()
	{
		var value = new { A = 1 };
		foreach ($var k in new [] { value }) {
		}
	}
}");
        }

        [Fact]
        public void TestAnonymousArrayType()
        {
            TestWrongContext(new ReplaceVarWithExplicitTypeCodeRefactoringProvider(), @"class TestClass
{
	void Test()
	{
		var value = new [] { new { A = 1 } };
		foreach ($var k in new [] { value }) {
		}
	}
}");
        }

        [Fact]
        public void TestAnonymousGenericType()
        {
            TestWrongContext(new ReplaceVarWithExplicitTypeCodeRefactoringProvider(), @"class TestClass
{
	T MakeList<T> (T value) { return new List<T> () { value }; }

	void Test()
	{
		var value = new { A = 1 };
		var list = MakeList (value);
		foreach ($var k in list) {
		}
	}
}");
        }
    }
}

