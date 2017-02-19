using RefactoringEssentials.CSharp.Diagnostics;
using Xunit;

namespace RefactoringEssentials.Tests.CSharp.Diagnostics
{
    public class FunctionNeverReturnsTests : CSharpDiagnosticTestBase
    {
        [Fact]
        public void TestEnd()
        {
            var input = @"
class TestClass
{
    void TestMethod () 
    {
        int i = 1;
    }
}";
            Analyze<FunctionNeverReturnsAnalyzer>(input);
        }

        [Fact]
        public void TestReturn()
        {
            var input = @"
class TestClass
{
    void TestMethod ()
    {
        return;
    }
}";
            Analyze<FunctionNeverReturnsAnalyzer>(input);
        }

        [Fact]
        public void TestThrow()
        {
            var input = @"
class TestClass
{
    void TestMethod ()
    {
        throw new System.NotImplementedException();	
    }
}";
            Analyze<FunctionNeverReturnsAnalyzer>(input);
        }

        [Fact]
        public void TestNeverReturns()
        {
            var input = @"
class TestClass
{
    void $TestMethod$ ()
    {
        while (true) ;
    }
}";
            Analyze<FunctionNeverReturnsAnalyzer>(input);
        }

        [Fact]
        public void TestIfWithoutElse()
        {
            var input = @"
class TestClass
{
    string TestMethod (int x)
    {
        if (x <= 0) return ""Hi"";
        return ""_"" + TestMethod(x - 1);
    }
}";
            Analyze<FunctionNeverReturnsAnalyzer>(input);
        }

        [Fact]
        public void TestRecursive()
        {
            var input = @"
class TestClass
{
    void $TestMethod$ ()
    {
        TestMethod ();
    }
}";
            Analyze<FunctionNeverReturnsAnalyzer>(input);
        }

        [Fact]
        public void TestRecursiveWithThis()
        {
            var input = @"
class TestClass
{
    void $TestMethod$ ()
    {
        this.TestMethod();
    }
}";
            Analyze<FunctionNeverReturnsAnalyzer>(input);
        }

        [Fact]
        public void TestNonRecursive()
        {
            var input = @"
class TestClass
{
    void TestMethod ()
    {
        TestMethod (0);
    }
    void TestMethod (int i)
    {
    }
}";
            Analyze<FunctionNeverReturnsAnalyzer>(input);
        }

        [Fact]
        public void TestVirtualNonRecursive()
        {
            var input = @"
class Base
{
    public Base parent;
    public virtual string Result {
        get { return parent.Result; }
    }
}";
            Analyze<FunctionNeverReturnsAnalyzer>(input);
        }

        [Fact]
        public void TestOverrideNonRecursive()
        {
            var input = @"
class Base
{
    public Base anotherInstance;
    public override int GetHashCode() {
        return anotherInstance.GetHashCode();
    }
}";
            Analyze<FunctionNeverReturnsAnalyzer>(input);
        }

        [Fact]
        public void TestNonRecursiveProperty()
        {
            var input = @"
class TestClass
{
    int foo;
    int Foo
    {
        get { return foo; }
        set
        {
            if (Foo != value)
                foo = value;
        }
    }
}";
            Analyze<FunctionNeverReturnsAnalyzer>(input);
        }

        [Fact]
        public void TestNonRecursivePropertyWithPassingPropertyName()
        {
            var input = @"
class TestClass
{
    private int TryProperty(string propertyName)
    {
    }
    private void SetProperty(string propertyName, int newValue)
    {
    }


    int Foo
    {
        get { return TryProperty(nameof(Foo)); }
        set
        {
            SetProperty(nameof(Foo), value);
        }
    }
}";
            Analyze<FunctionNeverReturnsAnalyzer>(input);
        }

        [Fact]
        public void TestGetterNeverReturns()
        {
            var input = @"
class TestClass
{
    int TestProperty
    {
        $get$ {
            while (true) ;
        }
    }
}";
            Analyze<FunctionNeverReturnsAnalyzer>(input);
        }

