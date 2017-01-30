using RefactoringEssentials.CSharp.Diagnostics;
using Xunit;

namespace RefactoringEssentials.Tests.CSharp.Diagnostics
{
    public class ConvertIfStatementToSwitchStatementTests : CSharpDiagnosticTestBase
    {
        [Fact]
        public void TestBreak()
        {
            Analyze<ConvertIfStatementToSwitchStatementAnalyzer>(@"
class TestClass
{
	void TestMethod (int a)
	{
		int b;
		$if$ (a == 0) {
			b = 0;
		} else if (a == 1) {
			b = 1;
		} else if (a == 2 || a == 3) {
			b = 2;
		} else {
			b = 3;
		}
	}
}");
        }

        [Fact]
        public void TestReturn()
        {
            Analyze<ConvertIfStatementToSwitchStatementAnalyzer>(@"
class TestClass
{
	int TestMethod (int a)
	{
		$if$ (a == 0) {
			int b = 1;
			return b + 1;
		} else if (a == 2 || a == 3) {
			return 2;
		} else if (a == 13) {
			return 12;
		} else {
			return -1;
		}
	}
}");
        }

        [Fact]
        public void TestConstantExpression()
        {
            Analyze<ConvertIfStatementToSwitchStatementAnalyzer>(@"
class TestClass
{
	int TestMethod (int? a)
	{
		$if$ (a == (1 == 1 ? 11 : 12)) {
			return 1;
		} else if (a == (2 * 3) + 1 || a == 6 / 2) {
			return 2;
		} else if (a == null || a == (int)(10L + 2) || a == default(int) || a == sizeof(int)) {
			return 3;		
		} else {
			return -1;
		}
	}
}");


            Analyze<ConvertIfStatementToSwitchStatementAnalyzer>(@"
class TestClass
{
	const int b = 0;
	int TestMethod (int a)
	{
		const int c = 1;
		$if$ (a == b) {
			return 1;
		} else if (a == b + c) {
			return 0;
		} else if (a ==  c) {
			return 2;
		} else {
			return -1;
		}
	}
}");
        }

        [Fact]
        public void TestNestedOr()
        {
            Analyze<ConvertIfStatementToSwitchStatementAnalyzer>(@"
class TestClass
{
	int TestMethod (int a)
	{
		$if$ (a == 0) {
			return 1;
		} else if ((a == 2 || a == 4) || (a == 3 || a == 5)) {
			return 2;
		} else if (a == 12) {
			return 23;
		} else {
			return -1;
		}
	}
}");
        }

        [Fact]
        public void TestComplexSwitchExpression()
        {
            Analyze<ConvertIfStatementToSwitchStatementAnalyzer>(@"
class TestClass
{
	int TestMethod (int a, int b)
	{
		$if$ (a + b == 0) {
			return 1;
		} else if (1 == a + b) {
			return 0;
		} else if (2 == a + b) {
			return 0;
		} else {
			return -1;
		}
	}
}");
        }

        [Fact]
        public void TestNonConstantExpression()
        {
            Analyze<ConvertIfStatementToSwitchStatementAnalyzer>(@"
class TestClass
{
	void TestMethod (int a, int c)
	{
		int b;
		if (a == 0) {
			b = 0;
		} else if (a == c) {
			b = 1;
		} else if (a == 2 || a == 3) {
			b = 2;
		} else {
			b = 3;
		}
	}
}");
            Analyze<ConvertIfStatementToSwitchStatementAnalyzer>(@"
class TestClass
{
	void TestMethod (int a, int c)
	{
		int b;
		if (a == c) {
			b = 0;
		} else if (a == 1) {
			b = 1;
		} else if (a == 2 || a == 3) {
			b = 2;
		} else {
			b = 3;
		}
	}
}");
            Analyze<ConvertIfStatementToSwitchStatementAnalyzer>(@"
class TestClass
{
	void TestMethod (int a, int c)
	{
		int b;
		if (a == 0) {
			b = 0;
		} else if (a == 1) {
			b = 1;
		} else if (a == 2 || a == c) {
			b = 2;
		} else {
			b = 3;
		}
	}
}");
        }

        [Fact]
        public void TestNonEqualityComparison()
        {
            Analyze<ConvertIfStatementToSwitchStatementAnalyzer>(@"
class TestClass
{
	void TestMethod (int a)
	{
		int b;
		if (a == 0) {
			b = 0;
		} else if (a > 4) {
			b = 1;
		} else if (a == 2 || a == 3) {
			b = 2;
		} else {
			b = 3;
		}
	}
}");
        }

        [Fact]
        public void TestValidType()
        {
            // enum
            Analyze<ConvertIfStatementToSwitchStatementAnalyzer>(@"
enum TestEnum
{
	First,
	Second,
	Third
}
class TestClass
{
	int TestMethod (TestEnum a)
	{
		$if$ (a == TestEnum.First) {
			return 1;
		} else if (a == TestEnum.Second) {
			return -1;
		} else if (a == TestEnum.Third) {
			return 3;
		} else {
			return 0;
		}
	}
}");

            // string, bool, char, integral, nullable
            TestValidType("string", "\"test\"");
            TestValidType("bool", "true");
            TestValidType("char", "'a'");
            TestValidType("byte", "0");
            TestValidType("sbyte", "0");
            TestValidType("short", "0");
            TestValidType("long", "0");
            TestValidType("ushort", "0");
            TestValidType("uint", "0");
            TestValidType("ulong", "0");
            TestValidType("bool?", "true");
        }

        void TestValidType(string type, string caseValue)
        {
            Analyze<ConvertIfStatementToSwitchStatementAnalyzer>(@"
class TestClass
{
	int TestMethod (" + type + @" a)
	{
		$if$ (a == " + caseValue + @") {
			return 1;
		} else if (a == default(" + type + @")) {
			return -1;
		} else if (a == null) {
			return -1;
		}
	}
}");
        }

        [Fact]
        public void TestInvalidType()
        {
            Analyze<ConvertIfStatementToSwitchStatementAnalyzer>(@"
class TestClass
{
	void TestMethod (double a)
	{
		int b;
		if (a == 0) {
			b = 0;
		} else {
			b = 3;
		}
	}
}");
        }

        [Fact]
        public void TestNoElse()
        {
            Analyze<ConvertIfStatementToSwitchStatementAnalyzer>(@"
class TestClass
{
	void TestMethod (int a)
	{
		int b;
		$if$ (a == 0) {
			b = 0;
		} else if (a == 1) {
			b = 1;
		} else if (a == 2) {
			b = 1;
		}
	}
}");
        }

        [Fact]
        public void TestNestedIf()
        {
            Analyze<ConvertIfStatementToSwitchStatementAnalyzer>(@"
class TestClass
{
	void TestMethod (int a)
	{
		int b;
		$if$ (a == 0) {
			if (b == 0)
				return;
		} else if (a == 2 || a == 3) {
			b = 2;
		} else if (a == 12) {
			b = 3;
		}
	}
}");
        }


        [Fact]
        public void TestτooSimpleCase1()
        {
            Analyze<ConvertIfStatementToSwitchStatementAnalyzer>(@"
class TestClass
{
	void TestMethod (int a)
	{
		int b;
		if (a == 0) {
			b = 0;
		} else if (a == 1) {
			b = 1;
		}
	}
}");
        }
        [Fact]
        public void TestτooSimpleCase2()
        {
            Analyze<ConvertIfStatementToSwitchStatementAnalyzer>(@"
class TestClass
{
	void TestMethod (int a)
	{
		int b;
		if (a == 0) {
			b = 0;
		} else if (a == 1) {
			b = 1;
		} else {
			b = 1;
		}
	}
}");
        }
    }
}

