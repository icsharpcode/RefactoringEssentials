using NUnit.Framework;
using RefactoringEssentials.CSharp.CodeRefactorings;

namespace RefactoringEssentials.Tests.CSharp.CodeRefactorings
{
    [TestFixture]
    public class ToAbstractVirtualNonVirtualConversionTests : CSharpCodeRefactoringTestBase
    {
        [Test]
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

        [Test]
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

        [Test]
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

        [Test]
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

        [Test]
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

        [Test]
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

        [Test]
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

        [Test]
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

        [Test]
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

        [Test]
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

        [Test]
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

        [Test]
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

        [Test]
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

        [Test]
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


        [Test]
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

        [Test]
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
            Assert.AreEqual(1, actions.Count);
        }


        [Test]
        public void TestNullReferenceException()
        {
            TestWrongContext<ToAbstractVirtualNonVirtualConversionCodeRefactoringProvider>(
                @"void $Foo()
{
    throw new System.NotImplementedException();
}
");
        }
    }
}