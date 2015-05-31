using NUnit.Framework;
using RefactoringEssentials.CSharp.Diagnostics;

namespace RefactoringEssentials.Tests.CSharp.Diagnostics
{
    [TestFixture]
    public class RedundantCaseLabelTests : CSharpDiagnosticTestBase
    {
        [Test]
        public void Test()
        {
            var input = @"
class TestClass
{
	void TestMethod (int i)
	{
		switch (i) {
			$case 1:$
			$case 2:$
			default:
				break;
		}
	}
}";
            var output = @"
class TestClass
{
	void TestMethod (int i)
	{
		switch (i) {
			default:
				break;
		}
	}
}";
            Analyze<RedundantCaseLabelAnalyzer>(input, output);
        }


        [Test]
        public void TestLabelAfterDefault()
        {
            var input = @"
class TestClass
{
	void TestMethod (int i)
	{
		switch (i) {
			default:
			$case 1:$
				break;
		}
	}
}";
            var output = @"
class TestClass
{
	void TestMethod (int i)
	{
		switch (i) {
			default:
				break;
		}
	}
}";
            Analyze<RedundantCaseLabelAnalyzer>(input, output);
        }


        [Test]
        public void TestDisable()
        {
            var input = @"
class TestClass
{
	void TestMethod (int i)
	{
		switch (i) {
#pragma warning disable " + CSharpDiagnosticIDs.RedundantCaseLabelAnalyzerID + @"
			case 1:
			default:
				break;
		}
	}
}";
            Analyze<RedundantCaseLabelAnalyzer>(input);
        }
    }
}
