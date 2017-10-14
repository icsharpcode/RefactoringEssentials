using RefactoringEssentials.CSharp.Diagnostics;
using Xunit;

namespace RefactoringEssentials.Tests.CSharp.Diagnostics
{
    public class StaticMethodInvocationToExtensionMethodInvocationTests : CSharpDiagnosticTestBase
    {

        [Fact]
        public void HandlesBasicCase()
        {
            Analyze<InvokeAsExtensionMethodAnalyzer>(@"
class A { }
static class B
{
	public static bool Ext (this A a, int i);
}
class C
{
	void F()
	{
		A a = new A();
		B.$Ext$(a, 1);
	}
}", @"
class A { }
static class B
{
	public static bool Ext (this A a, int i);
}
class C
{
	void F()
	{
		A a = new A();
		a.Ext(1);
	}
}");
        }

        [Fact]
        public void HandlesBasicCaseWithComment()
        {
            Analyze<InvokeAsExtensionMethodAnalyzer>(@"
class A { }
static class B
{
	public static bool Ext (this A a, int i);
}
class C
{
	void F()
	{
		A a = new A();
		// Some comment
		B.$Ext$(a, 1);
	}
}", @"
class A { }
static class B
{
	public static bool Ext (this A a, int i);
}
class C
{
	void F()
	{
		A a = new A();
		// Some comment
		a.Ext(1);
	}
}");
        }

        [Fact]
        public void HandlesBasicCaseWithFullNamespace()
        {
            Analyze<InvokeAsExtensionMethodAnalyzer>(@"
namespace TestNamespace
{
    class A { }
    static class B
    {
        public static bool Ext (this A a, int i);
    }
}
class C
{
	void F()
	{
		TestNamespace.A a = new TestNamespace.A();
		TestNamespace.B.Ext(a, 1);
	}
}");
        }

        [Fact]
        public void HandlesReturnValueUsage()
        {
            Analyze<InvokeAsExtensionMethodAnalyzer>(@"
class A { }
static class B
{
	public static void Ext (this A a, int i);
}
class C
{
	void F()
	{
		A a = new A();
		if (B.$Ext$ (a, 1))
			return;
	}
}", @"
class A { }
static class B
{
	public static void Ext (this A a, int i);
}
class C
{
	void F()
	{
		A a = new A();
		if (a.Ext (1))
			return;
	}
}");
        }

        [Fact]
        public void IgnoresIfNullArgument()
        {
            Analyze<InvokeAsExtensionMethodAnalyzer>(@"
class A { }
static class B
{
	public static void Ext (this A a);
}
class C
{
	void F()
	{
		B.Ext(null);
	}
}");
        }

        [Fact]
        public void IgnoresIfNotExtensionMethod()
        {
            Analyze<InvokeAsExtensionMethodAnalyzer>(@"
class A { }
static class B
{
	public static void Ext (A a);
}
class C
{
	void F()
	{
		B.Ext (new A());
	}
}");
        }

        [Fact]
        public void IgnoresIfAlreadyExtensionMethodCallSyntax()
        {
            Analyze<InvokeAsExtensionMethodAnalyzer>(@"
class A { }
static class B
{
	public static void Ext (this A a, int i);
}
class C
{
	void F()
	{
		A a = new A();
		a.Ext (1);
	}
}");
        }

        [Fact]
        public void IgnoresPropertyInvocation()
        {
            Analyze<InvokeAsExtensionMethodAnalyzer>(@"
static class B
{
	public static int Ext { get; set; }
}
class C
{
	void F()
	{
		B.Ext();
	}
}");
        }

        [Fact]
        public void IgnoresTypeMismatchImplicitConversion()
        {
            Analyze<InvokeAsExtensionMethodAnalyzer>(@"
using System;

static class Foo
{
    public static decimal? Abs(this decimal? value)
    {
        return value != null ? Math.Abs(value.Value) : (decimal?) null;
    }
}

class Program
{
    static void Main()
    {
        Console.WriteLine(Foo.Abs(1.0m));
    }
}");

            Analyze<InvokeAsExtensionMethodAnalyzer>(@"
using System;

struct ImplicitDecimal
{
    public static implicit operator decimal(ImplicitDecimal x) => 0;
}

static class Bar
{
    public static decimal Abs(this decimal value) => Math.Abs(value);
}

class Program
{
    static void Main()
    {
        var x = new ImplicitDecimal();
        Console.WriteLine(Bar.Abs(x));
    }
}");
        }

        [Fact]
        public void HandlesDelegateExtensionMethodOnVariable()
        {
            Analyze<InvokeAsExtensionMethodAnalyzer>(@"
using System;
static class B
{
    public static void CallPrintIntHandler(this Action<int> a, int i) { }
}

class C
{
    void F()
    {
        int a = 4;
        Action<int> printIntHandler = i => Console.WriteLine(i);
        B.$CallPrintIntHandler$(printIntHandler, a);
    }
}", @"
using System;
static class B
{
    public static void CallPrintIntHandler(this Action<int> a, int i) { }
}

class C
{
    void F()
    {
        int a = 4;
        Action<int> printIntHandler = i => Console.WriteLine(i);
        printIntHandler.CallPrintIntHandler(a);
    }
}");
        }

        [Fact]
        public void AddParenthesesIfNecessary()
        {
            Analyze<InvokeAsExtensionMethodAnalyzer>(@"using System;

static class Foo
{
    public static decimal Abs(this decimal value) => Math.Abs(value);
}

class Program
{
    static void Main()
    {
        Foo.$Abs$(-1.0m); // Apply code fix here
    }
}", @"using System;

static class Foo
{
    public static decimal Abs(this decimal value) => Math.Abs(value);
}

class Program
{
    static void Main()
    {
        (-1.0m).Abs(); // Apply code fix here
    }
}");
        }

        [Fact]
        public void IgnoresDelegateExtensionMethodOnMethod()
        {
            Analyze<InvokeAsExtensionMethodAnalyzer>(@"using System;
static class B
{
    public static void CallPrintIntHandler(this Action<int> a, int i) { }
}

class C
{
    void F()
    {
        int a = 4;
        B.CallPrintIntHandler(PrintIntHandler, a);
    }

    void PrintIntHandler(int i)
    {
        Console.WriteLine(i);
    }
}");
        }

        [Fact]
        public void IgnoresLambdaAsExtensionMethodParameter()
        {
            Analyze<InvokeAsExtensionMethodAnalyzer>(@"using System;
static class B
{
    public static void CallPrintIntHandler(this Action<int> a, int i) { }
}

class C
{
    void F()
    {
        int a = 4;
        B.CallPrintIntHandler(i => Console.WriteLine(i), a);
    }
}");
        }

        [Fact]
        public void TestDisable()
        {
            Analyze<InvokeAsExtensionMethodAnalyzer>(@"
class A { }
static class B
{
	public static bool Ext (this A a, int i);
}
class C
{
	void F()
	{
		A a = new A();
#pragma warning disable " + CSharpDiagnosticIDs.InvokeAsExtensionMethodAnalyzerID + @"
		B.Ext (a, 1);
	}
}");
        }
    }
}

