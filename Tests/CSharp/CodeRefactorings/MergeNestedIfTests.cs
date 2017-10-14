using RefactoringEssentials.CSharp.CodeRefactorings;
using Xunit;

namespace RefactoringEssentials.Tests.CSharp.CodeRefactorings
{
    public class MergeNestedIfTests : CSharpCodeRefactoringTestBase
    {
        [Fact]
        public void TestOuterIf()
        {
            Test<MergeNestedIfAction>(@"
class TestClass
{
    int TestMethod (int a)
    {
        $if (a > 0)
            if (a < 5) 
                return 1;
    }
}", @"
class TestClass
{
    int TestMethod (int a)
    {
        if (a > 0 && a < 5)
            return 1;
    }
}");
        }

        [Fact]
        public void TestOuterIfWithBlock()
        {
            Test<MergeNestedIfAction>(@"
class TestClass
{
    int TestMethod (int a)
    {
        $if (a > 0) {

        {
            if (a < 5) 
                return 1;
        }

        }
    }
}", @"
class TestClass
{
    int TestMethod (int a)
    {
        if (a > 0 && a < 5)
            return 1;
    }
}");
        }

        [Fact]
        public void TestInnerIf()
        {
            Test<MergeNestedIfAction>(@"
class TestClass
{
    int TestMethod (int a)
    {
        if (a > 0)
            $if (a < 5) 
                return 1;
    }
}", @"
class TestClass
{
    int TestMethod (int a)
    {
        if (a > 0 && a < 5)
            return 1;
    }
}");
        }

        [Fact]
        public void TestInnerIfWithBlock()
        {
            Test<MergeNestedIfAction>(@"
class TestClass
{
    int TestMethod (int a)
    {
        if (a > 0) {

        {
            $if (a < 5) 
                return 1;
        }

        }
    }
}", @"
class TestClass
{
    int TestMethod (int a)
    {
        if (a > 0 && a < 5)
            return 1;
    }
}");
        }

        [Fact]
        public void TestOuterIfElse()
        {
            TestWrongContext<MergeNestedIfAction>(@"
class TestClass
{
    int TestMethod (int a)
    {
        $if (a > 0)
            if (a < 5) 
                return 1;
        else
            return 0;
    }
}");
            TestWrongContext<MergeNestedIfAction>(@"
class TestClass
{
    int TestMethod (int a)
    {
        $if (a > 0) {
            if (a < 5) 
                return 1;
        } else
            return 0;
    }
}");
        }

        [Fact]
        public void TestInnerIfElse()
        {
            TestWrongContext<MergeNestedIfAction>(@"
class TestClass
{
    int TestMethod (int a)
    {
        $if (a > 0)
            if (a < 5) 
                return 1;
            else
                return 0;
    }
}");
            TestWrongContext<MergeNestedIfAction>(@"
class TestClass
{
    int TestMethod (int a)
    {
        $if (a > 0) {
            if (a < 5) 
                return 1;
            else
                return 0;
        }
    }
}");
        }

        [Fact]
        public void TestMultipleTrueStatements()
        {
            TestWrongContext<MergeNestedIfAction>(@"
class TestClass
{
    int TestMethod (int a)
    {
        $if (a > 0) {
            if (a < 5) 
                return 1;
            return 0;
        }
    }
}");
            TestWrongContext<MergeNestedIfAction>(@"
class TestClass
{
    int TestMethod (int a)
    {
        if (a > 0) {
            $if (a < 5) 
                return 1;
            return 0;
        }
    }
}");
        }

        [Fact]
        public void TestInnerIfWithComplexCondition()
        {
            Test<MergeNestedIfAction>(@"
class TestClass
{
    int TestMethod (int a)
    {
        if (a > 0) {

        {
            $if (a < 5 || a < 10) 
                return 1;
        }

        }
    }
}", @"
class TestClass
{
    int TestMethod (int a)
    {
        if (a > 0 && (a < 5 || a < 10))
            return 1;
    }
}");
        }

        [Fact]
        public void TestOuterIfWithComplexCondition()
        {
            Test<MergeNestedIfAction>(@"
class TestClass
{
    int TestMethod (int a)
    {
        if (a > 0 || a < 10) {

        {
            $if (a < 5) 
                return 1;
        }

        }
    }
}", @"
class TestClass
{
    int TestMethod (int a)
    {
        if ((a > 0 || a < 10) && a < 5)
            return 1;
    }
}");
        }
    }
}
