using RefactoringEssentials.CSharp.Diagnostics;
using Xunit;

namespace RefactoringEssentials.Tests.CSharp.Diagnostics
{
    public class BaseMemberHasParamsTests : CSharpDiagnosticTestBase
    {
        [Fact]
        public void TestBasicCase()
        {
            Analyze<BaseMemberHasParamsAnalyzer>(@"class FooBar
{
	public virtual void Foo(string fmt, params object[] args)
	{
	}
}

class FooBar2 : FooBar
{
	public override void Foo(string fmt, $object[] args$)
	{
		System.Console.WriteLine(fmt, args);
	}
}", @"class FooBar
{
	public virtual void Foo(string fmt, params object[] args)
	{
	}
}

class FooBar2 : FooBar
{
	public override void Foo(string fmt, params object[] args)
	{
		System.Console.WriteLine(fmt, args);
	}
}");
        }

        [Fact]
        public void TestValidCase()
        {
            Analyze<BaseMemberHasParamsAnalyzer>(@"class FooBar
{
	public virtual void Foo(string fmt, params object[] args)
	{
	}
}

class FooBar2 : FooBar
{
	public override void Foo(string fmt, params object[] args)
	{
		System.Console.WriteLine(fmt, args);
	}
}");
        }

        [Fact]
        public void TestDisable()
        {
            Analyze<BaseMemberHasParamsAnalyzer>(@"class FooBar
{
	public virtual void Foo(string fmt, params object[] args)
	{
	}
}

class FooBar2 : FooBar
{
#pragma warning disable " + CSharpDiagnosticIDs.BaseMemberHasParamsAnalyzerID + @"
	public override void Foo(string fmt, object[] args)
	{
		System.Console.WriteLine(fmt, args);
	}
}");
        }
    }
}