using NUnit.Framework;
using RefactoringEssentials.CSharp.Diagnostics;

namespace RefactoringEssentials.Tests.CSharp.Diagnostics
{
    [TestFixture]
    public class BaseMethodParameterNameMismatchTests : CSharpDiagnosticTestBase
    {
        [Test]
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

        [Test]
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

        [Test]
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

