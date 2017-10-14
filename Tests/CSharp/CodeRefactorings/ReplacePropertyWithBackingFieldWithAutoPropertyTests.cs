using System;
using RefactoringEssentials.CSharp.CodeRefactorings;
using Xunit;

namespace RefactoringEssentials.Tests.CSharp.CodeRefactorings
{
    public class ReplacePropertyWithBackingFieldWithAutoPropertyTests : CSharpCodeRefactoringTestBase
    {
        [Fact]
        public void TestSimpleStore()
        {
            Test<ReplacePropertyWithBackingFieldWithAutoPropertyCodeRefactoringProvider>(@"
class TestClass
{
    int field;
    public int $Field
    {
        get { return field; }
        set { field = value; }
    }
}
", @"
class TestClass
{
    public int Field { get; set; }
}
");
        }

        [Fact]
        public void TestSimpleStoreWithXmlDoc()
        {
            Test<ReplacePropertyWithBackingFieldWithAutoPropertyCodeRefactoringProvider>(@"
class TestClass
{
    int field;

    /// <summery>
    /// Description of this field.
    /// </summary>
    public int $Field
    {
        get { return field; }
        set { field = value; }
    }
}
", @"
class TestClass
{

    /// <summery>
    /// Description of this field.
    /// </summary>
    public int Field { get; set; }
}
");
        }

        /// <summary>
        /// Bug 3292 -Error in analysis service
        /// </summary>
        [Fact]
        public void TestBug3292()
        {
            TestWrongContext<ReplacePropertyWithBackingFieldWithAutoPropertyCodeRefactoringProvider>(
                "class TestClass" + Environment.NewLine +
                "{" + Environment.NewLine +
                "	int field;" + Environment.NewLine +
                "	public int $Field {" + Environment.NewLine +
                "		get { " +
                "			Console.WriteLine(field);" +
                "		}" + Environment.NewLine +
                "		set { field = value; }" + Environment.NewLine +
                "	}" + Environment.NewLine +
                "}"
            );
        }

        [Fact]
        public void TestBug3292Case2()
        {
            TestWrongContext<ReplacePropertyWithBackingFieldWithAutoPropertyCodeRefactoringProvider>(
                "class TestClass" + Environment.NewLine +
                "{" + Environment.NewLine +
                "	int field;" + Environment.NewLine +
                "	public int $Field {" + Environment.NewLine +
                "		get { " +
                "			return field;" +
                "		}" + Environment.NewLine +
                "		set { Console.WriteLine(field); }" + Environment.NewLine +
                "	}" + Environment.NewLine +
                "}"
            );
        }


        [Fact]
        public void TestWrongLocation()
        {
            TestWrongContext<ReplacePropertyWithBackingFieldWithAutoPropertyCodeRefactoringProvider>(@"class TestClass
{
	string test;
	public $string Test {
		get {
			return test;
		}
		set {
			test = value;
		}
	}
}");

            TestWrongContext<ReplacePropertyWithBackingFieldWithAutoPropertyCodeRefactoringProvider>(@"class TestClass
{
	string test;
	public string $FooBar.Test {
		get {
			return test;
		}
		set {
			test = value;
		}
	}
}");

            TestWrongContext<ReplacePropertyWithBackingFieldWithAutoPropertyCodeRefactoringProvider>(@"class TestClass
{
	string test;
	public string Test ${
		get {
			return test;
		}
		set {
			test = value;
		}
	}
}");
        }

        /// <summary>
        /// Bug 16108 - Convert to autoproperty issues
        /// </summary>
        [Fact]
        public void TestBug16108Case1()
        {
            TestWrongContext<ReplacePropertyWithBackingFieldWithAutoPropertyCodeRefactoringProvider>(@"
class MyClass
{
    [DebuggerHiddenAttribute]
    int a;
    int $A {
        get { return a; }
        set { a = value; }
    }
}
");
        }

        /// <summary>
        /// Bug 16108 - Convert to autoproperty issues
        /// </summary>
        [Fact]
        public void TestBug16108Case2()
        {
            TestWrongContext<ReplacePropertyWithBackingFieldWithAutoPropertyCodeRefactoringProvider>(@"
class MyClass
{
    int a = 4;
    int $A {
        get { return a; }
        set { a = value; }
    }
}
");
        }


        /// <summary>
        /// Bug 16447 - Convert to Auto Property removes multiple variable if declared inline
        /// </summary>
        [Fact]
        public void TestBug16447()
        {
            Test<ReplacePropertyWithBackingFieldWithAutoPropertyCodeRefactoringProvider>(@"
public class Foo
{
	int _bpm = 120, _index = 1, _count;
	int $Count {
		get { return _count; }
		set { _count = value; }
	}
}
", @"
public class Foo
{
    int _bpm = 120, _index = 1;
    int Count { get; set; }
}
");
        }


        [Fact]
        public void TestUnimplementedComputedProperty()
        {
            Test<ReplacePropertyWithBackingFieldWithAutoPropertyCodeRefactoringProvider>(@"
class TestClass
{
    public int $Field
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
}
", @"
class TestClass
{
    public int Field { get; set; }
}
");
        }

        [Fact]
        public void TestPreserveVisibility()
        {
            Test<ReplacePropertyWithBackingFieldWithAutoPropertyCodeRefactoringProvider>(@"
class TestClass
{
    int field;
    public int $Field
    {
        get { return field; }
        private set { field = value; }
    }
}
", @"
class TestClass
{
    public int Field { get; private set; }
}
");
        }
    }
}

