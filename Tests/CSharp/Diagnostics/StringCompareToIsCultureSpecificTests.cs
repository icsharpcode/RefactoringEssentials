using NUnit.Framework;
using RefactoringEssentials.CSharp.Diagnostics;

namespace RefactoringEssentials.Tests.CSharp.Diagnostics
{
    [TestFixture]
    public class StringCompareToIsCultureSpecificTests : CSharpDiagnosticTestBase
    {
        [Test]
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

        [Test]
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


        [Test]
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

