using NUnit.Framework;
using RefactoringEssentials.CSharp.Diagnostics;

namespace RefactoringEssentials.Tests.CSharp.Diagnostics
{
    [TestFixture]
    [Ignore("TODO: Issue not ported yet")]
    public class RedundantExplicitArrayCreationTests : CSharpDiagnosticTestBase
    {
        [Test]
        public void TestSimpleCase()
        {
            Test<RedundantExplicitArrayCreationAnalyzer>(@"
class Test
{
	void Foo ()
	{
		new int[] { 1, 2, 3 };
	}
}
", @"
class Test
{
	void Foo ()
	{
		new [] { 1, 2, 3 };
	}
}
");
        }

        [Test]
        public void TestInvalid()
        {
            Analyze<RedundantExplicitArrayCreationAnalyzer>(@"
class Test
{
	void Foo ()
	{
		new [] { 1, 2, 3 };
	}
}
");
        }

        [Test]
        public void TestInvalidCase2()
        {

            Analyze<RedundantExplicitArrayCreationAnalyzer>(@"
class Test
{
	void Foo ()
	{
		new long[] { 0, 1u, (byte)34 };
	}
}
");
        }

        [Test]
        public void TestDisable()
        {
            Analyze<RedundantExplicitArrayCreationAnalyzer>(@"
class Test
{
	void Foo ()
	{
		// ReSharper disable once RedundantExplicitArrayCreation
		new int[] { 1, 2, 3 };
	}
}
");
        }
    }
}

