using NUnit.Framework;
using RefactoringEssentials.CSharp.CodeRefactorings;

namespace RefactoringEssentials.Tests.CSharp.CodeRefactorings
{
    [TestFixture]
    public class ConvertStatementBodyToExpressionBodyTests : CSharpCodeRefactoringTestBase
    {
        [Test]
        public void TestMethodName()
        {
            Test<ConvertStatementBodyToExpressionBodyCodeRefactoringProvider>(@"
class TestClass
{
    int $TestMethod(int i)
    {
        return i << 8;
    }
}", @"
class TestClass
{
    int TestMethod(int i) => i << 8;
}");
        }

        [Test]
        public void TestMethodReturn()
        {
            Test<ConvertStatementBodyToExpressionBodyCodeRefactoringProvider>(@"
class TestClass
{
    int TestMethod(int i)
    {
        $return i << 8;
    }
}", @"
class TestClass
{
    int TestMethod(int i) => i << 8;
}");
        }

        [Test]
        public void TestVoidMethodNameWithExpression()
        {
            Test<ConvertStatementBodyToExpressionBodyCodeRefactoringProvider>(@"
class TestClass
{
    void DoSomething() { }

    void $TestMethod(int i)
    {
        DoSomething();
    }
}", @"
class TestClass
{
    void DoSomething() { }

    void TestMethod(int i) => DoSomething();
}");
        }

        [Test]
        public void TestMethodNameWithCommentInBody()
        {
            Test<ConvertStatementBodyToExpressionBodyCodeRefactoringProvider>(@"
class TestClass
{
    int $TestMethod(int i)
    {
        return i << 8; // Some comment
    }
}", @"
class TestClass
{
    int TestMethod(int i) => i << 8; // Some comment
}");
        }

        [Test]
        public void TestInvalidMethod()
        {
            TestWrongContext<ConvertStatementBodyToExpressionBodyCodeRefactoringProvider>(@"
class TestClass
{
    int TestMethod(int i)
    {
		System.Console.WriteLine ();
        $return i << 8;
    }
}");
        }


        [Test]
        public void TestPropertyName()
        {
            Test<ConvertStatementBodyToExpressionBodyCodeRefactoringProvider>(@"
class TestClass
{
    int $TestProperty  {
        get {
            return 321;
        }
    }
}", @"
class TestClass
{
    int TestProperty => 321;
}");
        }

        [Test]
        public void TestPropertyReturn()
        {
            Test<ConvertStatementBodyToExpressionBodyCodeRefactoringProvider>(@"
class TestClass
{
    int TestProperty  {
        get {
            $return 321;
        }
    }
}", @"
class TestClass
{
    int TestProperty => 321;
}");
        }

        [Test]
        public void TestPropertyNameWithCommentInBody()
        {
            Test<ConvertStatementBodyToExpressionBodyCodeRefactoringProvider>(@"
class TestClass
{
    int $TestProperty  {
        get {
            return 321; // Some comment
        }
    }
}", @"
class TestClass
{
    int TestProperty => 321; // Some comment
}");
        }

        [Test]
        public void TestInvalidProperty()
        {
            TestWrongContext<ConvertStatementBodyToExpressionBodyCodeRefactoringProvider>(@"
class TestClass
{
    int $TestProperty  {
        get {
            return 321;
        }
		set {} 
    }
}");
        }

        [Test]
        public void TestIndexerName()
        {
            Test<ConvertStatementBodyToExpressionBodyCodeRefactoringProvider>(@"
class TestClass
{
    int $this[int index]  {
        get {
            return list[index];
        }
    }
}", @"
class TestClass
{
    int this[int index] => list[index];
}");
        }

        [Test]
        public void TestIndexerReturn()
        {
            Test<ConvertStatementBodyToExpressionBodyCodeRefactoringProvider>(@"
class TestClass
{
    int $this[int index]  {
        get {
            return list[index];
        }
    }
}", @"
class TestClass
{
    int this[int index] => list[index];
}");
        }

        [Test]
        public void TestIndexerNameWithCommentInBody()
        {
            Test<ConvertStatementBodyToExpressionBodyCodeRefactoringProvider>(@"
class TestClass
{
    int $this[int index]  {
        get {
            return list[index]; // Some comment
        }
    }
}", @"
class TestClass
{
    int this[int index] => list[index]; // Some comment
}");
        }

        [Test]
        public void TestInvalidIndexer()
        {
            TestWrongContext<ConvertStatementBodyToExpressionBodyCodeRefactoringProvider>(@"
class TestClass
{
    int $this[int index]  {
        get {
            return list[index];
        }
		set {} 
    }
}");
        }
    }
}

