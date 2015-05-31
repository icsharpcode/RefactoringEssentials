using NUnit.Framework;
using RefactoringEssentials.CSharp.Diagnostics;

namespace RefactoringEssentials.Tests.CSharp.Diagnostics
{
    [TestFixture]
    public class StringStartsWithIsCultureSpecificTests : CSharpDiagnosticTestBase
    {
        [Test]
        public void TestStartsWith()
        {
            Analyze<StringStartsWithIsCultureSpecificAnalyzer>(@"
public class Test
{
    public void Foo (string bar)
    {
        $bar.StartsWith("".com"")$;
    }
}
", @"
public class Test
{
    public void Foo (string bar)
    {
        bar.StartsWith("".com"", System.StringComparison.Ordinal);
    }
}
");
        }

        [Test]
        public void TestDisable()
        {
            Analyze<StringStartsWithIsCultureSpecificAnalyzer>(@"
public class Test
{
	public void Foo (string bar)
	{
#pragma warning disable " + CSharpDiagnosticIDs.StringStartsWithIsCultureSpecificAnalyzerID + @"
		bar.StartsWith ("".com"");
	}
}
");
        }

    }
}

