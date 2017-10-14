using RefactoringEssentials.CSharp.CodeRefactorings;
using Xunit;

namespace RefactoringEssentials.Tests.CSharp.CodeRefactorings
{
    public class ConvertImplicitToExplicittImplementationTests : CSharpCodeRefactoringTestBase
    {
        [Fact]
        public void Test()
        {
            Test<ConvertImplicitToExplicitImplementationCodeRefactoringProvider>(@"
interface ITest
{
    void Method ();
}
class TestClass : ITest
{
    public void $Method()
    {
    }
}", @"
interface ITest
{
    void Method ();
}
class TestClass : ITest
{
    void ITest.Method()
    {
    }
}");
        }

        [Fact]
        public void TestWithXmlDoc()
        {
            Test<ConvertImplicitToExplicitImplementationCodeRefactoringProvider>(@"
interface ITest
{
    void Method ();
}
class TestClass : ITest
{
    /// <summary>
    /// Some method description.
    /// </summary>
    public void $Method()
    {
    }
}", @"
interface ITest
{
    void Method ();
}
class TestClass : ITest
{
    /// <summary>
    /// Some method description.
    /// </summary>
    void ITest.Method()
    {
    }
}");
        }

        [Fact]
        public void TestWithInlineComment()
        {
            Test<ConvertImplicitToExplicitImplementationCodeRefactoringProvider>(@"
interface ITest
{
    void Method ();
}
class TestClass : ITest
{
    public void $Method() // Some comment
    {
    }
}", @"
interface ITest
{
    void Method ();
}
class TestClass : ITest
{
    void ITest.Method() // Some comment
    {
    }
}");
        }

        [Fact]
        public void TestProperty()
        {
            Test<ConvertImplicitToExplicitImplementationCodeRefactoringProvider>(@"
interface ITest
{
    int Prop { get; set; }
}
class TestClass : ITest
{
    public int $Prop
    {
        get { }
        set { }
    }
}", @"
interface ITest
{
    int Prop { get; set; }
}
class TestClass : ITest
{
    int ITest.Prop
    {
        get { }
        set { }
    }
}");
        }

        [Fact]
        public void TestEvent()
        {
            Test<ConvertImplicitToExplicitImplementationCodeRefactoringProvider>(@"
using System;

interface ITest
{
    event EventHandler Evt;
}
class TestClass : ITest
{
    public event EventHandler $Evt
    {
        add { }
        remove { }
    }
}", @"
using System;

interface ITest
{
    event EventHandler Evt;
}
class TestClass : ITest
{
    event EventHandler ITest.Evt
    {
        add { }
        remove { }
    }
}");
        }

        [Fact]
        public void TestIndexer()
        {
            Test<ConvertImplicitToExplicitImplementationCodeRefactoringProvider>(@"
interface ITest
{
    int this[int i] { get; }
}
class TestClass : ITest
{
    public int $this[int i]
    {
        get { }
    }
}", @"
interface ITest
{
    int this[int i] { get; }
}
class TestClass : ITest
{
    int ITest.this[int i]
    {
        get { }
    }
}");
        }

        [Fact]
        public void TestMultipleInterfaces()
        {
            TestWrongContext<ConvertImplicitToExplicitImplementationCodeRefactoringProvider>(@"
interface ITest
{
    void Method ();
}
interface ITest2
{
    void Method ();
}
class TestClass : ITest, ITest2
{
    void $Method ()
    {
    }
}");
        }

        [Fact]
        public void TestNonImplicitImplementation()
        {
            TestWrongContext<ConvertImplicitToExplicitImplementationCodeRefactoringProvider>(@"
class TestClass
{
    void $Method ()
    {
    }
}");
        }

        [Fact]
        public void TestInterfaceMethod()
        {
            TestWrongContext<ConvertImplicitToExplicitImplementationCodeRefactoringProvider>(@"
interface ITest
{
    void Method ();
}
interface ITest2 : ITest
{
    void $Method ();
}");
        }

    }
}
