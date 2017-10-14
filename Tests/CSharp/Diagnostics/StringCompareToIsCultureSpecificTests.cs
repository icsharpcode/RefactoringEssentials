using RefactoringEssentials.CSharp.Diagnostics;
using Xunit;

namespace RefactoringEssentials.Tests.CSharp.Diagnostics
{
    public class StringCompareToIsCultureSpecificTests : CSharpDiagnosticTestBase
    {
        [Fact]
        public void TestSimpleCase()
        {
            Analyze<StringCompareToIsCultureSpecificAnalyzer>(@"
public class Test
{
	void Foo (string b)
	{
		Console.WriteLine ($""Foo"".CompareTo(b)$);
	}
}
", @"
public class Test
{
	void Foo (string b)
	{
		Console.WriteLine (string.Compare(""Foo"", b, System.StringComparison.Ordinal));
	}
}
");
        }

        [Fact]
        public void TestInvalidCase()
        {
            Analyze<StringCompareToIsCultureSpecificAnalyzer>(@"
public class Test
{
	void Foo (object b)
	{
		Console.WriteLine (""Foo"".CompareTo(b));
	}
}
");
        }


        [Fact]
        public void TestDisable()
        {
            Analyze<StringCompareToIsCultureSpecificAnalyzer>(@"
public class Test
{
	void Foo (string b)
	{
#pragma warning disable " + CSharpDiagnosticIDs.StringCompareToIsCultureSpecificAnalyzerID + @"
		Console.WriteLine (""Foo"".CompareTo(b));
	}
}
");
        }
    }
}

