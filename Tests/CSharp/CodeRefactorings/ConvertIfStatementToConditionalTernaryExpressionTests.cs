using RefactoringEssentials.CSharp.CodeRefactorings;
using Xunit;

namespace RefactoringEssentials.Tests.CSharp.CodeRefactorings
{
    public class ConvertIfStatementToConditionalTernaryExpressionTests : CSharpCodeRefactoringTestBase
    {
        [Fact]
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

        [Fact]
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

        [Fact]
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

        [Fact]
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

        [Fact]
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

        [Fact]
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

        [Fact]
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

        [Fact]
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

        [Fact]
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

        [Fact]
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