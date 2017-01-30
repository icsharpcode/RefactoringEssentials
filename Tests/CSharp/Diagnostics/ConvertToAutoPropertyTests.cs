using RefactoringEssentials.CSharp.Diagnostics;
using Xunit;

namespace RefactoringEssentials.Tests.CSharp.Diagnostics
{
    public class ConvertToAutoPropertyTests : CSharpDiagnosticTestBase
    {
        [Fact(Skip="TODO: Issue not ported yet")]
        public void TestBasicCase()
        {
            Analyze<ConvertToAutoPropertyAnalyzer>(@"
class FooBar
{
	int foo;
	public int $Foo$ {
		get { return foo; }
		set { foo = value; }
	}
}
");
        }

        [Fact(Skip="TODO: Issue not ported yet")]
        public void TestThisSyntaxCase()
        {
            Analyze<ConvertToAutoPropertyAnalyzer>(@"
class FooBar
{
	int foo;
	public int $Foo$ {
		get { return this.foo; }
		set { this.foo = value; }
	}
}
");
        }

        [Fact(Skip="TODO: Issue not ported yet")]
        public void TestDisable()
        {
            Analyze<ConvertToAutoPropertyAnalyzer>(@"
class FooBar
{
	int foo;

	// ReSharper disable once ConvertToAutoProperty
	public int Foo {
		get { return foo; }
		set { foo = value; }
	}
}
");
        }


        [Fact(Skip="TODO: Issue not ported yet")]
        public void TestArrayBug()
        {
            Analyze<ConvertToAutoPropertyAnalyzer>(@"
class Bar {
	public int foo;
}
class FooBar
{
	Bar bar;

	public int Foo {
		get { return bar.foo; }
		set { bar.foo = value; }
	}
}
");
        }

        /// <summary>
        /// Bug 16108 - Convert to autoproperty issues
        /// </summary>
        [Fact(Skip="TODO: Issue not ported yet")]
        public void TestBug16108Case1()
        {
            Analyze<ConvertToAutoPropertyAnalyzer>(@"
class MyClass
{
    [DebuggerHiddenAttribute]
    int a;
    int A {
        get { return a; }
        set { a = value; }
    }
}
");
        }

        /// <summary>
        /// Bug 16108 - Convert to autoproperty issues
        /// </summary>
        [Fact(Skip="TODO: Issue not ported yet")]
        public void TestBug16108Case2()
        {
            Analyze<ConvertToAutoPropertyAnalyzer>(@"
class MyClass
{
    int a = 4;
    int A {
        get { return a; }
        set { a = value; }
    }
}
");
        }


        /// <summary>
        /// Bug 16448 - Refactor incorrectly suggesting "Convert to Auto Property" on property containing custom logic
        /// </summary>
        [Fact(Skip="TODO: Issue not ported yet")]
        public void TestBug16448()
        {
            Analyze<ConvertToAutoPropertyAnalyzer>(@"
using System;

public class Foo
{
	int _bpm;

	public int BPM
	{
		get { return _bpm; }
		set
		{
			_bpm = Math.Min(Math.Max(60, value), 180);
		}
	}
}
");
        }

        /// <summary>
        /// Bug 17107 - Source Analysis ignores volatile keyword
        /// </summary>
        [Fact(Skip="TODO: Issue not ported yet")]
        public void TestBug17107()
        {
            Analyze<ConvertToAutoPropertyAnalyzer>(@"
using System;

public class Foo
{
	volatile Boolean willUpdate;

	Boolean WillUpdate {
		get { return willUpdate; }
		set { willUpdate = value; }
	}
}
");
        }

    }
}

