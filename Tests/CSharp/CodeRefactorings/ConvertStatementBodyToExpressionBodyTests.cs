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
    }
}

