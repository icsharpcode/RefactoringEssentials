using RefactoringEssentials.CSharp.Diagnostics;
using Xunit;

namespace RefactoringEssentials.Tests.CSharp.Diagnostics
{
    public class RedundantEmptyDefaultSwitchBranchTests : CSharpDiagnosticTestBase
    {
        [Fact]
        public void TestDefaultRedundantCase()
        {
            Analyze<RedundantEmptyDefaultSwitchBranchAnalyzer>(@"
class Test
{
	void TestMethod (int i = 0)
	{
		switch (i) {
		case 0:
			System.Console.WriteLine();
			break;
		$default$:
			break;
		}
	}
}", @"
class Test
{
	void TestMethod (int i = 0)
	{
		switch (i) {
		case 0:
			System.Console.WriteLine();
			break;
		}
	}
}");
        }

        [Fact]
        public void TestMinimal()
        {
            Analyze<RedundantEmptyDefaultSwitchBranchAnalyzer>(@"
class Test
{
	void TestMethod (int i = 0)
	{
		switch (i) {
		$default$:
			break;
		}
	}
}", @"
class Test
{
	void TestMethod (int i = 0)
	{
		switch (i) {
		}
	}
}");
        }

        [Fact]
        public void TestDefaultRedundantCaseInReverseOrder()
        {
            Analyze<RedundantEmptyDefaultSwitchBranchAnalyzer>(@"
class Test
{
	void TestMethod (int i = 0)
	{
		switch (i) {
		$default$:
			break;
		case 0:
			System.Console.WriteLine();
			break;
		}
	}
}", @"
class Test
{
	void TestMethod (int i = 0)
	{
		switch (i) {
		case 0:
			System.Console.WriteLine();
			break;
		}
	}
}");
        }

        [Fact]
        public void TestDefaultRedundantCaseCombined()
        {
            Analyze<RedundantEmptyDefaultSwitchBranchAnalyzer>(@"
class Test
{
	void TestMethod (int i = 0)
	{
		switch (i) {
		case 0:
		$default$:
			break;
		}
	}
}", @"
class Test
{
	void TestMethod (int i = 0)
	{
		switch (i) {
		case 0:
			break;
		}
	}
}");
        }

        [Fact]
        public void TestDefaultRedundantCaseCombinedReverseOrder()
        {
            Analyze<RedundantEmptyDefaultSwitchBranchAnalyzer>(@"
class Test
{
	void TestMethod (int i = 0)
	{
		switch (i) {
		$default$:
		case 0:
			break;
		}
	}
}", @"
class Test
{
	void TestMethod (int i = 0)
	{
		switch (i) {
		case 0:
			break;
		}
	}
}");
        }

        [Fact]
        public void TestDefaultWithCode()
        {
            Analyze<RedundantEmptyDefaultSwitchBranchAnalyzer>(@"
class Test
{
	void TestMethod (int i = 0)
	{
		switch (i) {
		case 0:
			System.Console.WriteLine();
			break;
		default:
            System.Console.WriteLine(""default"");
			break;
		}
	}
}");
        }

        [Fact]
        public void TestDisable()
        {
            Analyze<RedundantEmptyDefaultSwitchBranchAnalyzer>(@"
class Test
{
	void TestMethod (int i = 0)
	{
		switch (i) {
		case 0:
			System.Console.WriteLine();
			break;
#pragma warning disable " + CSharpDiagnosticIDs.RedundantEmptyDefaultSwitchBranchAnalyzerID + @"
		default:
			break;
		}
	}
}");
        }
    }
}

