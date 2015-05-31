using NUnit.Framework;
using RefactoringEssentials.CSharp.Diagnostics;

namespace RefactoringEssentials.Tests.CSharp.Diagnostics
{
    [TestFixture]
    [Ignore("TODO: Issue not ported yet")]
    public class RedundantParamsTests : CSharpDiagnosticTestBase
    {
        [Test]
        public void TestBasicCase()
        {
            Test<RedundantParamsAnalyzer>(@"class FooBar
{
	public virtual void Foo(string fmt, object[] args)
	{
	}
}

class FooBar2 : FooBar
{
	public override void Foo(string fmt, params object[] args)
	{
		System.Console.WriteLine(fmt, args);
	}
}", @"class FooBar
{
	public virtual void Foo(string fmt, object[] args)
	{
	}
}

class FooBar2 : FooBar
{
	public override void Foo(string fmt, object[] args)
	{
		System.Console.WriteLine(fmt, args);
	}
}");
        }

        [Test]
        public void TestValidCase()
        {
            Analyze<RedundantParamsAnalyzer>(@"class FooBar
{
	public virtual void Foo(string fmt, object[] args)
	{
	}
}

class FooBar2 : FooBar
{
	public override void Foo(string fmt, object[] args)
	{
		System.Console.WriteLine(fmt, args);
	}
}");
        }

        [Test]
        public void TestDisable()
        {
            Analyze<RedundantParamsAnalyzer>(@"class FooBar
{
	public virtual void Foo(string fmt, object[] args)
	{
	}
}

class FooBar2 : FooBar
{
	// ReSharper disable once RedundantParams
	public override void Foo(string fmt, params object[] args)
	{
		System.Console.WriteLine(fmt, args);
	}
}");
        }
    }
}

