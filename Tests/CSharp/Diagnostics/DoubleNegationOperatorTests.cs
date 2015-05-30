using NUnit.Framework;
using RefactoringEssentials.CSharp.Diagnostics;

namespace RefactoringEssentials.Tests.CSharp.Diagnostics
{
    [TestFixture]
    public class DoubleNegationOperatorTests : InspectionActionTestBase
    {
        [Test]
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

        [Test]
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

        [Test]
        public void TestDisable()
        {
            Analyze<DoubleNegationOperatorAnalyzer>(@"
class TestClass
{
	void TestMethod ()
	{
#pragma warning disable " + DiagnosticIDs.DoubleNegationOperatorAnalyzerID + @"
		var x = ~(~(123));
	}
}");
        }
    }
}
