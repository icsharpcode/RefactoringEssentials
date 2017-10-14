using RefactoringEssentials.CSharp.Diagnostics;
using Xunit;

namespace RefactoringEssentials.Tests.CSharp.Diagnostics
{
    public class ConvertClosureToMethodGroupTests : CSharpDiagnosticTestBase
    {
        [Fact]
        public void TestSimpleVoidLambda()
        {
            Analyze<ConvertClosureToMethodGroupAnalyzer>(@"using System;
class Foo
{
	void Bar (string str)
	{
		Action<int, int> action = $(foo, bar) => MyMethod (foo, bar)$;
	}
	void MyMethod(int foo, int bar) {}
}", @"using System;
class Foo
{
	void Bar (string str)
	{
		Action<int, int> action = MyMethod;
	}
	void MyMethod(int foo, int bar) {}
}");
        }

        [Fact]
        public void TestSimpleBoolLambda()
        {
            Analyze<ConvertClosureToMethodGroupAnalyzer>(@"using System;
class Foo
{
	void Bar (string str)
	{
		Func<int, int, bool> action = $(foo, bar) => MyMethod (foo, bar)$;
	}
	bool MyMethod(int foo, int bar) {}
}", @"using System;
class Foo
{
	void Bar (string str)
	{
		Func<int, int, bool> action = MyMethod;
	}
	bool MyMethod(int foo, int bar) {}
}");
        }

        [Fact]
        public void TestLambdaWithBody()
        {
            Analyze<ConvertClosureToMethodGroupAnalyzer>(@"using System;
class Foo
{
	void Bar (string str)
	{
		Action<int, int> action = $(foo, bar) => { return MyMethod (foo, bar); }$;
	}
	void MyMethod(int foo, int bar) {}
}", @"using System;
class Foo
{
	void Bar (string str)
	{
		Action<int, int> action = MyMethod;
	}
	void MyMethod(int foo, int bar) {}
}");
        }

        [Fact]
        public void Lambda_SwapParameterOrder()
        {
            Analyze<ConvertClosureToMethodGroupAnalyzer>(@"using System;
class Foo
{
	void Bar (string str)
	{
		Action<int, int> action = $(foo, bar) => MyMethod (bar, foo);
	}
	void MyMethod(int foo, int bar) {}
}");
        }

        [Fact]
        public void TestSimpleAnonymousMethod()
        {
            Analyze<ConvertClosureToMethodGroupAnalyzer>(@"using System;
class Foo
{
	int MyMethod (int x, int y) { return x * y; }

	void Bar (string str)
	{
		Func<int, int, int> action = $delegate(int foo, int bar) { return MyMethod (foo, bar); }$;
	}
}", @"using System;
class Foo
{
	int MyMethod (int x, int y) { return x * y; }

	void Bar (string str)
	{
		Func<int, int, int> action = MyMethod;
	}
}");
        }

        [Fact]
        public void TestSkipComplexCase()
        {
            Analyze<ConvertClosureToMethodGroupAnalyzer>(@"using System;
using System.Linq;

class Foo
{
	int MyMethod (int x, int y) { return x * y; }

	void Bar (string str)
	{
		Func<char[]> action = $() => str.Where (c => c != 'a').ToArray ();
	}
}");
        }

        [Fact]
        public void CallInvolvesOptionalParameter()
        {
            Analyze<ConvertClosureToMethodGroupAnalyzer>(@"using System;
class Foo
{
	int MyMethod (int x, int y = 1) { return x * y; }

	void Bar (string str)
	{
		Func<int, int> action = $foo => MyMethod (foo);
	}
}");
        }

        [Fact]
        public void CallExpandsParams()
        {
            Analyze<ConvertClosureToMethodGroupAnalyzer>(@"using System;
class Foo
{
	int MyMethod (params object[] args) { return 0; }

	void Bar (string str)
	{
		Func<string, int> action = $foo => MyMethod (foo);
	}
}");
        }

        [Fact]
        public void CallAsUnambiguousMethodParameter1()
        {
            Analyze<ConvertClosureToMethodGroupAnalyzer>(@"using System;
static class FooStringExtensions
{
    public static int MyMethod(this String str) { return 0; }
}

class Foo
{
    void Bar (string str)
    {
        SomeMethod($() => str.MyMethod()$);
    }

    void SomeMethod(Func<int> action) { }
}", @"using System;
static class FooStringExtensions
{
    public static int MyMethod(this String str) { return 0; }
}

class Foo
{
    void Bar (string str)
    {
        SomeMethod(str.MyMethod);
    }

    void SomeMethod(Func<int> action) { }
}");
        }

        [Fact]
        public void CallAsAmbiguousMethodParameter()
        {
            Analyze<ConvertClosureToMethodGroupAnalyzer>(@"using System;
static class FooStringExtensions
{
    public static void MyMethod(this String str) { return; }
}

class Foo
{
    void Bar (string str)
    {
        SomeMethod(() => str.MyMethod());
    }

    void SomeMethod(Action action) { }
    void SomeMethod(Func<object> action) { }
}");
        }

        [Fact]
        public void CallAsUnambiguousMethodParameter2()
        {
            Analyze<ConvertClosureToMethodGroupAnalyzer>(@"using System;
static class FooStringExtensions
{
    public static void MyMethod(this String str) { return; }
}

class Foo
{
    void Bar (string str)
    {
        SomeMethod($() => str.MyMethod()$);
    }

    void SomeMethod(Action action) { }
}", @"using System;
static class FooStringExtensions
{
    public static void MyMethod(this String str) { return; }
}

class Foo
{
    void Bar (string str)
    {
        SomeMethod(str.MyMethod);
    }

    void SomeMethod(Action action) { }
}");
        }

        /// <summary>
        /// Bug 12184 - Expression can be reduced to delegate fix can create ambiguity
        /// </summary>
        [Fact]
        public void TestBug12184()
        {
            Analyze<ConvertClosureToMethodGroupAnalyzer>(@"using System;
using System.Threading.Tasks;

class C
{
	public static C GetResponse () { return null; }

	public static void Foo ()
	{
		Task.Factory.StartNew (() => GetResponse());
	}
}");
            /*			CheckFix (context, issues, @"using System;
            using System.Threading.Tasks;

            class C
            {
                public static C GetResponse () { return null; }

                public static void Foo ()
                {
                    Task.Factory.StartNew ((Func<C>)GetResponse);
                }
            }");*/

        }

        [Fact]
        public void Return_ReferenceConversion()
        {
            Analyze<ConvertClosureToMethodGroupAnalyzer>(@"using System;
class Foo
{
	void Bar (string str)
	{
		Func<int, object> action = $foo => MyMethod(foo)$;
	}
	string MyMethod(int foo) {}
}");
        }

        [Fact]
        public void Return_BoxingConversion()
        {
            Analyze<ConvertClosureToMethodGroupAnalyzer>(@"using System;
class Foo
{
	void Bar (string str)
	{
		Func<int, object> action = foo => MyMethod(foo);
	}
	bool MyMethod(int foo) {}
}");
        }

        [Fact]
        public void Parameter_ReferenceConversion()
        {
            Analyze<ConvertClosureToMethodGroupAnalyzer>(@"using System;
class Foo
{
	void Bar (string str)
	{
		Action<string> action = $foo => MyMethod(foo)$;
	}
	void MyMethod(object foo) {}
}");
        }

        [Fact]
        public void Parameter_BoxingConversion()
        {
            Analyze<ConvertClosureToMethodGroupAnalyzer>(@"using System;
class Foo
{
	void Bar (string str)
	{
		Action<int> action = $foo => MyMethod(foo)$;
	}
	void MyMethod(object foo) {}
}");
        }

        /// <summary>
        /// Bug 14759 - Lambda expression can be simplified to method group issue
        /// </summary>
        [Fact]
        public void TestBug14759()
        {
            Analyze<ConvertClosureToMethodGroupAnalyzer>(@"using System;
using System.Collections.Generic;

class C
{
	static bool DoStuff (int i)
	{
		return true;
	}
 
	public static void Main(string[] args)
	{
		List<int> list = new List<int> (2);
		list.Add (1);
		list.Add (3);
		list.ForEach (u => DoStuff (u));
	}
}");
        }

        [Fact]
        public void TestTargetCollision()
        {
            Analyze<ConvertClosureToMethodGroupAnalyzer>(@"
using System;

class Program
{
	public void Foo(Action act) {}
	public void Foo(Action<int> act) {}

	void Test ()
	{
		Foo (i => Console.WriteLine (i));
	}
}");
        }

        /// <summary>
        /// Bug 15868 - Wrong context for Anonymous method can be simplified to method group
        /// </summary>
        [Fact]
        public void TestBug15868()
        {
            Analyze<ConvertClosureToMethodGroupAnalyzer>(@"
using System;

delegate bool FooBar ();

public class MyClass
{
	public static void Main ()
	{
		FooBar bar = () => true;
		Func<bool> b = () => bar ();
		Console.WriteLine (b());
	}
}
");
        }

        [Fact]
        public void TestBug15868Case2()
        {
            Analyze<ConvertClosureToMethodGroupAnalyzer>(@"
using System;

delegate bool FooBar ();

public class MyClass
{
	public static void Main ()
	{
		FooBar bar = () => true;
		FooBar b = $() => bar ()$;
		Console.WriteLine (b());
	}
}
", @"
using System;

delegate bool FooBar ();

public class MyClass
{
	public static void Main ()
	{
		FooBar bar = () => true;
		FooBar b = bar;
		Console.WriteLine (b());
	}
}
");
        }

        [Fact]
        public void TestSimpleVoidLambdaConditional()
        {
            Analyze<ConvertClosureToMethodGroupAnalyzer>(@"using System;
class Foo
{
	void Bar (string str)
	{
		Action<int, int> action = (foo, bar) => MyMethod (foo, bar);
	}
	[System.Diagnostics.Conditional(""DEBUG"")]
	void MyMethod(int foo, int bar) {}
}");
        }
    }
}

