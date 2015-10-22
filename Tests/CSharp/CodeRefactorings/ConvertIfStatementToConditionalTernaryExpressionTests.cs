using NUnit.Framework;
using RefactoringEssentials.CSharp.CodeRefactorings;

namespace RefactoringEssentials.Tests.CSharp.CodeRefactorings
{
    [TestFixture]
    public class ConvertIfStatementToConditionalTernaryExpressionTests : CSharpCodeRefactoringTestBase
    {
        [Test]
        public void TestAssignment()
        {
            Test<ConvertIfStatementToConditionalTernaryExpressionCodeRefactoringProvider>(@"
class TestClass
{
    void TestMethod(int i)
    {
        int a;
        $if (i > 0) {
            a = 0;
        } else {
            a = 1;
        }
    }
}", @"
class TestClass
{
    void TestMethod(int i)
    {
        int a;
        a = i > 0 ? 0 : 1;
    }
}");
        }

        [Test]
        public void TestAssignmentWithComment()
        {
            Test<ConvertIfStatementToConditionalTernaryExpressionCodeRefactoringProvider>(@"
class TestClass
{
    void TestMethod(int i)
    {
        int a;
        // Some comment
        $if (i > 0) {
            a = 0;
        } else {
            a = 1;
        }
    }
}", @"
class TestClass
{
    void TestMethod(int i)
    {
        int a;
        // Some comment
        a = i > 0 ? 0 : 1;
    }
}");
        }

        [Test]
        public void TestAssignmentWithDifferingTypes1()
        {
            Test<ConvertIfStatementToConditionalTernaryExpressionCodeRefactoringProvider>(@"
class TestClass
{
    void TestMethod(int i)
    {
        int a;
        $if (i > 0) {
            a = 0;
        } else {
            a = ""1"";
        }
    }
}", @"
class TestClass
{
    void TestMethod(int i)
    {
        int a;
        a = i > 0 ? 0 : (int)""1"";
    }
}");
        }

        [Test]
        public void TestAssignmentWithDifferingTypes2()
        {
            Test<ConvertIfStatementToConditionalTernaryExpressionCodeRefactoringProvider>(@"
class TestClass
{
    void TestMethod(int i)
    {
        Nullable<T> a;
        $if (i > 0) {
            a = 0;
        } else {
            a = ""1"";
        }
    }
}", @"
class TestClass
{
    void TestMethod(int i)
    {
        Nullable<T> a;
        a = i > 0 ? (Nullable<T>)0 : (Nullable<T>)""1"";
    }
}");
        }

        [Test]
        public void TestAddAssignment()
        {
            Test<ConvertIfStatementToConditionalTernaryExpressionCodeRefactoringProvider>(@"
class TestClass
{
    void TestMethod(int i)
    {
        int a;
        $if (i > 0)
            a += 0;
        else {
            a += 1;
        }
    }
}", @"
class TestClass
{
    void TestMethod(int i)
    {
        int a;
        a += i > 0 ? 0 : 1;
    }
}");
        }

        [Test]
        public void TestIfElse()
        {
            TestWrongContext<ConvertIfStatementToConditionalTernaryExpressionCodeRefactoringProvider>(@"
class TestClass
{
    void TestMethod(int i)
    {
        int a;
        $if (i > 0)
            a = 0;
        else if (i < 5) {
            a = 1;
        } else {
            a = 2;
        }
    }
}");
        }

        [Test]
        public void MultipleStatementsInIf()
        {
            TestWrongContext<ConvertIfStatementToConditionalTernaryExpressionCodeRefactoringProvider>(@"
class TestClass
{
    void TestMethod(int i)
    {
        int a;
        $if (i > 0) {
            a = 0;
            a = 2;
        } else {
            a = 2;
        }
    }
}");
        }

        [Test]
        public void TestDifferentAssignmentOperator()
        {

            TestWrongContext<ConvertIfStatementToConditionalTernaryExpressionCodeRefactoringProvider>(@"
class TestClass
{
    void TestMethod(int i)
    {
        int a;
        $if (i > 0)
            a += 0;
        else {
            a -= 1;
        }
    }
}");
        }

        [Ignore("Are there any cases where this is needed ?")]
        [Test]
        public void TestInsertNecessaryParentheses()
        {
            Test<ConvertIfStatementToConditionalTernaryExpressionCodeRefactoringProvider>(@"
class TestClass
{
    void TestMethod (int i)
    {
        int a;
        int b;
        $if (i > 0)
            a = b = 0;
        else {
            a = 1;
        }
    }
}", @"
class TestClass
{
    void TestMethod (int i)
    {
        int a;
        int b;
        a = i > 0 ? (b = 0) : 1;
    }
}");
        }

        [Test]
        public void TestInvalidImplicitElse()
        {

            TestWrongContext<ConvertIfStatementToConditionalTernaryExpressionCodeRefactoringProvider>(@"
class TestClass
{
    void TestMethod(int i)
    {
        int a;
        $if (i > 0)
            a = 0;
        a = 1;
    }
}");
        }
    }
}