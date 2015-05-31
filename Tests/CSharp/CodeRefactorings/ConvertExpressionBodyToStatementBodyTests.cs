using NUnit.Framework;
using RefactoringEssentials.CSharp.CodeRefactorings;

namespace RefactoringEssentials.Tests.CSharp.CodeRefactorings
{
    [TestFixture]
    public class ConvertExpressionBodyToStatementBodyTests : CSharpCodeRefactoringTestBase
    {
        [Test]
        public void TestMethodName()
        {
            Test<ConvertExpressionBodyToStatementBodyCodeRefactoringProvider>(@"
class TestClass
{
    int $TestMethod(int i) => i << 8;
}", @"
class TestClass
{
    int TestMethod(int i)
    {
        return i << 8;
    }
}");
        }


        [Test]
        public void TestPropertyName()
        {
            Test<ConvertExpressionBodyToStatementBodyCodeRefactoringProvider>(@"
class TestClass
{
    int $TestProperty => 321;
}", @"
class TestClass
{
    int TestProperty
    {
        get
        {
            return 321;
        }
    }
}");
        }
    }
}

