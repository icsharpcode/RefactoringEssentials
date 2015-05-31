using NUnit.Framework;
using RefactoringEssentials.CSharp.Diagnostics;

namespace RefactoringEssentials.Tests.CSharp.Diagnostics
{
    [TestFixture]
    public class StringLastIndexOfIsCultureSpecificTests : CSharpDiagnosticTestBase
    {
        [Test]
        public void TestLastIndexOf()
        {
            Analyze<StringLastIndexOfIsCultureSpecificAnalyzer>(@"
public class Test
{
    public void Foo (string bar)
    {
        $bar.LastIndexOf("".com"")$;
    }
}
", @"
public class Test
{
    public void Foo (string bar)
    {
        bar.LastIndexOf("".com"", System.StringComparison.Ordinal);
    }
}
");
        }

        [Test]
        public void TestBug()
        {
            Analyze<StringLastIndexOfIsCultureSpecificAnalyzer>(@"
class Program
{
	public int FooBar { get; }

	static void Main(string [] args, string fooBar)
	{
		System.Console.WriteLine($fooBar.LastIndexOf(""aeia"")$);
	}
}
", @"
class Program
{
	public int FooBar { get; }

	static void Main(string [] args, string fooBar)
	{
		System.Console.WriteLine(fooBar.LastIndexOf(""aeia"", System.StringComparison.Ordinal));
	}
}
");
        }

        [Test]
        public void TestDisable()
        {
            Analyze<StringLastIndexOfIsCultureSpecificAnalyzer>(@"
public class Test
{
	public void Foo (string bar)
	{
#pragma warning disable " + CSharpDiagnosticIDs.StringLastIndexOfIsCultureSpecificAnalyzerID + @"
		bar.LastIndexOf ("".com"");
	}
}
");
        }

    }
}

