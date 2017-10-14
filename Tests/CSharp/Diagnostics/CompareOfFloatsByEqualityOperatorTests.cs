using RefactoringEssentials.CSharp.Diagnostics;
using Xunit;

namespace RefactoringEssentials.Tests.CSharp.Diagnostics
{
    public class CompareOfFloatsByEqualityOperatorTests : CSharpDiagnosticTestBase
    {
        static void TestOperator(string inputOp, string outputOp)
        {
            Analyze<CompareOfFloatsByEqualityOperatorAnalyzer>(@"
class TestClass
{
	void TestMethod ()
	{
		double x = 0.1;
		bool test = $x " + inputOp + @" 0.1$;
		bool test2 = $x " + inputOp + @" 1ul$;
	}
}", @"
class TestClass
{
	void TestMethod ()
	{
		double x = 0.1;
		bool test = System.Math.Abs(x - 0.1) " + outputOp + @" EPSILON;
		bool test2 = System.Math.Abs(x - 1ul) " + outputOp + @" EPSILON;
	}
}");
        }

        [Fact]
        public void TestEquality()
        {
            TestOperator("==", "<");
        }

        [Fact]
        public void TestInequality()
        {
            TestOperator("!=", ">");
        }

        [Fact]
        public void TestZero()
        {
            Analyze<CompareOfFloatsByEqualityOperatorAnalyzer>(@"
class TestClass
{
	void TestMethod (double x, float y)
	{
		bool test = $x == 0$;
		bool test2 = $0.0e10 != x$;
		bool test3 = $0L == y$;
		bool test4 = $y != 0.0000$;
	}
}", @"
class TestClass
{
	void TestMethod (double x, float y)
	{
		bool test = System.Math.Abs(x) < EPSILON;
		bool test2 = System.Math.Abs(x) > EPSILON;
		bool test3 = System.Math.Abs(y) < EPSILON;
		bool test4 = System.Math.Abs(y) > EPSILON;
	}
}");
        }

        [Fact]
        public void TestNaN()
        {
            Analyze<CompareOfFloatsByEqualityOperatorAnalyzer>(@"
class TestClass
{
	void TestMethod (double x, float y)
	{
		bool test = $x == System.Double.NaN$;
		bool test2 = $x != double.NaN$;
		bool test3 = $y == float.NaN$;
		bool test4 = $x != float.NaN$;
	}
}", @"
class TestClass
{
	void TestMethod (double x, float y)
	{
		bool test = double.IsNaN(x);
		bool test2 = !double.IsNaN(x);
		bool test3 = float.IsNaN(y);
		bool test4 = !double.IsNaN(x);
	}
}");
        }


        [Fact]
        public void TestPositiveInfinity()
        {
            Analyze<CompareOfFloatsByEqualityOperatorAnalyzer>(@"
class TestClass
{
	void TestMethod (double x, float y)
	{
		bool test = $x == System.Double.PositiveInfinity$;
		bool test2 = $x != double.PositiveInfinity$;
		bool test3 = $y == float.PositiveInfinity$;
		bool test4 = $x != float.PositiveInfinity$;
	}
}", @"
class TestClass
{
	void TestMethod (double x, float y)
	{
		bool test = double.IsPositiveInfinity(x);
		bool test2 = !double.IsPositiveInfinity(x);
		bool test3 = float.IsPositiveInfinity(y);
		bool test4 = !double.IsPositiveInfinity(x);
	}
}");
        }

        [Fact]
        public void TestNegativeInfinity()
        {
            Analyze<CompareOfFloatsByEqualityOperatorAnalyzer>(@"
class TestClass
{
	void TestMethod (double x, float y)
	{
		bool test = $x == System.Double.NegativeInfinity$;
		bool test2 = $x != double.NegativeInfinity$;
		bool test3 = $y == float.NegativeInfinity$;
		bool test4 = $x != float.NegativeInfinity$;
	}
}", @"
class TestClass
{
	void TestMethod (double x, float y)
	{
		bool test = double.IsNegativeInfinity(x);
		bool test2 = !double.IsNegativeInfinity(x);
		bool test3 = float.IsNegativeInfinity(y);
		bool test4 = !double.IsNegativeInfinity(x);
	}
}");
        }

        [Fact]
        public void TestDisable()
        {
            Analyze<CompareOfFloatsByEqualityOperatorAnalyzer>(@"
class TestClass
{
	void TestMethod (double x, float y)
	{
#pragma warning disable " + CSharpDiagnosticIDs.CompareOfFloatsByEqualityOperatorAnalyzerID + @"
		if (x == y)
			System.Console.WriteLine (x);
	}
}");

        }
    }
}
