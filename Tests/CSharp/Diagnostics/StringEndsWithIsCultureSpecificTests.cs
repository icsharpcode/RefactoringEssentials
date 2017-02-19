using RefactoringEssentials.CSharp.Diagnostics;
using Xunit;

namespace RefactoringEssentials.Tests.CSharp.Diagnostics
{
    public class StringEndsWithIsCultureSpecificTests : CSharpDiagnosticTestBase
    {
        [Fact]
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

        [Fact]
        public void TestEndsWithStringComparisonOverload()
        {
            Analyze<StringEndsWithIsCultureSpecificAnalyzer>(@"
public class Test
{
    public void Foo (string bar)
    {
        bar.EndsWith("".com"", System.StringComparison.Ordinal);
    }
}
");
        }

        [Fact]
        public void TestEndsWithCultureInfoOverload()
        {
            Analyze<StringEndsWithIsCultureSpecificAnalyzer>(@"
public class Test
{
    public void Foo (string bar)
    {
        bar.EndsWith("".com"", true, System.Globalization.CultureInfo.CurrentCulture);
    }
}
");
        }

        [Fact]
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

