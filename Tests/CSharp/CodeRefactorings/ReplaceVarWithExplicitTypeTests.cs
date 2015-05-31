using NUnit.Framework;
using RefactoringEssentials.CSharp.CodeRefactorings;

namespace RefactoringEssentials.Tests.CSharp.CodeRefactorings
{
    [TestFixture]
    public class ReplaceVarWithExplicitTypeTests : CSharpCodeRefactoringTestBase
    {
        [Test()]
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
            Assert.AreEqual(@"class TestClass
{
	void Test ()
	{
		TestClass aVar = this;
	}
}", result);
        }

        [Test()]
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
            Assert.AreEqual(@"class TestClass
{
	void Test ()
	{
		foreach (TestClass aVar in new TestClass[] { }) {
		}
	}
}", result);
        }

        [Test()]
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

        [Test()]
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

        [Test()]
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

        [Test()]
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

