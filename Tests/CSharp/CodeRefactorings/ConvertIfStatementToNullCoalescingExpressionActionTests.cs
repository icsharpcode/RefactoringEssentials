using RefactoringEssentials.CSharp.CodeRefactorings;
using Xunit;

namespace RefactoringEssentials.Tests.CSharp.CodeRefactorings
{
    public class ConvertIfStatementToNullCoalescingExpressionActionTests : CSharpCodeRefactoringTestBase
    {
        [Fact]
        public void TestDeclaration()
        {
            Test<ConvertIfStatementToNullCoalescingExpressionAction>(@"
class TestClass
{
    void Foo()
    {
        return null;
    }

    void TestMethod()
    {
        object o = Foo();
        $if (o == null)
            o = new object();
    }
}", @"
class TestClass
{
    void Foo()
    {
        return null;
    }

    void TestMethod()
    {
        object o = Foo() ?? new object();
    }
}");
        }

        [Fact]
        public void TestDeclarationWithComment1()
        {
            Test<ConvertIfStatementToNullCoalescingExpressionAction>(@"
class TestClass
{
    void Foo()
    {
        return null;
    }

    void TestMethod()
    {
		// Some comment
        object o = Foo();
        $if (o == null)
            o = new object();
    }
}", @"
class TestClass
{
    void Foo()
    {
        return null;
    }

    void TestMethod()
    {
		// Some comment
        object o = Foo() ?? new object();
    }
}");
        }

        [Fact]
        public void TestDeclarationWithComment2()
        {
            Test<ConvertIfStatementToNullCoalescingExpressionAction>(@"
class TestClass
{
    void Foo()
    {
        return null;
    }

    void TestMethod()
    {
        object o = Foo();
		// Some comment
        $if (o == null)
            o = new object();
    }
}", @"
class TestClass
{
    void Foo()
    {
        return null;
    }

    void TestMethod()
    {
        object o = Foo() ?? new object();
    }
}");
        }

        [Fact]
        public void TestDeclarationWithComment3()
        {
            Test<ConvertIfStatementToNullCoalescingExpressionAction>(@"
class TestClass
{
    void Foo()
    {
        return null;
    }

    void TestMethod()
    {
        object o = Foo();
        $if (o == null)
            o = new object(); // Some comment
    }
}", @"
class TestClass
{
    void Foo()
    {
        return null;
    }

    void TestMethod()
    {
        object o = Foo() ?? new object();
    }
}");
        }

        [Fact]
        public void TestYodaConditionals()
        {
            Test<ConvertIfStatementToNullCoalescingExpressionAction>(@"
class TestClass
{
    void Foo()
    {
        return null;
    }
    void TestMethod()
    {
        object o = Foo();
        $if (null == o)
            o = new object();
    }
}", @"
class TestClass
{
    void Foo()
    {
        return null;
    }
    void TestMethod()
    {
        object o = Foo() ?? new object();
    }
}");
        }

        [Fact]
        public void TestAssignment()
        {
            Test<ConvertIfStatementToNullCoalescingExpressionAction>(@"
class TestClass
{
    void Foo()
    {
        return null;
    }
    void TestMethod()
    {
        object o;
        o = Foo();
        $if (o == null)
            o = new object();
    }
}", @"
class TestClass
{
    void Foo()
    {
        return null;
    }
    void TestMethod()
    {
        object o;
        o = Foo() ?? new object();
    }
}");
        }

        [Fact]
        public void TestIsolated()
        {
            Test<ConvertIfStatementToNullCoalescingExpressionAction>(@"
class TestClass
{
    object o;
    void TestMethod()
    {
        $if (o == null)
            o = new object();
    }
}", @"
class TestClass
{
    object o;
    void TestMethod()
    {
        o = o ?? new object();
    }
}");
        }

        [Fact]
        public void TestBlock()
        {
            Test<ConvertIfStatementToNullCoalescingExpressionAction>(@"
class TestClass
{
    void Foo()
    {
        return null;
    }
    void TestMethod()
    {
        object o = Foo();
        $if (o == null)
        {
            o = new object();
        }
    }
}", @"
class TestClass
{
    void Foo()
    {
        return null;
    }
    void TestMethod()
    {
        object o = Foo() ?? new object();
    }
}");
        }

        [Fact]
        public void TestInvertedCondition()
        {
            Test<ConvertIfStatementToNullCoalescingExpressionAction>(@"
class TestClass
{
    void Foo()
    {
        return null;
    }
    void TestMethod()
    {
        object o = Foo();
        $if (o != null)
        {
        } else {
            o = new object();
        }
    }
}", @"
class TestClass
{
    void Foo()
    {
        return null;
    }
    void TestMethod()
    {
        object o = Foo() ?? new object();
    }
}");
        }

        [Fact]
        public void TestDisabledForImproperCondition()
        {
            TestWrongContext<ConvertIfStatementToNullCoalescingExpressionAction>(@"
class TestClass
{
    void Foo()
    {
        return null;
    }
    void TestMethod()
    {
        object o = Foo ();
        $if (o != null)
        {
            o = new object();
        }
    }
}");
        }
    }
}
