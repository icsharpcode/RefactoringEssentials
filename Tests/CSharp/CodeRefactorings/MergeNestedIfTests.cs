using NUnit.Framework;
using RefactoringEssentials.CSharp.CodeRefactorings;

namespace RefactoringEssentials.Tests.CSharp.CodeRefactorings
{
    [TestFixture]
    public class MergeNestedIfTests : CSharpCodeRefactoringTestBase
    {
        [Test]
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

        [Test]
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

        [Test]
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

        [Test]
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

        [Test]
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

        [Test]
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

        [Test]
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

        [Test]
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

        [Test]
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
