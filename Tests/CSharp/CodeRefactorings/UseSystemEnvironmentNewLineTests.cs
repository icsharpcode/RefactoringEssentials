using RefactoringEssentials.CSharp.CodeRefactorings;
using Xunit;

namespace RefactoringEssentials.Tests.CSharp.CodeRefactorings
{
    public class UseSystemEnvironmentNewLineTests : CSharpCodeRefactoringTestBase
    {
        [Fact]
        public void TestLFString()
        {
            Test<UseSystemEnvironmentNewLineCodeRefactoringProvider>(@"
class TestClass
{
    void TestMethod()
    {
        string str = …""\n"";
    }
}", @"
class TestClass
{
    void TestMethod()
    {
        string str = System.Environment.NewLine;
    }
}");
        }

        [Fact]
        public void TestCRString()
        {
            Test<UseSystemEnvironmentNewLineCodeRefactoringProvider>(@"
class TestClass
{
    void TestMethod()
    {
        string str = …""\r"";
    }
}", @"
class TestClass
{
    void TestMethod()
    {
        string str = System.Environment.NewLine;
    }
}");
        }


        [Fact]
        public void TestCRLFString()
        {
            Test<UseSystemEnvironmentNewLineCodeRefactoringProvider>(@"
class TestClass
{
    void TestMethod()
    {
        string str = …""\r\n"";
    }
}", @"
class TestClass
{
    void TestMethod()
    {
        string str = System.Environment.NewLine;
    }
}");
        }
    }
}
