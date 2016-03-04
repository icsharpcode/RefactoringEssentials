using NUnit.Framework;
using RefactoringEssentials.CSharp.Diagnostics;

namespace RefactoringEssentials.Tests.CSharp.Diagnostics
{
    [TestFixture]
    public class PartialMethodParameterNameMismatchTests : CSharpDiagnosticTestBase
    {

        [Test]
        public void SimpleCaseFix()
        {
            Analyze<PartialMethodParameterNameMismatchAnalyzer>(@"
partial class Test
{
	partial void FooBar(int bar);
}

partial class Test
{
	partial void FooBar(int $foo$)
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
        public void NoMismatch()
        {
            Analyze<PartialMethodParameterNameMismatchAnalyzer>(@"
partial class Test
{
    partial void FooBar(int bar, string baz, object foo);
}

partial class Test
{
    partial void FooBar(int bar, string baz, object foo)
    {
    }
}
");
        }

        [Test]
        public void TestDisable()
        {
            Analyze<PartialMethodParameterNameMismatchAnalyzer>(@"
#pragma warning disable " + CSharpDiagnosticIDs.PartialMethodParameterNameMismatchAnalyzerID + @"
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

