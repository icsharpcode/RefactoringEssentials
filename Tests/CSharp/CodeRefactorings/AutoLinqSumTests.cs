using RefactoringEssentials.CSharp.CodeRefactorings;
using Xunit;

namespace RefactoringEssentials.Tests.CSharp.CodeRefactorings
{
    public class AutoLinqSumActionTests : CSharpCodeRefactoringTestBase
    {
        [Fact]
        public void TestSimpleIntegerLoop()
        {
            string source = @"
using System.Linq;

class TestClass
{
    void TestMethod() {
        int result = 0;
        var list = new int[] { 1, 2, 3 };
        $foreach (var x in list)
            result += x;
    }
}";

            string result = @"
using System.Linq;

class TestClass
{
    void TestMethod() {
        int result = 0;
        var list = new int[] { 1, 2, 3 };
        result += list.Sum();
    }
}";

            Test<AutoLinqSumAction>(source, result);
        }

        [Fact]
        public void TestMergedIntegerLoop()
        {
            string source = @"
using System.Linq;

class TestClass
{
    void TestMethod() {
        var list = new int[] { 1, 2, 3 };
        int result = 0;
        $foreach (var x in list)
            result += x;
    }
}";

            string result = @"
using System.Linq;

class TestClass
{
    void TestMethod() {
        var list = new int[] { 1, 2, 3 };
        int result = list.Sum();
    }
}";

            Test<AutoLinqSumAction>(source, result);
        }

        [Fact]
        public void TestNonZeroMergedIntegerLoop()
        {
            string source = @"
using System.Linq;

class TestClass
{
    void TestMethod() {
        var list = new int[] { 1, 2, 3 };
        int result = 1;
        $foreach (var x in list)
            result += x;
    }
}";

            string result = @"
using System.Linq;

class TestClass
{
    void TestMethod() {
        var list = new int[] { 1, 2, 3 };
        int result = 1 + list.Sum();
    }
}";

            Test<AutoLinqSumAction>(source, result);
        }

        [Fact]
        public void TestMergedAssignmentIntegerLoop()
        {
            string source = @"
using System.Linq;

class TestClass
{
    void TestMethod() {
        var list = new int[] { 1, 2, 3 };
        int result;
        result = 1;
        $foreach (var x in list)
            result += x;
    }
}";

            string result = @"
using System.Linq;

class TestClass
{
    void TestMethod() {
        var list = new int[] { 1, 2, 3 };
        int result;
        result = 1 + list.Sum();
    }
}";

            Test<AutoLinqSumAction>(source, result);
        }

        [Fact]
        public void TestMergedDecimal()
        {
            string source = @"
using System.Linq;

class TestClass
{
    void TestMethod() {
        var list = new int[] { 1, 2, 3 };
        decimal result = 0.0m;
        $foreach (var x in list)
            result += x;
    }
}";

            string result = @"
using System.Linq;

class TestClass
{
    void TestMethod() {
        var list = new int[] { 1, 2, 3 };
        decimal result = list.Sum();
    }
}";

            Test<AutoLinqSumAction>(source, result);
        }

        [Fact]
        public void TestIntegerLoopInBlock()
        {
            string source = @"
using System.Linq;

class TestClass
{
    void TestMethod() {
        int result = 0;
        var list = new int[] { 1, 2, 3 };
        $foreach (var x in list) {
            result += x;
        }
    }
}";

            string result = @"
using System.Linq;

class TestClass
{
    void TestMethod() {
        int result = 0;
        var list = new int[] { 1, 2, 3 };
        result += list.Sum();
    }
}";

            Test<AutoLinqSumAction>(source, result);
        }

        [Fact]
        public void TestExpression()
        {
            string source = @"
using System.Linq;

class TestClass
{
    void TestMethod() {
        int result = 0;
        var list = new int[] { 1, 2, 3 };
        $foreach (var x in list) {
            result += x * 2;
        }
    }
}";

            string result = @"
using System.Linq;

class TestClass
{
    void TestMethod() {
        int result = 0;
        var list = new int[] { 1, 2, 3 };
        result += list.Sum(x => x * 2);
    }
}";

            Test<AutoLinqSumAction>(source, result);
        }

        [Fact]
        public void TestDisabledForStrings()
        {
            string source = @"
using System.Linq;

class TestClass
{
    void TestMethod() {
        string result = string.Empty;
        var list = new string[] { ""a"", ""b"" };
        $foreach (var x in list) {
            result += x;
        }
    }
}";
            TestWrongContext<AutoLinqSumAction>(source);
        }

