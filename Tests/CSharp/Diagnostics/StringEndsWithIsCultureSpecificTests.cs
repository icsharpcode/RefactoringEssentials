using NUnit.Framework;
using RefactoringEssentials.CSharp.Diagnostics;

namespace RefactoringEssentials.Tests.CSharp.Diagnostics
{
    [TestFixture]
    public class StringEndsWithIsCultureSpecificTests : CSharpDiagnosticTestBase
    {
        [Test]
        public void TestEndsWith()
        {
            Analyze<StringEndsWithIsCultureSpecificAnalyzer>(@"
public class Test
{
    public void Foo (string bar)
    {
        $bar.EndsWith("".com"")$;
    }
}
", @"
public class Test
{
    public void Foo (string bar)
    {
        bar.EndsWith("".com"", System.StringComparison.Ordinal);
    }
}
");
        }

        [Test]
        public void TestDisable()
        {
            Analyze<StringEndsWithIsCultureSpecificAnalyzer>(@"
public class Test
{
	public void Foo (string bar)
	{
#pragma warning disable " + CSharpDiagnosticIDs.StringEndsWithIsCultureSpecificAnalyzerID + @"
		bar.EndsWith ("".com"");
	}
}
");
        }

    }
}

