using RefactoringEssentials.CSharp.Diagnostics;
using Xunit;

namespace RefactoringEssentials.Tests.CSharp.Diagnostics
{
    public class BaseMethodParameterNameMismatchTests : CSharpDiagnosticTestBase
    {
        [Fact]
        public void TestMethod()
        {
            Analyze<BaseMethodParameterNameMismatchAnalyzer>(@"
class Foo
{
	public virtual void FooBar (int bar) {}
}

class Bar : Foo
{
	public override void FooBar (int $foo$)
	{
        System.Console.WriteLine (foo);
	}
}
", @"
class Foo
{
	public virtual void FooBar (int bar) {}
}

class Bar : Foo
{
	public override void FooBar (int bar)
	{
        System.Console.WriteLine (bar);
	}
}
");
        }

        [Fact]
        public void TestMethodWithXmlDoc()
        {
            Analyze<BaseMethodParameterNameMismatchAnalyzer>(@"
class Foo
{
	public virtual void FooBar (int bar) {}
}

class Bar : Foo
{
	/// <summary>
	/// Method description
	/// </summary>
	public override void FooBar (int $foo$)
	{
	}
}
", @"
class Foo
{
	public virtual void FooBar (int bar) {}
}

class Bar : Foo
{
	/// <summary>
	/// Method description
	/// </summary>
	public override void FooBar (int bar)
	{
	}
}
");
        }

        [Fact]
        public void TestIndexer()
        {
            Analyze<BaseMethodParameterNameMismatchAnalyzer>(@"
class Foo
{
	protected virtual int this[int i, int j] { get { return 1; } }
}

class Bar : Foo
{
	protected override int this[int i, int $x$] { get { return 1; } }
}
", @"
class Foo
{
	protected virtual int this[int i, int j] { get { return 1; } }
}

class Bar : Foo
{
	protected override int this[int i, int j] { get { return 1; } }
}
");
        }
    }
}