        [Fact]
        public void TestRecursiveGetter()
        {
            var input = @"
class TestClass
{
    int TestProperty
    {
        $get$ {
            return TestProperty;
        }
    }
}";
            Analyze<FunctionNeverReturnsAnalyzer>(input);
        }

        [Fact]
        public void TestRecursiveSetter()
        {
            var input = @"
class TestClass
{
    int TestProperty
    {
        $set$ {
            TestProperty = value;
        }
    }
}";
            Analyze<FunctionNeverReturnsAnalyzer>(input);
        }

        [Fact]
        public void TestAutoProperty()
        {
            var input = @"
class TestClass
{
    int TestProperty
    {
        get;
        set;
    }
}";
            Analyze<FunctionNeverReturnsAnalyzer>(input);
        }

        [Fact]
        public void TestMethodGroupNeverReturns()
        {
            var input = @"
class TestClass
{
    int $TestMethod$()
    {
        return TestMethod();
    }
    int TestMethod(object o)
    {
        return TestMethod();
    }
}";
            Analyze<FunctionNeverReturnsAnalyzer>(input);
        }

        [Fact]
        public void TestIncrementProperty()
        {
            var input = @"
class TestClass
{
    int TestProperty
    {
        $get$ { return TestProperty++; }
        $set$ { TestProperty++; }
    }
}";
            Analyze<FunctionNeverReturnsAnalyzer>(input);
        }

        [Fact]
        public void TestLambdaNeverReturns()
        {
            var input = @"
class TestClass
{
    void TestMethod()
    {
        System.Action action = () $=>$ { while (true) ; };
    }
}";
            Analyze<FunctionNeverReturnsAnalyzer>(input);
        }

        [Fact]
        public void TestDelegateNeverReturns()
        {
            var input = @"
class TestClass
{
    void TestMethod()
    {
        System.Action action = $delegate$() { while (true) ; };
    }
}";

            Analyze<FunctionNeverReturnsAnalyzer>(input);
        }

        [Fact]
        public void YieldBreak()
        {
            var input = @"
class TestClass
{
    System.Collections.Generic.IEnumerable<string> TestMethod ()
    {
        yield break;
    }
}";
            Analyze<FunctionNeverReturnsAnalyzer>(input);
        }

        [Fact]
        public void TestDisable()
        {
            var input = @"
class TestClass
{
#pragma warning disable " + CSharpDiagnosticIDs.FunctionNeverReturnsAnalyzerID + @"
    void TestMethod ()
    {
        while (true) ;
    }
}";
            Analyze<FunctionNeverReturnsAnalyzer>(input);
        }

        [Fact]
        public void TestBug254()
        {
            //https://github.com/icsharpcode/NRefactory/issues/254
            var input = @"
class TestClass
{
    int state = 0;

    bool Foo()
    {
        return state < 10;
    }

    void TestMethod()
    {
        if (Foo()) {
            ++state;
            TestMethod ();	
        }
    }
}";
            Analyze<FunctionNeverReturnsAnalyzer>(input);
        }

        [Fact]
        public void TestSwitch()
        {
            //https://github.com/icsharpcode/NRefactory/issues/254
            var input = @"
class TestClass
{
    int foo;
    void TestMethod()
    {
        switch (foo) {
            case 0: TestMethod();
        }
    }
}";
            Analyze<FunctionNeverReturnsAnalyzer>(input);
        }

        [Fact]
        public void TestSwitchWithDefault()
        {
            //https://github.com/icsharpcode/NRefactory/issues/254
            var input = @"
class TestClass
{
    int foo;
    void $TestMethod$()
    {
        switch (foo) {
            case 0: case 1: TestMethod();
            default: TestMethod();
        }
    }
}";
            Analyze<FunctionNeverReturnsAnalyzer>(input);
        }

        [Fact]
        public void TestSwitchValue()
        {
            //https://github.com/icsharpcode/NRefactory/issues/254
            var input = @"
class TestClass
{
    int foo;
    int $TestMethod$()
    {
        switch (TestMethod()) {
            case 0: return 0;
        }
        return 1;
    }
}";
            Analyze<FunctionNeverReturnsAnalyzer>(input);
        }

