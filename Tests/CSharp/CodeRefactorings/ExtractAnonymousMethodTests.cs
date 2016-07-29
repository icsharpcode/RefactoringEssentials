using NUnit.Framework;
using RefactoringEssentials.CSharp.CodeRefactorings;

namespace RefactoringEssentials.Tests.CSharp.CodeRefactorings
{
    [TestFixture]
    public class ExtractAnonymousMethodTests : CSharpCodeRefactoringTestBase
    {
        [Test]
        public void TestLambdaWithBodyStatement()
        {
            Test<ExtractAnonymousMethodCodeRefactoringProvider>(@"
class TestClass
{
    void TestMethod ()
    {
        System.Action<Int32> a = i $=>  { i++; };
    }
}", @"
class TestClass
{
    void Method(Int32 i)
    {
        i++;
    }
    void TestMethod ()
    {
        System.Action<Int32> a = Method;
    }
}");
        }

        [Test]
        public void TestLambdaWithBodyExpression()
        {
            Test<ExtractAnonymousMethodCodeRefactoringProvider>(@"
class TestClass
{
    void TestMethod ()
    {
        System.Action<Int32> a = i $=> i++;
    }
}", @"
class TestClass
{
    void Method(Int32 i)
    {
        i++;
    }
    void TestMethod ()
    {
        System.Action<Int32> a = Method;
    }
}");

            Test<ExtractAnonymousMethodCodeRefactoringProvider>(@"
class TestClass
{
    void TestMethod ()
    {
        System.Func<Int32> a = () $=> 1;
    }
}", @"
class TestClass
{
    Int32 Method()
    {
        return 1;
    }
    void TestMethod ()
    {
        System.Func<Int32> a = Method;
    }
}");
        }

        [Test]
        public void TestAnonymousMethod()
        {
            Test<ExtractAnonymousMethodCodeRefactoringProvider>(@"
class TestClass
{
    void TestMethod ()
    {
        System.Action<Int32> a = $delegate (Int32 i) { i++; };
    }
}", @"
class TestClass
{
    void Method(Int32 i)
    {
        i++;
    }
    void TestMethod ()
    {
        System.Action<Int32> a = Method;
    }
}");
        }

        [Test]
        public void TestContainLocalReference()
        {
            TestWrongContext<ExtractAnonymousMethodCodeRefactoringProvider>(@"
class TestClass
{
    void TestMethod ()
    {
        Int32 j = 1;
        System.Func<Int32, Int32> a = $delegate (Int32 i) { return i + j; };
    }
}");
        }

        [Test]
        public void TestLambdaInField()
        {
            Test<ExtractAnonymousMethodCodeRefactoringProvider>(@"
class TestClass
{
    System.Action<Int32> a = i $=>  { i++; };
}", @"
class TestClass
{
    void Method(Int32 i)
    {
        i++;
    }
    System.Action<Int32> a = Method;
}");
        }

        [Test]
        public void TestNameClash()
        {
            Test<ExtractAnonymousMethodCodeRefactoringProvider>(@"
class TestClass
{
    int Method()
    {
    }

    void TestMethod ()
    {
        System.Action<Int32> a = i $=>  { i++; };
    }
}", @"
class TestClass
{
    int Method()
    {
    }

    void Method1(Int32 i)
    {
        i++;
    }

    void TestMethod ()
    {
        System.Action<Int32> a = Method1;
    }
}");
        }
    }
}
