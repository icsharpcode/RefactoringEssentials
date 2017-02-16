using RefactoringEssentials.CSharp.CodeRefactorings;
using Xunit;

namespace RefactoringEssentials.Tests.CSharp.CodeRefactorings
{
    public class ConvertStatementBodyToExpressionBodyTests : CSharpCodeRefactoringTestBase
    {
        [Fact]
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

        [Fact]
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

        [Fact]
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

        [Fact]
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

        [Fact]
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

        [Fact]
        public void TestMethodWithExpressionBody()
        {
            TestWrongContext<ConvertStatementBodyToExpressionBodyCodeRefactoringProvider>(@"
class TestClass
{
    int TestMethod(int i) => i << 8;
}");
        }

        [Fact]
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

        [Fact]
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

        [Fact]
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

        [Fact]
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

        [Fact]
        public void TestPropertyWithExpressionBody()
        {
            TestWrongContext<ConvertStatementBodyToExpressionBodyCodeRefactoringProvider>(@"
class TestClass
{
    int $TestProperty => 5;
}");
        }

        [Fact]
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

        [Fact]
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

        [Fact]
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

        [Fact]
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

