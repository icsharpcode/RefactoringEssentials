using RefactoringEssentials.CSharp.CodeRefactorings;
using Xunit;

namespace RefactoringEssentials.Tests.CSharp.CodeRefactorings
{
    public class ConvertExplicitToImplicitImplementationTests : CSharpCodeRefactoringTestBase
    {
        [Fact]
        public void TestMethod()
        {
            Test<ConvertExplicitToImplicitImplementationCodeRefactoringProvider>(@"
interface ITest
{
    void Method();
}
class TestClass : ITest
{
    void $ITest.Method()
    {
    }
}", @"
interface ITest
{
    void Method();
}
class TestClass : ITest
{
    public void Method()
    {
    }
}");
        }

        [Fact]
        public void TestMethodWithXmlDoc()
        {
            Test<ConvertExplicitToImplicitImplementationCodeRefactoringProvider>(@"
interface ITest
{
    void Method();
}
class TestClass : ITest
{
    /// <summary>
    /// Some method description.
    /// </summary>
    void $ITest.Method()
    {
    }
}", @"
interface ITest
{
    void Method();
}
class TestClass : ITest
{
    /// <summary>
    /// Some method description.
    /// </summary>
    public void Method()
    {
    }
}");
        }

        [Fact]
        public void TestMethodWithInlineComment()
        {
            Test<ConvertExplicitToImplicitImplementationCodeRefactoringProvider>(@"
interface ITest
{
    void Method();
}
class TestClass : ITest
{
    void $ITest.Method() // Some comment
    {
    }
}", @"
interface ITest
{
    void Method();
}
class TestClass : ITest
{
    public void Method() // Some comment
    {
    }
}");
        }

        [Fact]
        public void TestExistingMethod()
        {
            TestWrongContext<ConvertExplicitToImplicitImplementationCodeRefactoringProvider>(@"
interface ITest
{
    void Method ();
}
class TestClass : ITest
{
    void $ITest.Method ()
    {
    }
    void Method ()
    {
    }
}");
        }

        [Fact]
        public void TestProperty()
        {
            Test<ConvertExplicitToImplicitImplementationCodeRefactoringProvider>(@"
interface ITest
{
    int Prop { get; set; }
}
class TestClass : ITest
{
    int $ITest.Prop
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
    public int Prop
    {
        get { }
        set { }
    }
}");
        }

        [Fact]
        public void TestExistingProperty()
        {
            TestWrongContext<ConvertExplicitToImplicitImplementationCodeRefactoringProvider>(@"
interface ITest
{
    int Prop { get; set; }
}
class TestClass : ITest
{
    int $ITest.Prop
    {
        get { }
        set { }
    }
    public int Prop
    {
        get { }
        set { }
    }
}");
        }

        [Fact]
        public void TestEvent()
        {
            Test<ConvertExplicitToImplicitImplementationCodeRefactoringProvider>(@"
using System;

interface ITest
{
    event EventHandler Evt;
}
class TestClass : ITest
{
    event EventHandler $ITest.Evt
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
    public event EventHandler Evt
    {
        add { }
        remove { }
    }
}");
        }

        [Fact]
        public void TestExistingEvent()
        {
            TestWrongContext<ConvertExplicitToImplicitImplementationCodeRefactoringProvider>(@"
using System;

interface ITest
{
    event EventHandler Evt;
}
class TestClass : ITest
{
    event EventHandler $ITest.Evt
    {
        add { }
        remove { }
    }
    public event EventHandler Evt;
}");
        }

        [Fact]
        public void TestIndexer()
        {
            Test<ConvertExplicitToImplicitImplementationCodeRefactoringProvider>(@"
interface ITest
{
    int this[int i] { get; }
}
class TestClass : ITest
{
    int $ITest.this[int i]
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
    public int this[int i]
    {
        get { }
    }
}");
        }

        [Fact]
        public void TestExistingIndexer()
        {
            TestWrongContext<ConvertExplicitToImplicitImplementationCodeRefactoringProvider>(@"
interface ITest
{
    int this[int i] { get; }
}
class TestClass : ITest
{
    int $ITest.this[int i]
    {
        get { }
    }
    public int this[int i]
    {
        get { }
    }
}");
        }


        [Fact]
        public void TestNonExplitiImplementation()
        {
            TestWrongContext<ConvertExplicitToImplicitImplementationCodeRefactoringProvider>(@"
class TestClass
{
    void $Method ()
    {
    }
}");
        }
    }
}
