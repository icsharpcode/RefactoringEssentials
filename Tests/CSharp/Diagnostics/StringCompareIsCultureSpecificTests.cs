using NUnit.Framework;
using RefactoringEssentials.CSharp.Diagnostics;

namespace RefactoringEssentials.Tests.CSharp.Diagnostics
{
    [TestFixture]
    public class StringCompareIsCultureSpecificTests : CSharpDiagnosticTestBase
    {
        [Test]
        public void TestCase1()
        {
            Analyze<StringCompareIsCultureSpecificAnalyzer>(@"
class Test
{
	void Foo ()
	{
		Console.WriteLine ($string.Compare(""Foo"", ""Bar"")$);
	}
}", @"
class Test
{
	void Foo ()
	{
		Console.WriteLine (string.Compare(""Foo"", ""Bar"", System.StringComparison.Ordinal));
	}
}");
        }

        [Test]
        public void TestInvalidCase1()
        {
            Analyze<StringCompareIsCultureSpecificAnalyzer>(@"
class Test
{
	void Foo ()
	{
		Console.WriteLine (string.Compare(""Foo"", ""Bar"", System.StringComparison.Ordinal));
	}
}");
        }

        [Test]
        public void TestCase2()
        {
            Analyze<StringCompareIsCultureSpecificAnalyzer>(@"
class Test
{
	void Foo ()
	{
		Console.WriteLine ($System.String.Compare(""Foo"", ""Bar"", true)$);
	}
}", @"
class Test
{
	void Foo ()
	{
		Console.WriteLine (System.String.Compare(""Foo"", ""Bar"", System.StringComparison.OrdinalIgnoreCase));
	}
}");
        }

        [Test]
        public void TestInvalidCase2()
        {
            Analyze<StringCompareIsCultureSpecificAnalyzer>(@"
class Test
{
	void Foo ()
	{
		Console.WriteLine (string.Compare(""Foo"", ""Bar"", System.StringComparison.OrdinalIgnoreCase));
	}
}");
        }

        [Test]
        public void TestCase3()
        {
            Analyze<StringCompareIsCultureSpecificAnalyzer>(@"
class Test
{
	void Foo ()
	{
		Console.WriteLine ($string.Compare(""Foo"", ""Bar"", false)$);
	}
}", @"
class Test
{
	void Foo ()
	{
		Console.WriteLine (string.Compare(""Foo"", ""Bar"", System.StringComparison.Ordinal));
	}
}");
        }

        [Test]
        public void TestCase4()
        {
            Analyze<StringCompareIsCultureSpecificAnalyzer>(@"
class Test
{
	void Foo ()
	{
		Console.WriteLine ($string.Compare(""Foo"", 0, ""Bar"", 1, 1)$);
	}
}", @"
class Test
{
	void Foo ()
	{
		Console.WriteLine (string.Compare(""Foo"", 0, ""Bar"", 1, 1, System.StringComparison.Ordinal));
	}
}");
        }

        [Test]
        public void TestInvalidCase4()
        {
            Analyze<StringCompareIsCultureSpecificAnalyzer>(@"
class Test
{
	void Foo ()
	{
		Console.WriteLine (string.Compare(""Foo"", 0, ""Bar"", 1, 1, System.StringComparison.Ordinal));
	}
}");
        }

        [Test]
        public void TestCase5()
        {
            Analyze<StringCompareIsCultureSpecificAnalyzer>(@"
class Test
{
	void Foo ()
	{
		Console.WriteLine ($string.Compare(""Foo"", 0, ""Bar"", 1, 1, true)$);
	}
}", @"
class Test
{
	void Foo ()
	{
		Console.WriteLine (string.Compare(""Foo"", 0, ""Bar"", 1, 1, System.StringComparison.OrdinalIgnoreCase));
	}
}");
        }

        [Test]
        public void TestCase6()
        {
            Analyze<StringCompareIsCultureSpecificAnalyzer>(@"
class Test
{
	void Foo ()
	{
		Console.WriteLine ($string.Compare(""Foo"", 0, ""Bar"", 1, 1, false)$);
	}
}", @"
class Test
{
	void Foo ()
	{
		Console.WriteLine (string.Compare(""Foo"", 0, ""Bar"", 1, 1, System.StringComparison.Ordinal));
	}
}");
        }

        [Test]
        public void TestInvalid()
        {
            Analyze<StringCompareIsCultureSpecificAnalyzer>(@"
class Test
{
	void Foo ()
	{
		Console.WriteLine (string.Compare(""a"", ""b"", true, System.Globalization.CultureInfo.CurrentCulture));
	}
}");
        }

        [Test]
        public void TestComplex()
        {
            Analyze<StringCompareIsCultureSpecificAnalyzer>(@"
class Test
{
	void Foo (bool b)
	{
		Console.WriteLine ($string.Compare(""Foo"", ""Bar"", b)$);
	}
}", @"
class Test
{
	void Foo (bool b)
	{
		Console.WriteLine (string.Compare(""Foo"", ""Bar"", b ? System.StringComparison.OrdinalIgnoreCase : System.StringComparison.Ordinal));
	}
}");
        }

        [Test]
        public void TestDisable()
        {
            Analyze<StringCompareIsCultureSpecificAnalyzer>(@"
class Test
{
	void Foo()
	{
#pragma warning disable " + CSharpDiagnosticIDs.StringCompareIsCultureSpecificAnalyzerID + @"
		Console.WriteLine(string.Compare(""Foo"", ""Bar""));
	}
}");
        }
    }
}
