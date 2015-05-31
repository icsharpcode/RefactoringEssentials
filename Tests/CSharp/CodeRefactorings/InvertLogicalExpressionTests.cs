using NUnit.Framework;
using RefactoringEssentials.CSharp.CodeRefactorings;

namespace RefactoringEssentials.Tests.CSharp.CodeRefactorings
{
    [TestFixture]
    public class InvertLogicalExpressionTests : CSharpCodeRefactoringTestBase
    {
        [Test]
        public void ConditionlAnd()
        {
            Test<InvertLogicalExpressionCodeRefactoringProvider>(@"
class TestClass
{
    public void F()
    {
        bool a = true;
        bool b = false;
        if (a $&& b)
        {
        }
    }
}", @"
class TestClass
{
    public void F()
    {
        bool a = true;
        bool b = false;
        if (!(!a || !b))
        {
        }
    }
}");
        }

        [Test]
        public void ConditionlAndReverse()
        {
            Test<InvertLogicalExpressionCodeRefactoringProvider>(@"
class TestClass
{
    public void F()
    {
        bool a = true;
        bool b = false;
        if (!(!a $|| !b))
        {
        }
    }
}", @"
class TestClass
{
    public void F()
    {
        bool a = true;
        bool b = false;
        if (a && b)
        {
        }
    }
}");
        }

        [Test]
        public void ConditionlOr()
        {
            Test<InvertLogicalExpressionCodeRefactoringProvider>(@"
class TestClass
{
    public void F()
    {
        bool a = true;
        bool b = false;
        if (a $|| b)
        {
        }
    }
}", @"
class TestClass
{
    public void F()
    {
        bool a = true;
        bool b = false;
        if (!(!a && !b))
        {
        }
    }
}");
        }

        [Test]
        public void ConditionlOrReverse()
        {
            Test<InvertLogicalExpressionCodeRefactoringProvider>(@"
class TestClass
{
    public void F()
    {
        bool a = true;
        bool b = false;
        if (!(!a $&& !b))
        {
        }
    }
}", @"
class TestClass
{
    public void F()
    {
        bool a = true;
        bool b = false;
        if (a || b)
        {
        }
    }
}");
        }


        [Test]
        public void ConditionlAnd2()
        {
            Test<InvertLogicalExpressionCodeRefactoringProvider>(@"
class TestClass
{
    public void F()
    {
        int a = 1;
        bool b = true;
        if ((a > 1) $&& b)
        {
        }
    }
}", @"
class TestClass
{
    public void F()
    {
        int a = 1;
        bool b = true;
        if (!((a <= 1) || !b))
        {
        }
    }
}");
        }

        [Test]
        public void ConditionlOr2()
        {
            Test<InvertLogicalExpressionCodeRefactoringProvider>(@"
class TestClass
{
    public void F()
    {
        int a = 1;
        bool b = true;
        if (!((a > 1) $|| b))
        {
        }
    }
}", @"
class TestClass
{
    public void F()
    {
        int a = 1;
        bool b = true;
        if ((a <= 1) && !b)
        {
        }
    }
}");
        }

        [Test]
        public void Equals()
        {
            Test<InvertLogicalExpressionCodeRefactoringProvider>(@"
class TestClass
{
    public void F()
    {
        bool a = true;
        bool b = false;
        if (a $== b)
        {
        }
    }
}", @"
class TestClass
{
    public void F()
    {
        bool a = true;
        bool b = false;
        if (!(a != b))
        {
        }
    }
}");
        }

        [Test]
        public void EqualsReverse()
        {
            Test<InvertLogicalExpressionCodeRefactoringProvider>(@"
class TestClass
{
    public void F()
    {
        bool a = true;
        bool b = false;
        if (!(a $!= b))
        {
        }
    }
}", @"
class TestClass
{
    public void F()
    {
        bool a = true;
        bool b = false;
        if (a == b)
        {
        }
    }
}");
        }


        [Test]
        public void TestNullCoalescing()
        {
            TestWrongContext<InvertLogicalExpressionCodeRefactoringProvider>(@"
class Foo
{
    void Bar (object i, object j)
    {
        Console.WriteLine (i $?? j);
    }
}
");
        }


        [Test]
        public void TestUnaryExpression()
        {
            Test<InvertLogicalExpressionCodeRefactoringProvider>(@"
class Foo
{
    void Bar (bool a, bool b)
    {
        Console.WriteLine ($!(a && b));
    }
}
", @"
class Foo
{
    void Bar (bool a, bool b)
    {
        Console.WriteLine (!a || !b);
    }
}
");
        }
    }
}
