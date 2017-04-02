using RefactoringEssentials.CSharp.CodeRefactorings;
using Xunit;

namespace RefactoringEssentials.Tests.CSharp.CodeRefactorings
{
    public class ToAbstractVirtualNonVirtualConversionTests : CSharpCodeRefactoringTestBase
    {
        [Fact]
        public void VirtualToNonVirtualTest()
        {
            Test<ToAbstractVirtualNonVirtualConversionCodeRefactoringProvider>(
                @"class Test
{
    public $virtual void Foo()
    {
    }
}", @"class Test
{
    public void Foo()
    {
    }
}");
        }

        [Fact]
        public void VirtualToAbstractTest()
        {
            Test<ToAbstractVirtualNonVirtualConversionCodeRefactoringProvider>(
                @"abstract class Test
{
    public $virtual void Foo()
    {
    }
}", @"abstract class Test
{
    public abstract void Foo();
}");
        }

        [Fact]
        public void VirtualIndexerToAbstractTest()
        {
            Test<ToAbstractVirtualNonVirtualConversionCodeRefactoringProvider>(
                @"abstract class MainClass
{
    public virtual int $this[int i] {
        get {
            ;
        }
    }
}", @"abstract class MainClass
{
    public abstract int this[int i] { get; }
}");
        }

        [Fact]
        public void NonVirtualStaticToVirtualTest()
        {
            Test<ToAbstractVirtualNonVirtualConversionCodeRefactoringProvider>(
                @"class Test
{
    public static void $Foo()
    {
    }
}", @"class Test
{
    public virtual void Foo()
    {
    }
}");
        }

        [Fact]
        public void NonVirtualToVirtualTest()
        {
            Test<ToAbstractVirtualNonVirtualConversionCodeRefactoringProvider>(
                @"class Test
{
    public void $Foo()
    {
    }
}", @"class Test
{
    public virtual void Foo()
    {
    }
}");
        }

        [Fact]
        public void DoNotSuggestOnPrivateMethod()
        {
            TestWrongContext<ToAbstractVirtualNonVirtualConversionCodeRefactoringProvider>(
                "class Test { void $Foo() { } }");
        }

        [Fact]
        public void InvalidPrivateImplementationTypeTest()
        {
            TestWrongContext<ToAbstractVirtualNonVirtualConversionCodeRefactoringProvider>(
                @"using System;
class Test : IDisposable
{
    void IDisposable.$Dispose ()
    {
    }
}");
        }

		[Fact]
		public void AbstractToNonAbstractTest()
        {
            Test<ToAbstractVirtualNonVirtualConversionCodeRefactoringProvider>(
                @"abstract class Test
{
    public $abstract void Foo();
}", @"abstract class Test
{
    public void Foo()
    {
        throw new System.NotImplementedException();
    }
}");
        }

		[Fact]
		public void AbstractToVirtualTest()
        {
            Test<ToAbstractVirtualNonVirtualConversionCodeRefactoringProvider>(
                @"abstract class Test
{
    public $abstract void Foo();
}", @"abstract class Test
{
    public virtual void Foo()
    {
        throw new System.NotImplementedException();
    }
}", 1);
        }

		[Fact]
		public void AbstractPropertyToNonAbstractTest()
        {
            Test<ToAbstractVirtualNonVirtualConversionCodeRefactoringProvider>(
                @"abstract class Test
{
    public abstract int $Foo
    {
        get;
        set;
    }
}", @"abstract class Test
{
    public int Foo
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
}");
        }

        [Fact]
        public void AbstractEventToNonAbstractTest()
        {
            Test<ToAbstractVirtualNonVirtualConversionCodeRefactoringProvider>(
                @"using System;
abstract class Test
{
    public abstract event EventHandler $Foo;
}", @"using System;
abstract class Test
{
    public event EventHandler Foo;
}");
        }

		[Fact]
		public void NonAbstractToAbstractTest()
        {
            Test<ToAbstractVirtualNonVirtualConversionCodeRefactoringProvider>(
                @"abstract class Test
{
    public void $Foo()
    {
        throw new System.NotImplementedException();
    }
}", @"abstract class Test
{
    public abstract void Foo();
}");
        }

		[Fact]
		public void NonAbstractEventToAbstractTest()
        {
            Test<ToAbstractVirtualNonVirtualConversionCodeRefactoringProvider>(
                @"abstract class Test
{
    public event EventHandler $Foo  {
        add {
            throw new System.NotImplementedException();
        }
        remove {
            throw new System.NotImplementedException();
        }
    }
}", @"abstract class Test
{
    public abstract event EventHandler Foo;
}");
        }

        [Fact]
        public void StaticMethodInStaticClassTest()
        {
            TestWrongContext<ToAbstractVirtualNonVirtualConversionCodeRefactoringProvider>(
                @"public static class MyStaticClass
    {
        public static string $MyStaticMethod()
        {
            // Do something
        }
    }");
        }

        [Fact]
        public void InvalidLocalContext()
        {
            TestWrongContext<ToAbstractVirtualNonVirtualConversionCodeRefactoringProvider>(
                @"using System;
class Test
{
    public static void Main(string[] args)
    {
        int $fooBar = 1;
    }
}");
        }


        [Fact]
        public void InvalidOverrideTest()
        {
            TestWrongContext<ToAbstractVirtualNonVirtualConversionCodeRefactoringProvider>(
                @"using System;
class Test
{
    public override string $ToString()
    {
    }
}");
        }

        [Fact]
        public void ExternMethodTest()
        {
            TestWrongContext<ToAbstractVirtualNonVirtualConversionCodeRefactoringProvider>(
                @"using System;
using System.Runtime.InteropServices;

class Test
{
    [DllImport(""user32.dll"")]
    static extern bool $CloseWindow(IntPtr hWnd);
}");
        }

        [Fact]
        public void InvalidMethodTest()
        {
            var actions = GetActions<ToAbstractVirtualNonVirtualConversionCodeRefactoringProvider>(
                                       @"using System;
abstract class Test
{
    public virtual string $ToString()
    {
        Console.WriteLine (""Hello World"");
    }
}");
            // only virtual -> non virtual should be provided - no abstract conversion
            Assert.Equal(1, actions.Count);
        }


        [Fact]
        public void TestNullReferenceException()
        {
            TestWrongContext<ToAbstractVirtualNonVirtualConversionCodeRefactoringProvider>(
                @"void $Foo()
{
    throw new System.NotImplementedException();
}
");
        }

        [Fact]
        public void TestInterfaceContext()
        {
            TestWrongContext<ToAbstractVirtualNonVirtualConversionCodeRefactoringProvider>(
                @"interface Test
{
                void $Test2();
                int $Test { get; set; }
    event EventHandler $TestEvent;
    }
"
            );
        }
    }
}