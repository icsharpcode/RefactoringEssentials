using NUnit.Framework;
using RefactoringEssentials.CSharp.Diagnostics;

namespace RefactoringEssentials.Tests.CSharp.Diagnostics
{
    [TestFixture]
    public class RedundantExplicitArraySizeTests : CSharpDiagnosticTestBase
    {
        [Test]
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

        [Test]
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

        [Test]
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

        [Test]
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

        [Test]
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

