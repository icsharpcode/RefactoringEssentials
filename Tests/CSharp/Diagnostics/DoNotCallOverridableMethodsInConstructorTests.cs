using RefactoringEssentials.CSharp.Diagnostics;
using Xunit;

namespace RefactoringEssentials.Tests.CSharp.Diagnostics
{

    public class DoNotCallOverridableMethodsInConstructorTests : CSharpDiagnosticTestBase
    {
        [Fact]
        public void CatchesBadCase()
        {
            Analyze<DoNotCallOverridableMethodsInConstructorAnalyzer>(@"class Foo
{
    Foo()
    {
        $Bar()$;
        $this.Bar()$;
    }

    virtual void Bar ()
    {
    }
}");
        }

        [Fact]
        public void TestDisable()
        {
            Analyze<DoNotCallOverridableMethodsInConstructorAnalyzer>(@"class Foo
{
    Foo()
    {
#pragma warning disable " + CSharpDiagnosticIDs.DoNotCallOverridableMethodsInConstructorAnalyzerID + @"
        Bar();
    }

    virtual void Bar ()
    {
    }
}");
        }



        [Fact]
        public void IgnoresGoodCase()
        {
            Analyze<DoNotCallOverridableMethodsInConstructorAnalyzer>(@"class Foo
{
    Foo()
    {
        Bar();
        Bar();
    }

    void Bar ()
    {
    }
}");
        }

        [Fact]
        public void IgnoresSealedClasses()
        {
            Analyze<DoNotCallOverridableMethodsInConstructorAnalyzer>(@"sealed class Foo
{
    Foo()
    {
        Bar();
        Bar();
    }

    virtual void Bar ()
    {
    }
}");
        }

        [Fact]
        public void IgnoresOverriddenSealedMethods()
        {
            Analyze<DoNotCallOverridableMethodsInConstructorAnalyzer>(@"
class BaseClass
{
    protected virtual void Bar ()
    {
    }
}

class DerivedClass : BaseClass
{
    DerivedClass()
    {
        Bar();
        Bar();
    }

    protected override sealed void Bar ()
    {
    }
}");
        }

        [Fact]
        public void IgnoresNonLocalCalls()
        {
            Analyze<DoNotCallOverridableMethodsInConstructorAnalyzer>(@"class Foo
{
    Foo()
    {
        Foo f = new Foo();
        f.Bar();
    }

    virtual void Bar ()
    {
    }
}");
        }

        [Fact]
        public void IgnoresEventHandlers()
        {
            Analyze<DoNotCallOverridableMethodsInConstructorAnalyzer>(@"class Foo
{
    Foo()
    {
        SomeEvent += delegate { Bar(); };
    }

    virtual void Bar ()
    {
    }
}");
        }

        [Fact]
        public void IgnoresDelegates1()
        {
            Analyze<DoNotCallOverridableMethodsInConstructorAnalyzer>(@"
using System;
class Foo
{
    private Action barAction;

    Foo()
    {
        barAction = Bar;
    }

    virtual void Bar()
    {
    }
}");
        }

        [Fact]
        public void IgnoresDelegates2()
        {
            Analyze<DoNotCallOverridableMethodsInConstructorAnalyzer>(@"
using System;
class Foo
{
    Foo()
    {
        SaveBarAction(this.Bar);
    }

    void SaveBarAction(Action barAction)
    {
    }

    virtual void Bar()
    {
    }
}");
        }


        /// <summary>
        /// Bug 14450 - False positive of "Virtual member call in constructor"
        /// </summary>
        [Fact]
        public void TestBug14450()
        {
            Analyze<DoNotCallOverridableMethodsInConstructorAnalyzer>(@"
using System;

public class Test {
    public Test(Action action) {
        action();
    }
}
");
        }

        [Fact]
        public void SetVirtualPropertyThroughThis()
        {
            Analyze<DoNotCallOverridableMethodsInConstructorAnalyzer>(@"class Foo
{
    Foo()
    {
        $this.AutoProperty$ = 1;
    }

    public virtual int AutoProperty { get; set; }
}");
        }

        [Fact]
        public void SetVirtualProperty()
        {
            Analyze<DoNotCallOverridableMethodsInConstructorAnalyzer>(@"class Foo
{
    Foo()
    {
        $AutoProperty$ = 1;
    }

    public virtual int AutoProperty { get; set; }
}");
        }

        [Fact]
        public void GetVirtualPropertyThroughThis()
        {
            Analyze<DoNotCallOverridableMethodsInConstructorAnalyzer>(@"class Foo
{
    Foo()
    {
        var val = $this.AutoProperty$;
    }

    public virtual int AutoProperty { get; set; }
}");
        }

        [Fact]
        public void GetVirtualProperty()
        {
            Analyze<DoNotCallOverridableMethodsInConstructorAnalyzer>(@"class Foo
{
    Foo()
    {
        var val = $AutoProperty$;
    }

    public virtual int AutoProperty { get; set; }
}");
        }

        [Fact]
        public void GetVirtualPropertyWithPrivateSetter()
        {
            Analyze<DoNotCallOverridableMethodsInConstructorAnalyzer>(@"class Foo
{
    Foo()
    {
        var val = $AutoProperty$;
    }

    public virtual int AutoProperty { get; private set; }
}");
        }

        [Fact]
        public void SetVirtualPropertyWithPrivateSetter()
        {
            Analyze<DoNotCallOverridableMethodsInConstructorAnalyzer>(@"class Foo
{
    Foo()
    {
        AutoProperty = 1;
    }

    public virtual int AutoProperty { get; private set; }
}");
        }

        [Fact]
        public void SetVirtualPropertyWithPrivateSetterThroughThis()
        {
            Analyze<DoNotCallOverridableMethodsInConstructorAnalyzer>(@"class Foo
{
    Foo()
    {
        this.AutoProperty = 1;
    }

    public virtual int AutoProperty { get; private set; }
}");
        }

        /// <summary>
        /// Bug 39180 - "Virtual member call in constructor" when no call is made
        /// </summary>
        [Fact]
        public void TestBug39180()
        {
            Analyze<DoNotCallOverridableMethodsInConstructorAnalyzer>(@" class Test {
    public interface ITest { }
    public Test (ITest test) { 

    }
}");
        }

        [Fact]
        public void TestBug39180_Case2()
        {
            Analyze<DoNotCallOverridableMethodsInConstructorAnalyzer>(@" class Test {
    public interface ITest { }
    public Test () { 
        ITest test;
    }
}");
        }
       
    }
}