        [Fact]
        public void TestShort()
        {
            string source = @"
using System.Linq;

class TestClass
{
    void TestMethod() {
        short result = 0;
        var list = new short[] { 1, 2, 3 };
        $foreach (var x in list)
            result += x;
    }
}";

            string result = @"
using System.Linq;

class TestClass
{
    void TestMethod() {
        short result = 0;
        var list = new short[] { 1, 2, 3 };
        result += list.Sum();
    }
}";

            Test<AutoLinqSumAction>(source, result);
        }

        [Fact]
        public void TestLong()
        {
            string source = @"
using System.Linq;

class TestClass
{
    void TestMethod() {
        long result = 0;
        var list = new long[] { 1, 2, 3 };
        $foreach (var x in list)
            result += x;
    }
}";

            string result = @"
using System.Linq;

class TestClass
{
    void TestMethod() {
        long result = 0;
        var list = new long[] { 1, 2, 3 };
        result += list.Sum();
    }
}";
            Test<AutoLinqSumAction>(source, result);
        }

        [Fact]
        public void TestUnsignedLong()
        {
            string source = @"
using System.Linq;

class TestClass
{
    void TestMethod() {
        ulong result = 0;
        var list = new ulong[] { 1, 2, 3 };
        $foreach (var x in list)
            result += x;
    }
}";

            string result = @"
using System.Linq;

class TestClass
{
    void TestMethod() {
        ulong result = 0;
        var list = new ulong[] { 1, 2, 3 };
        result += list.Sum();
    }
}";

            Test<AutoLinqSumAction>(source, result);
        }

        [Fact]
        public void TestFloat()
        {
            string source = @"
using System.Linq;

class TestClass
{
    void TestMethod() {
        float result = 0;
        var list = new float[] { 1, 2, 3 };
        $foreach (var x in list)
            result += x;
    }
}";

            string result = @"
using System.Linq;

class TestClass
{
    void TestMethod() {
        float result = 0;
        var list = new float[] { 1, 2, 3 };
        result += list.Sum();
    }
}";

            Test<AutoLinqSumAction>(source, result);
        }

        [Fact]
        public void TestDouble()
        {
            string source = @"
using System.Linq;

class TestClass
{
    void TestMethod() {
        double result = 0;
        var list = new double[] { 1, 2, 3 };
        $foreach (var x in list)
            result += x;
    }
}";

            string result = @"
using System.Linq;

class TestClass
{
    void TestMethod() {
        double result = 0;
        var list = new double[] { 1, 2, 3 };
        result += list.Sum();
    }
}";

            Test<AutoLinqSumAction>(source, result);
        }

        [Fact]
        public void TestDecimal()
        {
            string source = @"
using System.Linq;

class TestClass
{
    void TestMethod() {
        decimal result = 0;
        var list = new decimal[] { 1, 2, 3 };
        $foreach (var x in list)
            result += x;
    }
}";

            string result = @"
using System.Linq;

class TestClass
{
    void TestMethod() {
        decimal result = 0;
        var list = new decimal[] { 1, 2, 3 };
        result += list.Sum();
    }
}";

            Test<AutoLinqSumAction>(source, result);
        }

        [Fact]
        public void TestMinus()
        {
            string source = @"
using System.Linq;

class TestClass
{
    void TestMethod() {
        int result = 0;
        var list = new int[] { 1, 2, 3 };
        $foreach (var x in list) {
            result -= x;
        }
    }
}";

            string result = @"
using System.Linq;

class TestClass
{
    void TestMethod() {
        int result = 0;
        var list = new int[] { 1, 2, 3 };
        result += list.Sum(x => -x);
    }
}";

            Test<AutoLinqSumAction>(source, result);
        }

        [Fact]
        public void TestCombined()
        {
            string source = @"
using System.Linq;

class TestClass
{
    void TestMethod() {
        int result = 0;
        var list = new int[] { 1, 2, 3 };
        $foreach (var x in list) {
            result += x;
            result += 2 * x;
        }
    }
}";

            string result = @"
using System.Linq;

class TestClass
{
    void TestMethod() {
        int result = 0;
        var list = new int[] { 1, 2, 3 };
        result += list.Sum(x => x + (2 * x));
    }
}";

            Test<AutoLinqSumAction>(source, result);
        }

        [Fact]
        public void TestCombinedPrecedence()
        {
            string source = @"
using System.Linq;

class TestClass
{
    void TestMethod() {
        int result = 0;
        var list = new int[] { 1, 2, 3 };
        $foreach (var x in list) {
            result += x;
            result += x << 1;
        }
    }
}";

            string result = @"
using System.Linq;

class TestClass
{
    void TestMethod() {
        int result = 0;
        var list = new int[] { 1, 2, 3 };
        result += list.Sum(x => x + (x << 1));
    }
}";

            Test<AutoLinqSumAction>(source, result);
        }

