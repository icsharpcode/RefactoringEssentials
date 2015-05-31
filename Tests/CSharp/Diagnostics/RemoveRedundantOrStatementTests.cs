using NUnit.Framework;
using RefactoringEssentials.CSharp.Diagnostics;

namespace RefactoringEssentials.Tests.CSharp.Diagnostics
{
    [TestFixture]
    public class RemoveRedundantOrStatementTests : CSharpDiagnosticTestBase
    {
        [Test]
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

        [Test]
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


        [Test]
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

