using NUnit.Framework;
using RefactoringEssentials.CSharp.CodeRefactorings;

namespace RefactoringEssentials.Tests.CSharp.CodeRefactorings
{
    [TestFixture]
    public class ConvertLambdaBodyExpressionToStatementTests : CSharpCodeRefactoringTestBase
    {
        [Test]
        public void TestReturn()
        {
            Test<ConvertLambdaBodyExpressionToStatementCodeRefactoringProvider>(@"
class TestClass
{
    void TestMethod ()
    {
        System.Func<int, int> f = i $=> i + 1;
    }
}", @"
class TestClass
{
    void TestMethod ()
    {
        System.Func<int, int> f = i =>
        {
            return i + 1;
        };
    }
}");
        }

        [Test]
        public void TestExprStatement()
        {
            Test<ConvertLambdaBodyExpressionToStatementCodeRefactoringProvider>(@"
class TestClass
{
    void TestMethod ()
    {
        System.Action<int> f = i $=> i++;
    }
}", @"
class TestClass
{
    void TestMethod ()
    {
        System.Action<int> f = i =>
        {
            i++;
        };
    }
}");
        }

        [Test]
        public void TestExprStatementWithComment()
        {
            Test<ConvertLambdaBodyExpressionToStatementCodeRefactoringProvider>(@"
class TestClass
{
    void TestMethod ()
    {
		// Some comment
        System.Action<int> f = i $=> i++;
    }
}", @"
class TestClass
{
    void TestMethod ()
    {
		// Some comment
        System.Action<int> f = i =>
        {
            i++;
        };
    }
}");
        }

        [Test]
        public void TestParenthesizedLambdaExprStatement()
        {
            Test<ConvertLambdaBodyExpressionToStatementCodeRefactoringProvider>(@"
class TestClass
{
    void TestMethod ()
    {
        System.Action<int> f = (i) $=> i++;
    }
}", @"
class TestClass
{
    void TestMethod ()
    {
        System.Action<int> f = (i) =>
        {
            i++;
        };
    }
}");
        }

        [Test]
        public void TestWrongContext()
        {
            TestWrongContext<ConvertLambdaBodyExpressionToStatementCodeRefactoringProvider>(@"
class TestClass
{
    void TestMethod ()
    {
        System.Func<int, int> f = i $=>
        {
            return i + 1;
        };
    }
}");
        }

        [Test]
        public void TestParenthesis()
        {
            Test<ConvertLambdaBodyExpressionToStatementCodeRefactoringProvider>(@"
class TestClass
{
    void TestMethod ()
    {
        System.Func<int, int> f;
        f = (i $=> i + 1);
    }
}", @"
class TestClass
{
    void TestMethod ()
    {
        System.Func<int, int> f;
        f = (i => { return i + 1; });
    }
}");
        }

        [Test]
        public void TestInvocation()
        {
            Test<ConvertLambdaBodyExpressionToStatementCodeRefactoringProvider>(@"
class TestClass
{
    void Test (int k, System.Func<int, int> f) { }
    void TestMethod ()
    {
        Test (1, i $=> i + 1);
    }
}", @"
class TestClass
{
    void Test (int k, System.Func<int, int> f) { }
    void TestMethod ()
    {
        Test (1, i => { return i + 1; });
    }
}");
            Test<ConvertLambdaBodyExpressionToStatementCodeRefactoringProvider>(@"
class TestClass
{
    void Test2 (System.Action<int> a) { }
    void TestMethod ()
    {
        Test2 (i $=> i++);
    }
}", @"
class TestClass
{
    void Test2 (System.Action<int> a) { }
    void TestMethod ()
    {
        Test2 (i => { i++; });
    }
}");
        }
    }
}