        [Fact]
        public void TestEmptyStatements()
        {
            string source = @"
using System.Linq;

class TestClass
{
    void TestMethod() {
        int result = 0;
        var list = new int[] { 1, 2, 3 };
        $foreach (var x in list) {
            result += x;
            ;
        }
    }
}";

            string result = @"
using System.Linq;

class TestClass
{
    void TestMethod() {
        int result = 0;
        var list = new int[] { 1, 2, 3 };
        result += list.Sum();
    }
}";

            Test<AutoLinqSumAction>(source, result);
        }

        [Fact]
        public void TestSimpleConditional()
        {
            string source = @"
using System.Linq;

class TestClass
{
    void TestMethod() {
        int result = 0;
        var list = new int[] { 1, 2, 3 };
        $foreach (var x in list) {
            if (x > 0)
                result += x;
        }
    }
}";

            string result = @"
using System.Linq;

class TestClass
{
    void TestMethod() {
        int result = 0;
        var list = new int[] { 1, 2, 3 };
        result += list.Where(x => x > 0).Sum();
    }
}";

            Test<AutoLinqSumAction>(source, result);
        }

        [Fact]
        public void TestInvertedConditional()
        {
            string source = @"
using System.Linq;

class TestClass
{
    void TestMethod() {
        int result = 0;
        var list = new int[] { 1, 2, 3 };
        $foreach (var x in list) {
            if (x > 0)
                ;
            else
                result += x;
        }
    }
}";

            string result = @"
using System.Linq;

class TestClass
{
    void TestMethod() {
        int result = 0;
        var list = new int[] { 1, 2, 3 };
        result += list.Where(x => x <= 0).Sum();
    }
}";

            Test<AutoLinqSumAction>(source, result);
        }

        [Fact]
        public void TestIncrement()
        {
            string source = @"
using System.Linq;

class TestClass
{
    void TestMethod() {
        int result = 0;
        var list = new int[] { 1, 2, 3 };
        $foreach (var x in list) {
            result++;
        }
    }
}";

            string result = @"
using System.Linq;

class TestClass
{
    void TestMethod() {
        int result = 0;
        var list = new int[] { 1, 2, 3 };
        result += list.Count();
    }
}";

            Test<AutoLinqSumAction>(source, result);
        }

        [Fact]
        public void TestCompleteConditional()
        {
            string source = @"
using System.Linq;

class TestClass
{
    void TestMethod() {
        int result = 0;
        var list = new int[] { 1, 2, 3 };
        $foreach (var x in list) {
            if (x > 0)
                result += x * 2;
            else
                result += x;
        }
    }
}";

            string result = @"
using System.Linq;

class TestClass
{
    void TestMethod() {
        int result = 0;
        var list = new int[] { 1, 2, 3 };
        result += list.Sum(x => x > 0 ? x * 2 : x);
    }
}";

            Test<AutoLinqSumAction>(source, result);
        }

        [Fact]
        public void TestDisabledForSideEffects()
        {
            string source = @"
using System.Linq;

class TestClass
{
    void TestMethod() {
        string result = string.Empty;
        var list = new string[] { ""a"", ""b"" };
        $foreach (var x in list) {
            TestMethod();
            result += x;
        }
    }
}";
            TestWrongContext<AutoLinqSumAction>(source);
        }

        [Fact]
        public void TestDisabledForInnerAssignments()
        {
            string source = @"
using System.Linq;

class TestClass
{
    void TestMethod() {
        int result = 0;
        var list = new int[] { 1, 2 };
        int p = 0;
        $foreach (var x in list) {
            result += (p = x);
        }
    }
}";
            TestWrongContext<AutoLinqSumAction>(source);
        }

        [Fact]
        public void TestDisabledForInnerIncrements()
        {
            string source = @"
using System.Linq;

class TestClass
{
    void TestMethod() {
        int result = 0;
        var list = new int[] { 1, 2 };
        int p = 0;
        $foreach (var x in list) {
            result += (p++);
        }
    }
}";
            TestWrongContext<AutoLinqSumAction>(source);
        }

        [Fact]
        public void TestDisabledForNoLinq()
        {
            string source = @"
class TestClass
{
    void TestMethod() {
        int result = 0;
        var list = new int[] { 1, 2 };
        $foreach (var x in list) {
            result += x;
        }
    }
}";
            TestWrongContext<AutoLinqSumAction>(source);
        }
    }
}

