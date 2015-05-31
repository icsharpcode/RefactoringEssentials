using NUnit.Framework;
using RefactoringEssentials.CSharp.CodeRefactorings;

namespace RefactoringEssentials.Tests.CSharp.CodeRefactorings
{
    [TestFixture]
    public class ReplaceAutoPropertyWithPropertyAndBackingFieldTests : CSharpCodeRefactoringTestBase
    {
        [Test]
        public void TestSimpleStore()
        {
            Test<ReplaceAutoPropertyWithPropertyAndBackingFieldCodeRefactoringProvider>(@"class TestClass
{
	string $Test { get; set; }
}", @"class TestClass
{
    string test;

    string Test
    {
        get
        {
            return test;
        }

        set
        {
            test = value;
        }
    }
}");
        }

        [Test]
        public void TestStaticStore()
        {
            Test<ReplaceAutoPropertyWithPropertyAndBackingFieldCodeRefactoringProvider>(@"class TestClass
{
	public static string $Test { get; set; }
}", @"class TestClass
{
    static string test;

    public static string Test
    {
        get
        {
            return test;
        }

        set
        {
            test = value;
        }
    }
}");
        }

        [Test]
        public void TestWrongLocation()
        {
            TestWrongContext<ReplaceAutoPropertyWithPropertyAndBackingFieldCodeRefactoringProvider>(@"class TestClass
{
	public $string Test {
		get;
		set;
	}
}");

            TestWrongContext<ReplaceAutoPropertyWithPropertyAndBackingFieldCodeRefactoringProvider>(@"class TestClass
{
	public string $FooBar.Test {
		get;
		set;
	}
}");

            TestWrongContext<ReplaceAutoPropertyWithPropertyAndBackingFieldCodeRefactoringProvider>(@"class TestClass
{
	public string Test ${
		get;
		set;
	}
}");
        }


        [Test]
        public void TestUnimplementedComputedProperty()
        {
            Test<ReplaceAutoPropertyWithPropertyAndBackingFieldCodeRefactoringProvider>(@"class TestClass
{
	string $Test 
    {
        get
        {
            throw new System.NotImplementedException();
        }

        set
        {
            throw new System.NotImplementedException();
        }
    }
}", @"class TestClass
{
    string test;

    string Test
    {
        get
        {
            return test;
        }

        set
        {
            test = value;
        }
    }
}");
        }

        [Test]
        public void TestGetter()
        {
            Test<ReplaceAutoPropertyWithPropertyAndBackingFieldCodeRefactoringProvider>(@"class TestClass
{
    string $Test { get { throw new System.NotImplementedException (); } }
}", @"class TestClass
{
    readonly string test;

    string Test
    {
        get
        {
            return test;
        }
    }
}");
        }

        [Test]
        public void TestGetterAndSetter()
        {
            Test<ReplaceAutoPropertyWithPropertyAndBackingFieldCodeRefactoringProvider>(@"class TestClass
{
    string $Test {
        get {
            throw new System.NotImplementedException ();
        }
        set {
            throw new System.NotImplementedException ();
        }
    }
}", @"class TestClass
{
    string test;

    string Test
    {
        get
        {
            return test;
        }

        set
        {
            test = value;
        }
    }
}");
        }


        [Test]
        public void TestWrongLocation2()
        {
            TestWrongContext<ReplaceAutoPropertyWithPropertyAndBackingFieldCodeRefactoringProvider>(@"class TestClass
{
    public $string Test {
        get {
            throw new System.NotImplementedException ();
        }
        set {
            throw new System.NotImplementedException ();
        }
    }
}");

            TestWrongContext<ReplaceAutoPropertyWithPropertyAndBackingFieldCodeRefactoringProvider>(@"class TestClass
{
    public string $FooBar.Test {
        get {
            throw new System.NotImplementedException ();
        }
        set {
            throw new System.NotImplementedException ();
        }
    }
}");

            TestWrongContext<ReplaceAutoPropertyWithPropertyAndBackingFieldCodeRefactoringProvider>(@"class TestClass
{
    public string Test ${
        get {
            throw new System.NotImplementedException ();
        }
        set {
            throw new System.NotImplementedException ();
        }
    }
}");
        }
    }
}

