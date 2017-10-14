using RefactoringEssentials.CSharp.CodeRefactorings;
using Xunit;

namespace RefactoringEssentials.Tests.CSharp.CodeRefactorings
{
    public class ConvertIfStatementToReturnStatementActionTests : CSharpCodeRefactoringTestBase
    {
        [Fact]
        public void TestReturn()
        {
            Test<ConvertIfStatementToReturnStatementAction>(@"
class TestClass
{
    int TestMethod(int i)
    {
        $if (i > 0)
            return 1;
        else
            return 0;
    }
}", @"
class TestClass
{
    int TestMethod(int i)
    {
        return i > 0 ? 1 : 0;
    }
}");
        }


        [Fact]
        public void TestReturnWithComment1()
        {
            Test<ConvertIfStatementToReturnStatementAction>(@"
class TestClass
{
    int TestMethod(int i)
    {
        // Some comment
        $if (i > 0)
            return 1;
        else
            return 0;
    }
}", @"
class TestClass
{
    int TestMethod(int i)
    {
        // Some comment
        return i > 0 ? 1 : 0;
    }
}");
        }


        [Fact]
        public void TestReturnWithComment2()
        {
            Test<ConvertIfStatementToReturnStatementAction>(@"
class TestClass
{
    int TestMethod(int i)
    {
        $if (i > 0)
            return 1; // Some comment
        else
            return 0;
    }
}", @"
class TestClass
{
    int TestMethod(int i)
    {
        return i > 0 ? 1 : 0;
    }
}");
        }


        [Fact]
        public void TestReturnWithComment3()
        {
            Test<ConvertIfStatementToReturnStatementAction>(@"
class TestClass
{
    int TestMethod(int i)
    {
        $if (i > 0)
            // Some comment
            return 1;
        else
            return 0;
    }
}", @"
class TestClass
{
    int TestMethod(int i)
    {
        return i > 0 ? 1 : 0;
    }
}");
        }

        [Fact]
        public void TestIfElseWithBlocks()
        {
            Test<ConvertIfStatementToReturnStatementAction>(@"class Foo
{
    bool Bar(string str)
    {
        $if (str.Length > 10) {
            return true;
        } else {
            return false;
        }
    }
}", @"class Foo
{
    bool Bar(string str)
    {
        return str.Length > 10;
    }
}");
        }

        [Fact]
        public void TestImplicitElse()
        {

            Test<ConvertIfStatementToReturnStatementAction>(@"
class TestClass
{
    int TestMethod(int i)
    {
        $if (i > 0)
            return 1;
        return 0;
    }
}", @"
class TestClass
{
    int TestMethod(int i)
    {
        return i > 0 ? 1 : 0;
    }
}");
        }

        /// <summary>
        /// Bug 'ConvertIfStatementToReturnStatementAction crashes on "if" without "else" #63'
        /// </summary>
        [Fact]
        public void TestIssue63()
        {
            var actions = GetActions<ConvertIfStatementToReturnStatementAction>(@"
class TestClass
{
    int TestMethod(int a)
    {
        if (a > 0)
            $if (a < 5)
            {
                return 1;
            }

        return 0;
    }
}");
            Assert.Equal(0, actions.Count);
        }
    }
}
