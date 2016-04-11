using NUnit.Framework;
using RefactoringEssentials.CSharp.Diagnostics;

namespace RefactoringEssentials.Tests.CSharp.Diagnostics
{
    [TestFixture]
    public class RedundantEmptyDefaultSwitchBranchTests : CSharpDiagnosticTestBase
    {
        [Test]
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

        [Test]
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

        [Test]
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

        [Test]
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

        [Test]
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

        [Test]
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

        [Test]
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

