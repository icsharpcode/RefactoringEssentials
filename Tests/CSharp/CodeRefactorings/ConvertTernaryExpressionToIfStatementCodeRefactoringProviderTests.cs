using RefactoringEssentials.CSharp.CodeRefactorings;
using Xunit;

namespace RefactoringEssentials.Tests.CSharp.CodeRefactorings
{
    public class ConvertTernaryExpressionToIfStatementCodeRefactoringProviderTests : CSharpCodeRefactoringTestBase
    {
        [Fact]
        public void TestConditionalOperator()
        {
            Test<ConvertTernaryExpressionToIfStatementCodeRefactoringProvider>(@"
class TestClass
{
    int TestMethod (int o, int p)
    {
        int z;
        z $= i > 0 ? o : p;
        return z;
    }
}", @"
class TestClass
{
    int TestMethod (int o, int p)
    {
        int z;
        if (i > 0)
            z = o;
        else
            z = p;
        return z;
    }
}");
        }

        [Fact]
        public void TestNullCoalescingOperator()
        {
            Test<ConvertTernaryExpressionToIfStatementCodeRefactoringProvider>(@"
class Test
{
    object TestMethod(object o, object p)
    {
        object z;
        z $= o ?? p;
        return z;
    }
}", @"
class Test
{
    object TestMethod(object o, object p)
    {
        object z;
        if (o != null)
            z = o;
        else
            z = p;
        return z;
    }
}");
        }

        [Fact]
        public void TestEmbeddedStatement()
        {
            Test<ConvertTernaryExpressionToIfStatementCodeRefactoringProvider>(@"
class TestClass
{
    void TestMethod(int i)
    {
        int a;
        if (i < 10)
            a $= i > 0 ? 0 : 1;
    }
}", @"
class TestClass
{
    void TestMethod(int i)
    {
        int a;
        if (i < 10)
            if (i > 0)
                a = 0;
            else
                a = 1;
    }
}");
        }


        [Fact]
        public void TestAssignment()
        {
            Test<ConvertTernaryExpressionToIfStatementCodeRefactoringProvider>(@"
class TestClass
{
    void TestMethod (int i)
    {
        int a;
        a $= i > 0 ? 0 : 1;
    }
}", @"
class TestClass
{
    void TestMethod (int i)
    {
        int a;
        if (i > 0)
            a = 0;
        else
            a = 1;
    }
}");
            Test<ConvertTernaryExpressionToIfStatementCodeRefactoringProvider>(@"
class TestClass
{
    void TestMethod (int i)
    {
        int a;
        a $+= i > 0 ? 0 : 1;
    }
}", @"
class TestClass
{
    void TestMethod (int i)
    {
        int a;
        if (i > 0)
            a += 0;
        else
            a += 1;
    }
}");
        }

        [Fact]
        public void TestReturnConditionalOperator()
        {
            Test<ConvertTernaryExpressionToIfStatementCodeRefactoringProvider>(@"
class TestClass
{
    int TestMethod(int i)
    {
        $return i > 0 ? 1 : 0;
    }
}", @"
class TestClass
{
    int TestMethod(int i)
    {
        if (i > 0)
            return 1;
        return 0;
    }
}");
        }

        [Fact]
        public void TestReturnConditionalOperatorWithComment()
        {
            Test<ConvertTernaryExpressionToIfStatementCodeRefactoringProvider>(@"
class TestClass
{
    int TestMethod(int i)
    {
        // Some comment
        $return i > 0 ? 1 : 0;
    }
}", @"
class TestClass
{
    int TestMethod(int i)
    {
        // Some comment
        if (i > 0)
            return 1;
        return 0;
    }
}");
        }

        [Fact]
        public void TestReturnNullCoalescingOperator()
        {
            Test<ConvertTernaryExpressionToIfStatementCodeRefactoringProvider>(@"
class Test
{
    object Foo(object o, object p)
    {
        $return o ?? p;
    }
}", @"
class Test
{
    object Foo(object o, object p)
    {
        if (o != null)
            return o;
        return p;
    }
}");
        }
    }
}