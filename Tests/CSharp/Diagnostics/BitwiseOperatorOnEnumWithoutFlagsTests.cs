using NUnit.Framework;
using RefactoringEssentials.CSharp.Diagnostics;

namespace RefactoringEssentials.Tests.CSharp.Diagnostics
{
    [TestFixture]
    public class BitwiseOperatorOnEnumWithoutFlagsTests : CSharpDiagnosticTestBase
    {
        [Test]
        public void TestUnary()
        {
            Analyze<BitwiseOperatorOnEnumWithoutFlagsAnalyzer>(@"
enum TestEnum
{
	Item1, Item2
}
class TestClass
{
	void TestMethod ()
	{
		var x = $~$TestEnum.Item1;
	}
}");
        }

        public void TestAssignment(string op, bool bitwise = true)
        {
            if (bitwise)
            {
                Analyze<BitwiseOperatorOnEnumWithoutFlagsAnalyzer>(@"
enum TestEnum
{
	Item1, Item2
}
class TestClass
{
	void TestMethod ()
	{
		var x = TestEnum.Item1;
		x $" + op + @"=$ TestEnum.Item2;
	}
}");
            }
            else
            {
                Analyze<BitwiseOperatorOnEnumWithoutFlagsAnalyzer>(@"
enum TestEnum
{
	Item1, Item2
}
class TestClass
{
	void TestMethod ()
	{
		var x = TestEnum.Item1;
		x " + op + @"= TestEnum.Item2;
	}
}");
            }
        }
        [Test]
        public void TestAssignment()
        {
            TestAssignment("|");
            TestAssignment("&");
            TestAssignment("^");
            TestAssignment("+", false);
        }

        public void TestBinary(string op, bool bitwise = true)
        {
            if (bitwise)
            {
                Analyze<BitwiseOperatorOnEnumWithoutFlagsAnalyzer>(@"
enum TestEnum
{
	Item1, Item2
}
class TestClass
{
	void TestMethod ()
	{
		var x = TestEnum.Item1 $" + op + @"$ TestEnum.Item2;
	}
}");
            }
            else
            {
                Analyze<BitwiseOperatorOnEnumWithoutFlagsAnalyzer>(@"
enum TestEnum
{
	Item1, Item2
}
class TestClass
{
	void TestMethod ()
	{
		var x = TestEnum.Item1 " + op + @" TestEnum.Item2;
	}
}");
            }
        }

        [Test]
        public void TestDisable()
        {
            Analyze<BitwiseOperatorOnEnumWithoutFlagsAnalyzer>(@"
enum TestEnum
{
    Item1, Item2
}
class TestClass
{
    void TestMethod()
    {
	    var x = TestEnum.Item1;
#pragma warning disable " + CSharpDiagnosticIDs.BitwiseOperatorOnEnumWithoutFlagsAnalyzerID + @"
        x = x ^ TestEnum.Item2;
    }
}
");
        }

        [Test]
        public void TestBinary()
        {
            TestBinary("&");
            TestBinary("|");
            TestBinary("^");
            TestBinary("+", false);
        }
    }
}
