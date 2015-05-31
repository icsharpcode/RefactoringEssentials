using NUnit.Framework;
using RefactoringEssentials.CSharp.CodeRefactorings;

namespace RefactoringEssentials.Tests.CSharp.CodeRefactorings
{
    [TestFixture]
    public class ConvertIfStatementToNullCoalescingExpressionActionTests : CSharpCodeRefactoringTestBase
    {
        [Test]
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

        [Test]
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

        [Test]
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

        [Test]
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

        [Test]
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

        [Test]
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

        [Test]
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

        [Test]
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

        [Test]
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

        [Test]
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
