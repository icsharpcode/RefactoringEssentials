using RefactoringEssentials.CSharp.CodeRefactorings;
using Xunit;

namespace RefactoringEssentials.Tests.CSharp.CodeRefactorings
{
    public class ReplaceAutoPropertyWithPropertyAndBackingFieldTests : CSharpCodeRefactoringTestBase
    {
        [Fact]
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

        [Fact]
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

        [Fact]
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

        [Fact]
        public void TestAlreadyExpressionBody()
        {
            TestWrongContext<ReplaceAutoPropertyWithPropertyAndBackingFieldCodeRefactoringProvider>(@"class TestClass
{
	public string Test => string.Empty;
}");

            TestWrongContext<ReplaceAutoPropertyWithPropertyAndBackingFieldCodeRefactoringProvider>(@"class TestClass
{
	string FooBar.Test => string.Empty;
}");
        }


        [Fact]
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

        [Fact]
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

        [Fact]
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


        [Fact]
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

        [Fact]
        public void TestPreserveVisibility()
        {
            Test<ReplaceAutoPropertyWithPropertyAndBackingFieldCodeRefactoringProvider>(@"class TestClass
{
    public string $Test { get; private set; }
}", @"class TestClass
{
    string test;

    public string Test
    {
        get
        {
            return test;
        }

        private set
        {
            test = value;
        }
    }
}");
        }

        [Fact]
        public void TestInterfaceContext()
        {
            TestWrongContext<ReplaceAutoPropertyWithPropertyAndBackingFieldCodeRefactoringProvider>(
                "interface Test { int $Test2 { get; set; } }"
            );
        }
    }
}

