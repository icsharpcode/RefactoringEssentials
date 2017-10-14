using RefactoringEssentials.CSharp.Diagnostics;
using Xunit;

namespace RefactoringEssentials.Tests.CSharp.Diagnostics
{
    public class StringStartsWithIsCultureSpecificTests : CSharpDiagnosticTestBase
    {
        [Fact]
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

        [Fact]
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

