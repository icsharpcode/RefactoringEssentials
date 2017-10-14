using RefactoringEssentials.CSharp.Diagnostics;
using Xunit;

namespace RefactoringEssentials.Tests.CSharp.Diagnostics
{
    public class RemoveRedundantOrStatementTests : CSharpDiagnosticTestBase
    {
        [Fact]
        public void TestOrCase()
        {
            Analyze<RemoveRedundantOrStatementAnalyzer>(@"
class MainClass
{
	static bool bb { get; set; }
	public static void Main(string[] args)
	{
		$MainClass.bb |= false$;
	}
}
", @"
class MainClass
{
	static bool bb { get; set; }
	public static void Main(string[] args)
	{
	}
}
");
        }

        [Fact]
        public void TestAndCase()
        {
            Analyze<RemoveRedundantOrStatementAnalyzer>(@"
class MainClass
{
	static bool bb { get; set; }
	public static void Main(string[] args)
	{
		$MainClass.bb &= true$;
	}
}
", @"
class MainClass
{
	static bool bb { get; set; }
	public static void Main(string[] args)
	{
	}
}
");
        }


        [Fact]
        public void TestDisable()
        {
            Analyze<RemoveRedundantOrStatementAnalyzer>(@"
class MainClass
{
	static bool bb { get; set; }
	public static void Main(string[] args)
	{
#pragma warning disable " + CSharpDiagnosticIDs.RemoveRedundantOrStatementAnalyzerID + @"
		MainClass.bb |= false;
	}
}
");
        }

    }
}

