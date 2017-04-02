using RefactoringEssentials.CSharp.Diagnostics;
using Xunit;

namespace RefactoringEssentials.Tests.CSharp.Diagnostics
{
    public class RedundantExplicitArrayCreationTests : CSharpDiagnosticTestBase
    {
        [Fact(Skip="TODO: Issue not ported yet")]
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

        [Fact(Skip="TODO: Issue not ported yet")]
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

        [Fact(Skip="TODO: Issue not ported yet")]
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

        [Fact(Skip="TODO: Issue not ported yet")]
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

