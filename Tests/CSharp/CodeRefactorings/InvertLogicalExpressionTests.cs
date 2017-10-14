using RefactoringEssentials.CSharp.CodeRefactorings;
using Xunit;

namespace RefactoringEssentials.Tests.CSharp.CodeRefactorings
{
    public class InvertLogicalExpressionTests : CSharpCodeRefactoringTestBase
    {
        [Fact]
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

        [Fact]
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

        [Fact]
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

        [Fact]
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


        [Fact]
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

        [Fact]
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

        [Fact]
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

        [Fact]
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


        [Fact]
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


        [Fact]
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
