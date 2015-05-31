using NUnit.Framework;
using RefactoringEssentials.CSharp.Diagnostics;

namespace RefactoringEssentials.Tests.CSharp.Diagnostics
{
    [TestFixture]
    [Ignore("TODO: Issue not ported yet")]
    public class PartialMethodParameterNameMismatchTests : CSharpDiagnosticTestBase
    {

        [Test]
        public void SimpleCaseFix()
        {
            Test<PartialMethodParameterNameMismatchAnalyzer>(@"
partial class Test
{
	partial void FooBar(int bar);
}

partial class Test
{
	partial void FooBar(int foo)
	{
	}
}
", @"
partial class Test
{
	partial void FooBar(int bar);
}

partial class Test
{
	partial void FooBar(int bar)
	{
	}
}
");
        }

        [Test]
        public void TestDisable()
        {
            Analyze<PartialMethodParameterNameMismatchAnalyzer>(@"
// ReSharper disable PartialMethodParameterNameMismatch
partial class Test
{
    partial void FooBar(int bar);
}

partial class Test
{
    partial void FooBar(int foo)
    {
    }
}
// ReSharper restore PartialMethodParameterNameMismatch
");
        }
    }
}

