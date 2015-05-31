using NUnit.Framework;
using RefactoringEssentials.CSharp.CodeRefactorings;

namespace RefactoringEssentials.Tests.CSharp.CodeRefactorings
{
    [TestFixture]
    public class UseSystemEnvironmentNewLineTests : CSharpCodeRefactoringTestBase
    {
        [Test]
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

        [Test]
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


        [Test]
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
