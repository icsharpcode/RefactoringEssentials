using RefactoringEssentials.CSharp.Diagnostics;
using Xunit;

namespace RefactoringEssentials.Tests.CSharp.Diagnostics
{
    public class SimplifyConditionalTernaryExpressionTests : CSharpDiagnosticTestBase
    {
        [Fact]
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

        [Fact]
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

        [Fact]
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

        [Fact]
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

        [Fact]
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

        [Fact]
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

        [Fact]
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


        [Fact]
        public void TestDisable()
        {
            Analyze<SimplifyConditionalTernaryExpressionAnalyzer>(@"
class Foo
{
	void Bar ()
	{
#pragma warning disable " + CSharpDiagnosticIDs.SimplifyConditionalTernaryExpressionAnalyzerID + @"
		var a = 1 < 2 ? false : true;
	}
}
");
        }


        [Fact]
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

        /// <summary>
        /// Bug 26669 - Source analysis "simplify conditional expression" generates invalid C# condition
        /// </summary>
        [Fact]
        public void TestBug26669()
        {
            Analyze<SimplifyConditionalTernaryExpressionAnalyzer>(@"
class Foo
{
    void Bar ()
    {
        var a = 1 < 2 ? (object)5 : false;
    }
}
");
        }
    }
}

