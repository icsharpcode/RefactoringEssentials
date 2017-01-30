using RefactoringEssentials.CSharp.CodeRefactorings;
using Xunit;

namespace RefactoringEssentials.Tests.CSharp.CodeRefactorings
{
    public class ConvertIfStatementToSwitchStatementTests : CSharpCodeRefactoringTestBase
    {

        [Fact]
        public void TestBreak()
        {
            Test<ConvertIfStatementToSwitchStatementCodeRefactoringProvider>(@"
class TestClass
{
    void TestMethod (int a)
    {
        int b;
        $if (a == 0) {
            b = 0;
        } else if (a == 1) {
            b = 1;
        } else if (a == 2 || a == 3) {
            b = 2;
        } else {
            b = 3;
        }
    }
}", @"
class TestClass
{
    void TestMethod (int a)
    {
        int b;
        switch (a)
        {
            case 0:
                b = 0;
                break;
            case 1:
                b = 1;
                break;
            case 2:
            case 3:
                b = 2;
                break;
            default:
                b = 3;
                break;
        }
    }
}");
        }

        [Fact]
        public void TestBreakWithComment()
        {
            Test<ConvertIfStatementToSwitchStatementCodeRefactoringProvider>(@"
class TestClass
{
    void TestMethod (int a)
    {
        int b;
        // Some comment
        $if (a == 0) {
            b = 0;
        } else if (a == 1) {
            b = 1;
        } else if (a == 2 || a == 3) {
            b = 2;
        } else {
            b = 3;
        }
    }
}", @"
class TestClass
{
    void TestMethod (int a)
    {
        int b;
        // Some comment
        switch (a)
        {
            case 0:
                b = 0;
                break;
            case 1:
                b = 1;
                break;
            case 2:
            case 3:
                b = 2;
                break;
            default:
                b = 3;
                break;
        }
    }
}");
        }

        [Fact]
        public void TestReturn()
        {
            Test<ConvertIfStatementToSwitchStatementCodeRefactoringProvider>(@"
class TestClass
{
    int TestMethod (int a)
    {
        $if (a == 0) {
            int b = 1;
            return b + 1;
        } else if (a == 2 || a == 3) {
            return 2;
        } else {
            return -1;
        }
    }
}", @"
class TestClass
{
    int TestMethod (int a)
    {
        switch (a)
        {
            case 0:
                int b = 1;
                return b + 1;
            case 2:
            case 3:
                return 2;
            default:
                return -1;
        }
    }
}");
        }

        [Fact]
        public void TestConstantExpression()
        {
            Test<ConvertIfStatementToSwitchStatementCodeRefactoringProvider>(@"
class TestClass
{
    int TestMethod (int? a)
    {
        $if (a == (1 == 1 ? 11 : 12)) {
            return 1;
        } else if (a == (2 * 3) + 1 || a == 6 / 2) {
            return 2;
        } else if (a == null || a == (int)(10L + 2) || a == default(int) || a == sizeof(int)) {
            return 3;
        } else {
            return -1;
        }
    }
}", @"
class TestClass
{
    int TestMethod (int? a)
    {
        switch (a)
        {
            case (1 == 1 ? 11 : 12):
                return 1;
            case (2 * 3) + 1:
            case 6 / 2:
                return 2;
            case null:
            case (int)(10L + 2):
            case default(int):
            case sizeof(int):
                return 3;
            default:
                return -1;
        }
    }
}");


            Test<ConvertIfStatementToSwitchStatementCodeRefactoringProvider>(@"
class TestClass
{
    const int b = 0;
    int TestMethod (int a)
    {
        const int c = 1;
        $if (a == b) {
            return 1;
        } else if (a == b + c) {
            return 0;
        } else {
            return -1;
        }
    }
}", @"
class TestClass
{
    const int b = 0;
    int TestMethod (int a)
    {
        const int c = 1;
        switch (a)
        {
            case b:
                return 1;
            case b + c:
                return 0;
            default:
                return -1;
        }
    }
}");
        }

        [Fact]
        public void TestNestedOr()
        {
            Test<ConvertIfStatementToSwitchStatementCodeRefactoringProvider>(@"
class TestClass
{
    int TestMethod (int a)
    {
        $if (a == 0) {
            return 1;
        } else if ((a == 2 || a == 4) || (a == 3 || a == 5)) {
            return 2;
        } else {
            return -1;
        }
    }
}", @"
class TestClass
{
    int TestMethod (int a)
    {
        switch (a)
        {
            case 0:
                return 1;
            case 2:
            case 4:
            case 3:
            case 5:
                return 2;
            default:
                return -1;
        }
    }
}");
        }

        [Fact]
        public void TestComplexSwitchExpression()
        {
            Test<ConvertIfStatementToSwitchStatementCodeRefactoringProvider>(@"
class TestClass
{
    int TestMethod (int a, int b)
    {
        $if (a + b == 0) {
            return 1;
        } else if (1 == a + b) {
            return 0;
        } else {
            return -1;
        }
    }
}", @"
class TestClass
{
    int TestMethod (int a, int b)
    {
        switch (a + b)
        {
            case 0:
                return 1;
            case 1:
                return 0;
            default:
                return -1;
        }
    }
}");
        }

        [Fact]
        public void TestNonConstantExpression()
        {
            TestWrongContext<ConvertIfStatementToSwitchStatementCodeRefactoringProvider>(@"
class TestClass
{
	void TestMethod (int a, int c)
	{
		int b;
		$if (a == 0) {
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
            TestWrongContext<ConvertIfStatementToSwitchStatementCodeRefactoringProvider>(@"
class TestClass
{
	void TestMethod (int a, int c)
	{
		int b;
		$if (a == c) {
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
            TestWrongContext<ConvertIfStatementToSwitchStatementCodeRefactoringProvider>(@"
class TestClass
{
	void TestMethod (int a, int c)
	{
		int b;
		$if (a == 0) {
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
            TestWrongContext<ConvertIfStatementToSwitchStatementCodeRefactoringProvider>(@"
class TestClass
{
	void TestMethod (int a)
	{
		int b;
		$if (a == 0) {
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
            Test<ConvertIfStatementToSwitchStatementCodeRefactoringProvider>(@"
enum TestEnum
{
    First,
    Second,
}
class TestClass
{
    int TestMethod (TestEnum a)
    {
        $if (a == TestEnum.First) {
            return 1;
        } else {
            return -1;
        }
    }
}", @"
enum TestEnum
{
    First,
    Second,
}
class TestClass
{
    int TestMethod (TestEnum a)
    {
        switch (a)
        {
            case TestEnum.First:
                return 1;
            default:
                return -1;
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
            TestValidType("bool?", "null");
        }

        void TestValidType(string type, string caseValue)
        {
            Test<ConvertIfStatementToSwitchStatementCodeRefactoringProvider>(@"
class TestClass
{
    int TestMethod (" + type + @" a)
    {
        $if (a == " + caseValue + @") {
            return 1;
        } else {
            return -1;
        }
    }
}", @"
class TestClass
{
    int TestMethod (" + type + @" a)
    {
        switch (a)
        {
            case " + caseValue + @":
                return 1;
            default:
                return -1;
        }
    }
}");
        }

        [Fact]
        public void TestInvalidType()
        {
            TestWrongContext<ConvertIfStatementToSwitchStatementCodeRefactoringProvider>(@"
class TestClass
{
	void TestMethod (double a)
	{
		int b;
		$if (a == 0) {
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
            Test<ConvertIfStatementToSwitchStatementCodeRefactoringProvider>(@"
class TestClass
{
    void TestMethod (int a)
    {
        int b;
        $if (a == 0) {
            b = 0;
        } else if (a == 1) {
            b = 1;
        }
    }
}", @"
class TestClass
{
    void TestMethod (int a)
    {
        int b;
        switch (a)
        {
            case 0:
                b = 0;
                break;
            case 1:
                b = 1;
                break;
        }
    }
}");
        }

        [Fact]
        public void TestNestedIf()
        {
            Test<ConvertIfStatementToSwitchStatementCodeRefactoringProvider>(@"
class TestClass
{
    void TestMethod (int a)
    {
        int b;
        $if (a == 0) {
            if (b == 0)
                return;
        } else if (a == 2 || a == 3) {
            b = 2;
        }
    }
}", @"
class TestClass
{
    void TestMethod (int a)
    {
        int b;
        switch (a)
        {
            case 0:
                if (b == 0)
                    return;
                break;
            case 2:
            case 3:
                b = 2;
                break;
        }
    }
}");
        }

        [Fact]
        public void TestInvalid()
        {
            TestWrongContext<ConvertIfStatementToSwitchStatementCodeRefactoringProvider>(@"
class TestClass
{
	void TestMethod (int a)
	{
		int b;
		while (true) {
			$if (a == 0) {
				b = 0;
			} else if (a == 1) {
				b = 1;
			} else if (a == 2 || a == 3) {
				b = 2;
			} else {
				break;
			}
		}
	}
}");
        }
        [Fact]
        public void TestInvalidCase2()
        {
            TestWrongContext<ConvertIfStatementToSwitchStatementCodeRefactoringProvider>(@"
class TestClass
{
	void TestMethod (int a)
	{
		int b;
		while (true) {
			$if (a == 0) {
				break;
			} else if (a == 1) {
				b = 1;
			} else {
				b = 2;
			}
		}
	}
}");
        }

    }
}
