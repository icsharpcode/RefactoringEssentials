using NUnit.Framework;
using RefactoringEssentials.CSharp.CodeRefactorings;

namespace RefactoringEssentials.Tests.CSharp.CodeRefactorings
{
    [TestFixture]
    public class ConvertImplicitToExplicittImplementationTests : CSharpCodeRefactoringTestBase
    {
        [Test]
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

        [Test]
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

        [Test]
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

        [Test]
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

        [Test]
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

        [Test]
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

        [Test]
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

        [Test]
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

        [Test]
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