        [Fact]
        public void TestSwitchDefault_CaseReturns()
        {
            var input = @"
class TestClass
{
    void TestSwitch(int i)
    {
        switch (i)
        {
            case 1:
                return;
            default:
                break;
        }
    }
}";
            Analyze<FunctionNeverReturnsAnalyzer>(input);
        }

        [Fact]
        public void TestLinqFrom()
        {
            //https://github.com/icsharpcode/NRefactory/issues/254
            var input = @"
using System.Linq;
using System.Collections.Generic;
class TestClass
{
    IEnumerable<int> $TestMethod$()
    {
        return from y in TestMethod() select y;
    }
}";
            Analyze<FunctionNeverReturnsAnalyzer>(input);
        }

        [Fact]
        public void TestWrongLinqContexts()
        {
            //https://github.com/icsharpcode/NRefactory/issues/254
            var input = @"
using System.Linq;
using System.Collections.Generic;
class TestClass
{
    IEnumerable<int> TestMethod()
    {
        return from y in Enumerable.Empty<int>()
               from z in TestMethod()
               select y;
    }
}";
            Analyze<FunctionNeverReturnsAnalyzer>(input);
        }

        [Fact]
        public void TestForeach()
        {
            //https://bugzilla.xamarin.com/show_bug.cgi?id=14732
            var input = @"
using System.Linq;
class TestClass
{
    void TestMethod()
    {
        foreach (var x in new int[0])
            TestMethod();
    }
}";
            Analyze<FunctionNeverReturnsAnalyzer>(input);
        }

        [Fact]
        public void TestNoExecutionFor()
        {
            var input = @"
using System.Linq;
class TestClass
{
    void TestMethod()
    {
        for (int i = 0; i < 0; ++i)
            TestMethod ();
    }
}";
            Analyze<FunctionNeverReturnsAnalyzer>(input);
        }

        [Fact]
        public void TestNullCoalescing()
        {
            //https://bugzilla.xamarin.com/show_bug.cgi?id=14732
            var input = @"
using System.Linq;
class TestClass
{
    TestClass parent;
    int? value;
    int TestMethod()
    {
        return value ?? parent.TestMethod();
    }
}";
            Analyze<FunctionNeverReturnsAnalyzer>(input);
        }

        [Fact]
        public void TestPropertyGetterInSetter()
        {
            Analyze<FunctionNeverReturnsAnalyzer>(@"using System;
class TestClass
{
    int a;
    int Foo {
        get { return 1; }
        set { a = Foo; }
    }
}");
        }

        [Fact]
        public void TestRecursiveFunctionBug()
        {
            Analyze<FunctionNeverReturnsAnalyzer>(@"using System;
class TestClass
{
    bool Foo (int i)
    {
        return i < 0 || Foo (i - 1);
    }
}");
        }

        /// <summary>
        /// Bug 17769 - Incorrect "method never returns" warning
        /// </summary>
        [Fact]
        public void TestBug17769()
        {
            Analyze<FunctionNeverReturnsAnalyzer>(@"
using System.Linq;
class A
{
    A[] list = new A[0];

    public bool Test ()
    {
        return list.Any (t => t.Test ());
    }
}
");
        }

        [Fact]
        public void TestSelfUnregisteringEvent()
        {
            var input = @"
using System;   
class TestClass
{
    public event EventHandler SomeEvent;

    void OnSomeEvent(object o, EventArgs e)
    {
        ((TestClass) o).SomeEvent -= OnSomeEvent;
    }
}";
            Analyze<FunctionNeverReturnsAnalyzer>(input);
        }

        [Fact]
        public void TestConditionalExpression()
        {
            var input = @"
using System;   
class TestClass
{
    TestClass another;

    int Test(bool recursive)
    {
        return recursive ? another.Test(true) : 0;
    }
}";
            Analyze<FunctionNeverReturnsAnalyzer>(input);
        }

        [Fact]
        public void TestConditionalAccessExpression()
        {
            var input = @"
using System;   
class TestClass
{
    TestClass another;

    void Test()
    {
        another?.Test();
    }
}";
            Analyze<FunctionNeverReturnsAnalyzer>(input);
        }
    }
}
