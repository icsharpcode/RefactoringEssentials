using RefactoringEssentials.CSharp.Diagnostics;
using Xunit;

namespace RefactoringEssentials.Tests.CSharp.Diagnostics
{
    public class BitwiseOperatorOnEnumWithoutFlagsTests : CSharpDiagnosticTestBase
    {
        [Fact]
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

		[Theory]
		[InlineData("|", true)]
		[InlineData("&", true)]
		[InlineData("^", true)]
		[InlineData("+", false)]
		public void TestAssignment(string op, bool bitwise)
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

		[Theory]
		[InlineData("|", true)]
		[InlineData("&", true)]
		[InlineData("^", true)]
		[InlineData("+", false)]
		public void TestBinary(string op, bool bitwise)
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

        [Fact]
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
    }
}
