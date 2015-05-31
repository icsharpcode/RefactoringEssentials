using NUnit.Framework;
using RefactoringEssentials.CSharp.Diagnostics;

namespace RefactoringEssentials.Tests.CSharp.Diagnostics
{
    [TestFixture]
    public class BaseMethodCallWithDefaultParameterTests : CSharpDiagnosticTestBase
    {
        [Test]
        public void TestBasicCase()
        {
            Analyze<BaseMethodCallWithDefaultParameterAnalyzer>(@"
public class MyBase
{
	public virtual void FooBar(int x = 12)
	{
		System.Console.WriteLine(""Foo Bar"" + x);
	}
}

public class MyClass : MyBase
{
	public override void FooBar(int x = 12)
	{
		$base.FooBar()$;
	}
}
");
        }

        [Test]
        public void TestInterfaceCase()
        {
            Analyze<BaseMethodCallWithDefaultParameterAnalyzer>(@"
public class MyBase
{
	public virtual int this[int x, int y = 12] {
		get {
			return 1;
		}
	}
}

public class MyClass : MyBase
{
	public override int this[int x, int y = 12] {
		get {
			return $base[x]$;
		}
	}
}
");

        }

        [Test]
        public void TestDoNotWarnCase()
        {
            Analyze<BaseMethodCallWithDefaultParameterAnalyzer>(@"
public class MyBase
{
	public virtual void FooBar(int x = 12)
	{
		System.Console.WriteLine(""Foo Bar"" + x);
	}
}

public class MyClass : MyBase
{
	public override void FooBar(int x = 12)
	{
		base.FooBar(11);
	}
}
");
        }

        [Test]
        public void TestDoNotWarnInParamsCase()
        {
            Analyze<BaseMethodCallWithDefaultParameterAnalyzer>(@"
public class MyBase
{
	public virtual void FooBar(params int[] x)
	{
		System.Console.WriteLine(""Foo Bar"" + x);
	}
}

public class MyClass : MyBase
{
	public override void FooBar(params int[] x)
	{
		base.FooBar();
	}
}
");
        }

        [Test]
        public void TestDisable()
        {
            Analyze<BaseMethodCallWithDefaultParameterAnalyzer>(@"
public class MyBase
{
	public virtual void FooBar(int x = 12)
	{
		System.Console.WriteLine(""Foo Bar"" + x);
	}
}

public class MyClass : MyBase
{
	public override void FooBar(int x = 12)
	{
#pragma warning disable " + CSharpDiagnosticIDs.BaseMethodCallWithDefaultParameterDiagnosticID + @"
		base.FooBar();
	}
}
");
        }
    }
}

