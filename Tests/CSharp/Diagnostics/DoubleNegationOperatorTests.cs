using RefactoringEssentials.CSharp.Diagnostics;
using Xunit;

namespace RefactoringEssentials.Tests.CSharp.Diagnostics
{
    public class DoubleNegationOperatorTests : CSharpDiagnosticTestBase
    {
        [Fact]
        public void TestLogicalNot()
        {
            Analyze<DoubleNegationOperatorAnalyzer>(@"
class TestClass
{
	bool GetBool () { }

	void TestMethod ()
	{
		var x = $!!GetBool ()$;
		x = $!(!(GetBool ()))$;
	}
}", @"
class TestClass
{
	bool GetBool () { }

	void TestMethod ()
	{
		var x = GetBool ();
		x = GetBool ();
	}
}");
        }

        [Fact]
        public void TestBitwiseNot()
        {
            Analyze<DoubleNegationOperatorAnalyzer>(@"
class TestClass
{
	void TestMethod ()
	{
		var x = $~(~(123))$;
	}
}", @"
class TestClass
{
	void TestMethod ()
	{
		var x = 123;
	}
}");
        }

        [Fact]
        public void TestDisable()
        {
            Analyze<DoubleNegationOperatorAnalyzer>(@"
class TestClass
{
	void TestMethod ()
	{
#pragma warning disable " + CSharpDiagnosticIDs.DoubleNegationOperatorAnalyzerID + @"
		var x = ~(~(123));
	}
}");
        }
    }
}
