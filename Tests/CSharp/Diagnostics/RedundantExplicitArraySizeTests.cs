using RefactoringEssentials.CSharp.Diagnostics;
using Xunit;

namespace RefactoringEssentials.Tests.CSharp.Diagnostics
{
    public class RedundantExplicitArraySizeTests : CSharpDiagnosticTestBase
    {
        [Fact]
        public void TestSimpleCase()
        {
            Analyze<RedundantExplicitArraySizeAnalyzer>(@"
class Test
{
	void Foo ()
	{
		var foo = new int[$3$] { 1, 2, 3 };
	}
}
", @"
class Test
{
	void Foo ()
	{
		var foo = new int[] { 1, 2, 3 };
	}
}
");
        }

        [Fact]
        public void TestInvalidCase1()
        {
            Analyze<RedundantExplicitArraySizeAnalyzer>(@"
class Test
{
	void Foo (int i)
	{
		var foo = new int[i] { 1, 2, 3 };
	}
}
");
        }

        [Fact]
        public void TestInvalidCase2()
        {
            Analyze<RedundantExplicitArraySizeAnalyzer>(@"
class Test
{
	void Foo ()
	{
		var foo = new int[10];
	}
}
");
        }

        [Fact]
        public void TestInvalidCase3()
        {
            Analyze<RedundantExplicitArraySizeAnalyzer>(@"
class Test
{
	void Foo ()
	{
		var foo = new int[0];
	}
}
");
        }

        [Fact]
        public void TestDisable()
        {
            Analyze<RedundantExplicitArraySizeAnalyzer>(@"
class Test
{
	void Foo ()
	{
		// ReSharper disable once RedundantExplicitArraySize
#pragma warning disable " + CSharpDiagnosticIDs.RedundantExplicitArraySizeAnalyzerID + @"
		var foo = new int[3] { 1, 2, 3 };
	}
}
");
        }
    }
}

