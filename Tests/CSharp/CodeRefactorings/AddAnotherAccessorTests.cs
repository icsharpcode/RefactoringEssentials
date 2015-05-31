using NUnit.Framework;
using RefactoringEssentials.CSharp.CodeRefactorings;

namespace RefactoringEssentials.Tests.CSharp.CodeRefactorings
{
    [TestFixture]
    public class AddAnotherAccessorTests : CSharpCodeRefactoringTestBase
    {
        [Test]
        public void TestAddSet()
        {
            Test<AddAnotherAccessorCodeRefactoringProvider>(@"
class TestClass
{
    int field;
	public int $Field 
    {
        get 
        {
            return field;
        }
	}
}", @"
class TestClass
{
    int field;
    public int Field
    {
        get
        {
            return field;
        }

        set
        {
            field = value;
        }
    }
}");
        }

        [Test]
        public void TestAddSet_ReadOnlyField()
        {
            Test<AddAnotherAccessorCodeRefactoringProvider>(@"
class TestClass
{
    readonly int field;
	public int $Field
    {
		get
        {
            return field;
        }
	}
}", @"
class TestClass
{
    readonly int field;
    public int Field
    {
        get
        {
            return field;
        }

        set
        {
            throw new System.NotImplementedException();
        }
    }
}");
        }

        [Test]
        public void TestAddGet()
        {
            Test<AddAnotherAccessorCodeRefactoringProvider>(@"
class TestClass
{
    int field;
    public int $Field {
        set 
        {
            field = value;
        }
    }
}", @"
class TestClass
{
    int field;
    public int Field
    {
        get
        {
            return field;
        }

        set
        {
            field = value;
        }
    }
}");
        }

        [Test]
        public void TestAddGetWithComment()
        {
            Test<AddAnotherAccessorCodeRefactoringProvider>(@"
class TestClass
{
    int field;
    public int $Field {
        // Some comment
        set 
        {
            field = value;
        }
    }
}", @"
class TestClass
{
    int field;
    public int Field
    {
        get
        {
            return field;
        }
        // Some comment
        set
        {
            field = value;
        }
    }
}");
        }

        [Test]
        public void TestAutoProperty()
        {
            Test<AddAnotherAccessorCodeRefactoringProvider>(@"
class TestClass
{
    string $Test 
    {
        get;
    }
}", @"
class TestClass
{
    string Test
    {
        get; set;
    }
}");
        }

        [Test]
        public void TestAutoPropertyWithComment()
        {
            Test<AddAnotherAccessorCodeRefactoringProvider>(@"
class TestClass
{
    string $Test 
    {
        // Some comment
        get;
    }
}", @"
class TestClass
{
    string Test
    {
        // Some comment
        get; set;
    }
}");
        }
    }
}
