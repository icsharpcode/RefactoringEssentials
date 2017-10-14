using RefactoringEssentials.CSharp.CodeRefactorings;
using Xunit;

namespace RefactoringEssentials.Tests.CSharp.CodeRefactorings
{
    public class ConvertSwitchToIfTests : CSharpCodeRefactoringTestBase
    {
        [Fact]
        public void TestReturn()
        {
            Test<ConvertSwitchToIfCodeRefactoringProvider>(@"
class TestClass
{
    int TestMethod (int a)
    {
        $switch (a) {
        case 0:
            return 0;
        case 1:
        case 2:
            return 1;
        case 3:
        case 4:
        case 5:
            return 1;
        default:
            return 2;
        }
    }
}", @"
class TestClass
{
    int TestMethod (int a)
    {
        if (a == 0)
        {
            return 0;
        }
        else if (a == 1 || a == 2)
        {
            return 1;
        }
        else if (a == 3 || a == 4 || a == 5)
        {
            return 1;
        }
        else
        {
            return 2;
        }
    }
}");
        }

        [Fact]
        public void TestWithoutDefault()
        {
            Test<ConvertSwitchToIfCodeRefactoringProvider>(@"
class TestClass
{
    int TestMethod(int a)
    {
        $switch (a) {
        case 0:
            return 0;
        case 1:
        case 2:
            return 1;
        case 3:
        case 4:
        case 5:
            return 1;
        }
    }
}", @"
class TestClass
{
    int TestMethod(int a)
    {
        if (a == 0)
        {
            return 0;
        }
        else if (a == 1 || a == 2)
        {
            return 1;
        }
        else if (a == 3 || a == 4 || a == 5)
        {
            return 1;
        }
    }
}");
        }

        [Fact]
        public void TestBreak()
        {
            Test<ConvertSwitchToIfCodeRefactoringProvider>(@"
class TestClass
{
    void TestMethod(int a)
    {
        $switch (a) {
        case 0:
            int b = 1;
            break;
        case 1:
        case 2:
            break;
        case 3:
        case 4:
        case 5:
            break;
        default:
            break;
        }
    }
}", @"
class TestClass
{
    void TestMethod(int a)
    {
        if (a == 0)
        {
            int b = 1;
        }
        else if (a == 1 || a == 2)
        {
        }
        else if (a == 3 || a == 4 || a == 5)
        {
        }
        else
        {
        }
    }
}");
        }

        [Fact]
        public void TestOperatorPriority()
        {
            Test<ConvertSwitchToIfCodeRefactoringProvider>(@"
class TestClass
{
    int TestMethod(int a)
    {
        $switch (a) {
        case 0:
            return 0;
        case 1 == 1 ? 1 : 2:
            return 1;
        default:
            return 2;
        }
    }
}", @"
class TestClass
{
    int TestMethod(int a)
    {
        if (a == 0)
        {
            return 0;
        }
        else if (a == (1 == 1 ? 1 : 2))
        {
            return 1;
        }
        else
        {
            return 2;
        }
    }
}");
        }

        [Fact]
        public void TestEmptySwitch()
        {
            TestWrongContext<ConvertSwitchToIfCodeRefactoringProvider>(@"
class TestClass
{
    void TestMethod (int a)
    {
        $switch (a)
        {
        }
    }
}");
        }

        [Fact]
        public void TestSwitchWithDefaultOnly()
        {
            TestWrongContext<ConvertSwitchToIfCodeRefactoringProvider>(@"
class TestClass
{
    void TestMethod (int a)
    {
        $switch (a)
        {
            case 0:
            default:
            break;
        }
    }
}");
        }

        [Fact]
        public void TestNonTrailingBreak()
        {
            TestWrongContext<ConvertSwitchToIfCodeRefactoringProvider>(@"
class TestClass
{
    void TestMethod (int a, int b)
    {
        $switch (a)
        {
            case 0:
                if (b == 0) break;
                b = 1;
                break;
            default:
            break;
        }
    }
}");
        }

    }
}
