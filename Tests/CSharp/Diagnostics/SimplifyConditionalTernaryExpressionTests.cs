using NUnit.Framework;
using RefactoringEssentials.CSharp.Diagnostics;

namespace RefactoringEssentials.Tests.CSharp.Diagnostics
{
    [TestFixture]
    public class SimplifyConditionalTernaryExpressionTests : CSharpDiagnosticTestBase
    {
        [Test]
        public void TestFalseTrueCase()
        {
            Analyze<SimplifyConditionalTernaryExpressionAnalyzer>(@"
class Foo
{
	void Bar ()
	{
		var a = $1 < 2 ? false : true$;
	}
}
", @"
class Foo
{
	void Bar ()
	{
		var a = 1 >= 2;
	}
}
");
        }

        [Test]
        public void TestFalseTrueCase2()
        {
            Analyze<SimplifyConditionalTernaryExpressionAnalyzer>(@"
class Foo
{
	void Bar ()
	{
		var a = $obj is foo ? false : true$;
	}
}
", @"
class Foo
{
	void Bar ()
	{
		var a = !(obj is foo);
	}
}
");
        }

        [Test]
        public void TestFalseExprCase()
        {
            Analyze<SimplifyConditionalTernaryExpressionAnalyzer>(@"
class Foo
{
	void FooBar (int a, int b, bool c)
	{
		Console.WriteLine ($a < b ? false : c$);
	}
}
", @"
class Foo
{
	void FooBar (int a, int b, bool c)
	{
		Console.WriteLine (a >= b && c);
	}
}
");
        }

        [Test]
        public void TestTrueExprCase()
        {
            Analyze<SimplifyConditionalTernaryExpressionAnalyzer>(@"
class Foo
{
	void FooBar (int a, int b, bool c)
	{
		Console.WriteLine ($a < b ? true : c$);
	}
}
", @"
class Foo
{
	void FooBar (int a, int b, bool c)
	{
		Console.WriteLine (a < b || c);
	}
}
");
        }

        [Test]
        public void TestExprFalseCase()
        {
            Analyze<SimplifyConditionalTernaryExpressionAnalyzer>(@"
class Foo
{
	void FooBar (int a, int b, bool c)
	{
		Console.WriteLine ($a < b ? c : false$);
	}
}
", @"
class Foo
{
	void FooBar (int a, int b, bool c)
	{
		Console.WriteLine (a < b && c);
	}
}
");
        }

        [Test]
        public void TestExprTrueCase()
        {
            Analyze<SimplifyConditionalTernaryExpressionAnalyzer>(@"
class Foo
{
	void FooBar (int a, int b, bool c)
	{
		Console.WriteLine ($a < b ? c : true$);
	}
}
", @"
class Foo
{
	void FooBar (int a, int b, bool c)
	{
		Console.WriteLine (a >= b || c);
	}
}
");
        }

        [Test]
        public void TestInvalidCase()
        {
            Analyze<SimplifyConditionalTernaryExpressionAnalyzer>(@"
class Foo
{
	void FooBar (int a, int b, bool c, boold d)
	{
		Console.WriteLine (a < b ? c : d);
	}
}");
        }


        [Test]
        public void TestDisable()
        {
            Analyze<SimplifyConditionalTernaryExpressionAnalyzer>(@"
class Foo
{
	void Bar ()
	{
#pragma warning disable " + DiagnosticIDs.SimplifyConditionalTernaryExpressionAnalyzerID + @"
		var a = 1 < 2 ? false : true;
	}
}
");
        }


        [Test]
        public void TestSkipRedundantCase()
        {
            Analyze<SimplifyConditionalTernaryExpressionAnalyzer>(@"
class Foo
{
	void Bar ()
	{
		var a = 1 < 2 ? true : false;
	}
}
");
        }

    }
}

