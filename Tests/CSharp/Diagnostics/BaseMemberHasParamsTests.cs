using NUnit.Framework;
using RefactoringEssentials.CSharp.Diagnostics;

namespace RefactoringEssentials.Tests.CSharp.Diagnostics
{
    [TestFixture]
    public class BaseMemberHasParamsTests : CSharpDiagnosticTestBase
    {
        [Test]
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

        [Test]
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

        [Test]
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