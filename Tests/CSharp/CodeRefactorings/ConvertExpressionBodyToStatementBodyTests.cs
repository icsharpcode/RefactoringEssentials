using RefactoringEssentials.CSharp.CodeRefactorings;
using Xunit;

namespace RefactoringEssentials.Tests.CSharp.CodeRefactorings
{
    public class ConvertExpressionBodyToStatementBodyTests : CSharpCodeRefactoringTestBase
    {
        [Fact]
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


        [Fact]
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

        [Fact]
        public void TestVoidMethod()
        {
            Test<ConvertExpressionBodyToStatementBodyCodeRefactoringProvider>(@"
class TestClass
{
    void Foo();
    void $TestMethod() => Foo();
}", @"
class TestClass
{
    void Foo();
    void TestMethod()
    {
        Foo();
    }
}");
        }
    }
}

